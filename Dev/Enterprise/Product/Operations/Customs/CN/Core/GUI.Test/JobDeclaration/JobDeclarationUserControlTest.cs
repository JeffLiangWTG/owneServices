using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(CNJobDeclarationUserControl))]
	class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<CNJobDeclarationUserControl, JobDeclaration>
	{
		public void TestJE_MessageSubTypeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var control = (CNJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			AssertEquals(DecTypeList.Codes.RecordListing, declaration.JE_MessageSubType);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			AssertEquals("All existing entries may be discarded by changing Declaration Type. Some of them have been sent to Customs or set Declaration Unified Number. Are you sure to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(DecTypeList.Codes.RecordListing, declaration.JE_MessageSubType);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			AssertEquals("All existing entries may be discarded by changing Declaration Type. Some of them have been sent to Customs or set Declaration Unified Number. Are you sure to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(DecTypeList.Codes.CustomsEntry, declaration.JE_MessageSubType);
		}

		public void TestBuyerAndManufacturerUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var buyerControl = form.CustomsBrokerageUserControl.Controls.Find("BuyerDocAddressControl", true).FirstOrDefault();
			var manufacturerControl = form.CustomsBrokerageUserControl.Controls.Find("ManufacturerDocAddressControl", true).FirstOrDefault();
			AssertNotNull(buyerControl);
			AssertNotNull(manufacturerControl);
			Assert(buyerControl is CNJobDocAddressControl);
			Assert(manufacturerControl is CNJobDocAddressControl);
		}

		public void TestControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			using var form = new JobDeclarationForm(declaration);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			form.Show();
			var control = (CNJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
			AssertEquals("DeclarationDetailsGroupBox is invisible", expected: true, control.DeclarationDetailsGroupBox.Visible);
			AssertEquals("ReceiptNumberTextBox is invisible", expected: false, control.ReceiptNumberTextBox.Visible);
			Assert(control.JE_CNLastPortBeforeEntryCodeFindBox.Visible);
			Assert(control.PortOfStopoverCodeFindBox.Visible);
			Assert(!control.JE_ContainerCountCalcEdit.Visible);
			var entryInstructionTabPage = form.CustomsBrokerageUserControl.EntryInstructionDetailsTabPage;
			AssertEquals("EntryInstructionTabPage is visible", true, entryInstructionTabPage?.TabVisible);
			declaration.JE_TransportMode = Business.TransportTypeList.Codes.Rail;
			form.Show();
			control = (CNJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
			AssertEquals("ReceiptNumberTextBox is visible", expected: true, control.ReceiptNumberTextBox.Visible);
			declaration.JE_MessageType = "EXP";
			form.Show();
			control = (CNJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
			Assert(!control.JE_CNLastPortBeforeEntryCodeFindBox.Visible);
			Assert(!control.PortOfStopoverCodeFindBox.Visible);
			Assert(!control.JE_ContainerCountCalcEdit.Visible);
			declaration.JE_TransportMode = Business.TransportTypeList.Codes.Sea;
			form.Show();
			control = (CNJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
			Assert(!control.JE_ContainerCountCalcEdit.Visible);
			form.Show();
			control.RightTabControl.SelectedTab = control.OrganisationsTabPage;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Drawback;
			Assert(!control.BondedWarehouseDocAddressControl.Visible);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			Assert(control.BondedWarehouseDocAddressControl.Visible);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			Assert(control.BondedWarehouseDocAddressControl.Visible);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.ExWarehouse;
			Assert(control.BondedWarehouseDocAddressControl.Visible);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
			Assert(!control.BondedWarehouseDocAddressControl.Visible);
		}

		public void TestJE_ContainerModeBoundDropDownEditVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var containerModeDropEdit = ((CNJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting).JE_ContainerModeBoundDropDownEdit;
			declaration.JE_TransportMode = "AIR";
			Assert(!containerModeDropEdit.Visible);
			declaration.JE_TransportMode = "MAI";
			Assert(!containerModeDropEdit.Visible);
			declaration.JE_TransportMode = "ROA";
			Assert(containerModeDropEdit.Visible);
			declaration.JE_TransportMode = "RAI";
			Assert(containerModeDropEdit.Visible);
			declaration.JE_TransportMode = "SEA";
			Assert(containerModeDropEdit.Visible);
			declaration.JE_TransportMode = "FIX";
			Assert(!containerModeDropEdit.Visible);
		}

		public void TestJE_CustomsOfficeCodeFindBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var control = form.CustomsBrokerageUserControl.Controls.Find("JE_CustomsOfficeCodeFindBox", searchAllChildren: true).FirstOrDefault();
			Assert(control is ZCodeFindBox);
		}

		public void TestCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var control = form.CustomsBrokerageUserControl.Controls.Find("JE_OfficeOfEntryExitCodeFindBox", searchAllChildren: true).FirstOrDefault();
			Assert(control is ZCodeFindBox);
			AssertEquals("Office of Entry", ((ZCodeFindBox)control).CaptionResourceString.Caption);
			var ciqOfficeDE = form.CustomsBrokerageUserControl.Controls.Find("JE_CIQOfficeOfEntryExitCodeFindBox", searchAllChildren: true).FirstOrDefault() as ZCodeFindBox;
			AssertEquals("Declaration is IMP, caption of CIQOffice should be CIQ Office of Entry", "CIQ Office of Entry", ciqOfficeDE.CaptionResourceString.Caption);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Office of Exit", ((ZCodeFindBox)control).CaptionResourceString.Caption);
			AssertEquals("Declaration is EXP, caption of CIQOffice should be CIQ Office of Entry", "CIQ Office of Exit", ciqOfficeDE.CaptionResourceString.Caption);
			var dischargeFindBox = form.CustomsBrokerageUserControl.Controls.Find("PortOfDischargeFindBox", searchAllChildren: true).FirstOrDefault() as ZCodeFindBox;
			AssertEquals("The Port at which the Vessel/Craft will arrive at in the Discharge Country. Country of this port will be submitted as Country Of Discharge.", dischargeFindBox.CaptionResourceString.FullDescription);
		}

		public void TestRefreshJE_MessageSubTypeBoundDropDownEdit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			Application.DoEvents();
			UserIdleWorker.Flush();
			var messageSubTypeDropEdit = form.CustomsBrokerageUserControl.Controls.Find("JE_MessageSubTypeBoundDropDownEdit", searchAllChildren: true).FirstOrDefault() as ZDropEdit;
			AssertEquals("Description for IMP+CUS", "进口报关单", messageSubTypeDropEdit.DescriptionBox.Text);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Description for EXP+CUS", "出口报关单", messageSubTypeDropEdit.DescriptionBox.Text);
		}

		public void TestControlsExistence()
		{
			using var control = new CNJobDeclarationUserControl();
			TestUtility.AssertControlExistance(control, "ImporterDocAddressControl", "ImporterDocumentaryAddress");
			TestUtility.AssertControlExistance(control, "SupplierDocAddressControl", "SupplierDocumentaryAddress");
			TestUtility.AssertControlExistance(control, "BuyerDocAddressControl", "BuyerDocAddress");
			TestUtility.AssertControlExistance(control, "ManufacturerDocAddressControl", "ManufacturerDocumentaryAddress");
			TestUtility.AssertControlExistance(control, "JE_CustomsOfficeCodeFindBox", "JE_CustomsOffice");
			TestUtility.AssertControlExistance(control, "JE_RN_NKCountryOfTradeCodeFindBox", "JE_RN_NKCountryOfTrade");
			TestUtility.AssertControlExistance(control, "JE_CNTransportModeDropDownEdit", "JE_CNTransportMode");
			TestUtility.AssertControlExistance(control, "JE_CIQOfficeOfEntryExitCodeFindBox", "JE_CIQOfficeOfEntryExit");
			TestUtility.AssertControlExistance(control, "JE_CNPortOfOriginCodeFindBox", "JE_CNPortOfOrigin");
			TestUtility.AssertControlExistance(control, "JE_CNPortOfDestinationCodeFindBox", "JE_CNPortOfDestination");
			TestUtility.AssertControlExistance(control, "JE_OfficeOfEntryExitCodeFindBox", "JE_OfficeOfEntryExit");
			TestUtility.AssertControlExistance(control, "ReceiptNumberTextBox", "JE_VesselName");
			TestUtility.AssertControlExistance(control, "JE_CNLastPortBeforeEntryCodeFindBox", "JE_CNLastPortBeforeEntry");
			TestUtility.AssertControlExistance(control, "PortOfStopoverCodeFindBox", "JE_LastPortBeforeEntry");
			TestUtility.AssertControlExistance(control, "CNCountryOfLoadingTextBox", "CNCountryOfLoading");
			TestUtility.AssertControlExistance(control, "CNCountryOfArrivalTextBox", "CNCountryOfArrival");
			TestUtility.AssertControlExistance(control, "LocationOfGoodsTextBox", "JE_LocationOfGoods");
			TestUtility.AssertControlExistance(control, "JE_DateOfUnloadCompleteDateEdit", "JE_DateOfUnloadComplete");
			TestUtility.AssertControlExistance(control, "OfficeOfDestinationCodeFindBox", "OfficeOfDestination");
			TestUtility.AssertControlExistance(control, "JE_MarksAndNumbersLongTextBox", "JE_MarksAndNumbers");
			TestUtility.AssertControlExistance(control, "JE_ClearanceModeDropEdit", "JE_ClearanceMode");
			TestUtility.AssertControlExistance(control, "JE_LicenseInvolvedCheckBox", "JE_LicenseInvolved");
			TestUtility.AssertControlExistance(control, "JE_InspectionInvolvedCheckBox", "JE_InspectionInvolved");
			TestUtility.AssertControlExistance(control, "JE_TaxInvolvedCheckBox", "JE_TaxInvolved");
			TestUtility.AssertControlExistance(control, "JE_TransitModeDropEdit", "JE_TransitMode");
			TestUtility.AssertControlExistance(control, "JE_TransportModeInlandDropEdit", "JE_TransportModeInland");
			TestUtility.AssertControlExistance(control, "JE_VoyageInlandTextBox", "JE_VoyageInland");
			TestUtility.AssertControlExistance(control, "JE_VesselInlandTextBox", "JE_VesselInland");
			TestUtility.AssertControlExistance(control, "FullValidationCheckBox", "FullValidation");
		}

		public void TestTriggerRequiresCIQOnCY_DataChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.OfficeOfDestination = ZString.Empty;
			Assert(!instruction.CEI_CIQRequires);
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			declaration.OfficeOfDestination = "0000";
			Assert(instruction.CEI_CIQRequires);
			instruction.CEI_CIQRequires = false;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			declaration.OfficeOfDestination = "0009";
			Assert(!instruction.CEI_CIQRequires);
		}

		#region two-step declaration customs clearance mode tests
		public void TestControlVisibilityWithRegistry()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), false))
			{
				using var form = new JobDeclarationForm(declaration);
				form.Show();
				var modeControl = form.Controls.Find("JE_ClearanceModeDropEdit", searchAllChildren: true).FirstOrDefault() as ZDropEdit;
				Assert(!modeControl.Visible);
				var licenseInvolved = form.CustomsBrokerageUserControl.Controls.Find("JE_LicenseInvolvedCheckBox", searchAllChildren: true).FirstOrDefault() as ZCheckBox;
				Assert(!licenseInvolved.Visible);
				var inspectionInvolved = form.CustomsBrokerageUserControl.Controls.Find("JE_InspectionInvolvedCheckBox", searchAllChildren: true).FirstOrDefault() as ZCheckBox;
				Assert(!inspectionInvolved.Visible);
				var taxInvolved = form.CustomsBrokerageUserControl.Controls.Find("JE_TaxInvolvedCheckBox", searchAllChildren: true).FirstOrDefault() as ZCheckBox;
				Assert(!taxInvolved.Visible);
			}

			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				using var form = new JobDeclarationForm(declaration);
				form.Show();
				var modeControl = form.Controls.Find("JE_ClearanceModeDropEdit", searchAllChildren: true).FirstOrDefault() as ZDropEdit;
				Assert(modeControl.Visible);
				var licenseInvolved = form.CustomsBrokerageUserControl.Controls.Find("JE_LicenseInvolvedCheckBox", searchAllChildren: true).FirstOrDefault() as ZCheckBox;
				Assert(!licenseInvolved.Visible);
				var inspectionInvolved = form.CustomsBrokerageUserControl.Controls.Find("JE_InspectionInvolvedCheckBox", searchAllChildren: true).FirstOrDefault() as ZCheckBox;
				Assert(!inspectionInvolved.Visible);
				var taxInvolved = form.CustomsBrokerageUserControl.Controls.Find("JE_TaxInvolvedCheckBox", searchAllChildren: true).FirstOrDefault() as ZCheckBox;
				Assert(!taxInvolved.Visible);
			}
		}

		public void TestBOValueChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			declaration.JE_LicenseInvolved = true;
			declaration.JE_InspectionInvolved = true;
			declaration.JE_TaxInvolved = true;
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				using var form = new JobDeclarationForm(declaration);
				form.Show();
				var modeControl = form.Controls.Find("JE_ClearanceModeDropEdit", searchAllChildren: true).FirstOrDefault() as ZDropEdit;
				var licenseInvolved = form.CustomsBrokerageUserControl.Controls.Find("JE_LicenseInvolvedCheckBox", searchAllChildren: true).FirstOrDefault() as ZCheckBox;
				var inspectionInvolved = form.CustomsBrokerageUserControl.Controls.Find("JE_InspectionInvolvedCheckBox", searchAllChildren: true).FirstOrDefault() as ZCheckBox;
				var taxInvolved = form.CustomsBrokerageUserControl.Controls.Find("JE_TaxInvolvedCheckBox", searchAllChildren: true).FirstOrDefault() as ZCheckBox;
				Assert(modeControl.Visible);
				Assert(licenseInvolved.Visible);
				Assert(inspectionInvolved.Visible);
				Assert(taxInvolved.Visible);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				Assert(!modeControl.ReadOnly);
				Assert(!licenseInvolved.Visible);
				Assert(!inspectionInvolved.Visible);
				Assert(!taxInvolved.Visible);
				Assert(!declaration.JE_LicenseInvolved);
				Assert(!declaration.JE_InspectionInvolved);
				Assert(!declaration.JE_TaxInvolved);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				Assert(modeControl.ReadOnly);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = DecTypeList.Codes.Both;
				Assert(modeControl.ReadOnly);
				declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				Assert(!modeControl.ReadOnly);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				Assert(licenseInvolved.Visible);
				Assert(inspectionInvolved.Visible);
				Assert(taxInvolved.Visible);
				declaration.JE_LicenseInvolved = true;
				declaration.JE_InspectionInvolved = true;
				declaration.JE_TaxInvolved = true;
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				Assert(modeControl.ReadOnly);
				AssertEquals(ClearanceModeList.Codes.Integrated, declaration.JE_ClearanceMode);
				Assert(!licenseInvolved.Visible);
				Assert(!inspectionInvolved.Visible);
				Assert(!taxInvolved.Visible);
				Assert(!declaration.JE_LicenseInvolved);
				Assert(!declaration.JE_InspectionInvolved);
				Assert(!declaration.JE_TaxInvolved);
			}
		}

		public void TestVisibilityOfClearanceModeAndCheckBoxesDescriptionLabel()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				using var form = new JobDeclarationForm(declaration);
				form.Show();
				var clearanceModeAndCheckBoxesDescriptionLabel = form.CustomsBrokerageUserControl.Controls.Find("ClearanceModeAndCheckBoxesDescriptionLabel", true).FirstOrDefault() as ZLabel;
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				Assert(!clearanceModeAndCheckBoxesDescriptionLabel.Visible);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				Assert(clearanceModeAndCheckBoxesDescriptionLabel.Visible);
			}
		}

		public void TestVisibilityOfFullValidationCheckBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				using var form = new JobDeclarationForm(declaration);
				form.Show();
				var fullValidationCheckBox = (form.CustomsBrokerageUserControl.DeclarationUserControl as CNJobDeclarationUserControl).FullValidationCheckBox;
				Assert(!fullValidationCheckBox.Visible);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				Assert(fullValidationCheckBox.Visible);
				declaration.JE_ClearanceMode = "";
				Assert(!fullValidationCheckBox.Visible);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				Assert(!fullValidationCheckBox.Visible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				Assert(!fullValidationCheckBox.Visible);
				declaration.JE_MessageSubType = DecTypeList.Codes.Both;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				Assert(!fullValidationCheckBox.Visible);
			}

			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), false))
			{
				using var form = new JobDeclarationForm(declaration);
				form.Show();
				var fullValidationCheckBox = (form.CustomsBrokerageUserControl.DeclarationUserControl as CNJobDeclarationUserControl).FullValidationCheckBox;
				Assert(!fullValidationCheckBox.Visible);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				Assert(!fullValidationCheckBox.Visible);
				declaration.JE_ClearanceMode = "";
				Assert(!fullValidationCheckBox.Visible);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				Assert(!fullValidationCheckBox.Visible);
			}
		}

		#endregion
		public void TestInlandControlProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var je_TransitModeDropEdit = form.Controls.Find("JE_TransitModeDropEdit", searchAllChildren: true).FirstOrDefault() as ZDropEdit;
			var je_TransportModeInlandDropEdit = form.Controls.Find("JE_TransportModeInlandDropEdit", searchAllChildren: true).FirstOrDefault() as ZDropEdit;
			var je_VoyageInlandTextBox = form.Controls.Find("JE_VoyageInlandTextBox", searchAllChildren: true).FirstOrDefault() as ZTextBox;
			var je_VesselInlandTextBox = form.Controls.Find("JE_VesselInlandTextBox", searchAllChildren: true).FirstOrDefault() as ZTextBox;
			declaration.JE_TransitMode = "";
			Assert("Invisible", !je_TransportModeInlandDropEdit.Visible);
			Assert("Invisible", !je_VoyageInlandTextBox.Visible);
			Assert("Invisible", !je_VesselInlandTextBox.Visible);
			declaration.JE_TransitMode = "A";
			declaration.JE_TransportModeInland = "SEA";
			Assert("Visible", je_TransportModeInlandDropEdit.Visible);
			Assert("Visible", je_VoyageInlandTextBox.Visible);
			Assert("Visible", je_VesselInlandTextBox.Visible);
			AssertEquals("Vessel/Voyage (Inland)", je_VesselInlandTextBox.CaptionResourceString.Caption);
			declaration.JE_TransportModeInland = "ROA";
			Assert("Visible", je_TransportModeInlandDropEdit.Visible);
			Assert("Invisible", !je_VoyageInlandTextBox.Visible);
			Assert("Visible", je_VesselInlandTextBox.Visible);
			AssertEquals("Car Number (Inland)", je_VesselInlandTextBox.CaptionResourceString.Caption);
			declaration.JE_TransportModeInland = "";
			Assert("Visible", je_TransportModeInlandDropEdit.Visible);
			Assert("Visible", je_VoyageInlandTextBox.Visible);
			Assert("Visible", je_VesselInlandTextBox.Visible);
			AssertEquals("Vessel/Voyage (Inland)", je_VesselInlandTextBox.CaptionResourceString.Caption);
			declaration.JE_TransportModeInland = "RAI";
			Assert("Visible", je_TransportModeInlandDropEdit.Visible);
			Assert("Invisible", !je_VoyageInlandTextBox.Visible);
			Assert("Visible", je_VesselInlandTextBox.Visible);
			AssertEquals("Car Number (Inland)", je_VesselInlandTextBox.CaptionResourceString.Caption);
		}

		public void TestSetRightTabControlSelectTab()
		{
			var declaration = Factory.New<JobDeclaration>();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var control = (CNJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
			AssertEquals("When open the declaration form, show Organizations as default.", control.OrganisationsTabPage, control.RightTabControl.SelectedTab);
		}

		public void TestNoExceptionThrownOnDispose()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.OfficeOfDestination = "2310";
			Factory.Save();
			using var form = new JobDeclarationForm(declaration);
			{
				form.Show();
				((INeedRow)declaration.CustomsOffices[0]).Row.Delete();
				AssertNoExceptionThrown(form.Close);
			}
		}
	}
}
