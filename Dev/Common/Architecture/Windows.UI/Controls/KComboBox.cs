using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="KComboBox"/></summary>
	[DesignTimeControlNameGenerator("cb")]
	[DefaultBindingProperty("BoundValue")]

	[ToolboxItem(false)] // not used in production as yet
	[DesignTimeVisible(false)]
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KComboBox : ComboBox, IDataBoundControl, IBindingMemberForCompileTimeCheckProvider
	{
		#region BoundValue

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength")]
		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		[BindingMetaDataProperty(MetaDataTypes.ListValueMember, "ValueMember")]
		[BindingMetaDataProperty(MetaDataTypes.ListDisplayMember, "DisplayMember")]
		[BindingMetaDataProperty(MetaDataTypes.Null, "EmptyValue")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object BoundValue
		{
			get
			{
				object result = null;
				if (ValueMember == DisplayMember)
				{
					if (DataValueProperty != null && DataValueProperty.Converter != null)
					{
						if (DataValueProperty.Converter.CanConvertFrom(typeof(string)))
						{
							result = DataValueProperty.Converter.ConvertFrom(Text);
						}
					}
					else
					{
						result = Text;
					}
				}
				if (result == null)
				{
					result = cachedBoundValue;
				}
				if (result == null)
				{
					if (SelectedIndex == -1)
					{
						int newSelectedIndex = FindStringExact(Text);
						if (newSelectedIndex != -1)
						{
							SelectedIndex = newSelectedIndex;
						}
					}
					result = SelectedItemOrValue;
					cachedBoundValue = result;
				}
				if (result == null)
				{
					result = EmptyValue;
				}
				return result;
			}
			set
			{
				object oldCachedBoundValue = cachedBoundValue;
				cachedBoundValue = value;

				SelectedItemOrValue = value ?? DBNull.Value;
				object selectedItemOrValue = this.SelectedItemOrValue;
				if (selectedItemOrValue == null || !object.Equals(value, selectedItemOrValue))
				{
					SelectedIndex = -1;
				}

				ComponentModel.INullable nullableValue = value as ComponentModel.INullable;
				bool isValueEmpty = value == null || value is DBNull || (nullableValue != null && nullableValue.IsNull);
				if (SelectedIndex == -1)
				{
					if (isValueEmpty)
					{
						Text = "";
					}
					else if (!string.IsNullOrEmpty(ValueMember) && ValueMember == DisplayMember)
					{
						if (DataValueProperty != null && DataValueProperty.Converter != null)
						{
							Text = (string)DataValueProperty.Converter.ConvertTo(value, typeof(string));
						}
						else
						{
							Text = value.ToString();
						}
					}
					else if (string.IsNullOrEmpty(ValueMember) && string.IsNullOrEmpty(DisplayMember))
					{
						Text = value.ToString();
					}
					else if (string.IsNullOrEmpty(ValueMember) && !string.IsNullOrEmpty(DisplayMember))
					{
						PropertyDescriptor displayProperty = TypeDescriptor.GetProperties(value)[DisplayMember];
						if (displayProperty != null)
						{
							Text = (string)displayProperty.GetValue(value);
						}
					}
				}

				if (oldCachedBoundValue != value)
				{
					OnBoundValueChanged(EventArgs.Empty);
				}
			}
		}
		object cachedBoundValue;

		object SelectedItemOrValue
		{
			get { return string.IsNullOrEmpty(ValueMember) ? SelectedItem : SelectedValue; }
			set
			{
				if (string.IsNullOrEmpty(ValueMember))
				{
					SelectedItem = value;
				}
				else
				{
					SelectedValue = value;
				}
			}
		}

		public event EventHandler BoundValueChanged;
		protected virtual void OnBoundValueChanged(EventArgs e)
		{
			if (BoundValueChanged != null)
			{
				BoundValueChanged(this, e);
			}
		}

		protected override void OnSelectionChangeCommitted(EventArgs e)
		{
			base.OnSelectionChangeCommitted(e);
			this.cachedBoundValue = null;
			OnBoundValueChanged(EventArgs.Empty);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			this.cachedBoundValue = null;
			OnBoundValueChanged(EventArgs.Empty);
		}

		#endregion

		#region Bound Meta-data

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new object SelectedValue
		{
			get { return base.SelectedValue; }
			set { base.SelectedValue = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new object SelectedItem
		{
			get { return base.SelectedItem; }
			set { base.SelectedItem = value; }
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[AttributeProvider(typeof(IListSource))]
		public new object DataSource
		{
			get { return base.DataSource; }
			set
			{
				object currentBoundValue = BoundValue;
				object source = (value == DBNull.Value) ? null : value;
				if (source == null || !source.Equals(DataSource))
				{
					string oldDisplayMember = DisplayMember;
					base.DataSource = source;
					if (source == null && DataBindings[nameof(DataSource)] != null)
					{
						DisplayMember = oldDisplayMember;
					}
				}
				try
				{
					BoundValue = currentBoundValue;
				}
				catch (Exception e) when (!e.IsCriticalException()) { }
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new string ValueMember
		{
			get { return base.ValueMember; }
			set { base.ValueMember = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new string DisplayMember
		{
			get { return base.DisplayMember; }
			set { base.DisplayMember = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object EmptyValue { get; set; }

		[DefaultValue(0)]
		public new int MaxLength
		{
			get { return base.MaxLength; }
			set { base.MaxLength = value; }
		}

		#endregion

		#region Binding ReadOnly

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyForBinding
		{
			get { return ReadOnlyForBindingProperty.ReadOnlyForBinding; }
			set { ReadOnlyForBindingProperty.ReadOnlyForBinding = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		public event EventHandler ReadOnlyForBindingChanged
		{
			add { ReadOnlyForBindingProperty.ReadOnlyForBindingChanged += value; }
			remove { ReadOnlyForBindingProperty.ReadOnlyForBindingChanged -= value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyForBindingIsNull
		{
			get { return false; }
			set
			{
				if (value)
				{
					ReadOnlyForBinding = true;
				}
			}
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			ReadOnlyForBindingProperty.OnReadOnlyChanged();
		}

		ControlReadOnlyPropertyHelper ReadOnlyForBindingProperty
		{
			get
			{
				if (readOnlyForBindingProperty == null)
				{
					readOnlyForBindingProperty = new ControlReadOnlyPropertyHelper(
						this,
						delegate
						{ return !Enabled; },
						delegate(bool value)
						{ Enabled = !value; });
				}
				return readOnlyForBindingProperty;
			}
		}
		ControlReadOnlyPropertyHelper readOnlyForBindingProperty;

		#endregion

		#region DataValueProperty / BindingManager

		PropertyDescriptor DataValueProperty
		{
			get
			{
				BindingManagerBase bindingManager = this.BindingManager;
				return (bindingManager == null) ? null : bindingManager.GetItemProperties()[new KBindingMemberInfo(DataBoundControlImpl.DataMember).BindingField];
			}
		}

		BindingManagerBase BindingManager
		{
			get
			{
				object bindingDataSource = DataBoundControlImpl.DataSource;
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(DataBoundControlImpl.DataMember);
				return (bindingDataSource == null) ? null : BindingContext[bindingDataSource, bindingMemberInfo.BindingPath];
			}
		}

		#endregion

		#region Binding the List Data Source

		enum KDataSourceBindTime
		{
			FirstBind,
			Focus,
			DropDownList,
		}

		KDataSourceBindTime DataSourceBindTime
		{
			get
			{
				KDataSourceBindTime result;
				if (!string.IsNullOrEmpty(ValueMember) && ValueMember != DisplayMember)
				{
					result = KDataSourceBindTime.FirstBind;
				}
				else if (AutoCompleteMode != AutoCompleteMode.None)
				{
					result = KDataSourceBindTime.Focus;
				}
				else
				{
					result = KDataSourceBindTime.DropDownList;
				}
				return result;
			}
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			if (DataSourceBindTime == KDataSourceBindTime.Focus)
			{
				int oldSelectionStart = SelectionStart;
				int oldSelectionLength = SelectionLength;
				EnsureListDataSourceBinding();
				try
				{
					SelectionLength = oldSelectionLength;
					SelectionStart = oldSelectionStart;
				}
				catch (ArgumentOutOfRangeException)
				{
					//The getter of SelectionLength uses windows messaging and the setter doesn't, so maybe there's some kind of WinForms glitch that can cause the getter to return garbage.
				}
			}
		}

		protected override void OnDropDown(EventArgs e)
		{
			if (DataSourceBindTime == KDataSourceBindTime.DropDownList)
			{
				EnsureListDataSourceBinding();
			}
			base.OnDropDown(e);
		}

		void EnsureListDataSourceBinding()
		{
			PropertyDescriptor dataValueProperty = this.DataValueProperty;
			if (DataBindings[nameof(DataSource)] == null && dataValueProperty != null && DataBoundControlImpl.DataSource != null)
			{
				PropertyDescriptor listDataSourceProperty = MetaData.GetMetaDataProperty(dataValueProperty.ComponentType, dataValueProperty, MetaDataTypes.ListDataSource);
				string listDataSourceMember = new KBindingMemberInfo(new KBindingMemberInfo(DataBoundControlImpl.DataMember).BindingPath, listDataSourceProperty.Name).BindingMember;
				DataBindings.Add(new KBinding(nameof(DataSource), DataBoundControlImpl.DataSource, listDataSourceMember, true, DataSourceUpdateMode.Never));
			}
		}

		void StopListDataSourceBinding()
		{
			Binding dataSourceBinding = DataBindings[nameof(DataSource)];
			if (dataSourceBinding != null)
			{
				DataBindings.Remove(dataSourceBinding);
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!IsDisposed)
				{
					try
					{
						DroppedDown = false;
					}
					catch (Win32Exception ex) when (ex.Message.Contains((NoResString)"Error creating window handle."))
					{
						// To avoid leak, the dispose process should be continued
					}
				}
				base.DataSource = null;
			}
			base.Dispose(disposing);
		}

		#endregion

		#region IDataBoundControl Members

		IDataBoundControl DataBoundControlImpl
		{
			get
			{
				BindingContext bindingContext = this.BindingContext;
				if (lastBindingContextForDataBoundControlImpl == null || bindingContext != lastBindingContextForDataBoundControlImpl)
				{
					dataBoundControlImpl = DataBoundControl.GetDefaultImplementation(this);
					lastBindingContextForDataBoundControlImpl = bindingContext;
				}
				return dataBoundControlImpl;
			}
		}
		IDataBoundControl dataBoundControlImpl;
		BindingContext lastBindingContextForDataBoundControlImpl;

		public virtual void SetDataBinding(object dataSource, string dataMember)
		{
			DataBoundControlImpl.SetDataBinding(dataSource, dataMember);
			if (dataSource == null)
			{
				StopListDataSourceBinding();
			}
			else if (DataSourceBindTime == KDataSourceBindTime.FirstBind)
			{
				EnsureListDataSourceBinding();
			}
		}

		Type IDataBoundControl.DataSourceType
		{ get { return DataBoundControlImpl.DataSourceType; } }

		object IDataBoundControl.DataSource
		{ get { return DataBoundControlImpl.DataSource; } }

		string IDataBoundControl.DataMember
		{ get { return DataBoundControlImpl.DataMember; } }

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider Members

		CompileTimeCheckBindingMemberCollection IBindingMemberForCompileTimeCheckProvider.GetBindingMembersForCompileTimeCheck(
				Type dataSourceType, string dataMember)
		{
			return new ListValueMemberCompileTimeCheckProvider().GetBindingMembersForCompileTimeCheck(dataSourceType, dataMember);
		}

		#endregion

		#region Implementation

#if !WINZOR

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		protected override void OnMouseHover(EventArgs e)
		{
			var selectedItemText = GetItemText(SelectedItem);
			if (ToolTipService.GetToolTip(this) != selectedItemText)
			{
				ToolTipService.SetToolTip(this, selectedItemText);
			}
			base.OnMouseHover(e);
		}

#endif

		public void UpdateDropDownWidth()
		{
			float result = Width;
			var textPaddingSize = ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
			using (var g = CreateGraphics())
			{
				foreach (var item in Items)
				{
					result = Math.Max(g.MeasureString(item.ToString(), Font).Width + textPaddingSize, result);
				}
			}

			DropDownWidth = (int)result;
		}

		#endregion
	}
}
