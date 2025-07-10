using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.Types;
using CargoWise.Windows.UI.Controls;
using CargoWise.Windows.UI.Controls.Internal;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Provides an empty control that can be used to create other controls.
	/// <summary><see cref="UserControl"/></summary>
	/// </summary>
	[SuppressFormDesignerAnalysis]
	[DesignTimeControlNameGenerator("uc")]
	// The designer foreach's TypeDescriptor.GetAttributes which sometimes queries the wrong designer.
	// This is fixed by applying the same attributes applied to System.Windows.Froms.UserControl.
	[Designer("System.Windows.Forms.Design.UserControlDocumentDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(IRootDesigner))]
	[Designer("System.Windows.Forms.Design.ControlDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class KUserControl : UserControl, IDataBoundControl, ICompositeControlBindingSourceProvider, IEndCurrentEdit, IKControl
	{
		public KUserControl()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			BindingSource = NewBindingSource();
			BindingSource.ContainerControl = this;
			GCCollectorTracker = GCTracker.Track(this.GetType().ToString(), this);
#if DEBUG
			if (instantiatedControlCounts != null)
			{
				instantiatedControlCounts.AddOrUpdate(GetType(), 1, (key, value) => ++value);
			}
#endif
		}

		readonly GCTracker GCCollectorTracker;
		#region DPI scaling overrides
#pragma warning disable CA1725
		protected override void OnLayout(LayoutEventArgs levent)
		{
			try
			{
				this.AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
				this.AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
				base.OnLayout(levent);
			}
			catch (ArgumentException ex)
			{
				if (ex.Message.Contains((NoResString)"Width and Height must be non-negative"))
				{
					throw new InvalidOperationException("Control name: " + this.Name + ", Control Type: " + this.GetType().Name + ", Height: " + this.Height + ", Width: " + this.Width, ex);
				}
			}
			catch (Win32Exception ex)
			{
				if (ex.Message.Contains((NoResString)"Error creating window handle."))
				{
					var controlPath = ControlDescription.GetControlPath(this);
					var messageBuilder = new ZStringBuilder();
					messageBuilder.AppendFormat((NoResString)"Control [name:'{0}' type:'{1}'] is already disposed.", this.Name, this.GetType().FullName).AppendLine();
					messageBuilder.AppendFormat((NoResString)"Control Path: {0}", string.IsNullOrEmpty(controlPath) ? (NoResString)"Empty" : controlPath).AppendLine();
					messageBuilder.AppendFormat((NoResString)"Please see WI00244466").AppendLine();
					ErrorReporter.ReportOnce("WI00244466_ERRORCREATINGHANDLE", messageBuilder.ToString()); // toRemove when WI00244466 is fixed
				}
				throw;
			}
		}
#pragma warning restore CA1725
		#endregion // DPI scaling overrides

		protected override bool ProcessDialogKey(Keys keyData)
		{
			return !(ActiveControl != null && ActiveControl.IsDisposed) && base.ProcessDialogKey(keyData);
		}

		[ThreadSafe]
		protected static bool HasReportedGraphicsDisplayFailure;

		public void SetBindingManagerCurrent(string path, object newCurrent)
		{ BindingContext.SetBindingManagerCurrent(this, path, newCurrent); }

		#region DataSource / DataMember

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected object DataSource
		{
			get { return DataSourceCore; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected internal virtual object DataSourceCore
		{ get { return BindingSource.DataSource; } }

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected string DataMember
		{
			get { return DataMemberCore; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected internal virtual string DataMemberCore
		{ get { return BindingSource.DataMember; } }

		#endregion

		#region SetDataSourceBinding

		/// <summary>
		/// Add a data binding that will be added to the data source when bound (and removed
		/// when unbound).
		/// </summary>
		protected internal void SetDataSourceBinding(string controlPropertyName, string dataMember)
		{
			SetDataSourceBinding(this, controlPropertyName, dataMember);
		}

		/// <summary>
		/// Add a data binding that will be added to the data source when bound (and removed
		/// when unbound).
		/// </summary>
		protected internal void SetDataSourceBinding(IBindableComponent bindableComponent, string controlPropertyName, string dataMember)
		{
			SetDataSourceBinding(bindableComponent, controlPropertyName, dataMember, false);
		}

		/// <summary>
		/// Add a data binding that will be added to the data source when bound (and removed
		/// when unbound).
		/// </summary>
		protected internal void SetDataSourceBinding(IBindableComponent bindableComponent, string controlPropertyName, string dataMember, bool formattingEnabled)
		{
			SetDataSourceBinding(bindableComponent, controlPropertyName, dataMember, formattingEnabled, false);
		}

		/// <summary>
		/// Add a data binding that will be added to the data source when bound (and removed
		/// when unbound).
		/// </summary>
		protected internal void SetAbsoluteDataSourceBinding(IBindableComponent bindableComponent, string controlPropertyName, string dataMember, bool formattingEnabled)
		{
			SetDataSourceBinding(bindableComponent, controlPropertyName, dataMember, formattingEnabled, true);
		}

		/// <summary>
		/// Add a data binding that will be added to the data source when bound (and removed
		/// when unbound).
		/// </summary>
		internal void SetDataSourceBinding(IBindableComponent bindableComponent, string controlPropertyName, string dataMember, bool formattingEnabled, bool isAbsoluteMember)
		{
			if (dataSourceBindings == null)
			{
				dataSourceBindings = new List<DataSourceBindingInfo>();
			}
			else
			{
				RemoveDataSourceBinding(bindableComponent, controlPropertyName);
			}
			if (!string.IsNullOrEmpty(dataMember))
			{
				DataSourceBindingInfo bindingInfo = new DataSourceBindingInfo { BindableComponent = bindableComponent, ControlPropertyName = controlPropertyName, DataMember = dataMember, FormattingEnabled = formattingEnabled, IsAbsoluteMember = isAbsoluteMember };
				dataSourceBindings.Add(bindingInfo);
				if (DataSourceCore != null)
				{
					ActivateDataSourceBinding(DataSourceCore, DataMemberCore, bindingInfo);
				}
			}
		}

		void RemoveDataSourceBinding(IBindableComponent bindableComponent, string controlPropertyName)
		{
			for (int i = 0; i < dataSourceBindings.Count; i++)
			{
				DataSourceBindingInfo bindingInfo = dataSourceBindings[i];
				if (bindingInfo.BindableComponent == bindableComponent && bindingInfo.ControlPropertyName == controlPropertyName)
				{
					Binding binding = bindableComponent.DataBindings[controlPropertyName];
					if (binding != null)
					{
						bindableComponent.DataBindings.Remove(bindableComponent.DataBindings[controlPropertyName]);
					}

					dataSourceBindings.RemoveAt(i);
					break;
				}
			}
		}

		protected BindingManagerBase GetBindingManager(string dataMember)
		{
			return DataSourceCore == null ? null : BindingContext[DataSourceCore, new KBindingMemberInfo(DataMemberCore, dataMember).BindingMember];
		}

		#endregion

		#region CurrentDataItem

		/// <summary>
		/// Get the currently bound data source.
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object CurrentDataItem
		{
			get
			{
				object result = currentDataItem;
				if (currentDataItem == null && BindingSource.DataSource != null && inBindingSourceSetDataBinding)
				{
					BindingManagerBase bm = BindingContext.EnsureListManager(BindingSource.DataSource, BindingSource.DataMember);
					result = bm != null && bm.Position != -1 ? bm.Current : null;
				}
				return result;
			}
			private set
			{
				ComponentModel.INullable nullableValue = value as ComponentModel.INullable;
				object newValue = (nullableValue != null && nullableValue.IsNull) ? null : value;
				if (currentDataItem != newValue)
				{
					OnCurrentDataItemChanging(EventArgs.Empty);
					currentDataItem = newValue;
					OnCurrentDataItemChanged(EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Called when the CurrentDataItem property is about to change.
		/// </summary>
		protected virtual void OnCurrentDataItemChanging(EventArgs e)
		{
		}

		/// <summary>
		/// Called when the CurrentDataItem property has changed.
		/// </summary>
		protected virtual void OnCurrentDataItemChanged(EventArgs e)
		{
		}

		#endregion

		#region IExtenderProviders

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		protected internal readonly KBindingSource BindingSource;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		protected readonly KNotificationProvider NotificationProvider = new KNotificationProvider();

		/// <summary>
		/// Create a new KBindingSource for the BindingSource field.
		/// </summary>
		protected virtual KBindingSource NewBindingSource()
		{
			return new KBindingSource();
		}

		#endregion

		#region Tabbing Support

		protected sealed override bool ProcessTabKey(bool forward)
		{
			return ProcessTabKeyCore(forward);
		}

		protected virtual bool ProcessTabKeyCore(bool forward)
		{
			return base.ProcessTabKey(forward);
		}

		protected sealed override void Select(bool directed, bool forward)
		{
			try
			{
				SelectCore(directed, forward);
			}
			catch (NullReferenceException)
			{
				//Caused on main form if you have tablet drivers installed only. Seems harmless to swallow, no observable misbehaviour.
			}
			finally
			{
				ContainerControlWithNoChildrenTabSkipper.AfterSelect(forward);
			}
		}

		protected virtual void SelectCore(bool directed, bool forward)
		{
			base.Select(directed, forward);
		}

		ContainerControlWithNoChildrenTabSkipper ContainerControlWithNoChildrenTabSkipper
		{
			get { return containerControlWithNoChildrenTabSkipper ?? (containerControlWithNoChildrenTabSkipper = new ContainerControlWithNoChildrenTabSkipper(this)); }
		}
		ContainerControlWithNoChildrenTabSkipper containerControlWithNoChildrenTabSkipper;

		#endregion

		#region Registering Design Time Services

		public override ISite Site
		{
			get { return base.Site; }
			set
			{
				base.Site = value;
				new DesignerServices(value).TryRegister();
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		protected internal readonly DesignerActionExtenderProvider DesignerActionExtenderProvider = new DesignerActionExtenderProvider();

		#endregion

		#region SetDataBinding

		public virtual void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != oldTopLevelDataSource ||
				dataMember != oldTopLevelDataMember)
			{
				RemoveDataSourceBindings();

				inBindingSourceSetDataBinding = true;
				try
				{
					BindingSource.SetDataBinding(dataSource, dataMember);
				}
				finally
				{
					inBindingSourceSetDataBinding = false;
				}

				AddDataSourceBindings(dataSource, dataMember);
				UpdateBindingManagerHooks();

				if (dataSource == null && oldFinalBindingManager != null)
				{
					PropertyManager propertyManager = oldFinalBindingManager as PropertyManager;
					if (propertyManager != null)
					{
						MethodInfo setDataSourceMethod = typeof(PropertyManager).GetMethod("SetDataSource", BindingFlags.NonPublic | BindingFlags.Instance);
						setDataSourceMethod.Invoke(propertyManager, new object[] { null });
					}
				}

				oldTopLevelDataSource = dataSource;
				oldTopLevelDataMember = dataMember;
				oldFinalBindingManager = oldTopLevelDataSource != null ? BindingContext[oldTopLevelDataSource, oldTopLevelDataMember] : null;
			}
		}
		bool inBindingSourceSetDataBinding;

		protected virtual Binding CreateBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled)
		{
			return new KBinding(propertyName, dataSource, dataMember, formattingEnabled);
		}

		void RemoveDataSourceBindings()
		{
			if (oldTopLevelDataSource != null && dataSourceBindings != null)
			{
				foreach (DataSourceBindingInfo bindingInfo in dataSourceBindings)
				{
					Binding binding = bindingInfo.BindableComponent.DataBindings[bindingInfo.ControlPropertyName];
					if (binding != null)
					{
						bindingInfo.BindableComponent.DataBindings.Remove(binding);
					}
				}
			}
		}

		void AddDataSourceBindings(object dataSource, string dataMember)
		{
			if (dataSource != null && dataSourceBindings != null)
			{
				foreach (DataSourceBindingInfo bindingInfo in dataSourceBindings)
				{
					ActivateDataSourceBinding(dataSource, dataMember, bindingInfo);
				}
			}
		}

		void ActivateDataSourceBinding(object dataSource, string dataMember, DataSourceBindingInfo bindingInfo)
		{
			string bindToPrefix = bindingInfo.IsAbsoluteMember ? "" : string.IsNullOrEmpty(dataMember) ? "" : dataMember;

			try
			{
				bindingInfo.BindableComponent.DataBindings.Add(CreateBinding(bindingInfo.ControlPropertyName, dataSource, new KBindingMemberInfo(bindToPrefix, bindingInfo.DataMember).BindingMember, bindingInfo.FormattingEnabled));
			}
			catch (ArgumentException ex)
			{
				// Dump all the elements of dataSourceBindings for future investigation when two bindings in the collection are bound to the same property.
				var msg = new StringBuilder();
				msg.AppendFormat((NoResString)"KUserControl.ActivateDataSourceBinding tries binding the dataMember {0} to the same property {1}.\r\n", dataMember ?? string.Empty, bindingInfo.ControlPropertyName ?? string.Empty);
				msg.AppendFormat((NoResString)"bindingInfo: DataMemember = {0}, FormattingEnabled = {1}, IsAbsoluteMember = {2}.\r\n", bindingInfo.DataMember ?? string.Empty, bindingInfo.FormattingEnabled, bindingInfo.IsAbsoluteMember);
				msg.AppendFormat((NoResString)"dataSourceBindings collections:\r\n");
				foreach (var elem in dataSourceBindings)
				{
					msg.AppendFormat((NoResString)"ControlPropertyName = {0}, DataMember = {1}, FormattingEnabled = {2}, IsAbsoluteMember = {3}.\r\n", elem.ControlPropertyName ?? string.Empty, elem.DataMember ?? string.Empty, elem.FormattingEnabled, elem.IsAbsoluteMember);
				}

				throw new ArgumentException(msg.ToString(), ex);
			}
		}

		#endregion

		#region ControlCollection

		protected override Control.ControlCollection CreateControlsInstance()
		{
			return new ControlCollection(this);
		}

		[SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped")]
		[SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
		[SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public new class ControlCollection : AutoDisposeControlCollection
		{
			public ControlCollection(Control owner)
				: base(owner)
			{
			}
		}

		#endregion

		#region OnScroll
#if !WINZOR

		[SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges", Justification = "We turn it off immediately. It's basically just a way to perform an action once on a delay.")]
		Timer ScrollRepaintTimer
		{
			get
			{
				if (scrollRepaintTimer == null)
				{
					scrollRepaintTimer = new Timer();
					scrollRepaintTimer.Interval = 50;
					scrollRepaintTimer.Tick += (o, e) => { scrollRepaintTimer.Stop(); Invalidate(true); };
				}
				return scrollRepaintTimer;
			}
		}
		Timer scrollRepaintTimer;

		protected override void OnScroll(ScrollEventArgs se)
		{
			ScrollRepaintTimer.Start();
			base.OnScroll(se);
		}

#endif
		#endregion

		#region Dispose

		protected bool IsDisposing { get; private set; }

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (DataSourceCore != null)
					{
						IsDisposing = true;
						try
						{
							SetDataBinding(null, "");
						}
						catch (SqlException)
						{
							// To avoid leak, the dispose process should be continued
						}
						catch (CommunicationException)
						{
							// To avoid leak, the dispose process should be continued
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							// just in case - as this was added recently
							ErrorReporter.ReportOnce("ExceptionInClearingOfDataBindingInDispose_" + GetType().Name, ex.Message, ex);
						}
					}
					BindingSource.Dispose();
					DesignerActionExtenderProvider.Dispose();
#if !WINZOR
					scrollRepaintTimer?.Dispose();
#endif
					DisposableLeakListener.Instance.UnRegisterDisposable(this);
				}
			}
			finally
			{
				GCCollectorTracker?.NotifyDisposed(disposing, ReportDisposeCalledToEventLog);
				try
				{
					base.Dispose(disposing);
				}
				catch (InvalidOperationException)
				{
					//continue disposing otherwise even if we get 'Value Dispose() cannot be called while doing CreateHandle().' somewhere
				}
			}
		}

		protected virtual bool ReportDisposeCalledToEventLog
		{
			get { return false; }
		}
		#endregion

		#region IDataBoundControl Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual Type DataSourceType
		{
			get { return BindingSource.DataSourceType; }
			set { BindingSource.DataSourceType = value; }
		}

		object IDataBoundControl.DataSource
		{ get { return this.DataSourceCore; } }

		string IDataBoundControl.DataMember
		{ get { return this.DataMemberCore; } }

		#endregion

		#region ICompositeControlBindingSourceProvider Members

		ICompositeControlBindingSource ICompositeControlBindingSourceProvider.BindingSource
		{
			get { return BindingSource; }
		}

		#endregion

		#region IEndCurrentEdit Members

		void IEndCurrentEdit.EndCurrentEdit()
		{ EndCurrentEdit(); }

		protected virtual void EndCurrentEdit()
		{
			foreach (Binding binding in DataBindings)
			{
				if (binding.BindingManagerBase != null)
				{
					var collectionChangingStackTrace = string.Empty;
					Binding removedBindingInfo = null;

					void OnBindingCollectionChanging(object obj, CollectionChangeEventArgs arg)
					{
						collectionChangingStackTrace = new StackTrace().ToString();
						removedBindingInfo = (Binding)arg.Element;
					}
					try
					{
						binding.BindingManagerBase.Bindings.CollectionChanging += OnBindingCollectionChanging;
						using (ControlExtensions.ForceInvokeCalls())
						{
							binding.BindingManagerBase.EndCurrentEdit();
						}
					}
					catch (ArgumentOutOfRangeException e) when (!e.IsCriticalException())
					{
						// This try-catch should be removed once the Issue 01156506 is solved (Please refer to WI00241043)
						var businessInfo = new StringBuilder(300);
						businessInfo.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"Data Source: {0}", binding.DataSource.ToString()).AppendLine();
						businessInfo.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"Control Name: {0}", Name).AppendLine();
						businessInfo.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"Control Type: {0}", GetType()).AppendLine();
						businessInfo.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"Information For Missing Binding:").AppendLine();
						businessInfo.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"Binding Control Name: {0}", removedBindingInfo?.Control?.Name).AppendLine();
						businessInfo.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"Binding Property Name: {0}", removedBindingInfo?.PropertyName).AppendLine();
						businessInfo.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"Binding Field: {0}", removedBindingInfo?.BindingMemberInfo.BindingField).AppendLine();
						businessInfo.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"Stack Trace:").AppendLine();

						ErrorReporter.ReportOnce("KUserControlEndCurrentEdit", businessInfo.ToString() + collectionChangingStackTrace, e);
					}
					finally
					{
						binding.BindingManagerBase.Bindings.CollectionChanging -= OnBindingCollectionChanging;
					}
				}
			}

			Hashtable controlsDone = new Hashtable();
			foreach (var control in BindingSource.BoundControls.ToImmutableArray())
			{
				if (control != null)
				{
					if (control != this)
					{
						KEndCurrentEdit.EndCurrentEdit(control);
					}
					controlsDone[control] = null;
				}
			}
			foreach (var control in Controls.OfType<Control>().ToImmutableArray())
			{
				if (control != null)
				{
					if (!controlsDone.Contains(control))
					{
						KEndCurrentEdit.EndCurrentEdit(control);
					}
				}
			}
		}

		#endregion

		#region IKControl implementation

		protected override void CreateHandle()
		{
			if (!IsDisposed)
			{
				base.CreateHandle();
				OnHandleFullyCreated(EventArgs.Empty);
			}
		}

		protected virtual void OnHandleFullyCreated(EventArgs e)
		{
			if (IsHandleCreated && HandleFullyCreated != null)
			{
				HandleFullyCreated(this, e);
			}
		}

		public event EventHandler HandleFullyCreated;

		#endregion

		#region Implementation

		List<DataSourceBindingInfo> dataSourceBindings;
		BindingManagerBase lastDataSourceBindingManager;
		object currentDataItem;
		object oldTopLevelDataSource;
		string oldTopLevelDataMember;
		BindingManagerBase oldFinalBindingManager;

		class DataSourceBindingInfo
		{
			public IBindableComponent BindableComponent;
			public string ControlPropertyName;
			public string DataMember;
			public bool FormattingEnabled;
			public bool IsAbsoluteMember;
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			UnhookCurrentBindingManager();
			base.OnBindingContextChanged(e);
			UpdateBindingManagerHooks();
		}

		void UnhookCurrentBindingManager()
		{
			if (lastDataSourceBindingManager != null)
			{
				lastDataSourceBindingManager.PositionChanged -= new EventHandler(BindingManager_PositionChanged);
				lastDataSourceBindingManager.CurrentChanged -= new EventHandler(BindingManager_PositionChanged);
			}
			lastDataSourceBindingManager = null;
		}

		void UpdateBindingManagerHooks()
		{
			UnhookCurrentBindingManager();
			if (BindingSource.DataSource != null)
			{
				lastDataSourceBindingManager = BindingContext.EnsureListManager(
					BindingSource.DataSource, BindingSource.DataMember);
			}
			if (lastDataSourceBindingManager != null)
			{
				// CurrentChanged is required because of Parent.Child.Parent where the child current changes
				lastDataSourceBindingManager.PositionChanged += new EventHandler(BindingManager_PositionChanged);
				lastDataSourceBindingManager.CurrentChanged += new EventHandler(BindingManager_PositionChanged);
			}
			BindingManager_PositionChanged(lastDataSourceBindingManager, EventArgs.Empty);
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule")]
		void BindingManager_PositionChanged(object sender, EventArgs e)
		{
			try
			{
				object newCurrentDataItem = null;
				BindingManagerBase bm = (BindingManagerBase)sender;
				if (bm != null && BindingSource.DataSource != null)
				{
					newCurrentDataItem = (bm.Position == -1 || bm.Position >= bm.Count) ? null : bm.Current;
				}

				ComponentModel.INullable nullable = newCurrentDataItem as ComponentModel.INullable;
				if (nullable != null && nullable.IsNull)
				{
					newCurrentDataItem = null;
				}
				CurrentDataItem = newCurrentDataItem;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce(ex.Message, ex);
			}
		}

		#endregion

		#region For Testing
#if DEBUG
		public static IDisposable TrackInstantiatedControls_ForTest()
		{
			instantiatedControlCounts = new ConcurrentDictionary<Type, int>();

			return new DisposableAction(() => instantiatedControlCounts = null);
		}

		public static IReadOnlyDictionary<Type, int> InstantiatedControls_ForTest => instantiatedControlCounts?.ToImmutableDictionary();

		internal static ConcurrentDictionary<Type, int> instantiatedControlCounts;
#endif
		#endregion
	}
}
