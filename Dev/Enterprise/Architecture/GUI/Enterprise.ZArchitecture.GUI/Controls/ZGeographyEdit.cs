using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormDesignerAnalysis]
	[DefaultDataSourceBindingMember(null)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[Designer(typeof(Designer))]
	[ToolboxItem(true)]
	[DefaultBindingProperty("GeographyValue")]
	[FrontMostControlProvider(typeof(ZDateEditFrontMostControlProvider))]
	public class ZGeographyEdit : ZUserControl, IExtendedControl, IGridControl, IBindTo, IBackColorMutable
	{
		#region Bare

		[ToolboxItem(false)]
		public class Bare : ZGeographyEdit
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		#region Constructors

		static ZGeographyEdit()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new ZGeographyEditAdornmentLayout());
		}

		public ZGeographyEdit()
		{
			InitializeComponent();

			Extensions = NewExtensionCollection();

			GeographyTextBox.Validating += delegate(object sender, CancelEventArgs e) { OnValidating(e); };
			GeographyTextBox.TextChanged += GeographyTextBox_TextChanged;
			GeographyTextBox.ReadOnlyChanged += delegate { OnReadOnlyChanged(); };
			InitializeComponentWhenNotDesignMode();

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		protected virtual IControlExtensionCollection NewExtensionCollection()
		{
			return new DefaultControlExtensionCollection(this);
		}

		#endregion

		#region Properties

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ControlBindingsCollection DataBindings
		{
			// Hiding the base property prevents serialisation in the InitializeComponents
			get { return base.DataBindings; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text
		{
			get { return GeographyTextBox.Text; }
			set { GeographyTextBox.Text = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsFixedReadOnly { get; set; }

		#region ReadOnly

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnly
		{
			get { return GeographyTextBox.ReadOnly; }
			set
			{
				if (ReadOnly != value)
				{
					GeographyTextBox.ReadOnly = value;
				}
			}
		}

		public event EventHandler ReadOnlyChanged
		{
			add { GeographyTextBox.ReadOnlyChanged += value; }
			remove { GeographyTextBox.ReadOnlyChanged -= value; }
		}

		protected void OnReadOnlyChanged()
		{
			ReadOnlyForBindingProperty.OnReadOnlyChanged();
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

		protected virtual ControlReadOnlyPropertyHelper ReadOnlyForBindingProperty
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

		#endregion

		#region IsOnGrid

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override bool IsOnGrid
		{
			get { return base.IsOnGrid; }
			set
			{
				base.IsOnGrid = value;
				if (IsOnGrid)
				{
					GeographyTextBox.BorderStyle = BorderStyle.None;
					GeographyTextBox.Dock = DockStyle.None;
					GeographyTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
					ControlDpiScalingHelper.SetLeft(GeographyTextBox, 2, true);
					ControlDpiScalingHelper.SetTop(GeographyTextBox, 2, true);
					ControlDpiScalingHelper.SetWidth(GeographyTextBox, Width, false);
				}
			}
		}

		#endregion

		#region GeographyValue

		[DefaultValue("")]
		[Bindable(true)]
		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		public virtual ZGeography GeographyValue
		{
			get
			{
				if (pushValueRequired)
				{
					PushValue();
				}
				return geographyValue;
			}
			set
			{
				Text = GeographyToString(value, Text);
				lastParsedValue = Text;
				geographyValue = value;
			}
		}
		ZGeography geographyValue;

		public event EventHandler GeographyValueChanged;

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			pushValueRequired = true;
			if (GeographyValueChanged != null)
			{
				GeographyValueChanged(this, e);
			}
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZGeographyEdit>()
				.Property("GeographyValue", ZGeography.Empty)
				.Property("ReadOnly", true)
				.Property("IsVisibleForBinding", ZBool.True)
				.Property("ReadOnlyForBinding", true)
				.Property("ReadOnlyForBindingIsNull", true, false)
				.Result;
		}

		#endregion

		#endregion

		#region Designer Support

		internal sealed class Designer : GenericControlDesignerWithTextBoxSnapLine<ZGeographyEdit>
		{
			protected override TextBox GetTextBox(ZGeographyEdit userControl)
			{
				return userControl.GeographyTextBox;
			}
		}

		#endregion

		#region Custom Adornment Layout

		class ZGeographyEditAdornmentLayout : AdornmentLayout<ZGeographyEdit>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(ZGeographyEdit source)
			{
				yield return source.GeographyTextBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZGeographyEdit source)
			{
				yield return new IconLayout(source.GeographyTextBox, IconAlignment.Right);
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				DataBindings.Clear();
			}

			base.Dispose(disposing);
		}

		#endregion

		#region IBindTo Interface

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindTo
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		#endregion

		#region IDataBoundControl

		[SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "dataSource")]
		[SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "dataMember")]
		public override void SetDataBinding(object dataSource, string dataMember)
		{
			DataBoundControl.GetDefaultImplementation(this).SetDataBinding(dataSource, dataMember);
			this.dataSource = dataSource;
			this.dataMember = dataMember;
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

		#endregion

		#region IGridControl Members

		int IGridControl.SelectionStart
		{
			get { return GeographyTextBox.SelectionStart; }
			set { GeographyTextBox.SelectionStart = value; }
		}

		int IGridControl.SelectionLength
		{
			get { return GeographyTextBox.SelectionLength; }
			set { GeographyTextBox.SelectionLength = value; }
		}

		string IGridControl.Text
		{
			get { return GeographyTextBox.Text; }
			set { GeographyTextBox.Text = value; }
		}

		int IGridControl.ButtonWidth
		{
			get { return 0; }
		}

		event KeyEventHandler IGridControl.KeyDown
		{
			add { GeographyTextBox.KeyDown += value; }
			remove { GeographyTextBox.KeyDown -= value; }
		}

		int IGridControl.MaxLength
		{
			get { return 0; }
			set { }
		}

		void IGridControl.ActivateEditControl()
		{
			GeographyTextBox.Focus();
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

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.GeographyTextBox = new ZTextBox.Bare();
			this.SuspendLayout();
			// 
			// DateTextBox
			// 
			this.GeographyTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GeographyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GeographyTextBox.Name = "GeographyTextBox";
			this.GeographyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.GeographyTextBox.TabIndex = 0;
			this.GeographyTextBox.Text = "";
			// 
			// ZGeographyEdit
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GeographyTextBox);
			this.Name = "ZGeographyEdit";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.ResumeLayout(false);
		}

		protected void InitializeComponentWhenNotDesignMode()
		{
			GeographyTextBox.Enter += new EventHandler(GeographyTextBox_Enter);
			Extensions.SetDataBinding(null, "");
		}

		#endregion

		#region Implementation

		protected string lastParsedValue = String.Empty;
		protected bool pushValueRequired;

		ActiveControlColorChanger IBackColorMutable.ColorChanger
		{
			get { return GeographyTextBox != null ? GeographyTextBox.ColorChanger : null; }
		}

		[DefaultValue(false), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool EnableValidStateColor { get; set; } = false;

		public ZTextBox GeographyTextBox { get; private set; }

		protected bool IsValidating
		{
			get { return validating; }
		}
		bool validating;

		protected void ValidateBoundData()
		{
			validating = true;
			try
			{
				Validate();
				OnValidating(new CancelEventArgs());
			}
			finally
			{
				validating = false;
			}
		}

		#region Parse & Format

		protected virtual string GeographyToString(ZGeography value, string inputText)
		{
			return value.ToString();
		}

		public virtual ZGeography StringToGeography(string inputText)
		{
			ZGeography geography;

			if (!ZGeography.TryParse(inputText, out geography))
			{
				geography = ZGeography.Invalid;
			}

			return geography;
		}

		#endregion

		void GeographyTextBox_TextChanged(object sender, EventArgs e)
		{
			OnTextChanged(e);
		}

		void GeographyTextBox_Enter(object sender, EventArgs e)
		{
			GeographyTextBox.SelectAll();
		}

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

		protected internal virtual void PushValue()
		{
			if (!GeographyTextBox.Text.Equals(lastParsedValue, StringComparison.OrdinalIgnoreCase))
			{
				geographyValue = StringToGeography(GeographyTextBox.Text); //We don't want to set Text when we just read the value from Text, so access field directly.
				lastParsedValue = Text;
			}
		}
#if DEBUG
		internal bool ProcessCmdKeyExposed(ref Message msg, Keys keyData)
		{
			return ProcessCmdKey(ref msg, keyData);
		}
#endif

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if ((keyData == (Keys.Control | Keys.X) || keyData == (Keys.Control | Keys.C)) && !string.IsNullOrEmpty(Text))
			{
				SafeClipboard.SetText(Text);
				if (keyData == (Keys.Control | Keys.X))
				{
					Text = ZString.Empty;
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		#endregion
	}
}
