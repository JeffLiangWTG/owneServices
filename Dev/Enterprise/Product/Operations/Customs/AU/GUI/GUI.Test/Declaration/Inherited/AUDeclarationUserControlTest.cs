using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(AUDeclarationUserControl))]
	sealed class AUDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<AUDeclarationUserControl, JobDeclaration>
	{
		public void TestCustomsProcessingTransmitDateVisibility()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Prerequisite: JE_MessageType = 'IMP'", JobMessageTypeList.Codes.Import, testDec.JE_MessageType);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			using (var testForm = new AUCustomsDeclarationFormForTest(testDec))
			{
				testForm.Show();
				var decUserControl = testForm.DeclarationUserControl;
				AssertEquals("customsProcessingUserControl", false, decUserControl.customsProcessingUserControl.Visible);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				using (var testForm = new AUCustomsDeclarationFormForTest(testDec))
				{
					testForm.Show();
					var decUserControl = testForm.DeclarationUserControl;
					AssertEquals("customsProcessingUserControl", true, decUserControl.customsProcessingUserControl.Visible);
					AssertEquals("transmitDateDateEdit", true, decUserControl.customsProcessingUserControl.transmitDateDateEdit.Visible);
				}

				testDec.JE_MessageType = JobMessageTypeList.Codes.Export;
				using (var testForm = new AUCustomsDeclarationFormForTest(testDec))
				{
					testForm.Show();
					var decUserControl = testForm.DeclarationUserControl;
					AssertEquals("customsProcessingUserControl is only visible when JE_MessageType = 'IMP'", false, decUserControl.customsProcessingUserControl.Visible);
				}
			}
		}

		public void TestControlsVisibilityForDrawback()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(testDec))
			{
				testForm.Show();
				testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl;
				AssertEquals("DrawbackAssessmentMethodDropEdit", true, decUserControl.drawbackAssessmentMethodDropEdit.Visible);
				AssertEquals("DrawbackEDNControl", true, decUserControl.drawbackEDNControl.Visible);
				AssertEquals("GoodsDescriptionTextBox", true, decUserControl.GoodsDescriptionTextBox.Visible);
				AssertEquals("OwnersReferenceTextBox", true, decUserControl.OwnersReferenceTextBox.Visible);
				AssertEquals("AgentReferenceTextBox", true, decUserControl.agentReferenceTextBox.Visible);
				AssertEquals("SupplierOrganisationControl", false, decUserControl.SupplierOrganisationControl.Visible);
				AssertEquals("JE_TransportModeBoundDropDownEdit", false, decUserControl.JE_TransportModeBoundDropDownEdit.Visible);
				AssertEquals("JE_ContainerModeBoundDropDownEdit", false, decUserControl.JE_ContainerModeBoundDropDownEdit.Visible);
				AssertEquals("CargoStatusGroupBox", false, decUserControl.cargoStatusGroupBox.Visible);
				AssertEquals("TransportDetailsGroupBox", false, decUserControl.TransportDetailsGroupBox.Visible);
				AssertEquals("Importer Caption", "Drawback Claimant", decUserControl.ImporterOrganisationControl.Text);
				foreach (Control control in decUserControl.ShipmentDetailsGroupBox.Controls)
				{
					if (control.Name != "DrawbackAssessmentMethodDropEdit" && control.Name != "DrawbackEDNControl" && control.Name != "GoodsDescriptionTextBox" && control.Name != "OwnersReferenceTextBox" && control.Name != "AgentReferenceTextBox")
					{
						AssertEquals("ShipmentDetailsGroupBoxControl: " + control.Name + " should not be visible", false, control.Visible);
					}
				}
			}
		}

		public void TestControlsVisibilityForSACWithoutLine()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(testDec))
			{
				testForm.Show();
				testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
				TestControlVisibilityForSAC(testForm, testDec);
			}
		}

		public void TestControlsVisibilityForSACWithLine()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(testDec))
			{
				testForm.Show();
				testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
				TestControlVisibilityForSAC(testForm, testDec);
			}
		}

		public void TestControlVisibilityForActivatedAlternativePartShipmentModel()
		{
			/*
					set Customs -> Australia -> Air Cargo -> Activate Alternative Part Shipment Model registry item on
					test JE_PartShipConsignmentReference field is visible.
						AssertEquals("CustShipNoOverrideCheckBox.Visible ", true, DecUserControl.PartShipConsignRefTextBox.Visible);

					set Customs -> Australia -> Air Cargo -> Activate Alternative Part Shipment Model registry item off
					test JE_PartShipConsignmentReference field is NOT visible.
						AssertEquals("CustShipNoOverrideCheckBox.Visible ", false, DecUserControl.PartShipConsignRefTextBox.Visible);
				*/
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(testDec))
			{
				testForm.Show();
				testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
				testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl;
				AssertEquals("PartShipConsignRefTextBox.Visible as this is an Import Air SAC declaration", true, decUserControl.PartShipConsignRefTextBox.Visible);
				AssertEquals("PartShipConsignRefAlt1TextBox should not be Visible", false, decUserControl.PartShipConsignRefAlt1TextBox.Visible);
				AssertEquals("PartShipConsignRefAlt2TextBox should not be Visible", false, decUserControl.PartShipConsignRefAlt2TextBox.Visible);
			}

			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(testDec))
			{
				testForm.Show();
				testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testDec.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl;
				AssertEquals("PartShipConsignRefAlt1TextBox.Visible when not a SAC entry as the registry is set on && MarksAndNumbers is not visible", true, decUserControl.PartShipConsignRefAlt1TextBox.Visible);
				AssertEquals("tab index", 13, decUserControl.PartShipConsignRefAlt1TextBox.TabIndex);
				AssertEquals("PartShipConsignRefAlt2TextBox should not be Visible", false, decUserControl.PartShipConsignRefAlt2TextBox.Visible);
				AssertEquals("PartShipConsignRefTextBox should not be Visible", false, decUserControl.PartShipConsignRefTextBox.Visible);
			}

			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(testDec))
			{
				testForm.Show();
				testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl;
				AssertEquals("Pre-condition: marks and numbers should be Visible", true, decUserControl.marksAndNumbersTextBox.Visible);
				AssertEquals("PartShipConsignRefAlt1TextBox should not be visible even though not a SAC entry and the registry is set as the MarksAndNumbers field is visible", false, decUserControl.PartShipConsignRefAlt1TextBox.Visible);
				AssertEquals("PartShipConsignRefAlt2TextBox should be Visible as Alt1 field cannot be used due to MarksAndNumbers being visible", true, decUserControl.PartShipConsignRefAlt2TextBox.Visible);
				AssertEquals("PartShipConsignRefTextBox should not be Visible", false, decUserControl.PartShipConsignRefTextBox.Visible);
			}

			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(testDec))
			{
				testForm.Show();
				testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testDec.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl;
				AssertEquals("PartShipConsignRefAlt1TextBox should not be Visible as the registry is set off", false, decUserControl.PartShipConsignRefAlt1TextBox.Visible);
				AssertEquals("PartShipConsignRefAlt2TextBox should not be Visible as the registry is set off", false, decUserControl.PartShipConsignRefAlt2TextBox.Visible);
			}

			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(testDec))
			{
				testForm.Show();
				testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
				testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl;
				AssertEquals("PartShipConsignRefAlt1TextBox should not be Visible", false, decUserControl.PartShipConsignRefAlt1TextBox.Visible);
				AssertEquals("PartShipConsignRefAlt2TextBox should not be Visible", false, decUserControl.PartShipConsignRefAlt2TextBox.Visible);
				AssertEquals("PartShipConsignRefTextBox.Visible as this is an Import Air SAC declaration - is not dependant upon the registry", true, decUserControl.PartShipConsignRefTextBox.Visible);
			}
		}

		public void TestDeliveryDocAddressRelatedControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var importer = OrgHeader.New(Factory);
			importer.OH_IsConsignee = true;
			declaration.ImporterDeliveryAddress.OrganisationPK = importer.PK;
			using (var form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				var decUserControl = form.DeclarationUserControl;
				decUserControl.RightTabControl.SelectedTab = decUserControl.OrganisationsTabPage;
				var separatorTextUserControl = decUserControl.OrganisationsTabPage.FindSingle<SeparatorUserControl>("TSS_SeparatorTextUserControl");
				var deliveryDocAddressControl = decUserControl.OrganisationsTabPage.FindSingle<MasterFiles.GUI.ZDocAddressControl>("DeliveryDocAddressControl");
				AssertNotNull("There should be a TSS_SeparatorTextUserControl on the OrganisationsTabPage.", separatorTextUserControl);
				AssertNotNull("There should be a DeliveryDocAddressControl on the OrganisationsTabPage.", deliveryDocAddressControl);
				var docAddressOrganisationControl = deliveryDocAddressControl.Find(c => c is MasterFiles.GUI.ZOrganisationControl).First() as MasterFiles.GUI.ZOrganisationControl;
				AssertEquals("Should bind to ImporterDeliveryAddress of the JobDeclaration", importer.PK, docAddressOrganisationControl.OrganisationForBinding.PK);
				Assert("Should be visible when import", separatorTextUserControl.Visible);
				Assert("Should be visible when import", deliveryDocAddressControl.Visible);
				var groupBox = deliveryDocAddressControl.FindSingle<ZGroupBox>("GroupBox");
				AssertEquals("Should be empty", string.Empty, groupBox.Text);
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				Assert("Should be invisible when export", !separatorTextUserControl.Visible);
				Assert("Should be invisible when export", !deliveryDocAddressControl.Visible);
			}
		}

		public void TestScreeningControlsVisibility()
		{
			AssertScreeningControlsVisibility(Common.Shared.SharedJobMessageTypeList.Codes.Export, true, false, false);
			AssertScreeningControlsVisibility(Common.Shared.SharedJobMessageTypeList.Codes.Export, false, false, true);
			AssertScreeningControlsVisibility(Common.Shared.SharedJobMessageTypeList.Codes.Export, false, true, false);
			AssertScreeningControlsVisibility(Common.Shared.SharedJobMessageTypeList.Codes.Drawback, true, false,false);
			AssertScreeningControlsVisibility(Common.Shared.SharedJobMessageTypeList.Codes.Drawback, false, false, false);
		}

		public void TestSetRightTabControlSelectTab()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				var decUserControl = form.DeclarationUserControl;
				AssertEquals("When open the declaration form, show Organizations as default.", decUserControl.OrganisationsTabPage, decUserControl.RightTabControl.SelectedTab);
			}
		}

		public void TestConsolidatedCargoStatusColor()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			AssertEquals("isCargoStatusAvailableAndCargoClear", true, declaration.isCargoStatusAvailableAndCargoClear);
			using (AUCustomsDeclarationFormForTest form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				AUDeclarationUserControl decUserControl = form.DeclarationUserControl;
				AssertEquals("ConsolidatedCargoStatusTextBox Colour", System.Drawing.Color.LightGreen, decUserControl.consolidatedCargoStatusTextBox.BackColor);
				declaration.JE_ConsolidatedCargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
				AssertEquals("isCargoStatusAvailableAndCargoClear", false, declaration.isCargoStatusAvailableAndCargoClear);
				AssertEquals("isCargoStatusAvailableAndCargoNotClear", true, declaration.isCargoStatusAvailableAndCargoNotClear);
				AssertEquals("ConsolidatedCargoStatusTextBox Colour", System.Drawing.Color.LightSalmon, decUserControl.consolidatedCargoStatusTextBox.BackColor);
				declaration.JE_ConsolidatedCargoStatus = ZString.Empty;
				AssertEquals("isCargoStatusAvailableAndCargoClear", false, declaration.isCargoStatusAvailableAndCargoClear);
				AssertEquals("isCargoStatusAvailableAndCargoNotClear", false, declaration.isCargoStatusAvailableAndCargoNotClear);
				AssertEquals("ConsolidatedCargoStatusTextBox Colour", System.Drawing.SystemColors.Control, decUserControl.consolidatedCargoStatusTextBox.BackColor);
			}
		}

		public void TestMessageStatusDescriptionTextBoxColor()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var logManager = new Customs.Business.OutstandingAmendmentLogManager(testDec);
			var newLog = logManager.AddANewOutstandingAmendmentLog("TESTTEST");
			AssertEquals("PreCondition:There is an outstanding amendment", true, testDec.HasOutstandingAmendment);
			AssertEquals("PreCondition:There is no outstanding ConsolidatedEntry Change", false, testDec.HasConsolidatedEntryChanges);

			using (var testForm = new AUCustomsDeclarationFormForTest(testDec))
			{
				testForm.Show();
				var decUserControl = testForm.DeclarationUserControl;
				AssertEquals("MessageStatusDescriptionTextBox Colour Pink", System.Drawing.Color.LightSalmon, decUserControl.messageStatusDescriptionTextBox.BackColor);

				newLog.Cancel();
				AssertEquals("PreCondition:There is no outstanding amendment", false, testDec.HasOutstandingAmendment);
				testDec.JE_MessageStatusDescriptionIncludingOustandingAmendmentsInfo.RefreshBinding();
				AssertEquals("MessageStatusDescriptionTextBox Colour Grey", System.Drawing.SystemColors.Control, decUserControl.messageStatusDescriptionTextBox.BackColor);

				var cecLog = testDec.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, ZDateTimeOffset.Now, null);
				AssertEquals("PreCondition:There is an outstanding ConsolidatedEntry Change", true, testDec.HasConsolidatedEntryChanges);
				testDec.JE_MessageStatusDescriptionIncludingOustandingAmendmentsInfo.RefreshBinding();
				AssertEquals("MessageStatusDescriptionTextBox Colour Pink", System.Drawing.Color.LightSalmon, decUserControl.messageStatusDescriptionTextBox.BackColor);

				cecLog.Cancel();
				AssertEquals("PreCondition:There is no ConsolidatedEntry Change", false, testDec.HasConsolidatedEntryChanges);
				testDec.JE_MessageStatusDescriptionIncludingOustandingAmendmentsInfo.RefreshBinding();
				AssertEquals("MessageStatusDescriptionTextBox Colour Grey", System.Drawing.SystemColors.Control, decUserControl.messageStatusDescriptionTextBox.BackColor);
			}
		}

		public void TestDeclarationModes()
		{
			var testDec = Factory.New<JobDeclaration>();
			using (var testForm = new AUCustomsDeclarationFormForTest(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "IMP";
				var decUserControl = testForm.DeclarationUserControl;
				AssertEquals("Importer Caption", "Importer", decUserControl.ImporterOrganisationControl.Text);
				AssertEquals("Export Goods type is not visible", false, decUserControl.JE_ExportGoodsTypeBoundDropDownEdit.Visible);
				AssertEquals("messageStatusDescriptionTextBox is visible", true, decUserControl.messageStatusDescriptionTextBox.Visible);
				AssertEquals("detailsButton is visible", true, decUserControl.detailsButton.Visible);
				testDec.JE_TransportMode = "SEA";
				AssertEquals("Container Count should be visible", true, decUserControl.TotalNoOfPiecesBoundCalcEdit.Visible);
				AssertEquals("Container Mode should be visible", true, decUserControl.ContainerModeBoundDropDownEdit.Visible);
				AssertEquals("Vessel should be visible", true, decUserControl.VesselBoundFindBox.Visible);
				AssertEquals("CustShipNoTextBox", false, decUserControl.custShipNoTextBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox.Visible ", true, decUserControl.custShipNoOverrideCheckBox.Visible);
				AssertEquals("ConsolMasterBillLabel Text", "Ocean Bill", decUserControl.JE_MasterBillForSeaBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				testDec.ZA_CustShipNoOverride_Hidden = true;
				AssertEquals("CustShipNoTextBox", true, decUserControl.custShipNoTextBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox.Visible ", true, decUserControl.custShipNoOverrideCheckBox.Visible);
				testDec.JE_TransportMode = "AIR";
				AssertEquals("Container Count should be visible", true, decUserControl.TotalNoOfPiecesBoundCalcEdit.Visible);
				AssertEquals("Vessel should not be visible", false, decUserControl.VesselBoundFindBox.Visible);
				AssertEquals("CustShipNoTextBox", false, decUserControl.custShipNoTextBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox.Visible ", false, decUserControl.custShipNoOverrideCheckBox.Visible);
				AssertEquals("Voyage/FlightNo should be visible", true, decUserControl.VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Voyage Box should be where the (not visible) Vessel Find Box was", decUserControl.VoyageFlightNoBoundTextBox.Location.X, decUserControl.VesselBoundFindBox.Location.X);
				AssertEquals("ConsolMasterBillLabel Text", "Master Bill", decUserControl.JE_MasterBillForAirBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Port of discharge label for import", "Discharge", decUserControl.PortOfDischargeFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("JE_PortOfFirstArrivalCodeFindBox", true, decUserControl.JE_PortOfFirstArrivalCodeFindBox.Visible);
				AssertEquals("DateOfFirstArrivalDateEdit", true, decUserControl.DateOfFirstArrivalDateEdit.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ExportDate = new ZDateTime(2004, 12, 12);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "EXP";
				testDec.ZA_CustShipNoOverride_Hidden = false;
				testDec.JE_TransportMode = "SEA";
				AssertEquals("Pieces Count should be invisible", false, decUserControl.TotalNoOfPiecesBoundCalcEdit.Visible);
				AssertEquals("Voyage should be visible", true, decUserControl.VesselBoundFindBox.Visible);
				AssertEquals("CustShipNoTextBox", false, decUserControl.custShipNoTextBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox.Visible ", true, decUserControl.custShipNoOverrideCheckBox.Visible);
				AssertEquals("ConsolMasterBillLabel Text", "Ocean Bill", decUserControl.JE_MasterBillForSeaBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("messageStatusDescriptionTextBox is not visible", false, decUserControl.messageStatusDescriptionTextBox.Visible);
				AssertEquals("detailsButton is not visible", false, decUserControl.detailsButton.Visible);
				testDec.ZA_CustShipNoOverride_Hidden = true;
				AssertEquals("CustShipNoTextBox", true, decUserControl.custShipNoTextBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox.Visible ", true, decUserControl.custShipNoOverrideCheckBox.Visible);
				AssertEquals("Export Goods type is visible", true, decUserControl.JE_ExportGoodsTypeBoundDropDownEdit.Visible);
				AssertEquals("Port of discharge label for export", "First Discharge", decUserControl.PortOfDischargeFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("JE_PortOfFirstArrivalCodeFindBox", false, decUserControl.JE_PortOfFirstArrivalCodeFindBox.Visible);
				AssertEquals("DateOfFirstArrivalDateEdit", false, decUserControl.DateOfFirstArrivalDateEdit.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "EXP";
				testDec.JE_TransportMode = "AIR";
				AssertEquals("Container Count should not be visible", false, decUserControl.TotalNoOfPiecesBoundCalcEdit.Visible);
				AssertEquals("Container Mode should not be visible", false, decUserControl.ContainerModeBoundDropDownEdit.Visible);
				AssertEquals("Vessel should not be visible", false, decUserControl.VesselBoundFindBox.Visible);
				AssertEquals("CustShipNoTextBox", false, decUserControl.custShipNoTextBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox.Visible ", false, decUserControl.custShipNoOverrideCheckBox.Visible);
				AssertEquals("ConsolMasterBillLabel Text", "Master Bill", decUserControl.JE_MasterBillForAirBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				testDec.JE_TransportMode = Core.Constants.TransportModes.Mail;
				AssertEquals("Container Count should not be visible", false, decUserControl.TotalNoOfPiecesBoundCalcEdit.Visible);
				AssertEquals("Container Mode should not be visible", false, decUserControl.ContainerModeBoundDropDownEdit.Visible);
				AssertEquals("Vessel should not be visible", false, decUserControl.VesselBoundFindBox.Visible);
				AssertEquals("CustShipNoTextBox", false, decUserControl.custShipNoTextBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox.Visible ", false, decUserControl.custShipNoOverrideCheckBox.Visible);
			}
		}

		public void TestLockingSeaShipmentData()
		{
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Factory.New<JobDeclaration>()))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "EXP";
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AUBrokerageUserControl brokerageControl = testForm.CustomsBrokerageUserControl;
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl;
				brokerageControl.SetDeclarationReadOnly(true);
				AssertVisibleAndReadOnly(decUserControl.MessageTypeBoundDropDownEdit, true);
				AssertVisibleAndReadOnly(decUserControl.TransportModeBoundDropDownEdit, true);
				AssertVisibleAndReadOnly(decUserControl.PortOfLoadingBoundFindBox, true);
				AssertVisibleAndReadOnly(decUserControl.VoyageFlightNoBoundTextBox, true);
				AssertVisibleAndReadOnly(decUserControl.PortOfDischargeBoundFindBox, true);
				//TODO: Uncomment when Geoff Implements IReadOnlyToggleControl
				//				AssertVisibleAndReadOnly(DecUserControl.BondedWarehouseAddress, true);
				//				AssertVisibleAndReadOnly(DecUserControl.ContainerTerminalOperatorAddress, true);
				//AssertVisibleAndReadOnly(DecUserControl.JE_TotalNoOfPacksBoundMeasurementControl, true);
				AssertVisibleAndReadOnly(decUserControl.HouseBillParcelPostBoundTextEdit, true);
				AssertVisibleAndReadOnly(decUserControl.OriginBoundFindBox, true);
				AssertVisibleAndReadOnly(decUserControl.FinalDestinationBoundFindBox, true);
				AssertVisibleAndReadOnly(decUserControl.ExportDeclarationNumberTextBox, true);
				AssertVisibleAndReadOnly(decUserControl.ContainerModeBoundDropDownEdit, true);
				//				AssertVisibleAndReadOnly(DecUserControl.DateOfFirstArrivalBoundDateEdit, true);
				//				AssertVisibleAndReadOnly(DecUserControl.PortOfFirstArrivalBoundFindBox, true);
				AssertVisibleAndReadOnly(decUserControl.DateOfArrivalBoundDateEdit2, true);
				AssertVisibleAndReadOnly(decUserControl.MasterBillForSeaBoundTextBox, true);
				AssertVisibleAndReadOnly(decUserControl.DateOfArrivalBoundDateEdit2, true);
				AssertVisibleAndReadOnly(decUserControl.ExportDateBoundDateEdit2, true);
				brokerageControl.SetDeclarationReadOnly(false);
				AssertVisibleAndReadOnly(decUserControl.ExportDeclarationNumberTextBox, true); // should stay read only
				AssertVisibleAndReadOnly(decUserControl.MessageTypeBoundDropDownEdit, false);
				AssertVisibleAndReadOnly(decUserControl.TransportModeBoundDropDownEdit, false);
				AssertVisibleAndReadOnly(decUserControl.PortOfLoadingBoundFindBox, false);
				AssertVisibleAndReadOnly(decUserControl.VoyageFlightNoBoundTextBox, false);
				//AssertVisibleAndReadOnly(DecUserControl.PortOfFirstArrivalBoundFindBox, false);
				//TODO : Uncomment when Geoff implements interface
				//				AssertVisibleAndReadOnly(DecUserControl.DepotAddress, false);
				//				AssertVisibleAndReadOnly(DecUserControl.ContainerTerminalOperatorAddress, false);
				//AssertVisibleAndReadOnly(DecUserControl.JE_TotalNoOfPacksBoundMeasurementControl, false);
				AssertVisibleAndReadOnly(decUserControl.HouseBillParcelPostBoundTextEdit, false);
				AssertVisibleAndReadOnly(decUserControl.OriginBoundFindBox, false);
				AssertVisibleAndReadOnly(decUserControl.FinalDestinationBoundFindBox, false);
				AssertVisibleAndReadOnly(decUserControl.JE_ContainerCountCalcEdit, false);
				//				AssertVisibleAndReadOnly(DecUserControl.DateOfFirstArrivalBoundDateEdit, false);
				//				AssertVisibleAndReadOnly(DecUserControl.PortOfFirstArrivalBoundFindBox, false);
				AssertVisibleAndReadOnly(decUserControl.DateOfArrivalBoundDateEdit2, false);
				AssertVisibleAndReadOnly(decUserControl.ExportDateBoundDateEdit2, false);
			}
		}

		public void TestSaveButtonsAreNotEnabledOnOpeningExistingRecord()
		{
			ZGuid jobDecPK = CreateJobDecInFactory();
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobDeclaration declaration = newFactory.Load<JobDeclaration>(jobDecPK);
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				Assert("Save Button is currently enabled, and should be disabled", !testForm.oPostingButtonsUserControl.SaveAndCloseButton.Enabled);
			}
		}

		[TestDate(2005, 5, 5)]
		public void TestExWarehouseMoveEnabledStateForEdifice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (AUDeclarationUserControl testUserControl = new AUDeclarationUserControl())
			{
				testUserControl.JobDeclaration = declaration;
				declaration.JE_MessageType = "IMP";
				declaration.JE_ApplicationCode = "LEG";
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertWarehouseControlsState(testUserControl, false);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertWarehouseControlsState(testUserControl, false);
				declaration.JE_MessageType = "EXW";
				AssertWarehouseControlsState(testUserControl, true);
			}
		}

		public void TestContainerModeControlForEdifice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Container drop edit", true, testForm.DeclarationUserControl.ContainerModeBoundDropDownEdit.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Container drop edit", false, testForm.DeclarationUserControl.ContainerModeBoundDropDownEdit.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				AssertEquals("Container drop edit", false, testForm.DeclarationUserControl.ContainerModeBoundDropDownEdit.Visible);
			}
		}

		public void TestContainerModeControlForCMR()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Container drop edit", true, testForm.DeclarationUserControl.ContainerModeBoundDropDownEdit.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Container drop edit", false, testForm.DeclarationUserControl.ContainerModeBoundDropDownEdit.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				AssertEquals("Container drop edit", false, testForm.DeclarationUserControl.ContainerModeBoundDropDownEdit.Visible);
			}
		}

		public void TestVoyageControlsForEdifice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Voyage text box", true, testForm.DeclarationUserControl.JE_VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Folio text box", false, testForm.DeclarationUserControl.FolioNumberTextBox.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Voyage text box", true, testForm.DeclarationUserControl.JE_VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Folio text box", true, testForm.DeclarationUserControl.FolioNumberTextBox.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				AssertEquals("Voyage text box", false, testForm.DeclarationUserControl.JE_VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Folio text box", false, testForm.DeclarationUserControl.FolioNumberTextBox.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Voyage text box", false, testForm.DeclarationUserControl.JE_VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Folio text box", false, testForm.DeclarationUserControl.FolioNumberTextBox.Visible);
			}
		}

		public void TestVoyageControlsForCMR()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Voyage text box", true, testForm.DeclarationUserControl.JE_VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Folio text box", false, testForm.DeclarationUserControl.FolioNumberTextBox.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Voyage text box", true, testForm.DeclarationUserControl.JE_VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Folio text box", true, testForm.DeclarationUserControl.FolioNumberTextBox.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				AssertEquals("Voyage text box", false, testForm.DeclarationUserControl.JE_VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Folio text box", false, testForm.DeclarationUserControl.FolioNumberTextBox.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Other;
				AssertEquals("Voyage text box", false, testForm.DeclarationUserControl.JE_VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Folio text box", false, testForm.DeclarationUserControl.FolioNumberTextBox.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Voyage text box", false, testForm.DeclarationUserControl.JE_VoyageFlightNoBoundTextBox.Visible);
				AssertEquals("Folio text box", false, testForm.DeclarationUserControl.FolioNumberTextBox.Visible);
			}
		}

		public void TestHouseBillControlsForEdificeImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("House bill text box is visible", true, testForm.DeclarationUserControl.HouseBillParcelPostTextEdit.Visible);
			}
		}

		[TestDate(2005, 5, 5)]
		public void TestHouseBillControlsForEdificeExWarehouse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "LEG";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				AssertEquals("House bill text box is visible", true, testForm.DeclarationUserControl.HouseBillParcelPostTextEdit.Visible);
			}
		}

		public void TestHouseBillControlsForCMRExWarehouse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_MessageType = "IMP";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				AssertEquals("House bill text box is visible", false, testForm.DeclarationUserControl.HouseBillParcelPostTextEdit.Visible);
			}
		}

		[TestDate(2005, 5, 5)]
		public void TestMarksAndNumbersForEdificeImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "LEG";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Marks and Numbers text box is visible", true, testForm.DeclarationUserControl.marksAndNumbersTextBox.Visible);
			}
		}

		public void TestMarksAndNumbersControlsForCMRImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				AssertEquals("Marks and Numbers text box is visible", false, testForm.DeclarationUserControl.marksAndNumbersTextBox.Visible);
			}
		}

		public void TestConsolidatedJobIndicator()
		{
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				using (var form = new ZForm())
				using (var control = new AUDeclarationUserControl())
				{
					form.Controls.Add(control);
					form.SetDataBinding(declaration, ".");
					form.Show();
					Assert("ConsolidatedJobIndicator should not display when EnableConsolidatedEntries is No.", !control.ConsolidatedDeclarationAdviceLabel.Visible);
				}

				var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
				using (var form = new ZForm())
				using (var control = new AUDeclarationUserControl())
				{
					form.Controls.Add(control);
					form.SetDataBinding(consolidatedDeclaration.LeadDeclaration, ".");
					form.Show();
					Assert("ConsolidatedJobIndicator should not display when EnableConsolidatedEntries is No.", !control.ConsolidatedDeclarationAdviceLabel.Visible);
				}
			}

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				using (var form = new ZForm())
				using (var control = new AUDeclarationUserControl())
				{
					form.Controls.Add(control);
					form.SetDataBinding(declaration, ".");
					form.Show();
					Assert("ConsolidatedJobIndicator should not display for a regular declaration.", !control.ConsolidatedDeclarationAdviceLabel.Visible);
					AssertEquals("declarationMessageAndWHSTransactionStatusPanel", ControlDpiScalingHelper.ScaleToCurrentDpiY(22), control.declarationMessageAndWHSTransactionStatusPanel.Size.Height);
					AssertEquals("ExportDeclarationNumberBoundTextBox", ControlDpiScalingHelper.NewScaledPoint(100, 20, true), control.ExportDeclarationNumberBoundTextBox.Location);
					AssertEquals("StatusTextBox", ControlDpiScalingHelper.NewScaledPoint(358, 20, true), control.StatusTextBox.Location);
				}

				var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
				using (var form = new ZForm())
				using (var control = new AUDeclarationUserControl())
				{
					form.Controls.Add(control);
					form.SetDataBinding(consolidatedDeclaration.LeadDeclaration, ".");
					form.Show();
					Assert("ConsolidatedJobIndicator should display for a Consolidated Declaration.", control.ConsolidatedDeclarationAdviceLabel.Visible);
					AssertEquals("declarationMessageAndWHSTransactionStatusPanel", ControlDpiScalingHelper.ScaleToCurrentDpiY(20), control.declarationMessageAndWHSTransactionStatusPanel.Size.Height);
					AssertEquals("ExportDeclarationNumberBoundTextBox", ControlDpiScalingHelper.NewScaledPoint(100, 25, true), control.ExportDeclarationNumberBoundTextBox.Location);
					AssertEquals("StatusTextBox", ControlDpiScalingHelper.NewScaledPoint(358, 25, true), control.StatusTextBox.Location);
				}
			}
		}

		[TestDate(2005, 5, 5)]
		public void TestMarksAndNumbersControlsForEdificeExWarehouse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "LEG";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				AssertEquals("Marks and Numbers text box is visible", false, testForm.DeclarationUserControl.marksAndNumbersTextBox.Visible);
			}
		}

		public void TestMarksAndNumbersControlsForCMRExWarehouse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXW";
			declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				AssertEquals("Marks and Numbers text box is visible", true, testForm.DeclarationUserControl.marksAndNumbersTextBox.Visible);
			}
		}

		public void TestMarksAndNumbersControlsInCMRSwitchingBetweenIMPAndEXP()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "CMR";
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				AssertEquals("Marks and Numbers text box is visible", false, testForm.DeclarationUserControl.marksAndNumbersTextBox.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Marks and Numbers text box is visible", true, testForm.DeclarationUserControl.marksAndNumbersTextBox.Visible);
			}
		}

		void TestControlVisibilityForSAC(AUCustomsDeclarationFormForTest testForm, JobDeclaration testDec)
		{
			AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl;
			AssertEquals("JE_VoyageFlightNoBoundTextBox", false, decUserControl.JE_VoyageFlightNoBoundTextBox.Visible);
			AssertEquals("FolioNumberTextBox", false, decUserControl.FolioNumberTextBox.Visible);
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("JE_VoyageFlightNoBoundTextBox", true, decUserControl.JE_VoyageFlightNoBoundTextBox.Visible);
			AssertEquals("FolioNumberTextBox", true, decUserControl.FolioNumberTextBox.Visible);
			AssertEquals("CustShipNoTextBox", false, decUserControl.custShipNoTextBox.Visible);
			AssertEquals("CustShipNoOverrideCheckBox.Visible ", false, decUserControl.custShipNoOverrideCheckBox.Visible);
			AssertEquals("CustShipNoOverrideCheckBox.Visible ", true, decUserControl.PartShipConsignRefTextBox.Visible);
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("JE_VoyageFlightNoBoundTextBox", true, decUserControl.JE_VoyageFlightNoBoundTextBox.Visible);
			AssertEquals("FolioNumberTextBox as this is Sea", false, decUserControl.FolioNumberTextBox.Visible);
			AssertEquals("CustShipNoTextBox", false, decUserControl.custShipNoTextBox.Visible);
			AssertEquals("CustShipNoOverrideCheckBox.Visible ", true, decUserControl.custShipNoOverrideCheckBox.Visible);
			AssertEquals("CustShipNoOverrideCheckBox.Visible ", false, decUserControl.PartShipConsignRefTextBox.Visible);
			AssertEquals("OriginFindBox", false, decUserControl.OriginFindBox.Visible);
			AssertEquals("JE_ExportDateBoundDateEdit2", false, decUserControl.JE_ExportDateBoundDateEdit2.Visible);
			AssertEquals("FinalDestinationFindBox", false, decUserControl.FinalDestinationFindBox.Visible);
			AssertEquals("JE_DateOfArrivalBoundDateEdit2", false, decUserControl.JE_DateOfArrivalBoundDateEdit2.Visible);
			AssertEquals("WeightzCalcDropEdit", true, decUserControl.WeightzCalcDropEdit.Visible);
			AssertEquals("VolumeCalcDropEdit", true, decUserControl.VolumeCalcDropEdit.Visible);
			AssertEquals("JE_TotalNoOfPiecesBoundCalcEdit", false, decUserControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
			AssertEquals("TotalNoOfPacksCalcDropEdit", true, decUserControl.TotalNoOfPacksCalcDropEdit.Visible);
			AssertEquals("IncoTermDropEdit", false, decUserControl.IncoTermDropEdit.Visible);
			AssertEquals("IncoTermExplainButton", false, decUserControl.IncoTermExplainButton.Visible);
			AssertEquals("JE_PortOfFirstArrivalCodeFindBox", false, decUserControl.JE_PortOfFirstArrivalCodeFindBox.Visible);
			AssertEquals("DateOfFirstArrivalDateEdit", false, decUserControl.DateOfFirstArrivalDateEdit.Visible);
		}

		public void TestVesselOverrideVisibility()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(testDec))
			{
				testForm.Show();
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl;
				testDec.JE_MessageType = "IMP";
				testDec.JE_TransportMode = "SEA";
				AssertEquals("Vessel should be visible for SEA", true, decUserControl.VesselBoundFindBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox.Visible ", true, decUserControl.custShipNoOverrideCheckBox.Visible);

				testDec.ZA_CustShipNoOverride_Hidden = true;
				AssertEquals("CustShipNoTextBox", true, decUserControl.custShipNoTextBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox.Visible ", true, decUserControl.custShipNoOverrideCheckBox.Visible);

				testDec.JE_TransportMode = "AIR";
				AssertEquals("Vessel should not be visible for AIR", false, decUserControl.VesselBoundFindBox.Visible);
				AssertEquals("CustShipNoTextBox should not be visible", false, decUserControl.custShipNoTextBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox should not be visible ", false, decUserControl.custShipNoOverrideCheckBox.Visible);

				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_ExportDate = new ZDateTime(2023, 06, 26);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "EXP";
				testDec.ZA_CustShipNoOverride_Hidden = false;
				testDec.JE_TransportMode = "SEA";
				AssertEquals("Vessel should be visible for SEA", true, decUserControl.VesselBoundFindBox.Visible);
				AssertEquals("CustShipNoTextBox should not be visible when not checked", false, decUserControl.custShipNoTextBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox should be visible ", true, decUserControl.custShipNoOverrideCheckBox.Visible);

				testDec.ZA_CustShipNoOverride_Hidden = true;
				AssertEquals("CustShipNoTextBox should now be visible", true, decUserControl.custShipNoTextBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox.Visible ", true, decUserControl.custShipNoOverrideCheckBox.Visible);
				AssertEquals("CustShipNoTextBox should completely cover the Vessel Find Box description (Lloyds No.) component - @ 100% display value", decUserControl.custShipNoTextBox.Size.Width + 232, decUserControl.VesselBoundFindBox.Size.Width);

				testDec.JE_TransportMode = "AIR";
				AssertEquals("Vessel should not be visible", false, decUserControl.VesselBoundFindBox.Visible);
				AssertEquals("CustShipNoTextBox", false, decUserControl.custShipNoTextBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox.Visible ", false, decUserControl.custShipNoOverrideCheckBox.Visible);

				testDec.JE_TransportMode = Core.Constants.TransportModes.Mail;
				AssertEquals("Vessel should not be visible", false, decUserControl.VesselBoundFindBox.Visible);
				AssertEquals("CustShipNoTextBox", false, decUserControl.custShipNoTextBox.Visible);
				AssertEquals("CustShipNoOverrideCheckBox.Visible ", false, decUserControl.custShipNoOverrideCheckBox.Visible);
			}
		}

		void AssertWarehouseControlsState(AUDeclarationUserControl userControl, bool exWarehouseActive)
		{
			AssertControlState(userControl.JE_ExportDateBoundDateEdit2, !exWarehouseActive);
			AssertControlState(userControl.JE_DateOfArrivalBoundDateEdit2, !exWarehouseActive);
			AssertControlState(userControl.SupplierOrganisationControl, !exWarehouseActive);
			AssertControlState(userControl.JE_ContainerModeBoundDropDownEdit, userControl.JE_ContainerModeBoundDropDownEditVisibleForTesting);
			AssertControlState(userControl.OriginFindBox, !exWarehouseActive);
			AssertControlState(userControl.marksAndNumbersTextBox, !exWarehouseActive);
			AssertControlState(userControl.marksAndNumberButton, !exWarehouseActive);
			AssertControlState(userControl.WeightzCalcDropEdit, !exWarehouseActive);
			AssertControlState(userControl.VolumeCalcDropEdit, !exWarehouseActive);
			AssertControlState(userControl.JE_TotalNoOfPiecesBoundCalcEdit, !exWarehouseActive);
			AssertControlState(userControl.TotalNoOfPacksCalcDropEdit, true);
			AssertControlState(userControl.IncoTermDropEdit, !exWarehouseActive);
			AssertControlState(userControl.IncoTermExplainButton, !exWarehouseActive);
			AssertControlState(userControl.JE_VoyageFlightNoBoundTextBox, !exWarehouseActive && (userControl.JobDeclaration.IsSea || userControl.JobDeclaration.IsAir));
			AssertControlState(userControl.FolioNumberTextBox, !exWarehouseActive && userControl.JobDeclaration.IsAir);
			AssertControlState(userControl.PortOfLoadingFindBox, !exWarehouseActive);
			AssertControlState(userControl.JE_ExportDateBoundDateEdit, !exWarehouseActive);
			AssertControlState(userControl.PortOfDischargeBoundFindBox, !exWarehouseActive);
			AssertControlState(userControl.JE_DateOfArrivalBoundDateEdit, !exWarehouseActive);
			AssertControlState(userControl.JE_PortOfFirstArrivalCodeFindBox, !exWarehouseActive);
			AssertControlState(userControl.DateOfFirstArrivalDateEdit, !exWarehouseActive);
		}

		void AssertControlState(ZButton control, bool visible)
		{
			AssertEquals("Enabled", visible, control.Visible);
		}

		void AssertControlState(ZTextBox control, bool visible)
		{
			AssertEquals("Enabled", visible, control.Visible);
		}

		void AssertControlState(ZCalcEdit control, bool visible)
		{
			AssertEquals("Enabled", visible, control.Visible);
		}

		void AssertControlState(UserControl control, bool visible)
		{
			AssertEquals("Enabled", visible, control.Visible);
		}

		void AssertScreeningControlsVisibility(string messageType, bool linkWithShipment, bool enableCompliance, bool expectedToBeVisible)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			if (linkWithShipment)
			{
				var shipment = Factory.New<ForwardingShipment>();
				declaration.JE_JS = shipment.PK;
			}

			var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(enableCompliance);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			using (ObjectFactory.Substitute(featureControlMock.Object))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new AUCustomsDeclarationFormForTest(declaration))
			{
				form.Show();
				var decUserControl = form.DeclarationUserControl;
				AssertEquals("ScreeningStatusDropEdit", expectedToBeVisible, decUserControl.ScreeningStatusDropEdit.Visible);
				AssertEquals("ScreenButton", expectedToBeVisible, decUserControl.ScreenButton.Visible);
			}
		}

		void AssertVisibleAndReadOnly(Control control, bool isExpectedReadOnly)
		{
			AssertEquals(control.Name + " Visible", true, control.Visible);
			AssertEquals(control.Name + " ReadOnly", isExpectedReadOnly, control.GetReadOnly());
		}

		ZGuid CreateJobDecInFactory()
		{
			var helper = new Business.Testing.ZTestHelper(Factory);
			helper.CreateMockInvoiceForCPDecQuestions();
			return helper.Declaration.PK;
		}

		sealed class AUCustomsDeclarationFormForTest : ZAUCustomsDeclarationForm
		{
			public AUCustomsDeclarationFormForTest(JobDeclaration jobDeclaration) : base(jobDeclaration)
			{
			}

			public AUCustomsDeclarationFormForTest() : this(null)
			{
			}

			internal new Core.Forms.ZPostingButtonsUserControl oPostingButtonsUserControl => base.oPostingButtonsUserControl;

			internal AUDeclarationUserControl DeclarationUserControl => (AUDeclarationUserControl)CustomsBrokerageUserControl.DeclarationUserControlForTesting;
		}
	}
}
