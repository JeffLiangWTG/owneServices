using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;

namespace Enterprise.Services.OperationalActions.GUI
{
	[SuppressFormDesignerAnalysis]
	[DefaultDataSourceBindingMember(null)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[ToolboxItem(true)]
	[DefaultBindingProperty("FieldName")]
	internal sealed partial class FieldFindBoxGridControl : ZUserControl, IExtendedControl, IGridControl, IFieldFindBox
	{
		static FieldFindBoxGridControl()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new FieldFindBoxAdornmentLayout());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<FieldFindBoxGridControl>()
				.Property("FieldName", ZDateTime.Empty)
				.Property("ReadOnly", true)
				.Property("ReadOnlyForBinding", true)
				.Property("ReadOnlyForBindingIsNull", true, false)
				.Result;
		}

		public FieldFindBoxGridControl()
		{
			InitializeComponent();

			Extensions = new ControlExtensionCollection(this);
			Extensions.SetDataBinding(null, "");

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override bool IsOnGrid
		{
			get { return base.IsOnGrid; }
			set
			{
				base.IsOnGrid = value;
				if (IsOnGrid)
				{
					ControlDpiScalingHelper.SetLeft(ref fieldNameTextBox, 2, true);
					ControlDpiScalingHelper.SetTop(ref fieldNameTextBox, 2, true);
					ControlDpiScalingHelper.SetWidth(ref fieldNameTextBox, Width - lookupButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(3), false);
				}
			}
		}

		public override string Text
		{
			get { return fieldNameTextBox.Text; }
			set { fieldNameTextBox.Text = value; }
		}

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get { return fieldNameTextBox.ReadOnly; }
			set
			{
				if (ReadOnly != value)
				{
					fieldNameTextBox.ReadOnly = value;
					lookupButton.Visible = !value;
					OnReadOnlyChanged();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AllowReadOnly { get; set; }

		public event EventHandler ReadOnlyChanged;

		public Type RootType { get; set; }

		public string WorkflowType { get; set; }

		[DefaultValue("")]
		[Bindable(true)]
		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		public ZString FieldName
		{
			get { return fieldNameTextBox.Text; }
			set { fieldNameTextBox.Text = value; }
		}

		public event EventHandler FieldNameChanged
		{
			add { fieldNameTextBox.TextChanged += value; }
			remove { fieldNameTextBox.TextChanged -= value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyForBindingIsNull
		{
			get { return false; }
			set { if (value)
				{
					ReadOnlyForBinding = true;
				}
			}
		}

		#region IGridControl Members

		int IGridControl.SelectionStart
		{
			get { return fieldNameTextBox.SelectionStart; }
			set { fieldNameTextBox.SelectionStart = value; }
		}

		int IGridControl.SelectionLength
		{
			get { return fieldNameTextBox.SelectionLength; }
			set { fieldNameTextBox.SelectionLength = value; }
		}

		string IGridControl.Text
		{
			get { return fieldNameTextBox.Text; }
			set { fieldNameTextBox.Text = value; }
		}

		int IGridControl.ButtonWidth
		{
			get { return lookupButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(2); }
		}

		event KeyEventHandler IGridControl.KeyDown
		{
			add { fieldNameTextBox.KeyDown += value; }
			remove { fieldNameTextBox.KeyDown -= value; }
		}

		int IGridControl.MaxLength
		{
			get { return 0; }
			set { }
		}

		void IGridControl.ActivateEditControl()
		{
			fieldNameTextBox.Focus();
		}

		bool IGridControl.ShouldHandleKey(Keys keyData)
		{
			return false;
		}

		bool IGridControl.ShownForReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region IFieldFindBox Members

		string IFieldFindBox.Value
		{
			get { return fieldNameTextBox.Text; }
			set { fieldNameTextBox.Text = value; }
		}

		#endregion

		#region AdornmentLayout

		class FieldFindBoxAdornmentLayout : AdornmentLayout<FieldFindBoxGridControl>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(FieldFindBoxGridControl source)
			{
				yield return source.fieldNameTextBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(FieldFindBoxGridControl source)
			{
				yield return
					source.lookupButton.Visible
						? new IconLayout(source.lookupButton, IconAlignment.Center)
						: new IconLayout(source.fieldNameTextBox, IconAlignment.Right);
			}
		}

		#endregion

		#region Implementation

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			DataBoundControl.GetDefaultImplementation(this).SetDataBinding(dataSource, dataMember);
			this.dataSource = dataSource;
			this.dataMember = dataMember;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
				DataBindings.Clear();
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}

			base.Dispose(disposing);
		}

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

		protected override object DataSourceCore
		{
			get { return dataSource; }
		}
		object dataSource;

		protected override string DataMemberCore
		{
			get { return dataMember; }
		}
		string dataMember;

		void ShowPopup()
		{
			if (Parent != null && RootType != null)
			{
				fieldNameTextBox.Focus();
				FieldFindBoxPopup.Show(this, FindForm(), WorkflowType);
			}
		}

		void OnReadOnlyChanged()
		{
			ReadOnlyForBindingProperty.OnReadOnlyChanged();
			if (ReadOnlyChanged != null)
			{
				ReadOnlyChanged(this, EventArgs.Empty);
			}
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			ControlDpiScalingHelper.SetTop(ref lookupButton, 0, true);
			ControlDpiScalingHelper.SetHeight(ref lookupButton, Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
			ControlDpiScalingHelper.SetWidth(ref lookupButton, 18, true);

			ControlDpiScalingHelper.SetLeft(ref fieldNameTextBox, 1, true);
			ControlDpiScalingHelper.SetTop(ref fieldNameTextBox, 1, true);
			ControlDpiScalingHelper.SetWidth(ref fieldNameTextBox, Width - lookupButton.Width - fieldNameTextBox.Left, false);
		}

		ControlReadOnlyPropertyHelper ReadOnlyForBindingProperty
		{
			get
			{
				if (readOnlyForBindingProperty == null)
				{
					readOnlyForBindingProperty = new ControlReadOnlyPropertyHelper(
						this,
						delegate { return ReadOnly; },
						delegate(bool value) { ReadOnly = value; });
				}
				return readOnlyForBindingProperty;
			}
		}
		ControlReadOnlyPropertyHelper readOnlyForBindingProperty;

		void CheckForSpecialKeys(object sender, KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.F4:
					if (!ReadOnly)
					{
						ShowPopup();
					}
					e.Handled = true;
					break;

				case Keys.F2:
					fieldNameTextBox.SelectionLength = 0;
					fieldNameTextBox.SelectionStart = fieldNameTextBox.Text.Length;
					e.Handled = true;
					break;
			}
		}

		void FieldNameTextBox_Enter(object sender, EventArgs e)
		{
			fieldNameTextBox.SelectAll();
		}

		void FieldNameTextBox_Validating(object sender, CancelEventArgs e)
		{
			OnValidating(e);
		}

		void FieldNameTextBox_TextChanged(object sender, EventArgs e)
		{
			OnTextChanged(e);
		}

		void LookupButton_Click(object sender, EventArgs e)
		{
			ShowPopup();
		}

		#endregion
	}
}
