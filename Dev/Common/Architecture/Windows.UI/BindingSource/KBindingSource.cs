using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing.Design;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI.Design;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Extends the behaviour of System.Windows.Forms.BindingSource. The IExtenderProvider
	/// implementation of this class surface a BindingMember property for each control on
	/// the design surface. The string value of BindingMember describes the property to bind
	/// the value of the control to.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
	[ProvideProperty("BindingMember", typeof(Control))]
	[ProvideProperty("BindingMembersForCompileTimeCheck", typeof(Control))]
	[ProvideProperty("BindingMemberTypeFilterForEditor", typeof(Control))]
	[DesignerSerializer(typeof(BindingSourceCodeDomSerialiser), typeof(CodeDomSerializer))]
	public class KBindingSource :
				Component,
				ICompositeControlBindingSource,
				IExtenderProvider,
				IDesignerActionItemSource,
				IBindingListView,
				ITypedList,
				ICancelAddNew,
				ISupportInitializeNotification,
				ICurrencyManagerProvider
	{
		internal class BindingSourceCodeDomSerialiser : TypeFixCodeDomSerializer { public BindingSourceCodeDomSerialiser() : base(typeof(KBindingSource)) { } }

		#region Constructors

		public KBindingSource()
		{
		}

		public KBindingSource(IContainer container)
		{ container.Add(this); }

		public KBindingSource(Control containerControl)
		{ this.ContainerControl = containerControl; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public KBindingSource(Control containerControl, Type dataSourceType)
			: this(containerControl)
		{ this.DataSourceType = dataSourceType; }

		#endregion

		#region GetBindingSource

		/// <summary>
		/// Get an instance of IBindingSource that is implemented on the given control, or one of
		/// the control's parents in the hierarchy.
		/// </summary>
		public static ICompositeControlBindingSource GetBindingSource(Control control)
		{
			return GetBindingSource(control, false);
		}

		/// <summary>
		/// Get an instance of IBindingSource that is implemented on the given control, or one of
		/// the control's parents in the hierarchy.
		/// </summary>
		public static ICompositeControlBindingSource GetBindingSource(Control control, bool throwOnError)
		{
			ICompositeControlBindingSource result = (ICompositeControlBindingSource)control.GetUserData(BindingSourceKey);
			if (result == null)
			{
				ICompositeControlBindingSourceProvider provider = null;
				Control current = control.Parent;
				while (current != null && provider == null)
				{
					provider = current as ICompositeControlBindingSourceProvider;
					current = current.Parent;
				}
				if (throwOnError && provider == null)
				{
					throw new InvalidOperationException("Could not find IBindingSource implemented on control '" + control.Name + "' or any of it's parents.");
				}
				result = provider == null ? null : provider.BindingSource;
			}
			return result;
		}
		static readonly int BindingSourceKey = ControlExtensions.CreateUserDataKey();

		#endregion

		#region ContainerControl

		[Browsable(false)]
		public Control ContainerControl { get; set; }

		Control ContainerControlInternal
		{
			get
			{
				if (ContainerControl == null)
				{
					throw new InvalidOperationException("ContainerControl must be set");
				}
				return ContainerControl;
			}
		}

		#endregion

		#region Site

		public override ISite Site
		{
			get { return base.Site; }
			set
			{
				base.Site = value;
				if (value != null)
				{
					IDesignerHost host = value.GetService(typeof(IDesignerHost)) as IDesignerHost;
					if (host != null)
					{
						ContainerControl root = host.RootComponent as ContainerControl;
						if (root != null)
						{
							ContainerControl = root;
						}
					}
				}
				InheritedPropertyValues.NotifySiteChanged(this);
				isDesignModePopulated = false;
			}
		}

		#endregion

		#region BindingContext

		[Browsable(false)]
		public BindingContext BindingContext
		{
			get
			{
				if (bindingContext == null && DataSource != null)
				{
					bindingContext = ContainerControl == null ? new BindingContext() : ContainerControl.BindingContext;
				}
				return bindingContext;
			}
		}
		BindingContext bindingContext;

		#endregion

		#region IsDesignMode / SynchroniseNameWithBindingMemberInSmartTags / SupportMultipleTwoWayBoundPropertiesOnOneControl

		internal bool IsDesignMode
		{
			get
			{
				if (!isDesignModePopulated)
				{
					isDesignMode =
						this.IsDesignMode() ||
						(ContainerControl != null && ContainerControl.IsDesignMode());
					isDesignModePopulated = true;
				}
				return isDesignMode;
			}
		}
		bool isDesignMode;
		bool isDesignModePopulated;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Synchronise")]
		[DefaultValue(false)]
		[Category(DesignerConstants.Category)]
		public bool SynchroniseNameWithBindingMemberInSmartTags { get; set; }

		[DefaultValue(false)]
		[Category(DesignerConstants.Category)]
		public bool SupportMultipleTwoWayBoundPropertiesOnOneControl { get; set; }

		#endregion

		#region BoundControls

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IEnumerable<Control> BoundControls
		{
			get
			{
				foreach (Control control in bindings.Keys)
				{
					yield return control;
				}
			}
		}

		#endregion

		#region Compile Time Checking Code

		/// <summary>
		/// This method is intended for compile-time checking only and should not be used in code.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CompileTimeCheckBindingMemberCollection GetBindingMembersForCompileTimeCheck(Control control)
		{
			ControlBinding b = null;
			bindings.TryGetValue(control, out b);
			if (b != null)
			{
				return b.BindingMembersForCompileTimeCheck;
			}
			else
			{
				return new CompileTimeCheckBindingMemberCollection();
			}
		}

		protected bool ShouldSerializeBindingMembersForCompileTimeCheck(Control control)
		{
			ControlBinding b = null;
			bindings.TryGetValue(control, out b);
			return
				(b != null) &&
				ShouldSerializeBindingMember(control) &&
				b.ShouldSerializeBindingMembersForCompileTimeCheck;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "dataSourceType")]
		protected internal virtual CompileTimeCheckBindingMember NewCompileTimeCheckBindingMember(Type dataSourceType, Type controlPropertyType, string bindingMember)
		{
			return new CompileTimeCheckBindingMember(dataSourceType, controlPropertyType, bindingMember);
		}

		#endregion

		#region Get/SetBindingMember

		/// <summary>
		/// Get the value of BindingMember for a given control.
		/// </summary>
		[Category(DesignerConstants.Category)]
		[SmartTagVisible(0)]
		[Editor(DesignerTypes.BindingMemberEditor, typeof(UITypeEditor))]
		[BindingMemberEditor("DataSourceTypeForBindingMemberEditor", "BindingMemberTypeFilterForEditor")]
		public virtual string GetBindingMember(Control control)
		{
			ControlBinding b = null;
			bindings.TryGetValue(control, out b);
			return (b == null) ? "" : b.RelativeBindingMember;
		}

		/// <summary>
		/// Set the value of BindingMember for a given control.
		/// </summary>
		[Category(DesignerConstants.Category)]
		[SmartTagVisible(0)]
		[Editor(DesignerTypes.BindingMemberEditor, typeof(UITypeEditor))]
		[BindingMemberEditor("DataSourceTypeForBindingMemberEditor", "BindingMemberTypeFilterForEditor")]
		public virtual void SetBindingMember(Control control, string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				ControlBinding binding = GetControlBinding(control);
				if (binding != null)
				{
					binding.Dispose();
					control.Disposed -= Control_Disposed;
					bindings.Remove(control);
					OnFullBindingMemberChanged(new ControlBindingMemberChangedEventArgs(control, value));
				}
			}
			else
			{
				ControlBinding binding = GetOrCreateControlBinding(control);
				if (binding.RelativeBindingMember != value)
				{
					binding.RelativeBindingMember = value;
					OnFullBindingMemberChanged(new ControlBindingMemberChangedEventArgs(control, value));
				}
				binding.Enabled = DataSource != null;

				if (this.IsDesignMode() && !IsDataSourceTypeValid) // too early to access KBindingSource.IsDesignMode here
				{
					WarnUserIfDataSourceTypeNotValid_OnDesignerLoaded();
				}
			}
			if (!string.IsNullOrEmpty(value) && !BelongsToFormOrUserControl)
			{
				control.SetUserData(BindingSourceKey, this);
			}
		}

		protected bool ShouldSerializeBindingMember(Control control)
		{
			string inheritedValue = (string)InheritedPropertyValues.GetInheritedValue(control, "BindingMember");
			string currentValue = GetBindingMember(control);
			return
				(inheritedValue == null && !string.IsNullOrEmpty(currentValue)) ||
				(inheritedValue != null && !object.Equals(inheritedValue, currentValue));
		}

		bool BelongsToFormOrUserControl
		{
			get
			{
				ICompositeControlBindingSourceProvider provider = ContainerControl as ICompositeControlBindingSourceProvider;
				return provider != null && provider.BindingSource == this;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Obsolete("Not intended to be used in your code", true)]
		public Type GetBindingMemberTypeFilterForEditor(Control control)
		{
			IDataBoundControl boundControl = control == null ? null : DataBoundControl.Get(control);
			return (boundControl == null) ? typeof(object) : boundControl.DataSourceType;
		}

		#endregion

		#region DataSourceType

		/// <summary>
		/// Get or set the type of the data source.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Category(DesignerConstants.Category)]
		[Editor(DesignerTypes.TypeValueIntellisenseEditor, typeof(UITypeEditor))]
		[TypeConverter(typeof(TypeTypeConverter))]
		[Description(
			"You should set this property before compiling. Set it to the type of the root entity " +
			"that the control binds to. Set to System.Object to allow binding to any type of " +
			"object. Set to void to prevent binding to this control.")]
		public Type DataSourceType
		{
			get { return dataSourceType; }
			set
			{
				dataSourceType = value;
				if (this.IsDesignMode())
				{
					if (IsTypeFakeAndNotExistsInSolution(value))
					{
						DesignTimeUI.ShowMessage(this, (NoResString)"Could not find the type '" + value.FullName + "'");
					}
					PokeCompileTimeCheckPropertiesForDesignerSerialization();
				}
			}
		}
		Type dataSourceType;

		protected Type DataSourceTypeForBindingMemberEditor
		{
			get { return DataSourceType; }
		}

		void PokeCompileTimeCheckPropertiesForDesignerSerialization()
		{
			if (Site != null && Site.DesignMode)
			{
				IComponentChangeService changeService = (IComponentChangeService)Site.GetService(typeof(IComponentChangeService));
				if (changeService != null)
				{
					foreach (IComponent component in Site.Container.Components)
					{
						Control next = component as Control;
						if (next != null && CanExtend(next))
						{
							PropertyDescriptor compileTimeCheckProperty = TypeDescriptor.GetProperties(next)["BindingMembersForCompileTimeCheck"];
							if (compileTimeCheckProperty != null)
							{
								changeService.OnComponentChanged(next, compileTimeCheckProperty, null, GetBindingMembersForCompileTimeCheck(next));
							}
						}
					}
				}
			}
		}

		static bool IsTypeFakeAndNotExistsInSolution(Type value)
		{ return TypeNameHolder.IsTypeFakeAndNotExistsInSolution(value); }

		bool IsDataSourceTypeValid
		{ get { return DataSourceType != null && !IsTypeFakeAndNotExistsInSolution(DataSourceType); } }

		/// <summary>
		/// Not intended to be used in your code.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions"), EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void WarnUserIfDataSourceTypeNotValid()
		{
			if (!IsDataSourceTypeValid && this.IsDesignMode())
			{
				string message = (NoResString)"You must set a valid DataSourceType property on the " + nameof(KBindingSource) + ".";
				DesignTimeUI.ShowMessage(this, message);
			}
		}

		void WarnUserIfDataSourceTypeNotValid_OnDesignerLoaded()
		{
			IDesignerHost designerHost = Site == null ? null : (IDesignerHost)Site.GetService(typeof(IDesignerHost));
			if (designerHost != null && designerHost.Loading)
			{
				designerHost.LoadComplete -= new EventHandler(DesignerHostLoadComplete_WarnUserIfDataSourceTypeNotValid);
				designerHost.LoadComplete += new EventHandler(DesignerHostLoadComplete_WarnUserIfDataSourceTypeNotValid);
			}
			else
			{
				WarnUserIfDataSourceTypeNotValid();
			}
		}

		void DesignerHostLoadComplete_WarnUserIfDataSourceTypeNotValid(object sender, EventArgs e)
		{
			IDesignerHost designerHost = (IDesignerHost)sender;
			designerHost.LoadComplete -= new EventHandler(DesignerHostLoadComplete_WarnUserIfDataSourceTypeNotValid);
			WarnUserIfDataSourceTypeNotValid();
		}

		#endregion

		#region DataSource, DataMember

		/// <summary>
		/// Get or set the DataSource of the data that controls will be bound to.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[AttributeProvider(typeof(IListSource))]
		public object DataSource
		{
			get { return currentDataSource; }
			set { SetDataBinding(value, DataMember); }
		}

		/// <summary>
		/// Get or set the top-level data member on the data source.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string DataMember
		{
			get { return currentDataMember; }
			set { SetDataBinding(DataSource, value); }
		}

		#endregion

		#region Current

		/// <summary>
		/// Get the data that is specified from the DataSource to the DataMember property
		/// (ie. using BindingContext[DataSource, DataMember]).
		/// DataSourceType is used as a hint to determine whether the current element or the current list should be returned.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object Current
		{
			get
			{
				object result = null;
				if (DataSource != null)
				{
					BindingManagerBase bm = BindingContext[DataSource, DataMember];
					CurrencyManager cm = bm as CurrencyManager;
					if (cm != null && (DataSourceType == null || DataSourceType.IsInstanceOfType(cm.List)))
					{
						result = cm.List;
					}
					else if (bm.Position != -1)
					{
						result = bm.Count == 0 ? null : bm.Current;
					}
				}
				return result;
			}
		}

		#endregion

		#region SetDataBinding

		/// <summary>
		/// Set the DataSource and dataMember in one go.
		/// </summary>
		public virtual void SetDataBinding(object dataSource, string dataMember)
		{
			bool dataSourceHasChanged = dataSource != this.DataSource;
			bool dataMemberHasChanged = dataMember != this.DataMember;
			if (dataSourceHasChanged || dataMemberHasChanged)
			{
				if (dataSource != null && this.inSetDataBinding)
				{
					throw new InvalidOperationException("You cannot call this method recursively");
				}
				if (!this.inSetDataBinding)
				{
					if (dataSource != null &&
							(DataSourceType == null || DataSourceType == typeof(void)) &&
	ContainerControl != null &&
							!IsDesignMode)
					{
						throw new InvalidOperationException("Top-level data source type not set on the KBindingSource for '" + ContainerControlInternal.GetType().FullName + "'");
					}

					this.inSetDataBinding = true;
					try
					{
						SetDataBindingCore(dataSource, dataMember);
					}
					finally
					{
						inSetDataBinding = false;
					}
				}
			}
			if (dataSourceHasChanged)
			{
				OnDataSourceChanged(EventArgs.Empty);
			}

			if (dataMemberHasChanged)
			{
				OnDataMemberChanged(EventArgs.Empty);
			}
		}
		bool inSetDataBinding;

		void SetDataBindingCore(object dataSource, string dataMember)
		{
			object oldDataSource = currentDataSource;

			IDataSourceEvents oldEntity = oldDataSource as IDataSourceEvents;
			if (oldEntity != null)
			{
				oldEntity.DataSourcePosting -= new EventHandler(DataSource_Posting);
			}

			currentDataSource = dataSource;
			currentDataMember = dataMember;

			if (oldDataSource != null)
			{
				StopBinding();
			}

			if (currentDataSource != null)
			{
				StartBinding();
			}

			if (bindingSource != null)
			{
				bindingSource.DataSource = null;
				bindingSource.DataMember = dataMember;
				bindingSource.DataSource = dataSource;
			}

			IDataSourceEvents newEntity = DataSource as IDataSourceEvents;
			if (newEntity != null)
			{
				newEntity.DataSourcePosting += new EventHandler(DataSource_Posting);
			}
		}

		void StartBinding()
		{
			KDataBindingException errors = null;

			using (PerformDataSourceInitialization(DataSource as ISupportInitialize)) // for performance
			{
				foreach (ControlBinding binding in TryAndRetryEnumerationInfinitely(bindings.Values))
				{
					try
					{
						binding.Enabled = true;
					}
					catch (KDataBindingException e)
					{
						if (errors == null)
						{
							errors = new KDataBindingException(DataSource, e);
						}

						errors.Add(e);
					}
				}
			}
			if (errors != null)
			{
				throw errors;
			}
		}

		void StopBinding()
		{
			foreach (ControlBinding binding in TryAndRetryEnumerationInfinitely(bindings.Values))
			{
				binding.Enabled = false;
			}
			bindingContext = null;
		}

		static IEnumerable<T> TryAndRetryEnumerationInfinitely<T>(IEnumerable<T> enumerable)
		{
			IEnumerator<T> enumerator = enumerable.GetEnumerator();
			while (true)
			{
				try
				{
					if (!enumerator.MoveNext())
					{
						break;
					}
				}
				catch (InvalidOperationException)
				{
					enumerator = enumerable.GetEnumerator();
					continue;
				}
				yield return enumerator.Current;
			}
		}

		static IDisposable PerformDataSourceInitialization(ISupportInitialize initialization)
		{
			IDisposable result;
			if (initialization != null)
			{
				initialization.BeginInit();
				result = new DisposableAction(initialization.EndInit);
			}
			else
			{
				result = DisposableAction.NoAction;
			}
			return result;
		}

		#endregion

		#region ForceBinding

		public void ForceBinding(Control control)
		{
			ControlBinding binding = GetControlBinding(control);
			if (binding != null)
			{
				binding.ForceBinding();
			}
		}

		#endregion

		#region EndCurrentEdit

		void EndCurrentEdit()
		{
			Control activeDataBoundControl = this.ActiveDataBoundControl;
			if (activeDataBoundControl != null)
			{
				KEndCurrentEdit.EndCurrentEdit(activeDataBoundControl);
			}
		}

		Control ActiveDataBoundControl
		{
			get
			{
				Control current = ActiveControl;
				while (current != null)
				{
					if (current is IDataBoundControl || GetControlBinding(current) != null)
					{
						return current;
					}
					current = current.Parent;
				}
				return null;
			}
		}

		Control ActiveControl
		{
			get
			{
				Control result = null;
				IContainerControl container = ContainerControlInternal as IContainerControl;
				Control current = ContainerControl == null ? null : (container ?? ContainerControlInternal.GetContainerControl()).ActiveControl;
				while (current != null)
				{
					result = current;
					ContainerControl currentContainerControl = current as ContainerControl;
					current = (currentContainerControl == null) ? null : currentContainerControl.ActiveControl;
				}
				return result;
			}
		}

		#endregion

		#region Design-time Code Navigation Verbs

		void NavigateToBoundProperty(object control)
		{
			Type endDataSourceType;
			string bindingField;
			if (TryGetBindingComponentTypeAndBindingField(control, out endDataSourceType, out bindingField))
			{
				InvokeDataSourceTypeMemberOverrider(endDataSourceType, "NavigateToMember", bindingField);
			}
		}

		void OverrideBoundProperty(object control)
		{
			Type endDataSourceType;
			string bindingField;
			if (TryGetBindingComponentTypeAndBindingField(control, out endDataSourceType, out bindingField))
			{
				InvokeDataSourceTypeMemberOverrider(endDataSourceType, "OverrideOrNavigateToMember", bindingField);
			}
		}

		void InvokeDataSourceTypeMemberOverrider(Type componentType, string method, params object[] args)
		{
			using (new CursorSwitcher(Cursors.WaitCursor))
			{
				object overrider = GetDataSourceTypeMemberOverrider(componentType);
				try
				{
					overrider.GetType().InvokeMember(method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, overrider, args, CultureInfo.InvariantCulture);
				}
				catch (TargetInvocationException ex)
				{
					throw ex.InnerException;
				}
			}
		}

		[ThreadSafe]
		static readonly Lazy<Dictionary<Type, Func<IServiceProvider, string, object>>> _delegateConstructorCache =
			new Lazy<Dictionary<Type, Func<IServiceProvider, string, object>>>(() => new Dictionary<Type, Func<IServiceProvider, string, object>>());
		object GetDataSourceTypeMemberOverrider(Type componentType)
		{
			Type overriderType = Type.GetType(DesignerTypes.CodeTypeMemberOverrider);
			if (!_delegateConstructorCache.Value.TryGetValue(overriderType, out var constructorDelegate))
			{
				ConstructorInfo constructor = overriderType.GetConstructor(new Type[] { typeof(IServiceProvider), typeof(string) });

				ParameterExpression serviceProviderParam = Expression.Parameter(typeof(IServiceProvider));
				ParameterExpression typeNameParam = Expression.Parameter(typeof(string));
				NewExpression newExpression = Expression.New(constructor, serviceProviderParam, typeNameParam);

				constructorDelegate =
					Expression.Lambda<Func<IServiceProvider, string, object>>(newExpression, serviceProviderParam, typeNameParam).Compile();
				_delegateConstructorCache.Value[overriderType] = constructorDelegate;
			}
			return constructorDelegate(Site, componentType.FullName);
		}

		string GetBindingMemberOrBindTo(Control control)
		{
			var bindableControl = control as IBindTo;
			if (bindableControl != null)
			{
				return bindableControl.BindTo;
			}

			var bindToProperty = TypeDescriptor.GetProperties(control)["BindTo"];
			if (bindToProperty != null)
			{
				return (string)bindToProperty.GetValue(control);
			}

			return GetBindingMember(control);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		bool TryGetBindingComponentTypeAndBindingField(object controlObj, out Type bindingComponentType, out string bindingField)
		{
			bindingComponentType = null;
			bindingField = null;

			bool result = false;
			Control control = controlObj as Control;
			if (control != null)
			{
				KBindingMemberInfo bindingMemberInfo = new KBindingMemberInfo(GetBindingMemberOrBindTo(control));
				bindingField = bindingMemberInfo.BindingField;
				if (string.IsNullOrEmpty(DataMember) && string.IsNullOrEmpty(bindingMemberInfo.BindingPath))
				{
					bindingComponentType = ListUtil.GetListElementType(DataSourceType) ?? DataSourceType;
					result = true;
				}
				else if (DesignTimeDataSource != null)
				{
					BindingManagerBase bm = BindingContext[DesignTimeDataSource, bindingMemberInfo.BindingPath];
					bindingComponentType = GetBindingComponentTypeFromBM(bm);
					result = bindingComponentType != null;
				}
			}

			if (!result)
			{
				DesignTimeUI.ShowMessage(this, (NoResString)"Could not find the data type this control binds to. Try rebuilding the project that contains the data source type."); // Used for design time only
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule")]
		static Type GetBindingComponentTypeFromBM(BindingManagerBase bm)
		{
			Type result = null;
			CurrencyManager cm = bm as CurrencyManager;
			if (cm != null)
			{
				result = ListUtil.GetListElementType(cm.List);
			}
			else
			{
				result = bm.Current.GetType();
			}
			return result;
		}

		#endregion

		#region .net BindingSource and interface delegation

		FixedDotNetBindingSource BindingSource
		{
			get
			{
				if (bindingSource == null)
				{
					bindingSource = new FixedDotNetBindingSource(DataSource, DataMember);
				}
				return bindingSource;
			}
		}
		FixedDotNetBindingSource bindingSource;

		#region IBindingListView Members

		void IBindingListView.ApplySort(ListSortDescriptionCollection sorts)
		{ (BindingSource as IBindingListView).ApplySort(sorts); }

		string IBindingListView.Filter
		{
			get { return (BindingSource as IBindingListView).Filter; }
			set { (BindingSource as IBindingListView).Filter = value; }
		}

		void IBindingListView.RemoveFilter()
		{ (BindingSource as IBindingListView).RemoveFilter(); }

		ListSortDescriptionCollection IBindingListView.SortDescriptions
		{ get { return (BindingSource as IBindingListView).SortDescriptions; } }

		bool IBindingListView.SupportsAdvancedSorting
		{ get { return (BindingSource as IBindingListView).SupportsAdvancedSorting; } }

		bool IBindingListView.SupportsFiltering
		{ get { return (BindingSource as IBindingListView).SupportsFiltering; } }

		#endregion

		#region IBindingList Members

		void IBindingList.AddIndex(PropertyDescriptor property)
		{ (BindingSource as IBindingList).AddIndex(property); }

		object IBindingList.AddNew()
		{ return (BindingSource as IBindingList).AddNew(); }

		bool IBindingList.AllowEdit
		{ get { return (BindingSource as IBindingList).AllowEdit; } }

		bool IBindingList.AllowNew
		{ get { return (BindingSource as IBindingList).AllowNew; } }

		bool IBindingList.AllowRemove
		{ get { return (BindingSource as IBindingList).AllowRemove; } }

		void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{ (BindingSource as IBindingList).ApplySort(property, direction); }

		int IBindingList.Find(PropertyDescriptor property, object key)
		{ return (BindingSource as IBindingList).Find(property, key); }

		bool IBindingList.IsSorted
		{ get { return (BindingSource as IBindingList).IsSorted; } }

		event ListChangedEventHandler IBindingList.ListChanged
		{
			add { (BindingSource as IBindingList).ListChanged += value; }
			remove { (BindingSource as IBindingList).ListChanged -= value; }
		}

		void IBindingList.RemoveIndex(PropertyDescriptor property)
		{ (BindingSource as IBindingList).RemoveIndex(property); }

		void IBindingList.RemoveSort()
		{ (BindingSource as IBindingList).RemoveSort(); }

		ListSortDirection IBindingList.SortDirection
		{ get { return (BindingSource as IBindingList).SortDirection; } }

		PropertyDescriptor IBindingList.SortProperty
		{ get { return (BindingSource as IBindingList).SortProperty; } }

		bool IBindingList.SupportsChangeNotification
		{ get { return (BindingSource as IBindingList).SupportsChangeNotification; } }

		bool IBindingList.SupportsSearching
		{ get { return (BindingSource as IBindingList).SupportsSearching; } }

		bool IBindingList.SupportsSorting
		{ get { return (BindingSource as IBindingList).SupportsSorting; } }

		#endregion

		#region IList Members

		int IList.Add(object value)
		{ return (BindingSource as IList).Add(value); }

		void IList.Clear()
		{ (BindingSource as IList).Clear(); }

		bool IList.Contains(object value)
		{ return (BindingSource as IList).Contains(value); }

		int IList.IndexOf(object value)
		{ return (BindingSource as IList).IndexOf(value); }

		void IList.Insert(int index, object value)
		{ (BindingSource as IList).Insert(index, value); }

		bool IList.IsFixedSize
		{ get { return (BindingSource as IList).IsFixedSize; } }

		bool IList.IsReadOnly
		{ get { return (BindingSource as IList).IsReadOnly; } }

		void IList.Remove(object value)
		{ (BindingSource as IList).Remove(value); }

		void IList.RemoveAt(int index)
		{ (BindingSource as IList).RemoveAt(index); }

		object IList.this[int index]
		{
			get { return (BindingSource as IList)[index]; }
			set { (BindingSource as IList)[index] = value; }
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo(Array array, int index)
		{ (BindingSource as ICollection).CopyTo(array, index); }

		int ICollection.Count
		{ get { return (BindingSource as ICollection).Count; } }

		bool ICollection.IsSynchronized
		{ get { return (BindingSource as ICollection).IsSynchronized; } }

		object ICollection.SyncRoot
		{ get { return (BindingSource as ICollection).SyncRoot; } }

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{ return (BindingSource as IEnumerable).GetEnumerator(); }

		#endregion

		#region ITypedList Members

		PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
		{ return (bindingSource as ITypedList).GetItemProperties(listAccessors); }

		string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
		{ return (bindingSource as ITypedList).GetListName(listAccessors); }

		#endregion

		#region ICancelAddNew Members

		void ICancelAddNew.CancelNew(int itemIndex)
		{ (BindingSource as ICancelAddNew).CancelNew(itemIndex); }

		void ICancelAddNew.EndNew(int itemIndex)
		{ (BindingSource as ICancelAddNew).EndNew(itemIndex); }

		#endregion

		#region ISupportInitializeNotification Members

		event EventHandler ISupportInitializeNotification.Initialized
		{
			add
			{
				if (bindingSource != null)
				{
					(bindingSource as ISupportInitializeNotification).Initialized += value;
				}
			}
			remove
			{
				if (bindingSource != null)
				{
					(bindingSource as ISupportInitializeNotification).Initialized -= value;
				}
			}
		}

		bool ISupportInitializeNotification.IsInitialized
		{ get { return bindingSource != null && (bindingSource as ISupportInitializeNotification).IsInitialized; } }

		#endregion

		#region ISupportInitialize Members

		void ISupportInitialize.BeginInit()
		{
			if (bindingSource != null)
			{
				(bindingSource as ISupportInitialize).BeginInit();
			}
		}

		void ISupportInitialize.EndInit()
		{
			if (bindingSource != null)
			{
				(bindingSource as ISupportInitialize).EndInit();
			}
		}

		#endregion

		#region ICurrencyManagerProvider Members

		CurrencyManager ICurrencyManagerProvider.CurrencyManager
		{ get { return (BindingSource as ICurrencyManagerProvider).CurrencyManager; } }

		CurrencyManager ICurrencyManagerProvider.GetRelatedCurrencyManager(string dataMember)
		{ return (BindingSource as ICurrencyManagerProvider).GetRelatedCurrencyManager(dataMember); }

		#endregion

		#endregion

		#region ICompositeControlBindingSource

		public string GetFullBindingMember(Control control)
		{
			ControlBinding binding = GetControlBinding(control);
			return binding == null ? "" : binding.FullBindingMember.BindingMember;
		}

		[Browsable(false)]
		public IEnumerable<KeyValuePair<Control, string>> FullBindingMembers
		{
			get
			{
				foreach (ControlBinding binding in bindings.Values)
				{
					yield return new KeyValuePair<Control, string>(binding.Control, binding.FullBindingMember.BindingMember);
				}
			}
		}

		public event EventHandler DataSourceChanged;
		public event EventHandler<ControlBindingMemberChangedEventArgs> FullBindingMemberChanged;

		protected virtual void OnDataSourceChanged(EventArgs e)
		{
			if (DataSourceChanged != null)
			{
				DataSourceChanged(this, EventArgs.Empty);
			}
		}

		protected virtual void OnDataMemberChanged(EventArgs e)
		{ OnFullBindingMemberChanged(new ControlBindingMemberChangedEventArgs(null, null)); }

		protected virtual void OnFullBindingMemberChanged(ControlBindingMemberChangedEventArgs e)
		{
			if (FullBindingMemberChanged != null)
			{
				FullBindingMemberChanged(this, e);
			}
		}

		#endregion

		#region IExtenderProvider

		readonly InheritedExtendedPropertyValueMemory InheritedPropertyValues = new InheritedExtendedPropertyValueMemory();

		bool IExtenderProvider.CanExtend(object value)
		{
			return CanExtend(value);
		}

		protected bool CanExtend(object value)
		{
			bool result = false;
			if (value is Control && ContainerControl != value)
			{
				IDataBoundControl dataBoundControl = value as IDataBoundControl;
				result =
					BindableComponentMetaDataPropertyLocator.GetInstance(value.GetType()).DefaultBindingProperty != null ||
					(dataBoundControl != null && dataBoundControl.DataSourceType != null);
			}
			return result;
		}

		#endregion

		#region IDesignerActionItemSource Members

		DesignerActionItem[] IDesignerActionItemSource.GetSortedActionItems(DesignerActionList actionList)
		{ return GetSortedActionItems(actionList); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods")]
		protected DesignerActionItem[] GetSortedActionItems(DesignerActionList actionList)
		{
			List<DesignerActionItem> result = new List<DesignerActionItem>();
			Control control = actionList.Component as Control;
			if (control != null)
			{
				bool hasBindingMember = !string.IsNullOrEmpty(GetBindingMemberOrBindTo(control));
				result.Add(new KDesignerActionMethodItem(actionList, delegate
				{ NavigateToBoundProperty(control); }, (NoResString)"Go to Bound Property", hasBindingMember));
				result.Add(new KDesignerActionMethodItem(actionList, delegate
				{ OverrideBoundProperty(control); }, (NoResString)"Override Bound Property", hasBindingMember));
				//result.Add(new KDesignerActionMethodItem(actionList, delegate { NavigateToValidationMember(control); }, "Go to Validation Member", true));
				//result.Add(new KDesignerActionMethodItem(actionList, delegate { OverrideValidationMember(control); }, "Override Validation Member", true));
			}
			return result.ToArray();
		}

		#endregion

		#region Implementation

		readonly Dictionary<Control, ControlBinding> bindings = new Dictionary<Control, ControlBinding>();
		object currentDataSource;
		string currentDataMember = "";

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!IsDesignMode)
				{
					try
					{
						SetDataBinding(null, "");
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						//don't halt disposal early, we're tearing down the form anyways
					}
				}
				foreach (Control control in bindings.Keys)
				{
					control.Disposed -= Control_Disposed;
				}
				bindings.Clear();
			}
			base.Dispose(disposing);
		}

		ControlBinding GetOrCreateControlBinding(Control control)
		{
			ControlBinding result = GetControlBinding(control);
			if (result == null)
			{
				result = new ControlBinding(control, this);
				control.Disposed += Control_Disposed;
				bindings[control] = result;
			}
			return result;
		}

		internal ControlBinding GetControlBinding(Control control)
		{
			ControlBinding result = null;
			bindings.TryGetValue(control, out result);
			return result;
		}

		DesignTimeBindingSource DesignTimeDataSource
		{
			get
			{
#pragma warning disable 252, 253
				if (designTimeDataSource == null || designTimeDataSource.DataSource != DataSourceType)
#pragma warning restore 252, 253
				{
					designTimeDataSource = null;
					if (DataSourceType != null && !(DataSourceType is TypeNameHolder))
					{
						designTimeDataSource = new DesignTimeBindingSource();
						if (DataSourceType != typeof(void))
						{
							designTimeDataSource.DataSource = DesignTimeBindingSource.GetTypeToUseAsDataSource(DataSourceType);
						}
					}
				}
				return designTimeDataSource;
			}
		}
		DesignTimeBindingSource designTimeDataSource;

		void DataSource_Posting(object sender, EventArgs e)
		{ EndCurrentEdit(); }

		void Control_Disposed(object sender, EventArgs e)
		{ bindings.Remove((Control)sender); }

		#endregion
	}
}
