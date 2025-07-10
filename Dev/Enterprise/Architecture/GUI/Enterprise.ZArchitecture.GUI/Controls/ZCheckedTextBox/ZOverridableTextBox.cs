using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[ToolboxItem(true)]
	[SuppressFormsLocalizedTest]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class ZOverridableTextBox : ZCheckedTextBox,
		IOverridableTextBox
	{
		public ZOverridableTextBox()
			: base()
		{
			Mediator = new CheckedTextControlOverridingMediator(this);
		}

		internal CheckedTextControlOverridingMediator Mediator;

		#region Binding between the Control and Business Layer

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToForText { get; set; }

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToForPlaceholderText { get; set; }

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToForTextIsOverridden { get; set; }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			DataBindings.RemoveBinding(nameof(Text)); //we'll redo binding for Text
			DataBindings.RemoveBinding(nameof(TextOverride));
			DataBindings.RemoveBinding(nameof(PlaceholderText));
			DataBindings.RemoveBinding(nameof(TextIsOverridden));
			if (dataSource != null && !this.IsDesignMode())
			{
				//now the default member for binding is TextOverride, not Text
				textOverrideBinding = new KBinding(nameof(TextOverride), dataSource, dataMember, true);
				DataBindings.Add(textOverrideBinding);

				var propertyInfo = dataSource.GetType().GetProperty(dataMember + "Info");
				if (propertyInfo != null)
				{
					textOverrideBoundPropertyInfo = (ZPropertyInfo)propertyInfo.GetValue(dataSource);
				}

				if (!string.IsNullOrEmpty(BindToForText))
				{
					textBinding = new KBinding(nameof(Text), dataSource, BindToForText, true);
					DataBindings.Add(textBinding);
				}
				if (!string.IsNullOrEmpty(BindToForPlaceholderText))
				{
					placeholderTextBinding = new KBinding(nameof(PlaceholderText), dataSource, BindToForPlaceholderText, true);
					DataBindings.Add(placeholderTextBinding);
				}
				if (!string.IsNullOrEmpty(BindToForTextIsOverridden))
				{
					textIsOverriddenBinding = new KBinding(nameof(TextIsOverridden), dataSource, BindToForTextIsOverridden, true);
					DataBindings.Add(textIsOverriddenBinding);
				}
			}
		}
		KBinding textOverrideBinding;
		KBinding placeholderTextBinding;
		KBinding textIsOverriddenBinding;

		ZPropertyInfo textOverrideBoundPropertyInfo;

		#endregion

		#region Properties for Binding to Business Layer

		#region TextOverride

		public string TextOverride
		{
			get
			{
				return Mediator.TextOverride;
			}
			set
			{
				if (Mediator.TextOverride != value)
				{
					Mediator.TextOverride = value;
				}
				OnTextOverrideChanged(EventArgs.Empty);
			}
		}

		public event EventHandler TextOverrideChanged;

		protected virtual void OnTextOverrideChanged(EventArgs e)
		{
			if (textOverrideBinding != null)
			{
				textOverrideBinding.WriteValue();
			}
			if (TextOverrideChanged != null)
			{
				TextOverrideChanged(this, e);
			}
			UpdateTextBoxNotifications();
		}

		#endregion

		#region PlaceholderText

		public string PlaceholderText
		{
			get
			{
				return Mediator.PlaceholderText;
			}
			set
			{
				if (Mediator.PlaceholderText != value)
				{
					Mediator.PlaceholderText = value;
				}
				OnPlaceholderTextChanged(EventArgs.Empty);
			}
		}

		public event EventHandler PlaceholderTextChanged;

		protected virtual void OnPlaceholderTextChanged(EventArgs e)
		{
			if (placeholderTextBinding != null)
			{
				placeholderTextBinding.WriteValue();
			}
			if (PlaceholderTextChanged != null)
			{
				PlaceholderTextChanged(this, e);
			}
		}

		#endregion

		#region TextIsOverridden

		[DefaultValue(false)]
		public bool TextIsOverridden
		{
			get
			{
				return Mediator.TextIsOverridden;
			}
			set
			{
				if (Mediator.TextIsOverridden != value)
				{
					Mediator.TextIsOverridden = value;
				}
				OnTextIsOverriddenChanged(EventArgs.Empty);
			}
		}

		public event EventHandler TextIsOverriddenChanged;

		protected virtual void OnTextIsOverriddenChanged(EventArgs e)
		{
			if (textIsOverriddenBinding != null)
			{
				textIsOverriddenBinding.WriteValue();
			}
			if (TextIsOverriddenChanged != null)
			{
				TextIsOverriddenChanged(this, e);
			}
		}

		#endregion

		#endregion

		#region Additional Properties

		#region UpdateTextOnChecked

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(UpdateTextOnCheckedMode.ClearWhenPlaceholderTextIsTruncated)]
		public UpdateTextOnCheckedMode UpdateTextOnChecked
		{
			get
			{
				return Mediator.UpdateTextOnChecked;
			}
			set
			{
				if (Mediator.UpdateTextOnChecked != value)
				{
					Mediator.UpdateTextOnChecked = value;
				}
				OnUpdatesTextOnCheckedChanged(EventArgs.Empty);
			}
		}

		public event EventHandler UpdatesTextOnCheckedChanged;

		protected virtual void OnUpdatesTextOnCheckedChanged(EventArgs e)
		{
			if (UpdatesTextOnCheckedChanged != null)
			{
				UpdatesTextOnCheckedChanged(this, e);
			}
		}

		#endregion

		#endregion

		#region Notifications

		protected override void UpdateTextBoxNotifications()
		{
			if (textOverrideBoundPropertyInfo != null)
			{
				TextBox.GetExtension<NotificationExtension>().Notifications = new NotificationCollection(textOverrideBoundPropertyInfo.Notifications);
			}
		}

		#endregion

		#region PropertyDescriptors

		public new static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZOverridableTextBox>()
				.Property(nameof(Text), string.Empty)
				.Property(nameof(Checked), false)
				.Property(nameof(MaxLength), 0)
				.Property(nameof(TextOverride), string.Empty)
				.Property(nameof(PlaceholderText), string.Empty)
				.Property(nameof(TextIsOverridden), false)
				.Result;
		}

		#endregion
	}
}
