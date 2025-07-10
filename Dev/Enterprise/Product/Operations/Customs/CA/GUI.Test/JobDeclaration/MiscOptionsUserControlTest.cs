using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class MiscOptionsUserControlTest : TestCaseWithFactory
	{
		public void TestCSAEntryCheckBoxVisibilityAndCaption()
		{
			AssertCSAEntryCheckBoxVisibilityAndCaption(true);
			AssertCSAEntryCheckBoxVisibilityAndCaption(false);
		}

		public void TestBondGroupBoxVisibility()
		{
			AssertBondGroupBoxVisibility(true);
			AssertBondGroupBoxVisibility(false);
		}

		public void TestRefreshBondButton_Click()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var bondCollection = new Business.CusBondDetailCollection(org);
			var bondData1 = bondCollection.AddNew();
			bondData1.PW_BondType = BondTypeList.Codes.SingleTransactionBond;
			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			bondData1.PW_BondNumber = "00001";
			bondData1.PW_SuretyCode = "001";
			var bondData2 = bondCollection.AddNew();
			bondData2.PW_BondType = BondTypeList.Codes.ContinuousBond;
			bondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-5);
			bondData2.PW_BondNumber = "00002";
			bondData2.PW_SuretyCode = "002";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = org.PK;
			declaration.CA_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertEquals(BondTypeList.Codes.SingleTransactionBond, declaration.CA_BondType);
			AssertEquals("00001", declaration.CA_BondNo);
			AssertEquals("001", declaration.CA_SuretyCode);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
				var miscControl = (CAMiscOptionsUserControl)brokerageControl.MiscOptions;
				var refreshBondButton = miscControl.FindSingle<ZButton>("RefreshBondButton");
				AssertNotNull(refreshBondButton);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				refreshBondButton.PerformClick();
				AssertEquals(BondTypeList.Codes.ContinuousBond, declaration.CA_BondType);
				AssertEquals("00002", declaration.CA_BondNo);
				AssertEquals("002", declaration.CA_SuretyCode);
			}
		}

		public void TestControlVisibility_Export()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
				var miscControl = (CAMiscOptionsUserControl)brokerageControl.MiscOptions;
				var oGDNRCheckBox = miscControl.FindSingle<ZCheckBox>("OGDNRCheckBox");
				var oGDTCCheckBox = miscControl.FindSingle<ZCheckBox>("OGDTCCheckBox");
				var oGDICCheckBox = miscControl.FindSingle<ZCheckBox>("OGDICCheckBox");
				var oGDCFIACheckBox = miscControl.FindSingle<ZCheckBox>("OGDCFIACheckBox");
				var woodPackagingIndCheckBox = miscControl.FindSingle<ZCheckBox>("WoodPackagingIndCheckBox");
				var permitApplicationCheckBox = miscControl.FindSingle<ZCheckBox>("PermitApplicationCheckBox");
				var inspectionArrangementsCompleteCheckBox = miscControl.FindSingle<ZCheckBox>("InspectionArrangementsCompleteCheckBox");
				var pGAOptionsGroupBox = miscControl.FindSingle<ZGroupBox>("PGAOptionsGroupBox");
				CombineAssertions(() =>
				{
					AssertEquals("ImportDeclarationOptionsGroupBox.Visible", false, miscControl.Controls.Find("ImportDeclarationOptionsGroupBox", true)[0].Visible);
					AssertEquals("CAMergeByDropEdit.Visible", false, miscControl.Controls.Find("CAMergeByDropEdit", true)[0].Visible);
					AssertEquals("MergeByDropEdit.Caption", "Merge By", miscControl.Controls.Find("MergeByDropEdit", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("OGDNRCheckBox.Visible", false, oGDNRCheckBox.Visible);
					AssertEquals("OGDTCCheckBox.Visible", false, oGDTCCheckBox.Visible);
					AssertEquals("OGDICCheckBox.Visible", false, oGDICCheckBox.Visible);
					AssertEquals("OGDCFIACheckBox.Visible", false, oGDCFIACheckBox.Visible);
					AssertEquals("WoodPackagingIndCheckBox.Visible", false, woodPackagingIndCheckBox.Visible);
					AssertEquals("PermitApplicationCheckBox.Visible", false, permitApplicationCheckBox.Visible);
					AssertEquals("InspectionArrangementsCompleteCheckBox.Visible", false, inspectionArrangementsCompleteCheckBox.Visible);
					AssertEquals("PGAOptionsGroupBox.Visible", false, pGAOptionsGroupBox.Visible);
				});
			}
		}

		public void TestControlVisibility_ImportNotLVSAndIID()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ZString.Empty;
				declaration.CA_OGDNR = true;
				declaration.CA_OGDTC = true;
				declaration.CA_OGDIC = true;
				declaration.CA_OGDCFIA = true;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
				var miscControl = (CAMiscOptionsUserControl)brokerageControl.MiscOptions;
				var oGDNRCheckBox = miscControl.FindSingle<ZCheckBox>("OGDNRCheckBox");
				var oGDTCCheckBox = miscControl.FindSingle<ZCheckBox>("OGDTCCheckBox");
				var oGDICCheckBox = miscControl.FindSingle<ZCheckBox>("OGDICCheckBox");
				var oGDCFIACheckBox = miscControl.FindSingle<ZCheckBox>("OGDCFIACheckBox");
				var woodPackagingIndCheckBox = miscControl.FindSingle<ZCheckBox>("WoodPackagingIndCheckBox");
				var permitApplicationCheckBox = miscControl.FindSingle<ZCheckBox>("PermitApplicationCheckBox");
				var inspectionArrangementsCompleteCheckBox = miscControl.FindSingle<ZCheckBox>("InspectionArrangementsCompleteCheckBox");
				CombineAssertions(() =>
				{
					AssertEquals("ImportDeclarationOptionsGroupBox.Visible", true, miscControl.Controls.Find("ImportDeclarationOptionsGroupBox", true)[0].Visible);
					AssertEquals("CAMergeByDropEdit.Visible", true, miscControl.Controls.Find("CAMergeByDropEdit", true)[0].Visible);
					AssertEquals("MergeByDropEdit.Caption", "EDI Release Merge By", miscControl.Controls.Find("MergeByDropEdit", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("OGDNRCheckBox.Visible", true, oGDNRCheckBox.Visible);
					AssertEquals("OGDTCCheckBox.Visible", true, oGDTCCheckBox.Visible);
					AssertEquals("OGDICCheckBox.Visible", true, oGDICCheckBox.Visible);
					AssertEquals("OGDCFIACheckBox.Visible", true, oGDCFIACheckBox.Visible);
					AssertEquals("WoodPackagingIndCheckBox.Visible", false, woodPackagingIndCheckBox.Visible);
					AssertEquals("PermitApplicationCheckBox.Visible", false, permitApplicationCheckBox.Visible);
					AssertEquals("InspectionArrangementsCompleteCheckBox.Visible", false, inspectionArrangementsCompleteCheckBox.Visible);
				});
			}
		}

		public void TestControlVisibility_ImportVSAndIID()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;

				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
				var miscControl = (CAMiscOptionsUserControl)brokerageControl.MiscOptions;
				var oGDNRCheckBox = miscControl.FindSingle<ZCheckBox>("OGDNRCheckBox");
				var oGDTCCheckBox = miscControl.FindSingle<ZCheckBox>("OGDTCCheckBox");
				var oGDICCheckBox = miscControl.FindSingle<ZCheckBox>("OGDICCheckBox");
				var oGDCFIACheckBox = miscControl.FindSingle<ZCheckBox>("OGDCFIACheckBox");
				var woodPackagingIndCheckBox = miscControl.FindSingle<ZCheckBox>("WoodPackagingIndCheckBox");
				var permitApplicationCheckBox = miscControl.FindSingle<ZCheckBox>("PermitApplicationCheckBox");
				var inspectionArrangementsCompleteCheckBox = miscControl.FindSingle<ZCheckBox>("InspectionArrangementsCompleteCheckBox");
				var pGAOptionsGroupBox = miscControl.FindSingle<ZGroupBox>("PGAOptionsGroupBox");
				CombineAssertions(() =>
				{
					AssertEquals("ImportDeclarationOptionsGroupBox.Visible", true, miscControl.Controls.Find("ImportDeclarationOptionsGroupBox", true)[0].Visible);
					AssertEquals("CAMergeByDropEdit.Visible", false, miscControl.Controls.Find("CAMergeByDropEdit", true)[0].Visible);
					AssertEquals("MergeByDropEdit.Caption", "Merge By", miscControl.Controls.Find("MergeByDropEdit", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("OGDNRCheckBox.Visible", false, oGDNRCheckBox.Visible);
					AssertEquals("OGDTCCheckBox.Visible", false, oGDTCCheckBox.Visible);
					AssertEquals("OGDICCheckBox.Visible", false, oGDICCheckBox.Visible);
					AssertEquals("OGDCFIACheckBox.Visible", false, oGDCFIACheckBox.Visible);
					AssertEquals("WoodPackagingIndCheckBox.Visible", false, woodPackagingIndCheckBox.Visible);
					AssertEquals("PermitApplicationCheckBox.Visible", false, permitApplicationCheckBox.Visible);
					AssertEquals("InspectionArrangementsCompleteCheckBox.Visible", false, inspectionArrangementsCompleteCheckBox.Visible);
					AssertEquals("PGAOptionsGroupBox.Visible", false, pGAOptionsGroupBox.Visible);
				});
			}
		}

		public void TestControlVisibilityForIM2()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;

				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
				var miscControl = (CAMiscOptionsUserControl)brokerageControl.MiscOptions;
				AssertEquals("ImportDeclarationOptionsGroupBox.Visible", false, miscControl.Controls.Find("ImportDeclarationOptionsGroupBox", true)[0].Visible);
				AssertEquals("MiscOptionsGroupBox.Caption", "Administered By", miscControl.Controls.Find("MiscOptionsGroupBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("BrokerCodeFindBox.Caption", "B2 Signed By", miscControl.Controls.Find("BrokerCodeFindBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("CAMergeByDropEdit.Caption", "B2 Merge By", miscControl.Controls.Find("CAMergeByDropEdit", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("PGAOptionsGroupBox.Visible", false, miscControl.Controls.Find("PGAOptionsGroupBox", true)[0].Visible);
				AssertEquals("MergeByDropEdit.Visible", false, miscControl.Controls.Find("MergeByDropEdit", true)[0].Visible);
				AssertEquals("DefaultFreightCalcEdit.Visible", false, miscControl.Controls.Find("DefaultFreightCalcEdit", true)[0].Visible);
				AssertEquals("CommentLabel.Visible", false, miscControl.Controls.Find("CommentLabel", true)[0].Visible);
			}
		}

		public void TestCAMergeByDropEditVisibility()
		{
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CarmR2, "CA", ZDateTime.Now, true))
			{
				AssertCAMergeByDropEditVisibility(JobMessageTypeList.Codes.Import, false);

				AssertCAMergeByDropEditVisibility(JobMessageTypeList.Codes.LowValueShipments, false);

				AssertCAMergeByDropEditVisibility(JobMessageTypeList.Codes.LVSForConsolidation, false);

				AssertCAMergeByDropEditVisibility(JobMessageTypeList.Codes.ImportCopyforB2, true);
			}

			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CarmR2, "CA", ZDateTime.Now, false))
			{
				AssertCAMergeByDropEditVisibility(JobMessageTypeList.Codes.Import, true);
			}
		}

		void AssertCAMergeByDropEditVisibility(ZString messageType, bool visible)
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				declaration.JE_MessageType = messageType;
				declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
				var miscControl = (CAMiscOptionsUserControl)brokerageControl.MiscOptions;
				AssertEquals("CAMergeByDropEdit.Visible", visible, miscControl.Controls.Find("CAMergeByDropEdit", true)[0].Visible);
			}
		}

		void AssertCSAEntryCheckBoxVisibilityAndCaption(ZBool csaApproved)
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName2";
			OrgImpAddInfo.Get(importer).ZO_IsCSAApprovedImporter = csaApproved;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;

				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
				var miscControl = (CAMiscOptionsUserControl)brokerageControl.MiscOptions;

				var csaEntryCheckBox = miscControl.FindSingle<ZCheckBox>("CSAEntryCheckBox");
				AssertEquals("OGDNRCheckBox.Visible", csaApproved, csaEntryCheckBox.Visible);
				AssertEquals("CSAEntryCheckBox.Caption", "CSA Release Only", csaEntryCheckBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		void AssertBondGroupBoxVisibility(bool enableCADMessage)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, enableCADMessage))
			{
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.MiscOptionsTabPage;
					var miscControl = (CAMiscOptionsUserControl)brokerageControl.MiscOptions;
					var bondGroupBox = miscControl.FindSingle<ZGroupBox>("BondGroupBox");
					AssertEquals("BondGroupBox.Visible", enableCADMessage, bondGroupBox.Visible);
				}
			}
		}
	}
}
