using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	sealed class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
		public void TestShipmentTypeGroupBoxTabOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = "SEA";
			declaration.Invoices.AddNew();

			using (var jobDeclarationForm = new JobDeclarationFormForTest(declaration))
			{
				jobDeclarationForm.Show();
				var jobDeclarationUserControl = (JobDeclarationUserControl)jobDeclarationForm.CustomsBrokerageUserControl.DeclarationUserControl;
				var shipmentTypeGroupBox = jobDeclarationUserControl.ShipmentTypeGroupBox;

				var messageTypeDropEdit = shipmentTypeGroupBox.FindSingle<ZDropEdit>("MessageTypeDropEdit");
				AssertEquals(messageTypeDropEdit.CharacterCasing, CharacterCasing.Upper);

				var transportModeDropEdit = shipmentTypeGroupBox.FindSingle<ZDropEdit>("TransportModeDropEdit");
				AssertEquals(transportModeDropEdit, shipmentTypeGroupBox.GetNextControl(messageTypeDropEdit, true));
				AssertEquals(transportModeDropEdit.CharacterCasing, CharacterCasing.Upper);

				var containerModeDropEdit = shipmentTypeGroupBox.FindSingle<ZDropEdit>("ContainerModeDropEdit");
				AssertEquals(containerModeDropEdit, shipmentTypeGroupBox.GetNextControl(transportModeDropEdit, true));
				AssertEquals(containerModeDropEdit.CharacterCasing, CharacterCasing.Upper);

				var transactionDropEdit = shipmentTypeGroupBox.FindSingle<ZDropEdit>("TransactionDropEdit");
				AssertEquals(transactionDropEdit, shipmentTypeGroupBox.GetNextControl(containerModeDropEdit, true));
				AssertEquals(transactionDropEdit.CharacterCasing, CharacterCasing.Upper);

				var declarationTypeDropEdit = shipmentTypeGroupBox.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");
				AssertEquals(declarationTypeDropEdit, shipmentTypeGroupBox.GetNextControl(transactionDropEdit, true));
				AssertEquals(declarationTypeDropEdit.CharacterCasing, CharacterCasing.Upper);

				var messageSubTypeDropEdit = shipmentTypeGroupBox.FindSingle<ZDropEdit>("MessageSubTypeDropEdit");
				AssertEquals(messageSubTypeDropEdit, shipmentTypeGroupBox.GetNextControl(declarationTypeDropEdit, true));
				AssertEquals(messageSubTypeDropEdit.CharacterCasing, CharacterCasing.Upper);

				var transactionTypeDropEdit = shipmentTypeGroupBox.FindSingle<ZDropEdit>("TransactionTypeDropEdit");
				AssertEquals(transactionTypeDropEdit, shipmentTypeGroupBox.GetNextControl(messageSubTypeDropEdit, true));
				AssertEquals(transactionTypeDropEdit.CharacterCasing, CharacterCasing.Upper);

				var exporterTypeDropEdit = shipmentTypeGroupBox.FindSingle<ZDropEdit>("ExporterTypeDropEdit");
				AssertEquals(exporterTypeDropEdit, shipmentTypeGroupBox.GetNextControl(transactionTypeDropEdit, true));
				AssertEquals(exporterTypeDropEdit.CharacterCasing, CharacterCasing.Upper);

				var paymentTypeDropEdit = shipmentTypeGroupBox.FindSingle<ZDropEdit>("PaymentTypeDropEdit");
				AssertEquals(paymentTypeDropEdit, shipmentTypeGroupBox.GetNextControl(exporterTypeDropEdit, true));
				AssertEquals(paymentTypeDropEdit.CharacterCasing, CharacterCasing.Upper);

				var planTypeDropEdit = shipmentTypeGroupBox.FindSingle<ZDropEdit>("PlanTypeDropEdit");
				AssertEquals(planTypeDropEdit, shipmentTypeGroupBox.GetNextControl(paymentTypeDropEdit, true));
				AssertEquals(planTypeDropEdit.CharacterCasing, CharacterCasing.Upper);

				var importerTypeDropEdit = shipmentTypeGroupBox.FindSingle<ZDropEdit>("ImporterTypeDropEdit");
				AssertEquals(importerTypeDropEdit, shipmentTypeGroupBox.GetNextControl(planTypeDropEdit, true));
				AssertEquals(importerTypeDropEdit.CharacterCasing, CharacterCasing.Upper);

				var serviceLevelCodeFindBox = shipmentTypeGroupBox.FindSingle<ZCodeFindBox>("ServiceLevelCodeFindBox");
				AssertEquals(serviceLevelCodeFindBox, shipmentTypeGroupBox.GetNextControl(importerTypeDropEdit, true));

				var applicationCodeDropEdit = shipmentTypeGroupBox.FindSingle<ZDropEdit>("ApplicationCodeDropEdit");
				AssertEquals(applicationCodeDropEdit, shipmentTypeGroupBox.GetNextControl(serviceLevelCodeFindBox, true));
				AssertEquals(applicationCodeDropEdit.CharacterCasing, CharacterCasing.Upper);
			}
		}

		public void TestDeleteColumn()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.KoreaSouth))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = "SEA";
				declaration.Invoices.AddNew();

				using (var testForm = new JobDeclarationFormForTest(declaration))
				{
					testForm.Show();

					JobDeclarationUserControl jobDeclarationUserControl = (JobDeclarationUserControl)testForm.CustomsBrokerageUserControl.DeclarationUserControl;
					AssertEquals(false, jobDeclarationUserControl.DeclarationDetailsGroupBox.Visible);
					AssertEquals(false, jobDeclarationUserControl.JE_ContainerCountCalcEdit.Visible);
				}
			}
		}

		public void TestSEDDetailsGroupBox_EXP()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = "SEA";
			declaration.Invoices.AddNew();

			using (var jobDeclarationForm = new JobDeclarationFormForTest(declaration))
			{
				jobDeclarationForm.Show();
				var jobDeclarationUserControl = (JobDeclarationUserControl)jobDeclarationForm.CustomsBrokerageUserControl.DeclarationUserControl;
				var sEDDetailsUserControl = jobDeclarationUserControl.FindSingle<ZUserControl>("ExportSEDDetailUserControl");

				var customsOfficeCodeFindBox = sEDDetailsUserControl.FindSingle<ZCodeFindBox>("CustomsOfficeCodeFindBox");
				AssertEquals(true, customsOfficeCodeFindBox.Visible);

				var customsDivisionCodeFindBox = sEDDetailsUserControl.FindSingle<ZCodeFindBox>("CustomsDivisionCodeFindBox");
				AssertEquals(true, customsDivisionCodeFindBox.Visible);
				AssertEquals(customsDivisionCodeFindBox, sEDDetailsUserControl.GetNextControl(customsOfficeCodeFindBox, true));

				var goodsDestinationCodeFindBox = sEDDetailsUserControl.FindSingle<ZCodeFindBox>("GoodsDestinationCodeFindBox");
				AssertEquals(true, goodsDestinationCodeFindBox.Visible);
				AssertEquals(goodsDestinationCodeFindBox, sEDDetailsUserControl.GetNextControl(customsDivisionCodeFindBox, true));

				var inspectionDateEdit = sEDDetailsUserControl.FindSingle<ZDateEdit>("InspectionDateEdit");
				AssertEquals(true, inspectionDateEdit.Visible);
				AssertEquals(inspectionDateEdit, sEDDetailsUserControl.GetNextControl(goodsDestinationCodeFindBox, true));

				var simpleDRWAppDropEdit = sEDDetailsUserControl.FindSingle<ZDropEdit>("SimpleDRWAppDropEdit");
				AssertEquals(true, simpleDRWAppDropEdit.Visible);
				AssertEquals(simpleDRWAppDropEdit, sEDDetailsUserControl.GetNextControl(inspectionDateEdit, true));
				AssertEquals(simpleDRWAppDropEdit.CharacterCasing, CharacterCasing.Upper);

				var containerPackModeDropEdit = sEDDetailsUserControl.FindSingle<ZDropEdit>("ContainerPackModeDropEdit");
				AssertEquals(true, containerPackModeDropEdit.Visible);
				AssertEquals(containerPackModeDropEdit, sEDDetailsUserControl.GetNextControl(simpleDRWAppDropEdit, true));
				AssertEquals(containerPackModeDropEdit.CharacterCasing, CharacterCasing.Upper);

				var goodsConditionDropEdit = sEDDetailsUserControl.FindSingle<ZDropEdit>("GoodsConditionDropEdit");
				AssertEquals(true, goodsConditionDropEdit.Visible);
				AssertEquals(goodsConditionDropEdit, sEDDetailsUserControl.GetNextControl(containerPackModeDropEdit, true));
				AssertEquals(goodsConditionDropEdit.CharacterCasing, CharacterCasing.Upper);

				var bondedAreaCodeFindtBox = sEDDetailsUserControl.FindSingle<ZCodeFindBox>("BondedAreaCodeFindtBox");
				AssertEquals(true, bondedAreaCodeFindtBox.Visible);
				AssertEquals(bondedAreaCodeFindtBox, sEDDetailsUserControl.GetNextControl(goodsConditionDropEdit, true));

				var locationIDInBondedAreaTextBox = sEDDetailsUserControl.FindSingle<ZTextBox>("LocationIDInBondedAreaTextBox");
				AssertEquals(true, locationIDInBondedAreaTextBox.Visible);
				AssertEquals(locationIDInBondedAreaTextBox, sEDDetailsUserControl.GetNextControl(bondedAreaCodeFindtBox, true));

				var subLocationOfGoodsTextBox = sEDDetailsUserControl.FindSingle<ZTextBox>("SubLocationOfGoodsTextBox");
				AssertEquals(true, subLocationOfGoodsTextBox.Visible);
				AssertEquals(subLocationOfGoodsTextBox, sEDDetailsUserControl.GetNextControl(locationIDInBondedAreaTextBox, true));

				var locationOfGoodsTextBox = sEDDetailsUserControl.FindSingle<ZTextBox>("LocationOfGoodsTextBox");
				AssertEquals(true, locationOfGoodsTextBox.Visible);
				AssertEquals(locationOfGoodsTextBox, sEDDetailsUserControl.GetNextControl(subLocationOfGoodsTextBox, true));

				var locationQualiferTextBox = sEDDetailsUserControl.FindSingle<ZTextBox>("LocationQualifierTextBox");
				AssertEquals(true, locationQualiferTextBox.Visible);
				AssertEquals(locationQualiferTextBox, sEDDetailsUserControl.GetNextControl(locationOfGoodsTextBox, true));
			}
		}

		public void TestSEDDetailsGroupBoxDisplay_EXP()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = "SEA";
			declaration.Invoices.AddNew();

			using (var jobDeclarationForm = new JobDeclarationFormForTest(declaration))
			{
				jobDeclarationForm.Show();
				var jobDeclarationUserControl = (JobDeclarationUserControl)jobDeclarationForm.CustomsBrokerageUserControl.DeclarationUserControl;
				var sEDDetailsUserControl = jobDeclarationUserControl.FindSingle<ZUserControl>("ExportSEDDetailUserControl");

				AssertEquals(true, sEDDetailsUserControl.FindSingle<ZDateEdit>("InspectionDateEdit").Visible);

				declaration.JE_MessageSubType = ExportTypeCodeList.Codes.G;

				AssertEquals(false, sEDDetailsUserControl.FindSingle<ZDateEdit>("InspectionDateEdit").Visible);

				declaration.JE_MessageSubType = ExportTypeCodeList.Codes.A;

				AssertEquals(true, sEDDetailsUserControl.FindSingle<ZDateEdit>("InspectionDateEdit").Visible);
			}
		}

		public void TestSEDDetailsGroupBox_LEX()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
			using (var jobDeclarationForm = new JobDeclarationFormForTest(declaration))
			{
				jobDeclarationForm.Show();
				var jobDeclarationUserControl = (JobDeclarationUserControl)jobDeclarationForm.CustomsBrokerageUserControl.DeclarationUserControl;
				var sEDDetailsUserControl = jobDeclarationUserControl.FindSingle<ZUserControl>("LocalExportSEDDetailUserControl");
				var mainPanel = sEDDetailsUserControl.FindSingle<DynamicLayoutPanel>("MainPanel");
				AssertEquals(true, mainPanel.Visible);

				var customsOfficeCodeFindBox = mainPanel.FindSingle<ZCodeFindBox>("CustomsOfficeCodeFindBox");
				AssertEquals(true, customsOfficeCodeFindBox.Visible);

				var customsDivisionCodeFindBox = mainPanel.FindSingle<ZCodeFindBox>("CustomsDivisionCodeFindBox");
				AssertEquals(true, customsDivisionCodeFindBox.Visible);
				AssertEquals(customsDivisionCodeFindBox, mainPanel.GetNextControl(customsOfficeCodeFindBox, true));

				var bondedAreaCodeFindBox = mainPanel.FindSingle<ZCodeFindBox>("BondedAreaCodeFindBox");
				AssertEquals(true, bondedAreaCodeFindBox.Visible);
				AssertEquals(customsDivisionCodeFindBox, mainPanel.GetNextControl(customsOfficeCodeFindBox, true));

				var crewCountCalEdit = mainPanel.FindSingle<ZCalcEdit>("CrewCountCalcEdit");
				AssertEquals(false, crewCountCalEdit.Visible);
				AssertEquals(crewCountCalEdit, mainPanel.GetNextControl(bondedAreaCodeFindBox, true));

				var subLocationOfGoodsTextBox = mainPanel.FindSingle<ZTextBox>("SubLocationOfGoodsTextBox");
				AssertEquals(true, subLocationOfGoodsTextBox.Visible);
				AssertEquals(subLocationOfGoodsTextBox, mainPanel.GetNextControl(crewCountCalEdit, true));

				var blanketDeclarationDropEdit = mainPanel.FindSingle<ZDropEdit>("BlanketDeclarationDropEdit");
				AssertEquals(false, blanketDeclarationDropEdit.Visible);
				AssertEquals(blanketDeclarationDropEdit, mainPanel.GetNextControl(subLocationOfGoodsTextBox, true));

				var declarationDateEdit = mainPanel.FindSingle<ZDateEdit>("DeclarationDateEdit");
				AssertEquals(false, declarationDateEdit.Visible);
				AssertEquals(declarationDateEdit, mainPanel.GetNextControl(blanketDeclarationDropEdit, true));

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
				AssertLocalExportSEDDetailsVisibleByDeclarationType(mainPanel, true, true, false);
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._02;
				AssertLocalExportSEDDetailsVisibleByDeclarationType(mainPanel, true, true, false);
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._03;
				AssertLocalExportSEDDetailsVisibleByDeclarationType(mainPanel, true, true, false);
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._04;
				AssertLocalExportSEDDetailsVisibleByDeclarationType(mainPanel, true, true, false);
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._06;
				AssertLocalExportSEDDetailsVisibleByDeclarationType(mainPanel, true, true, false);
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
				AssertLocalExportSEDDetailsVisibleByDeclarationType(mainPanel, false, true, false);

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
				AssertLocalExportSEDDetailsVisibleByDeclarationType(mainPanel, false, false, true);
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
				AssertLocalExportSEDDetailsVisibleByDeclarationType(mainPanel, false, false, true);
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._17;
				AssertLocalExportSEDDetailsVisibleByDeclarationType(mainPanel, false, false, true);
				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._18;
				AssertLocalExportSEDDetailsVisibleByDeclarationType(mainPanel, false, false, false);
			}
		}
		void AssertLocalExportSEDDetailsVisibleByDeclarationType(DynamicLayoutPanel mainPanel, bool isBlanketDeclaration, bool isDeclarationDateEdit, bool isCrewCount)
		{
			var blanketDeclarationDropEdit = mainPanel.FindSingle<ZDropEdit>("BlanketDeclarationDropEdit");
			AssertEquals(isBlanketDeclaration, blanketDeclarationDropEdit.Visible);

			var declarationDateEdit = mainPanel.FindSingle<ZDateEdit>("DeclarationDateEdit");
			AssertEquals(isDeclarationDateEdit, declarationDateEdit.Visible);

			var crewCountCalcEdit = mainPanel.FindSingle<ZCalcEdit>("CrewCountCalcEdit");
			AssertEquals(isCrewCount, crewCountCalcEdit.Visible);
		}

		public void TestCaptionsWhenChangingShipmentType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			using (var jobDeclarationForm = new JobDeclarationFormForTest(declaration))
			{
				jobDeclarationForm.Show();
				var jobDeclarationUserControl = (JobDeclarationUserControl)jobDeclarationForm.CustomsBrokerageUserControl.DeclarationUserControl;
				var shipmentTypeGroupBox = jobDeclarationUserControl.ShipmentTypeGroupBox;

				declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
				AssertEquals("Transaction Type", shipmentTypeGroupBox.FindSingle<ZDropEdit>("TransactionTypeDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Export Type", shipmentTypeGroupBox.FindSingle<ZDropEdit>("MessageSubTypeDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);

				declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
				AssertEquals("Transaction Type", shipmentTypeGroupBox.FindSingle<ZDropEdit>("TransactionTypeDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Declaration Type", shipmentTypeGroupBox.FindSingle<ZDropEdit>("MessageSubTypeDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);

				declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
				AssertEquals("Goods Type", shipmentTypeGroupBox.FindSingle<ZDropEdit>("TransactionTypeDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Declaration Type", shipmentTypeGroupBox.FindSingle<ZDropEdit>("MessageSubTypeDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestTransportDetails_LEX()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			using (var jobDeclarationForm = new JobDeclarationFormForTest(declaration))
			{
				jobDeclarationForm.Show();
				var jobDeclarationUserControl = (JobDeclarationUserControl)jobDeclarationForm.CustomsBrokerageUserControl.DeclarationUserControl;
				var transportDetailsLayoutPanel = jobDeclarationUserControl.FindSingle<DynamicLayoutPanel>("TransportDetailsLayoutPanel");

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
				AssertAllFalseOfLocalExportTransportDetailsVisibility(transportDetailsLayoutPanel);

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._02;
				AssertAllFalseOfLocalExportTransportDetailsVisibility(transportDetailsLayoutPanel);

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._03;
				AssertAllFalseOfLocalExportTransportDetailsVisibility(transportDetailsLayoutPanel);

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._04;
				AssertAllFalseOfLocalExportTransportDetailsVisibility(transportDetailsLayoutPanel);

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._06;
				AssertAllFalseOfLocalExportTransportDetailsVisibility(transportDetailsLayoutPanel);

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
				AssertEquals(6, transportDetailsLayoutPanel.Controls.Count);
				AssertEquals(false, transportDetailsLayoutPanel.FindSingle<ZCheckBox>("OverrideValuesCheckBox").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZTextBox>("RadioCallSignTextBox").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZDropEdit>("MRNTypeDropEdit").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZTextBox>("MRNNumberTextBox").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZCalcEdit>("VoyageDurationCalcEdit").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZCodeFindBox>("VesselCodeFindBox").Visible);

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
				AssertEquals(2, transportDetailsLayoutPanel.Controls.Count);
				AssertEquals(false, transportDetailsLayoutPanel.FindSingle<ZCheckBox>("OverrideValuesCheckBox").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<TransportDetailsFlightUserControl>("FlightUserControl").Visible);

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
				AssertEquals(6, transportDetailsLayoutPanel.Controls.Count);
				AssertEquals(false, transportDetailsLayoutPanel.FindSingle<ZCheckBox>("OverrideValuesCheckBox").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZTextBox>("RadioCallSignTextBox").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZDropEdit>("MRNTypeDropEdit").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZTextBox>("MRNNumberTextBox").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZCalcEdit>("VoyageDurationCalcEdit").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZCodeFindBox>("VesselCodeFindBox").Visible);

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._17;
				AssertEquals(6, transportDetailsLayoutPanel.Controls.Count);
				AssertEquals(false, transportDetailsLayoutPanel.FindSingle<ZCheckBox>("OverrideValuesCheckBox").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZTextBox>("RadioCallSignTextBox").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZDropEdit>("MRNTypeDropEdit").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZTextBox>("MRNNumberTextBox").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZCalcEdit>("VoyageDurationCalcEdit").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<ZCodeFindBox>("VesselCodeFindBox").Visible);

				declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._18;
				AssertEquals(2, transportDetailsLayoutPanel.Controls.Count);
				AssertEquals(false, transportDetailsLayoutPanel.FindSingle<ZCheckBox>("OverrideValuesCheckBox").Visible);
				AssertEquals(true, transportDetailsLayoutPanel.FindSingle<TransportDetailsFlightUserControl>("FlightUserControl").Visible);
			}

			void AssertAllFalseOfLocalExportTransportDetailsVisibility(DynamicLayoutPanel transportDetailsLayoutPanel)
			{
				AssertEquals(1, transportDetailsLayoutPanel.Controls.Count);
				AssertEquals(false, transportDetailsLayoutPanel.FindSingle<ZCheckBox>("OverrideValuesCheckBox").Visible);
			}
		}

		public void TestShipmentDetailsGroupBox_LEX()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;

			using (var jobDeclarationForm = new JobDeclarationFormForTest(declaration))
			{
				jobDeclarationForm.Show();
				var jobDeclarationUserControl = (JobDeclarationUserControl)jobDeclarationForm.CustomsBrokerageUserControl.DeclarationUserControl;
				var shipmentDetailsGroupBox = jobDeclarationUserControl.ShipmentDetailsGroupBox;

				var goodsDescriptionTextBox = shipmentDetailsGroupBox.FindSingle<ZTextBox>("GoodsDescriptionTextBox");
				AssertEquals(true, goodsDescriptionTextBox.Visible);

				var ownersReferenceTextBox = shipmentDetailsGroupBox.FindSingle<ZTextBox>("OwnersReferenceTextBox");
				AssertEquals(true, ownersReferenceTextBox.Visible);
				AssertEquals(ownersReferenceTextBox, shipmentDetailsGroupBox.GetNextControl(goodsDescriptionTextBox, true));

				var shipmentDetailsWeightCalcDropEdit = shipmentDetailsGroupBox.FindSingle<ZCalcDropEdit>("WeightCalcDropEdit");
				AssertEquals(true, shipmentDetailsWeightCalcDropEdit.Visible);
				AssertEquals(shipmentDetailsWeightCalcDropEdit, shipmentDetailsGroupBox.GetNextControl(ownersReferenceTextBox, true));

				var shipmentDetailsVolumeCalcDropEdit = shipmentDetailsGroupBox.FindSingle<ZCalcDropEdit>("VolumeCalcDropEdit");
				AssertEquals(true, shipmentDetailsVolumeCalcDropEdit.Visible);
				AssertEquals(shipmentDetailsVolumeCalcDropEdit, shipmentDetailsGroupBox.GetNextControl(shipmentDetailsWeightCalcDropEdit, true));

				var totalNoOfPacksCalcDropEdit = shipmentDetailsGroupBox.FindSingle<ZCalcDropEdit>("TotalNoOfPacksCalcDropEdit");
				AssertEquals(true, totalNoOfPacksCalcDropEdit.Visible);
				AssertEquals(totalNoOfPacksCalcDropEdit, shipmentDetailsGroupBox.GetNextControl(shipmentDetailsVolumeCalcDropEdit, true));

				var shipmentDetailsIncoTermsUserControl = shipmentDetailsGroupBox.FindSingle<ZUserControl>("ShipmentDetailsIncoTermsUserControl");
				AssertEquals(true, shipmentDetailsIncoTermsUserControl.Visible);
				AssertEquals(shipmentDetailsIncoTermsUserControl, shipmentDetailsGroupBox.GetNextControl(totalNoOfPacksCalcDropEdit, true));

				var shipmentDetailsScreeningUserControl = shipmentDetailsGroupBox.FindSingle<ZUserControl>("ShipmentDetailsScreeningUserControl");
				AssertEquals(true, shipmentDetailsScreeningUserControl.Visible);
				AssertEquals(shipmentDetailsScreeningUserControl, shipmentDetailsGroupBox.GetNextControl(shipmentDetailsIncoTermsUserControl, true));
				Assert(!shipmentDetailsScreeningUserControl.TabStop);
			}
		}

		public void TestShipmentDetailsGroupBox_IMP()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			using (var jobDeclarationForm = new JobDeclarationFormForTest(declaration))
			{
				jobDeclarationForm.Show();
				var jobDeclarationUserControl = (JobDeclarationUserControl)jobDeclarationForm.CustomsBrokerageUserControl.DeclarationUserControl;
				var shipmentDetailsGroupBox = jobDeclarationUserControl.ShipmentDetailsGroupBox;

				var houseBillParcelPostTextBox = shipmentDetailsGroupBox.FindSingle<ZTextBox>("HouseBillParcelPostTextBox");
				AssertEquals(true, houseBillParcelPostTextBox.Visible);

				var shipmentDetailsOriginCodeFindBox = shipmentDetailsGroupBox.FindSingle<ZCodeFindBox>("OriginCodeFindBox");
				AssertEquals(true, shipmentDetailsOriginCodeFindBox.Visible);
				AssertEquals(shipmentDetailsOriginCodeFindBox, shipmentDetailsGroupBox.GetNextControl(houseBillParcelPostTextBox, true));

				var shipmentDetailsEstimatedDepartureDateEdit = shipmentDetailsGroupBox.FindSingle<ZDateEdit>("EstimatedDepartureDateEdit");
				AssertEquals(true, shipmentDetailsEstimatedDepartureDateEdit.Visible);
				AssertEquals(shipmentDetailsEstimatedDepartureDateEdit, shipmentDetailsGroupBox.GetNextControl(shipmentDetailsOriginCodeFindBox, true));

				var shipmentDetailsFinalDestinationCodeFindBox = shipmentDetailsGroupBox.FindSingle<ZCodeFindBox>("FinalDestinationCodeFindBox");
				AssertEquals(true, shipmentDetailsFinalDestinationCodeFindBox.Visible);
				AssertEquals(shipmentDetailsFinalDestinationCodeFindBox, shipmentDetailsGroupBox.GetNextControl(shipmentDetailsEstimatedDepartureDateEdit, true));

				var shipmentDetailsEstimatedArrivalDateEdit = shipmentDetailsGroupBox.FindSingle<ZDateEdit>("EstimatedArrivalDateEdit");
				AssertEquals(true, shipmentDetailsEstimatedArrivalDateEdit.Visible);
				AssertEquals(shipmentDetailsEstimatedArrivalDateEdit, shipmentDetailsGroupBox.GetNextControl(shipmentDetailsFinalDestinationCodeFindBox, true));

				var goodsDescriptionTextBox = shipmentDetailsGroupBox.FindSingle<ZTextBox>("GoodsDescriptionTextBox");
				AssertEquals(true, goodsDescriptionTextBox.Visible);
				AssertEquals(goodsDescriptionTextBox, shipmentDetailsGroupBox.GetNextControl(shipmentDetailsEstimatedArrivalDateEdit, true));

				var ownersReferenceTextBox = shipmentDetailsGroupBox.FindSingle<ZTextBox>("OwnersReferenceTextBox");
				AssertEquals(true, ownersReferenceTextBox.Visible);
				AssertEquals(ownersReferenceTextBox, shipmentDetailsGroupBox.GetNextControl(goodsDescriptionTextBox, true));

				var shipmentDetailsWeightCalcDropEdit = shipmentDetailsGroupBox.FindSingle<ZCalcDropEdit>("WeightCalcDropEdit");
				AssertEquals(true, shipmentDetailsWeightCalcDropEdit.Visible);
				AssertEquals(shipmentDetailsWeightCalcDropEdit, shipmentDetailsGroupBox.GetNextControl(ownersReferenceTextBox, true));

				var shipmentDetailsVolumeCalcDropEdit = shipmentDetailsGroupBox.FindSingle<ZCalcDropEdit>("VolumeCalcDropEdit");
				AssertEquals(true, shipmentDetailsVolumeCalcDropEdit.Visible);
				AssertEquals(shipmentDetailsVolumeCalcDropEdit, shipmentDetailsGroupBox.GetNextControl(shipmentDetailsWeightCalcDropEdit, true));

				var totalNoOfPacksCalcDropEdit = shipmentDetailsGroupBox.FindSingle<ZCalcDropEdit>("TotalNoOfPacksCalcDropEdit");
				AssertEquals(true, totalNoOfPacksCalcDropEdit.Visible);
				AssertEquals(totalNoOfPacksCalcDropEdit, shipmentDetailsGroupBox.GetNextControl(shipmentDetailsVolumeCalcDropEdit, true));

				var containerCountCalcEdit = shipmentDetailsGroupBox.FindSingle<ZCalcEdit>("ContainerCountCalcEdit");
				AssertEquals(false, containerCountCalcEdit.Visible);
				AssertEquals(containerCountCalcEdit, shipmentDetailsGroupBox.GetNextControl(totalNoOfPacksCalcDropEdit, true));

				var shipmentDetailsIncoTermsUserControl = shipmentDetailsGroupBox.FindSingle<ZUserControl>("ShipmentDetailsIncoTermsUserControl");
				AssertEquals(true, shipmentDetailsIncoTermsUserControl.Visible);
				AssertEquals(shipmentDetailsIncoTermsUserControl, shipmentDetailsGroupBox.GetNextControl(containerCountCalcEdit, true));

				var shipmentDetailsScreeningUserControl = shipmentDetailsGroupBox.FindSingle<ZUserControl>("ShipmentDetailsScreeningUserControl");
				AssertEquals(true, shipmentDetailsScreeningUserControl.Visible);
				AssertEquals(shipmentDetailsScreeningUserControl, shipmentDetailsGroupBox.GetNextControl(shipmentDetailsIncoTermsUserControl, true));
				Assert(!shipmentDetailsScreeningUserControl.TabStop);
			}
		}

		public void TestGetAddressFormatted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			declaration.JE_OA_SupplierAddress = Factory.New<OrgAddress>().PK;
			declaration.JE_OA_ImporterAddress = Factory.New<OrgAddress>().PK;
			using (var jobDeclarationUserControl = new JobDeclarationUserControl())
			{
				AssertEquals(typeof(KRAddressFormatter), jobDeclarationUserControl.SupplierOrganisationControl.OrgAddressFormatter.Invoke(Factory, declaration.SupplierAddress).GetType());
				AssertEquals(typeof(KRAddressFormatter), jobDeclarationUserControl.ImporterOrganisationControl.OrgAddressFormatter.Invoke(Factory, declaration.ImporterAddress).GetType());
			}
		}

		public void TestDeclarationUserControlTabIndex_IMP()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			using (var jobDeclarationForm = new JobDeclarationFormForTest(declaration))
			{
				jobDeclarationForm.Show();
				var jobDeclarationUserControl = (JobDeclarationUserControl)jobDeclarationForm.CustomsBrokerageUserControl.DeclarationUserControl;
				var supplierOrganisationControl = jobDeclarationUserControl.SupplierOrganisationControl;
				AssertEquals(0, supplierOrganisationControl.TabIndex);

				var importerOrganisationControl = jobDeclarationUserControl.ImporterOrganisationControl;
				AssertEquals(1, importerOrganisationControl.TabIndex);

				var shipmentTypeGroupBox = jobDeclarationUserControl.ShipmentTypeGroupBox;
				AssertEquals(2, shipmentTypeGroupBox.TabIndex);

				var declarationDetailsGroupBox = jobDeclarationUserControl.DeclarationDetailsGroupBox;
				AssertEquals("When it is IMP type, it is not displayed on the screen.", 3, declarationDetailsGroupBox.TabIndex);

				var transportDetailsGroupBox = jobDeclarationUserControl.TransportDetailsGroupBox;
				AssertEquals(4, transportDetailsGroupBox.TabIndex);

				var shipmentDetailsGroupBox = jobDeclarationUserControl.ShipmentDetailsGroupBox;
				AssertEquals(5, shipmentDetailsGroupBox.TabIndex);

				var sedDetailsPanel = jobDeclarationUserControl.FindSingle<DynamicLayoutPanel>("SEDDetailsPanel");
				AssertEquals("When it is IMP type, it is not displayed on the screen.", 6, sedDetailsPanel.TabIndex);

				var customsDetailsGroupBox = jobDeclarationUserControl.FindSingle<ZGroupBox>("CustomsDetailsGroupBox");
				AssertEquals(7, customsDetailsGroupBox.TabIndex);

				var rightTabControl = jobDeclarationUserControl.RightTabControl;
				AssertEquals(8, rightTabControl.TabIndex);
			}
		}

		public void TestOverrideValuesCheckBoxVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertOverrideValuesCheckBoxVisible(declaration, false);
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertOverrideValuesCheckBoxVisible(declaration, false);
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertOverrideValuesCheckBoxVisible(declaration, false);

			var shipment = Factory.New<ForwardingShipment>();
			var declaration_AttachedShipment = Factory.New<JobDeclaration>();
			declaration_AttachedShipment.JE_JS = shipment.BookingParentPK;
			declaration_AttachedShipment.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertOverrideValuesCheckBoxVisible(declaration_AttachedShipment, true);
			declaration_AttachedShipment.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertOverrideValuesCheckBoxVisible(declaration_AttachedShipment, true);
			declaration_AttachedShipment.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertOverrideValuesCheckBoxVisible(declaration_AttachedShipment, true);

			void AssertOverrideValuesCheckBoxVisible(JobDeclaration declaration, bool isVisible)
			{
				using (var jobDeclarationForm = new JobDeclarationFormForTest(declaration))
				{
					jobDeclarationForm.Show();

					var jobDeclarationUserControl = (JobDeclarationUserControl)jobDeclarationForm.CustomsBrokerageUserControl.DeclarationUserControl;
					var transportDetailsLayoutPanel = jobDeclarationUserControl.FindSingle<DynamicLayoutPanel>("TransportDetailsLayoutPanel");
					var overrideValuesCheckBox = transportDetailsLayoutPanel.FindSingle<ZCheckBox>("OverrideValuesCheckBox");
					AssertEquals(isVisible, overrideValuesCheckBox.Visible);
				}
			}
		}
	}
}
