using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[ToolboxItem(true)]
	[SuppressFormsLocalizedTest] //otherwise tests will require CaptionRenderingEnabled to be set to true
	public class ZCheckedTextBox : ZUserControl,
		IExtendedControl,
		IBindTo
	{
		#region Constructors

		public ZCheckedTextBox()
			: base()
		{
			InitializeComponent();
			Extensions = NewExtensionCollection();
		}

		protected virtual IControlExtensionCollection NewExtensionCollection()
		{
			return new DefaultControlExtensionCollection(this);
		}

		#endregion

		#region InitializeComponent

		void InitializeComponent()
		{
			SuspendLayout();

			CheckBox = new ZCheckBox();
			TextBox = new ZTextBox();

			CheckBox.Name = "CheckBox";
			CheckBox.TabIndex = 0;
			CheckBox.TabStop = true;
			CheckBox.CheckedChanged += CheckBox_CheckedChanged;

			TextBox.Name = "TextBox";
			TextBox.TabIndex = 1;
			TextBox.TabStop = true;
			TextBox.TextChanged += TextBox_TextChanged;

			SetupPlacement();

			Controls.Add(CheckBox);
			Controls.Add(TextBox);

			this.CaptionRenderingEnabled = false; //we do not want to provide caption resource strings for the nested TextBox and CheckBox

			Checked = false;

			ResumeLayout(false);
		}

		#endregion

		#region Placement

		void SetupPlacement()
		{
			CheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			// Check box expands vertically, but we want both check box and text box to have the same height
			ControlDpiScalingHelper.SetHeight(CheckBox, TextBox.Height, false);
			ControlDpiScalingHelper.SetWidth(CheckBox, CheckBox.Height, false);
			CheckBox.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			CheckBox.CheckAlign = ZContentAlignment.Left;
			TextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
			Gap = ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
		}

		void ResizeTextBox()
		{
			TextBox.Location = ControlDpiScalingHelper.NewScaledPoint(CheckBox.Width + Gap, 0, false);
			ControlDpiScalingHelper.SetWidth(TextBox, Width - CheckBox.Width - Gap, false);
		}

		[DefaultValue(0)]
		public int Gap
		{
			get
			{
				return gap;
			}
			set
			{
				gap = value;
				ResizeTextBox();
			}
		}
		int gap;

		#endregion

		#region MaxLength

		[DefaultValue(0)]
		public int MaxLength
		{
			get
			{
				return TextBox.MaxLength;
			}
			set
			{
				TextBox.MaxLength = value;
				if (MaxLength != 0 && Text.Length > MaxLength)
				{
					Text = Text.Substring(0, MaxLength);
				}
				OnMaxLengthChanged(EventArgs.Empty);
			}
		}

		public event EventHandler MaxLengthChanged;

		protected virtual void OnMaxLengthChanged(EventArgs e)
		{
			if (MaxLengthChanged != null)
			{
				MaxLengthChanged(this, e);
			}
		}

		#endregion

		#region CharacterCasing

		[DefaultValue(CharacterCasing.Upper)]
		public CharacterCasing CharacterCasing
		{
			get
			{
				return TextBox.CharacterCasing;
			}
			set
			{
				TextBox.CharacterCasing = value;
			}
		}

		#endregion

		#region Text and Checked

		public override string Text
		{
			get
			{
				return TextBox.Text;
			}
			set
			{
				TextBox.Text = MaxLength == 0 || value.Length <= MaxLength ? value : (new ZString(value).SubstringSafe(0, MaxLength)).ToString();
				OnTextChanged(EventArgs.Empty);
			}
		}

		public new event EventHandler TextChanged;

		protected override void OnTextChanged(EventArgs e)
		{
			if (textBinding != null)
			{
				textBinding.WriteValue();
			}
			if (TextChanged != null)
			{
				TextChanged(this, e);
			}
			UpdateTextBoxNotifications();
		}

		void TextBox_TextChanged(object sender, EventArgs e)
		{
			OnTextChanged(e);
		}

		[DefaultValue(false)]
		public bool Checked
		{
			get
			{
				return CheckBox.Checked;
			}
			set
			{
				CheckBox.Checked = value;
				OnCheckedChanged(EventArgs.Empty);
			}
		}

		public event EventHandler CheckedChanged;

		protected virtual void OnCheckedChanged(EventArgs e)
		{
			TextBox.Enabled = Checked;
			if (checkedBinding != null)
			{
				checkedBinding.WriteValue();
			}
			if (CheckedChanged != null)
			{
				CheckedChanged(this, e);
			}
		}

		void CheckBox_CheckedChanged(object sender, EventArgs e)
		{
			OnCheckedChanged(e);
		}

		#endregion

		#region IExtendedControl Members

		public Control Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region IBindTo Members

		[DefaultValue("")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
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

		#region BindToForChecked

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindToForChecked
		{
			get { return bindToForChecked; }
			set { bindToForChecked = value; }
		}
		string bindToForChecked = "";

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			DataBindings.RemoveBinding(nameof(Text));
			DataBindings.RemoveBinding(nameof(Checked));
			if (dataSource != null && !this.IsDesignMode())
			{
				textBinding = new KBinding(nameof(Text), dataSource, dataMember, true);
				DataBindings.Add(textBinding);

				var propertyInfo = dataSource.GetType().GetProperty(dataMember + "Info");
				if (propertyInfo != null)
				{
					textBoundPropertyInfo = (ZPropertyInfo)propertyInfo.GetValue(dataSource);
				}

				if (!string.IsNullOrEmpty(BindToForChecked))
				{
					checkedBinding = new KBinding(nameof(Checked), dataSource, BindToForChecked, true);
					DataBindings.Add(checkedBinding);
				}
			}
		}
		protected KBinding textBinding;
		protected KBinding checkedBinding;

		ZPropertyInfo textBoundPropertyInfo;

		public override Type DataSourceType
		{
			get { return typeof(ZString); }
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZCheckedTextBox>()
				.Property(nameof(Text), string.Empty)
				.Property(nameof(Checked), false)
				.Result;
		}

		#endregion

		#region Implementation

		public ZCheckBox CheckBox { get; protected set; }
		public ZTextBox TextBox { get; protected set; }

		protected virtual void UpdateTextBoxNotifications()
		{
			if (textBoundPropertyInfo != null)
			{
				TextBox.GetExtension<NotificationExtension>().Notifications = new NotificationCollection(textBoundPropertyInfo.Notifications);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

