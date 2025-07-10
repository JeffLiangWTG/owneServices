using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(LoginPasswordListRegistryItemEditor))]
	sealed class LoginPasswordListRegistryItemEditorTest : CodeDescriptionListRegistryItemEditorTest
	{
		public void TestEditControl()
		{
			var editor = GetEditor();
			using (var control = editor.NewWinFormsEditorPane())
			{
				AssertEquals("Should use specialized login/password edit control for registry", typeof(LoginPasswordListEditControlForRegistry).FullName, control.GetType().FullName);
			}
		}

		public void TestDescriptionColumnPasswordChar()
		{
			var editor = GetEditor();
			using (var codeDescriptionControl = (CodeDescriptionListEditControlForRegistry)editor.NewWinFormsEditorPane())
			{
				var descriptionColumnStyle = (ZTextBoxColumnStyle)codeDescriptionControl.CodeDescriptionGrid.Columns["Description"].ColumnStyle;
				AssertEquals("Description column should be set to show system password char", true, descriptionColumnStyle.TextBox.UseSystemPasswordChar);
			}
		}

		public override void TestGetCustomValidation_AllowEmptyCodeAndDescription()
		{
			Assert("LoginPasswordListRegistryItemEditor does not allow empty login/password, so skip this parent test.", true);
		}

		protected override RegistryItemEditor GetEditor()
		{
			var registryItem = GetRegistryItemWithSystemStorageLevel();
			var dataType = new CodeDescriptionPairListRegistryDataType(10) { AllowEmptyCodes = false, AllowEmptyDescriptions = false };
			return new LoginPasswordListRegistryItemEditor(registryItem, dataType, new LoginPasswordPairListEditorInfo());
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(LoginPasswordListEditControlForRegistry);
		}
	}
}
