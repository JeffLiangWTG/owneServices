using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUCMRMiscOptionsUserControlTest : TestCaseWithFactory
	{
		public void TestPremisesIdColumnModuleID()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var control = new AUCMRMiscOptionsUserControl())
			{
				var premisesIdColumn = control.aQISPremisesIdProcessingTypeGrid.GetColumnStyle(nameof(AQISPremisesIdAndProcessingType.PremisesId)) as ZCodeFindBoxColumnStyleInfo;
				AssertEquals("ModuleID - Registry is off", Enterprise.ZArchitecture.Modules.ModuleIDs.Premises, premisesIdColumn.ModuleID);
			}

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var control = new AUCMRMiscOptionsUserControl())
			{
				var premisesIdColumn = control.aQISPremisesIdProcessingTypeGrid.GetColumnStyle(nameof(AQISPremisesIdAndProcessingType.PremisesId)) as ZCodeFindBoxColumnStyleInfo;
				AssertEquals("ModuleID - Registry is on", Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList, premisesIdColumn.ModuleID);
			}
		}

		public void TestControlVisibility()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_MessageType = "IMP";
			using (var testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var miscOptionsControl = testForm.CustomsBrokerageUserControl.MiscOptions as AUCMRMiscOptionsUserControl;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Other;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				miscOptionsControl = testForm.CustomsBrokerageUserControl.MiscOptions as AUCMRMiscOptionsUserControl;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				miscOptionsControl = testForm.CustomsBrokerageUserControl.MiscOptions as AUCMRMiscOptionsUserControl;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				miscOptionsControl = testForm.CustomsBrokerageUserControl.MiscOptions as AUCMRMiscOptionsUserControl;
				AssertEquals("PeriodicSettlementGroupBox is not visible", false, miscOptionsControl.PeriodicSettlementGroupBox.Visible);
			}
		}

		public void TestControlVisibilityForDrawback()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
			AUCustomsDataRegistry.Instance.UPEImplementationDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-1).ToDateTime());
			using (var testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var miscOptionsControl = testForm.CustomsBrokerageUserControl.MiscOptions as AUCMRMiscOptionsUserControl;
				AssertEquals("DeclarationIndicators", false, miscOptionsControl.DeclarationIndicators.Visible);
				AssertEquals("ExternalDeclaration", false, miscOptionsControl.ExternalDeclaration.Visible);
				AssertEquals("AQISInformationGroupBox", false, miscOptionsControl.AQISInformationGroupBox.Visible);
				AssertEquals("AQISDocumentGroupBox", false, miscOptionsControl.AQISDocumentGroupBox.Visible);
				AssertEquals("AQISPremisesIdAndPackagesGroupBox", false, miscOptionsControl.AQISPremisesIdAndPackagesGroupBox.Visible);
				AssertEquals("AQISInspectionLocationGroupBox", false, miscOptionsControl.AQISInspectionLocationGroupBox.Visible);
				AssertEquals("LandedCostingDefaultsGroupBox", false, miscOptionsControl.LandedCostingDefaultsGroupBox.Visible);
				AssertEquals("UnaccompaniedPersonalEffectsID", false, miscOptionsControl.UnaccompaniedPersonalEffectsID.Visible);
				AssertEquals("FirstPaidUnderProtestID", false, miscOptionsControl.FirstPaidUnderProtestID.Visible);
				AssertEquals("CustomsReceiptForGoodsIdTextBox", false, miscOptionsControl.CustomsReceiptForGoodsIdTextBox.Visible);
				AssertEquals("ForceManualTILVCheckBox", false, miscOptionsControl.ForceManualTILVCheckBox.Visible);
				AssertEquals("HeaderAmberReasonTypeCodeFindBox", false, miscOptionsControl.HeaderAmberReasonTypeCodeFindBox.Visible);
				AssertEquals("HeaderAmberReasonTypeDropEdit", true, miscOptionsControl.HeaderAmberReasonTypeDropEdit.Visible);
				AssertEquals("AmberStatementTextBox", true, miscOptionsControl.AmberStatementTextBox.Visible);
				AssertEquals("PeriodicSettlementGroupBox is not visible", false, miscOptionsControl.PeriodicSettlementGroupBox.Visible);
				AssertEquals("SOFACheckBox should not be visible for non-import jobs", false, miscOptionsControl.SOFACheckBox.Visible);
			}
		}

		public void TestAQISGroupBoxVisibility()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			using (var testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var miscControl = testForm.CustomsBrokerageUserControl.MiscOptions as AUCMRMiscOptionsUserControl;
				AssertEquals("Quarantine group box is visible", true, miscControl.AQISInspectionLocationGroupBox.Visible);
				AssertEquals("Quarantine group box is visible", true, miscControl.AQISConcernTypes.Visible);
				AssertEquals("Quarantine group box is visible", true, miscControl.AQISInformationGroupBox.Visible);
				AssertEquals("Quarantine group box is visible", true, miscControl.AQISDocumentGroupBox.Visible);
				AssertEquals("Quarantine group box is visible", true, miscControl.AQISPremisesIdAndPackagesGroupBox.Visible);
				AssertEquals("PeriodicSettlementGroupBox is not visible", false, miscControl.PeriodicSettlementGroupBox.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				miscControl = testForm.CustomsBrokerageUserControl.MiscOptions as AUCMRMiscOptionsUserControl;
				AssertEquals("Quarantine group box is not visible", false, miscControl.AQISInspectionLocationGroupBox.Visible);
				AssertEquals("Quarantine group box is visible", false, miscControl.AQISConcernTypes.Visible);
				AssertEquals("Quarantine group box is visible", false, miscControl.AQISInformationGroupBox.Visible);
				AssertEquals("Quarantine group box is visible", false, miscControl.AQISDocumentGroupBox.Visible);
				AssertEquals("Quarantine group box is visible", false, miscControl.AQISPremisesIdAndPackagesGroupBox.Visible);
				AssertEquals("PeriodicSettlementGroupBox is visible", true, miscControl.PeriodicSettlementGroupBox.Visible);
			}
		}

		public void TestSOFAControlVisibility()
		{
			var importDeclaration = JobDeclaration.New(Factory);
			importDeclaration.JE_ApplicationCode = "CMR";
			importDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			using (var testForm = new ZAUCustomsDeclarationForm(importDeclaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var miscOptionsControl = testForm.CustomsBrokerageUserControl.MiscOptions as AUCMRMiscOptionsUserControl;
				AssertEquals("SOFACheckBox should  be visible for import jobs", true, miscOptionsControl.SOFACheckBox.Visible);
			}

			var drawbackDeclaration = JobDeclaration.New(Factory);
			drawbackDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
			using (var testForm = new ZAUCustomsDeclarationForm(drawbackDeclaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var miscOptionsControl = testForm.CustomsBrokerageUserControl.MiscOptions as AUCMRMiscOptionsUserControl;
				AssertEquals("SOFACheckBox should not be visible for non-import jobs", false, miscOptionsControl.SOFACheckBox.Visible);
			}
		}

		public void TestPeriodicSettlementGroupBox_Caption()
		{
			using (var miscOptionsControl = new AUCMRMiscOptionsUserControl())
			{
				AssertEquals("Periodic Settlement Details", miscOptionsControl.PeriodicSettlementGroupBox.Text);
			}
		}

		public void TestSettlementTypeDropEdit()
		{
			using (var miscOptionsControl = new AUCMRMiscOptionsUserControl())
			{
				var control = miscOptionsControl.SettlementTypeDropEdit;
				AssertType<ZDropEdit>("Type", control);
				AssertEquals("BindTo", "JE_SettlementPeriodType", control.BindTo);
				AssertEquals("BindToList", "AddInfo+Lookups+SettlementPeriodTypeList", control.BindToList);
			}
		}

		public void TestSettlementPeriodEndDate()
		{
			using (var miscOptionsControl = new AUCMRMiscOptionsUserControl())
			{
				var control = miscOptionsControl.settlementPeriodEndDate;
				AssertType<ZDateEdit>("Type", control);
				AssertEquals("BindTo", "JE_SettlementPeriodEndDate", control.BindTo);
			}
		}
	}
}
