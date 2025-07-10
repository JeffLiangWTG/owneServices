using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EmailDestinationOverrideItemEditor))]
	sealed class EmailDestinationOverrideItemEditorTestCase : RegistryItemEditorTestCase
	{
		public void TestClearOverrideEmailArrdessNDR()
		{
			using (var control = (ZUserControl)Editor.NewWinFormsEditorPane())
			{
				var emailAddressNDR = Factory.NewWithValidTestData<GlbEmailAddress>();
				emailAddressNDR.GI_EmailAddress = "test@test.com";
				emailAddressNDR.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
				Factory.Save();
				Env.Registry.EmailDestinationOverride = emailAddressNDR.GI_EmailAddress;
				Editor.SetValueFromEditorPane(control, emailAddressNDR.GI_EmailAddress);

				var clearButton = (ZButton)control.Controls[1];

				clearButton.PerformClick();
				AssertEquals("The NDR status has been cleared successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

				clearButton.PerformClick();
				AssertEquals("Email Destination Override is not NDR.", UnitTestUserNotification.Instance.LastMessage.Text);

				Editor.SetValueFromEditorPane(control, emailAddressNDR.GI_EmailAddress + "1");
				clearButton.PerformClick();
				AssertEquals("Please save the data first.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public override void TestEditorPaneLayout()
		{
			using (var control = (ZUserControl)Editor.NewWinFormsEditorPane())
			{
				var box = (ZTextBox)control.Controls[0];
				var originalHeight = box.Height;
				Assert("Precondition: Box.Width should not be 300", box.Width != 300);

				Editor.SetEditorPaneLayout(box, 300, 200);
				AssertEquals("Box.Width", 300, box.Width);
				AssertEquals("Box.Height", originalHeight, box.Height);
			}
		}

		#region Implementation

		protected sealed override RegistryItemEditor GetEditor()
		{
			return new EmailDestinationOverrideItemEditor(new EmailDestinationOverrideDataType(), new BusinessObjectFactory(), new TextRegistryEditorInfo(TextEditorType.TextBox));
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new string[]
			{
				"AllYourBase",
				"12345"
			};
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !editorPane.GetReadOnly();
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ZUserControl);
		}
		#endregion
	}
}
