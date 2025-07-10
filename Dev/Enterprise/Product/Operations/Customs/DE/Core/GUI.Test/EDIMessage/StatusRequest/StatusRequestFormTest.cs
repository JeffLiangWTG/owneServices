using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ExportStatusRequestForm))]
	class StatusRequestFormTest : ZFormBasherTest
	{
		public void TestSendButton_Click()
		{
			var statusRequest = Factory.New<StatusRequest>();
			using (var form = new ExportStatusRequestForm(statusRequest))
			{
				form.Show();
				CombineAssertions(() =>
				{
					var sendButton = form.FindSingle<ZButton>("SendButton");
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddUserResponse("Yes");
					sendButton.PerformClick();
					AssertEquals("Notify message errors", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Do you want to send the message(s) despite these message errors?"));
					AssertEquals("Successful message", MessagingMenuExtension.MessageHasBeenSent, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Message is generated", true, !statusRequest.EM_MessageText.IsEmpty);
				});
			}
		}

		public void TestModuleInfoOnValueChanged()
		{
			var statusRequest = Factory.New<StatusRequest>();
			statusRequest.Module = ExportStatusRequestModuleCodeList.Codes.NCTS;
			statusRequest.Role = ExportStatusRequestNCTSRoleList.Codes.Consignor;
			using (var form = new ExportStatusRequestForm(statusRequest))
			{
				form.Show();
				statusRequest.Module = ExportStatusRequestModuleCodeList.Codes.AES;
				AssertEquals("Updated Role Description", ExportStatusRequestAESRoleList.Descriptions.Exporter, form.FindSingle<ZDropEdit>("roleDropEdit").DescriptionBox.Text);
			}
		}

		public void TestControlsVisibilityOnViewMode_New()
		{
			using (var form = new ExportStatusRequestForm(Factory.New<StatusRequest>()))
			{
				form.Show();
				Application.DoEvents();
				CombineAssertions(() =>
				{
					AssertEquals("SendButton is visible", true, form.FindSingle<ZButton>("SendButton").Visible);
					AssertEquals("identificationAddress is visible", true, form.FindSingle<ZGuidFindBox>("identificationFindBox").Visible);
					AssertEquals("roleDropEdit is visible", true, form.FindSingle<ZDropEdit>("roleDropEdit").Visible);
					AssertEquals("CancelButton2's Caption is Cancel", "Cancel", form.FindSingle<ZButton>("CancelButton2").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertContains($"Should be 'New Status Request'", "New Status Request", form.FormHeading);
				});
			}
		}

		public void TestControlsVisibilityOnViewMode_View()
		{
			using (var form = new ExportStatusRequestForm(Factory.New<StatusRequest>()))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();
				Application.DoEvents();
				CombineAssertions(() =>
				{
					AssertEquals("SendButton is NOT visible", false, form.FindSingle<ZButton>("SendButton").Visible);
					AssertEquals("identificationAddress is NOT  visible", false, form.FindSingle<ZGuidFindBox>("identificationFindBox").Visible);
					AssertEquals("roleDropEdit is NOT visible", false, form.FindSingle<ZDropEdit>("roleDropEdit").Visible);
					AssertEquals("CancelButton2's Caption is Close", "Close", form.FindSingle<ZButton>("CancelButton2").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertContains($"Should be 'View Status Request'", "View Status Request", form.FormHeading);
				});
			}
		}

		public void TesteDocsPlugin()
		{
			using (var form = new ExportStatusRequestForm(Factory.New<StatusRequest>()))
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			}
		}

		protected override Form GetFormToBashCore() => new ExportStatusRequestForm(Factory.New<StatusRequest>());
	}
}
