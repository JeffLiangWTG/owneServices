using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	class TextBoxWrapper : NonPersistentBusinessObject
	{
		public TextBoxWrapper()
		{
		}

		[MaxLength("ValueMaxLength")]
		public ZString Value
		{
			get { return _value; }
			set { SetNonPersistentPropertyValue(ValueInfo, ref _value, value); }
		}
		ZString _value;

		public bool Value_ReadOnly { get; set; }

		public ZPropertyInfo ValueInfo => GetZPropertyInfo(nameof(Value));
	}

	public class TextBoxRegistryItemEditor : RegistryItemEditor
	{
		public TextBoxRegistryItemEditor(IRegistryDataType dataType, TextRegistryEditorInfo info)
			: base(dataType)
		{
			this.Info = info;
		}

		public TextBoxRegistryItemEditor(IRegistryItem item, IRegistryDataType dataType, TextRegistryEditorInfo info)
		: base(dataType)
		{
			this.Info = info;
			this.item = item;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			var wrapper = new TextBoxWrapper();

			ZUserControl control = null;
			try
			{
				control = GetInnerControl();
				control.SetDataBinding(wrapper, "");
				control.Dock = DockStyle.Fill;

				var textbox = control.Controls[0];
				textbox.SetBindingMember("Value");
				textbox.KeyPress += (o, e) => wrapper.HasChanges = true;

				AddButtonClickEvent(control);

				if (DataType is StringRegistryDataType stringDataType)
				{
					var maxLengthProperty = BindableComponentMetaDataPropertyLocator.GetDefaultMetaDataProperty(textbox.GetType(), MetaDataTypes.MaxLength);
					if (maxLengthProperty != null)
					{
						maxLengthProperty.SetValue(textbox, stringDataType.MaxLength);
					}

					if (stringDataType.ReadOnly)
					{
						wrapper.Value_ReadOnly = true;
					}
				}

				return control;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				control?.Dispose();
				throw;
			}
		}

		ZUserControl GetInnerControl()
		{
			if ((GlbStaff.CurrentUser.GS_IsDeveloper || (GlbStaff.CurrentUser.GS_IsController && item != null && item.HasOption(RegistryOptions.IsPasswordVisibleForControllerUser))) && Info.EditorType.HasFlag(TextEditorType.Password))
			{
				return new PasswordControl();
			}
			else if (Info.EditorType == TextEditorType.AWBCustomisableText)
			{
				return new AWBCustomTextControl();
			}
			else if (Info.EditorType == TextEditorType.AWBCustomisableMultilineText)
			{
				return new AWBCustomTextControl { Multiline = true };
			}
			else
			{
				var textEdit = new ZTextBox() { CharacterCasing = CharacterCasing, Dock = DockStyle.Fill };
				if (Info.EditorType == TextEditorType.Memo ||
					Info.EditorType == TextEditorType.HTML)
				{
					textEdit.Multiline = true;
					textEdit.ScrollBars = ScrollBars.Vertical;
					textEdit.AcceptsReturn = true;
				}
				else if (Info.EditorType.HasFlag(TextEditorType.Password))
				{
					textEdit.PasswordChar = '*';
				}

				var control = new ZUserControl { Size = textEdit.Size + textEdit.Margin.Size };
				control.Controls.Add(textEdit);

				return control;
			}
		}

		protected virtual void AddButtonClickEvent(ZUserControl control)
		{
		}

		CharacterCasing CharacterCasing
		{
			get
			{
				var stringDataType = DataType as StringRegistryDataType;
				if (stringDataType == null)
				{
					return CharacterCasing.Normal;
				}

				switch (stringDataType.CharacterCase)
				{
					case CharacterCase.Lower:
						return CharacterCasing.Lower;
					case CharacterCase.Upper:
						return CharacterCasing.Upper;
					default:
						return CharacterCasing.Normal;
				}
			}
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			if (DataType is StringRegistryDataType stringDataType && stringDataType.ReadOnly)
			{
				return stringDataType.OverrideValue;
			}

			var o = (TextBoxWrapper)((ZUserControl)(editorPane)).CurrentDataItem;
			return o.Value.ToString();
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var resString = value as ResourceString;
			var o = (TextBoxWrapper)((ZUserControl)(editorPane)).CurrentDataItem;

			var isEditPaneReadOnly = editorPane.GetReadOnly();

			if (DataType is StringRegistryDataType stringDataType && stringDataType.ReadOnly && !isEditPaneReadOnly)
			{
				o.Value = stringDataType.OverrideValue;
			}
			else
			{
				o.Value = value != null ? (resString != null ? resString.GetUnresolvedString() : value.ToString()) : "";
			}
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return (Info.EditorType == TextEditorType.Memo || Info.EditorType == TextEditorType.HTML || Info.EditorType == TextEditorType.AWBCustomisableMultilineText) ? EditorPaneAnchor.All : EditorPaneAnchor.TopLeftRight; }
		}

		readonly TextRegistryEditorInfo Info;
		readonly IRegistryItem item;
	}
}
