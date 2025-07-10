using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	sealed class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
		public void TestControlsVisibilityForImport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ControlMessageUnreadEDocStatus = YesNoList.Codes.Yes;

			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertControlVisibility(userControl, "GoodsDescriptionTextBox", true);
					AssertControlVisibility(userControl, "JE_ShipmentIncoTermPlaceTextBox", true);
					AssertControlVisibility(userControl, "IncoTermDropEdit", true);
					AssertControlVisibility(userControl, "IncoTermExplainButton", true);
					AssertControlVisibility(userControl, "ZG_AgreedPlaceCodeDropEdit", true);
					AssertControlVisibility(userControl, "AgentsReference", false);
					AssertControlVisibility(userControl, "GatewayDropEdit", false);
					AssertControlVisibility(userControl, "GoodsLocationDropEdit", false);
					AssertControlVisibility(userControl, "GoodsLocationTextBox", true);
					AssertControlVisibility(userControl, "BorderTransportMeansDropEdit", true);
					AssertControlVisibility(userControl, "CustomsOfficeFindBox", true);
					AssertControlVisibility(userControl, "PresentationGroupBox", false);
					AssertControlVisibility(userControl, "PresentationEndDateEdit", false);
					AssertControlVisibility(userControl, "PresentationStartDateEdit", false);
					AssertControlVisibility(userControl, "EXPCTLLinkLabel", false);
					AssertControlVisibility(userControl, "StyleOfEntrySOEDropDown", false);

					AssertEquals("PresentationEndDateEdit", false, userControl.FindSingle<Control>("PresentationEndDateEdit").Enabled);
					AssertEquals("PresentationStartDateEdit", false, userControl.FindSingle<Control>("PresentationStartDateEdit").Enabled);
					AssertEquals("SupplierDocAddress", true, userControl.FindSingle<ZDocAddressControl>("SupplierDocAddress").Enabled);
					AssertEquals("ImporterDocAddress", true, userControl.FindSingle<ZDocAddressControl>("ImporterDocAddress").Enabled);
					AssertEquals("IncoTermExplainButton", true, userControl.FindSingle<ZButton>("IncoTermExplainButton").Enabled);
				});
			}
		}

		public void TestControlsVisibilityForExport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.ControlMessageUnreadEDocStatus = YesNoList.Codes.Yes;

			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertControlVisibility(userControl, "GoodsDescriptionTextBox", true);
					AssertControlVisibility(userControl, "JE_ShipmentIncoTermPlaceTextBox", true);
					AssertControlVisibility(userControl, "IncoTermDropEdit", true);
					AssertControlVisibility(userControl, "IncoTermExplainButton", true);
					AssertControlVisibility(userControl, "ZG_AgreedPlaceCodeDropEdit", true);
					AssertControlVisibility(userControl, "AgentsReference", true);
					AssertControlVisibility(userControl, "GatewayDropEdit", false);
					AssertControlVisibility(userControl, "GoodsLocationDropEdit", true);
					AssertControlVisibility(userControl, "GoodsLocationTextBox", false);
					AssertControlVisibility(userControl, "BorderTransportMeansDropEdit", true);
					AssertControlVisibility(userControl, "PresentationGroupBox", true);
					AssertControlVisibility(userControl, "PresentationEndDateEdit", true);
					AssertControlVisibility(userControl, "PresentationStartDateEdit", true);
					AssertControlVisibility(userControl, "EXPCTLLinkLabel", true);

					AssertEquals("PresentationEndDateEdit", true, userControl.FindSingle<Control>("PresentationEndDateEdit").Enabled);
					AssertEquals("PresentationStartDateEdit", true, userControl.FindSingle<Control>("PresentationStartDateEdit").Enabled);
					AssertEquals("SupplierDocAddress", true, userControl.FindSingle<ZDocAddressControl>("SupplierDocAddress").Enabled);
					AssertEquals("ImporterDocAddress", true, userControl.FindSingle<ZDocAddressControl>("ImporterDocAddress").Enabled);
					AssertEquals("IncoTermExplainButton", true, userControl.FindSingle<ZButton>("IncoTermExplainButton").Enabled);
				});
			}
		}

		public void TestControlsVisibilityForWarehouseAdjustment()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;

			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("SupplierDocAddress", true, userControl.FindSingle<ZDocAddressControl>("SupplierDocAddress").Enabled);
					AssertEquals("ImporterDocAddress", false, userControl.FindSingle<ZDocAddressControl>("ImporterDocAddress").Enabled);
					AssertEquals("IncoTermExplainButton", false, userControl.FindSingle<ZButton>("IncoTermExplainButton").Enabled);
				});
			}
		}

		public void TestEXPCTLDefaultProperties()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				var linkLabel = (LinkLabel)userControl.FindSingle<Control>("EXPCTLLinkLabel");
				CombineAssertions(() =>
				{
					AssertEquals(System.Drawing.Color.Black, linkLabel.DisabledLinkColor);
					AssertEquals(System.Drawing.Color.Red, linkLabel.LinkColor);
				});
			}
		}

		public void TestEXPCTLLinkLabelEnabled() => AssertEXPCTLLinkLabelStatus(true);

		public void TestEXPCTLLinkLabelDisabled() => AssertEXPCTLLinkLabelStatus(false);

		public void TestEXPCTLLinkLabelClick()
		{
			const string message = "Customs has sent a control message. Please confirm that you have read the eDoc for Message E_EXP_CTL attached to this declaration.";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.ControlMessageUnreadEDocStatus = YesNoList.Codes.Yes;

			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var linkLabelIButtonControl = (IButtonControl)userControl.FindSingle<LinkLabel>("EXPCTLLinkLabel");

				linkLabelIButtonControl.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(message));
			}
		}

		public void TestExportControlMessagePopUpBehaviour()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.ControlMessageUnreadEDocStatus = YesNoList.Codes.Yes;

			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			using (Factory.SetTemporaryCurrentUser(code: "KCH"))
			{
				form.Controls.Add(userControl);
				form.Show();
				var linkLabelControl = userControl.FindSingle<LinkLabel>("EXPCTLLinkLabel");
				var linkLabelIButtonControl = (IButtonControl)linkLabelControl;

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					linkLabelIButtonControl.PerformClick();
					AssertEquals("ControlMessageUnreadEDocStatus stays 'Y'", true, YesNoList.IsYes(declaration.ControlMessageUnreadEDocStatus));
					AssertNull("No Log created", GetExportControlMessageLog());
					AssertCorrectLinkLabelPropertiesAfterExportControlMessagePopUp("DialogResult 'Cancel'", true, true);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					linkLabelIButtonControl.PerformClick();
					AssertEquals("ControlMessageUnreadEDocStatus changes to 'N'", true, YesNoList.IsNo(declaration.ControlMessageUnreadEDocStatus));
					AssertNotNull("Log created", GetExportControlMessageLog());
					AssertCorrectLinkLabelPropertiesAfterExportControlMessagePopUp("DialogResult 'OK'", false, true);
				});

				StmALog GetExportControlMessageLog() => declaration.Logs.Find(x => x.SL_Reference == "Export Control Message" && x.SL_GS_NKUser == "KCH" && x.SL_SE_NKEvent == AutoEvents.RelatedEDocsReadCode).SingleOrDefault();

				void AssertCorrectLinkLabelPropertiesAfterExportControlMessagePopUp(string testCase, bool expectedEnabled, bool expectedVisible)
				{
					AssertEquals($"{testCase}: LinkLabel enabled", expectedEnabled, linkLabelControl.Enabled);
					AssertEquals($"{testCase}: LinkLabel visible", expectedVisible, linkLabelControl.Visible);
				}
			}
		}

		public void TestControlsCaption_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				AssertEquals("BorderTransportMeansDropEdit", "Border M.O.T.", userControl.FindSingle<ZDropEdit>("BorderTransportMeansDropEdit").CaptionResourceString.Caption);
			}
		}

		public void TestControlsCaption_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				AssertEquals("BorderTransportMeansDropEdit", "Border T.O.ID.", userControl.FindSingle<ZDropEdit>("BorderTransportMeansDropEdit").CaptionResourceString.Caption);
			}
		}

		public void TestIsHighValueOvrdCheckBoxVisibility_Default()
		{
			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				AssertEquals("IsHighValueOvrdCheckBox Enabled by default", true, userControl.IsHighValueOvrdCheckBox.Visible);
			}
		}

		public void TestOrganizationTabShouldBeSelectedByDefault()
		{
			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var tabControl = form.FindSingle<ZTabControl>("RightTabControl");
				var selectedTab = tabControl.SelectedTab;

				AssertEquals("The Organizations tab should be selected by default, even though it's not the first tab.", userControl.OrganisationsTabPage, selectedTab);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		void AssertControlVisibility(Control userControl, string controlName, bool isVisible)
		{
			AssertEquals(controlName, isVisible, userControl.FindSingle<Control>(controlName).Visible);
		}

		void AssertEXPCTLLinkLabelStatus(ZBool enabled)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.ControlMessageUnreadEDocStatus = enabled ? YesNoList.Codes.Yes : YesNoList.Codes.No;

			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var linkLabel = (LinkLabel)userControl.FindSingle<Control>("EXPCTLLinkLabel");
				AssertEquals("EXPCTLLinkLabel", enabled, linkLabel.Enabled);
			}
		}
	}
}
