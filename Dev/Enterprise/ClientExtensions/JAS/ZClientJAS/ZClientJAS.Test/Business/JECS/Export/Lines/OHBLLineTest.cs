using CargoWise.Types;
using Enterprise.Client.JAS.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class OHBLLineTest : MessageLineTestCase
	{
		public void TestLineIdentifier()
		{
			PopulateShipmentData();
			PopulateHeaderData_V2();
			PopulateFreightInfoData_V2();
			PopulateVesselAndVoyageData_V2();

			Shipment.JS_HouseBillIssueDate = ZDateTime.Today;

			AssertEquals("OHBL3100", new OHBLLine(HouseLevelRecordType.Standard, Consol, HouseBillOfLading).LineIdentifier);
			AssertEquals("COHB3100", new OHBLLine(HouseLevelRecordType.CoLoad, Consol, HouseBillOfLading).LineIdentifier);
			AssertEquals("PSBL3100", new OHBLLine(HouseLevelRecordType.PreShipment, Consol, HouseBillOfLading).LineIdentifier);

			HouseBillOfLadingMock.VerifyAll();
			ShipmentMock.VerifyAll();
			ConsolMock.VerifyAll();
		}

		[TestDate(2002, 12, 13)]
		public void TestLineAsString()
		{
			PopulateShipmentData();
			PopulateHeaderData_V1();
			PopulateFreightInfoData_V1();
			PopulateVesselAndVoyageData_V1();
			PopulateDeliveryAgentData_V1();
			PopulateDateAndPlaceOfIssue();
			PopulateNoOfOriginalBillOfLadings();

			AssertEquals(ExpectedLineAsString, Line.LineAsString);

			HouseBillOfLadingMock.VerifyAll();
			ShipmentMock.VerifyAll();
			ConsolMock.VerifyAll();
		}

		public void TestLineAsStringDoesntBlowUpWhenConsolDoesNotExist()
		{
			PopulateShipmentData();
			PopulateHeaderData_V1();
			PopulateFreightInfoData_V1();
			PopulateVesselAndVoyageData_V1();
			PopulateDeliveryAgentData_V1();
			PopulateDateAndPlaceOfIssue();
			PopulateNoOfOriginalBillOfLadings();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			Shipment.Consols.RemoveAll();
			AssertNotNull(Line.LineAsString);

			HouseBillOfLadingMock.VerifyAll();
			ShipmentMock.VerifyAll();
			ConsolMock.VerifyAll();
		}

		[TestDate(2002, 12, 13)]
		public void TestLineAsStringWhenReceivingAgentDoesNotExist()
		{
			IsReceivingAgentNull = true;
			PopulateShipmentData();
			PopulateHeaderData_V1();
			PopulateFreightInfoData_V1();
			PopulateVesselAndVoyageData_V1();
			PopulateDeliveryAgentData_V1();
			PopulateDateAndPlaceOfIssue();
			PopulateNoOfOriginalBillOfLadings();

			AssertEquals(ExpectedLineAsStringWhenReceivingAgentDoesNotExist, Line.LineAsString);

			HouseBillOfLadingMock.VerifyAll();
			ShipmentMock.VerifyAll();
			ConsolMock.VerifyAll();
		}

		[TestDate(2002, 12, 13)]
		public void TestLineAsStringWhenFieldsExceedMaxLength()
		{
			ExceedMaxLength = true;
			PopulateShipmentData();
			PopulateHeaderData_V1();
			PopulateFreightInfoData_V1();
			PopulateVesselAndVoyageData_V1();
			PopulateDeliveryAgentData_V2();
			PopulateDateAndPlaceOfIssue();
			PopulateNoOfOriginalBillOfLadings();

			AssertEquals(ExpectedLineAsStringWhenFieldsExceedMaxLength, Line.LineAsString);

			HouseBillOfLadingMock.VerifyAll();
			ShipmentMock.VerifyAll();
			ConsolMock.VerifyAll();
		}

		#region Overrides
		protected override int ExpectedFieldCount
		{
			get
			{
				return 110;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return "OHBL";
			}
		}

		protected override MessageLine GetMessageLine()
		{
			return new OHBLLine(HouseLevelRecordType.Standard, Consol, HouseBillOfLading);
		}

		protected override void SetUp()
		{
			OriginalCurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = OriginalCurrencyNK;
			JASDataRegistryTest.UnsetJASWWOrganisationItemForTest();
		}

		ZString OriginalCurrencyNK;
		#endregion
		#region Implementation
		Mock<ExportHouseBillOfLading> HouseBillOfLadingMock
		{
			get
			{
				if (fHouseBillOfLadingMock == null)
				{
					fHouseBillOfLadingMock = new Mock<ExportHouseBillOfLading>(new object[] { Consol, Shipment });
					fHouseBillOfLadingMock.CallBase = true;
				}

				return fHouseBillOfLadingMock;
			}
		}

		ExportHouseBillOfLading HouseBillOfLading
		{
			get
			{
				return HouseBillOfLadingMock.Object;
			}
		}

		#region Create Data for Test
		Mock<JASForwardingShipment> ShipmentMock
		{
			get
			{
				if (fShipmentMock == null)
				{
					fShipmentMock = Factory.NewMoq<JASForwardingShipment>();
				}

				return fShipmentMock;
			}
		}

		JASForwardingShipment Shipment
		{
			get
			{
				return ShipmentMock.Object;
			}
		}

		void PopulateShipmentData()
		{
			ShipmentMock.Setup(m => m.JS_TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
			SetConsol();
			PopulatePortOfLoadingAndDischargeData();
			PopulateCarrierData();
			PopulateShipperData();
			PopulateConsigneeData();
			PopulateNotifyPartyData();
			PopulateFreightChargesData();
			PopulateSpecialInstructionsData();
			PopulatePlaceOfReceiptAndDelivery();
			PopulateReceivingAgentData();
			PopulateHandlingInstructionsData();
			PopulateMarksAndNumbers();
			PopulatePayableBy();
		}

		#region Consol
		Mock<JASForwardingConsol> ConsolMock
		{
			get
			{
				if (fConsolMock == null)
				{
					fConsolMock = Factory.NewMoq<JASForwardingConsol>();
				}

				return (Mock<JASForwardingConsol>)fConsolMock;
			}
		}

		JASForwardingConsol Consol
		{
			get
			{
				return ConsolMock.Object;
			}
		}

		void SetConsol()
		{
			Shipment.Consols.RemoveAll();
			Shipment.Consols.Add(Consol);
			ConsolMock.Setup(m => m.JK_TransportMode).Returns((ZString)Core.Constants.TransportModes.Sea);
		}

		Mock fConsolMock;
		#endregion
		#region Header
		void PopulateHeaderData_V1()
		{
			ConsolMock.Setup(m => m.JK_MasterBillNum).Returns((ZString)"OBLBLAH");
			PopulateSendingAndReceivingForwarder();
			ShipmentMock.Setup(m => m.JS_UniqueConsignRef).Returns((ZString)"SHP001");
			ShipmentMock.Setup(m => m.JS_HouseBill).Returns((ZString)"HBL101");
			if (ExceedMaxLength)
			{
				ShipmentMock.Setup(m => m.JS_UniqueConsignRef).Returns((ZString)"0123456789012345678901234567890123456789012345678901234567890123456789");
			}
		}

		void PopulateHeaderData_V2()
		{
			PopulateSendingAndReceivingForwarder();
		}

		void PopulateSendingAndReceivingForwarder()
		{
			Consol.SetDefaultSendingForwarderAddress(Factory.New<JASOrgHeader>());
			Consol.SendingForwarder.NettingCode = "SN";
			Consol.SendingForwarder.OfficeCode = "SO";
			Consol.SetDefaultReceivingForwarderAddress(Factory.New<JASOrgHeader>());
			Consol.ReceivingForwarder.NettingCode = "DN";
			Consol.ReceivingForwarder.OfficeCode = "DO";
		}

		#endregion
		#region Port of Loading / Discharge
		void PopulatePortOfLoadingAndDischargeData()
		{
			ConsolMock.Setup(m => m.JK_RL_NKLoadPort).Returns((ZString)"AUBNE");
			ConsolMock.Setup(m => m.JK_RL_NKDischargePort).Returns((ZString)"ITMIL");
			if (ExceedMaxLength)
			{
				HouseBillOfLadingMock.Setup(m => m.PortOfLoadingName).Returns((ZString)"1234567890123456789012345678901234567890");
				HouseBillOfLadingMock.Setup(m => m.PortOfDischargeName).Returns((ZString)"*23456789012345678901234567890123456789*");
			}
		}

		#endregion
		#region Carrier
		void PopulateCarrierData()
		{
			OrgHeader shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "Fast Shipper Inc.";
			shippingLine.MainAddress.OA_Address1 = "30/111 Martin Place";
			shippingLine.MainAddress.OA_Address2 = "Boogeyville";
			shippingLine.MainAddress.OA_City = "Ashmore";
			OrgPatternMatchOverride @override = shippingLine.CreatePatternMatchOverrideForTest();
			@override.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			@override.OO_LocalGuid = JASDataRegistry.Instance.JASWWOrganisationPK;
			@override.OO_ForeignCode = "TESTSSL";
			ConsolMock.Setup(m => m.JK_OA_ShippingLineAddress).Returns(shippingLine.MainAddress.PK);
			if (ExceedMaxLength)
			{
				HouseBillOfLadingMock.Setup(m => m.ShippingLineName).Returns((ZString)"CARRIER890123456789012345678901234567890123456");
				HouseBillOfLadingMock.Setup(m => m.ShippingLineAddress1).Returns((ZString)"CARADD1890123456789012345678901234567890123456");
				HouseBillOfLadingMock.Setup(m => m.ShippingLineAddress2).Returns((ZString)"CARADD2890123456789012345678901234567890123456");
				HouseBillOfLadingMock.Setup(m => m.ShippingLineCity).Returns((ZString)"CARCITY890123456789012345678901234567890123456");
			}
		}

		#endregion
		#region Shipper
		void PopulateShipperData()
		{
			OrgHeader shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "Dummy Shipper";
			shipper.OH_RL_NKClosestPort = "DEFRA";
			shipper.MainAddress.OA_Address1 = "Dummy Add1";
			shipper.MainAddress.OA_Address2 = "DummyADD2";
			shipper.MainAddress.OA_City = "Dummyville";
			shipper.MainAddress.OA_State = "DMY";
			shipper.MainAddress.OA_PostCode = "10222";
			shipper.MainAddress.OA_Phone = "40303059";
			shipper.MainAddress.OA_Email = "shipper@edi.com.au";
			shipper.OH_Code = "DDNY";
			shipper.OH_IsConsignor = true;
			Shipment.ConsignorPK = shipper.PK;
			if (ExceedMaxLength)
			{
				HouseBillOfLadingMock.Setup(m => m.ShipperName).Returns((ZString)"SHIPPER890123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ShipperAddress1).Returns((ZString)"SHPADD1890123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ShipperAddress2).Returns((ZString)"SHPADD2890123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ShipperCity).Returns((ZString)"SHPCiTY890123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ShipperPostalCode).Returns((ZString)"SHPPOCODE0123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ShipperState).Returns((ZString)"SHPSTATE90123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ShipperPhone).Returns((ZString)"SHPPHONE90123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ShipperAccount).Returns((ZString)"SHPACC7890123456789012345678901234567");
			}
		}

		#endregion
		#region Consignee
		void PopulateConsigneeData()
		{
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Test Consignee";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.MainAddress.OA_Address1 = "A01";
			consignee.MainAddress.OA_Address2 = "Line 002";
			consignee.MainAddress.OA_City = "Lidcombe";
			consignee.MainAddress.OA_State = "NSW";
			consignee.MainAddress.OA_PostCode = "2031";
			consignee.MainAddress.OA_Phone = "1023827";
			consignee.MainAddress.OA_Email = "consignee@edi.com.au";
			consignee.OH_Code = "TESTCO";
			consignee.OH_IsConsignee = true;
			Shipment.ConsigneePK = consignee.PK;
			if (ExceedMaxLength)
			{
				HouseBillOfLadingMock.Setup(m => m.ConsigneeName).Returns((ZString)"CONSIGNEE0123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ConsigneeAddress1).Returns((ZString)"CNEADD1890123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ConsigneeAddress2).Returns((ZString)"CNEADD2890123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ConsigneeCity).Returns((ZString)"CNECiTY890123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ConsigneePostalCode).Returns((ZString)"CNEPOCODE0123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ConsigneeState).Returns((ZString)"CNESTATE90123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ConsigneePhone).Returns((ZString)"CNEPHONE90123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.ConsigneeAccount).Returns((ZString)"CNEACC7890123456789012345678901234567");
			}
		}

		#endregion
		#region Delivery Agent
		void PopulateDeliveryAgentData_V1()
		{
			OrgHeader deliveryAgent = Factory.New<OrgHeader>();
			deliveryAgent.OH_FullName = "Fast Deliverer Co.";
			deliveryAgent.MainAddress.OA_Address1 = "999 Fifth Avenue";
			deliveryAgent.MainAddress.OA_Address2 = "Thompson Close";
			deliveryAgent.MainAddress.OA_City = "Whidbey";
			ShipmentMock.Setup(m => m.JS_OH_DeliveryAgent).Returns(deliveryAgent.PK);
			if (ExceedMaxLength)
			{
				HouseBillOfLadingMock.Setup(m => m.DeliveryAgentName).Returns((ZString)"DELIVERY9012345678901234567890123456789012346");
				HouseBillOfLadingMock.Setup(m => m.DeliveryAgentAddress1).Returns((ZString)"DLVADDY19012345678901234567890123456789012346");
				HouseBillOfLadingMock.Setup(m => m.DeliveryAgentAddress2).Returns((ZString)"DLVADDY29012345678901234567890123456789012346");
				HouseBillOfLadingMock.Setup(m => m.DeliveryAgentCity).Returns((ZString)"DLVCITY89012345678901234567890123456789012346");
			}
		}

		void PopulateDeliveryAgentData_V2()
		{
			OrgHeader deliveryAgent = Factory.New<OrgHeader>();
			deliveryAgent.OH_FullName = "Fast Deliverer Co.";
			deliveryAgent.MainAddress.OA_Address1 = "999 Fifth Avenue";
			deliveryAgent.MainAddress.OA_Address2 = "Thompson Close";
			deliveryAgent.MainAddress.OA_City = "Whidbey";
			if (ExceedMaxLength)
			{
				HouseBillOfLadingMock.Setup(m => m.DeliveryAgentName).Returns((ZString)"DELIVERY9012345678901234567890123456789012346");
				HouseBillOfLadingMock.Setup(m => m.DeliveryAgentAddress1).Returns((ZString)"DLVADDY19012345678901234567890123456789012346");
				HouseBillOfLadingMock.Setup(m => m.DeliveryAgentAddress2).Returns((ZString)"DLVADDY29012345678901234567890123456789012346");
				HouseBillOfLadingMock.Setup(m => m.DeliveryAgentCity).Returns((ZString)"DLVCITY89012345678901234567890123456789012346");
			}
		}

		#endregion
		#region Notify Party
		void PopulateNotifyPartyData()
		{
			OrgHeader notifyOrg = Factory.New<OrgHeader>();
			notifyOrg.MainAddress.OA_Address1 = "Addd1";
			notifyOrg.MainAddress.OA_Address2 = "LineAII";
			notifyOrg.MainAddress.OA_City = "Bangkok";
			notifyOrg.MainAddress.OA_Phone = "89888";
			notifyOrg.MainAddress.OA_Email = "email@editest.com.au";
			OrgContact notifyParty = Factory.New<OrgContact>();
			notifyParty.OC_ContactName = "Test Notify";
			notifyParty.OC_OH = notifyOrg.PK;
			OrgDocument doc1 = notifyParty.Documents.AddNew();
			doc1.OD_DocumentGroup = ContactType.NotifyParty.Code;
			Shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyOrg.PK;
			if (ExceedMaxLength)
			{
				HouseBillOfLadingMock.Setup(m => m.NotifyPartyName).Returns((ZString)"NOTIFY789012345678901234567890123456789012");
				HouseBillOfLadingMock.Setup(m => m.NotifyPartyAddress1).Returns((ZString)"NTFADD189012345678901234567890123456789012");
				HouseBillOfLadingMock.Setup(m => m.NotifyPartyAddress2).Returns((ZString)"NTFADD289012345678901234567890123456789012");
				HouseBillOfLadingMock.Setup(m => m.NotifyPartyCity).Returns((ZString)"NTFCITY89012345678901234567890123456789012");
				HouseBillOfLadingMock.Setup(m => m.NotifyPartyPhone).Returns((ZString)"NTFPHONE9012345678901234567890123456789012");
			}
		}

		#endregion
		#region Freight Charges
		void PopulateFreightChargesData()
		{
			ShipmentMock.Setup(m => m.JS_INCO).Returns((ZString)Core.Constants.IncoTerms.DefaultIncoFromPaymentType(Core.Constants.PaymentType.Prepaid));
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "IDR";
			PopulateJobChargeForShipment();
		}

		void PopulateJobChargeForShipment()
		{
			JobHeader header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.JH_ParentID = Shipment.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			JobCharge charge = Factory.New<JobCharge>();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_LocalSellAmt = 88899M;
			charge.JR_JH = header.PK;
			header.LoadCharges_ForTestOnly();
		}

		#endregion
		#region Vessel
		void PopulateVesselAndVoyageData_V1()
		{
			RefVessel newVessel = Factory.New<RefVessel>();
			newVessel.RV_Code = "KOREAN PRIDE";
			newVessel.RV_RN_NKCountryOfReg = "KR";
			newVessel.RV_LloydsNumber = "0208880";
			Transport transport = Consol.Transports[0];
			transport.JW_Vessel = "KOREAN PRIDE";
			transport.JW_VoyageFlight = "VOY123";
			ShipmentMock.Setup(m => m.JS_ShippedOnBoardDate).Returns(new ZDateTime(2005, 4, 22));
			if (ExceedMaxLength)
			{
				HouseBillOfLadingMock.Setup(m => m.VesselName).Returns((ZString)"VESSEL789012345678901234567890123456");
				HouseBillOfLadingMock.Setup(m => m.VoyageNumber).Returns((ZString)"Voyage789012345678901234567890123456");
			}
		}

		void PopulateVesselAndVoyageData_V2()
		{
			RefVessel newVessel = Factory.New<RefVessel>();
			newVessel.RV_Code = "KOREAN PRIDE";
			newVessel.RV_RN_NKCountryOfReg = "KR";
			newVessel.RV_LloydsNumber = "0208880";
			Transport transport = Consol.Transports[0];
			transport.JW_Vessel = "KOREAN PRIDE";
			transport.JW_VoyageFlight = "VOY123";
		}

		#endregion
		#region Special Instructions
		void PopulateSpecialInstructionsData()
		{
			StmNote[] notes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.SpecialInstructions.Description);
			string instructions = new string('X', 60) + new string('y', 60) + "ZZ";
			if (notes.Length > 0)
			{
				notes[0].ST_NoteText = instructions;
			}
			else
			{
				Shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, instructions);
			}
		}

		#endregion
		#region Freight Info
		void PopulateFreightInfoData_V1()
		{
			ShipmentMock.Setup(m => m.JS_OuterPacks).Returns((ZInt)23);
			ShipmentMock.Setup(m => m.JS_F3_NKPackType).Returns((ZString)Core.Constants.PkgUnit.Bag);
			ShipmentMock.Setup(m => m.JS_UnitOfWeight).Returns((ZString)Core.Constants.Weight.Kilograms);
			ShipmentMock.Setup(m => m.JS_ActualWeight).Returns((ZDecimal)1785);
			ShipmentMock.Setup(m => m.JS_UnitOfVolume).Returns((ZString)Core.Constants.Volume.CubicMetres);
			ShipmentMock.Setup(m => m.JS_ActualVolume).Returns((ZDecimal)8256);
			ShipmentMock.Setup(m => m.JS_UnitFreightRate).Returns((ZDecimal)87.89m);
			Shipment.DetailedGoodsDescriptionNoteText = new string('1', 160) + new string('2', 160) + new string('3', 160) + new string('4', 160) + new string('5', 160) + new string('6', 160) + new string('7', 160) + new string('8', 160);
			PopulateContainers();
			if (ExceedMaxLength)
			{
				HouseBillOfLadingMock.Setup(m => m.TotalNoOfPackagesAndUnitInWords).Returns((ZString)"TOTALPKG901234567890123456789012345678901234567890123456789012345");
			}
		}

		void PopulateFreightInfoData_V2()
		{
			ShipmentMock.Setup(m => m.JS_F3_NKPackType).Returns((ZString)Core.Constants.PkgUnit.Bag);
			ShipmentMock.Setup(m => m.JS_UnitOfWeight).Returns((ZString)Core.Constants.Weight.Kilograms);
			ShipmentMock.Setup(m => m.JS_UnitOfVolume).Returns((ZString)Core.Constants.Volume.CubicMetres);
			Shipment.DetailedGoodsDescriptionNoteText = new string('1', 160) + new string('2', 160) + new string('3', 160) + new string('4', 160) + new string('5', 160) + new string('6', 160) + new string('7', 160) + new string('8', 160);
			PopulateContainers();
			if (ExceedMaxLength)
			{
				HouseBillOfLadingMock.Setup(m => m.TotalNoOfPackagesAndUnitInWords).Returns((ZString)"TOTALPKG901234567890123456789012345678901234567890123456789012345");
			}
		}

		void PopulateContainers()
		{
			CommonContainer container = Consol.Containers.AddNew();
			PackLine outerPack = Shipment.OuterPackLines.AddNew();
			outerPack.SetContainer(Consol, container);
		}

		#endregion
		#region Date and Place of Issue
		void PopulateDateAndPlaceOfIssue()
		{
			Shipment.JS_HouseBillIssueDate = ZDateTime.Today;
			if (ExceedMaxLength)
			{
				HouseBillOfLadingMock.Setup(m => m.PlaceOfIssue).Returns((ZString)"ISSUEPLACE123456789012345678901234567890");
			}
			else
			{
				HouseBillOfLadingMock.Setup(m => m.PlaceOfIssue).Returns((ZString)"Far far away land");
			}
		}

		#endregion
		#region Receiving Agent
		void PopulateReceivingAgentData()
		{
			if (!IsReceivingAgentNull)
			{
				ZString fullName;
				ZString address;
				if (ExceedMaxLength)
				{
					fullName = "RA3456789012345678901234567890123456";
					address = "RAADD6789012345678901234567890123456";
				}
				else
				{
					fullName = "ReceivingAgent";
					address = "ReceivingAgentAddress";
				}

				OrgHeader receivingAgent = Factory.New<OrgHeader>();
				receivingAgent.OH_FullName = fullName;
				receivingAgent.MainAddress.OA_Address1 = address;
				receivingAgent.MainAddress.OA_City = "ReceivingAgentCity";
				ConsolMock.Setup(m => m.JK_OA_ReceivingForwarderAddress).Returns(receivingAgent.MainAddress.PK);
			}
		}

		#endregion
		#region Handling Instructions
		void PopulateHandlingInstructionsData()
		{
			StmNote[] notes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			string instructions = new string('X', 35) + new string('y', 35) + "ZZ";
			if (notes.Length > 0)
			{
				notes[0].ST_NoteText = instructions;
			}
			else
			{
				Shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, instructions);
			}
		}

		#endregion
		#region Place of Receipt / Delivery
		void PopulatePlaceOfReceiptAndDelivery()
		{
			RefUNLOCO origin = Factory.New<RefUNLOCO>();
			origin.RL_Code = "AU123";
			origin.RL_PortName = "Far East";
			RefUNLOCO destination = Factory.New<RefUNLOCO>();
			destination.RL_Code = "US123";
			destination.RL_PortName = "Far West";
			ShipmentMock.Setup(m => m.JS_RL_NKOrigin).Returns((ZString)"AU123");
			ShipmentMock.Setup(m => m.JS_RL_NKDestination).Returns((ZString)"US123");
			if (ExceedMaxLength)
			{
				HouseBillOfLadingMock.Setup(m => m.PlaceOfReceipt).Returns((ZString)"RCPTPLACE0123456789012345678901234567");
				HouseBillOfLadingMock.Setup(m => m.PlaceOfDelivery).Returns((ZString)"RCPTDLV890123456789012345678901234567");
			}
		}

		#endregion
		#region Marks and Numbers
		void PopulateMarksAndNumbers()
		{
			StmNote[] notes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
			ZString text = (ExceedMaxLength) ? "M&N45678901234567890123456789012345678901234567890" : "MARKS&NUMS";
			if (notes.Length > 0)
			{
				notes[0].ST_NoteText = text;
			}
			else
			{
				Shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, text);
			}
		}

		#endregion
		#region Payable By
		void PopulatePayableBy()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(Core.Constants.PaymentType.Prepaid);
			if (ExceedMaxLength)
			{
				HouseBillOfLadingMock.Setup(m => m.FreightPayableBy).Returns((ZString)"PAYABLEBY01234567890");
			}
		}

		#endregion
		#region No Of Original BoL
		void PopulateNoOfOriginalBillOfLadings()
		{
			ShipmentMock.Setup(m => m.JS_NoOriginalBills).Returns((ZByte)3);
		}

		#endregion
		#endregion
		#region ExpectedLineAsString
		const string ExpectedLineAsString = "OHBL3100;" + "N;SHP001;SO;HBL101;30;AUBNE;TESTSSL;Fast Shipper Inc.;30/111 Martin Place;Boogeyville;;Ashmore;OBLBLAH;Dummy Shipper;Dummy Add1;DummyADD2;;Dummyville;DMY;10222;DE;shipper@edi.com.au;N;40303059;DDNY;;Test Consignee;A01;Line 002;;Lidcombe;NSW;2031;AU;consignee@edi.com.au;N;1023827;TESTCO;;Fast Deliverer Co.;999 Fifth Avenue;Thompson Close;;Whidbey;;Brisbane;Test Notify;Addd1;LineAII;;Bangkok;email@editest.com.au;N;89888;;;;;;N;;ITMIL;IDR;88899;P;Milano;KOREAN PRIDE;VOY123;KR;0208880;22/04/2005;XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX;yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy;ZZ;" + "23;BAG;1785;K;8256;87.89;1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111;2222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222;3333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333;4444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444;5555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555;6666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666;7777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777;8888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888;ZZ;TWENTY THREE BAG(S);JAS OCEAN SERVICES INC. AS CARRIER;13/12/2002;Far far away land;AEST;ReceivingAgent;ReceivingAgentAddress;ReceivingAgentCity;XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX;yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy;ZZ;Far East;MARKS&NUMS;ORIGIN;ORIGIN;ORIGIN;3;Far West;1;;Y";
		const string ExpectedLineAsStringWhenFieldsExceedMaxLength = "OHBL3100;" + "N;0123456789012345678901234567890123456789012345678901234567890123;SO;HBL101;30;AUBNE;TESTSSL;CARRIER890123456789012345678901234567890;CARADD1890123456789012345678901234567890;CARADD2890123456789012345678901234567890;;CARCITY890123456789012345678901234567890;OBLBLAH;SHIPPER8901234567890123456789012345;SHPADD18901234567890123456789012345;SHPADD28901234567890123456789012345;;SHPCiTY8901234567;SHPSTATE9;SHPPOCODE012;DE;shipper@edi.com.au;N;SHPPHONE90123456789012345;SHPACC78901234;;CONSIGNEE01234567890123456789012345;CNEADD18901234567890123456789012345;CNEADD28901234567890123456789012345;;CNECiTY8901234567;CNESTATE9;CNEPOCODE012;AU;consignee@edi.com.au;N;CNEPHONE90123456789012345;CNEACC78901234;;DELIVERY90123456789012345678901234567890;DLVADDY190123456789012345678901234567890;DLVADDY290123456789012345678901234567890;;DLVCITY890123456789012345678901234567890;;12345678901234567890123456789012345;NOTIFY7890123456789012345678901234567890;NTFADD1890123456789012345678901234567890;NTFADD2890123456789012345678901234567890;;NTFCITY890123456789012345678901234567890;email@editest.com.au;N;NTFPHONE90123456789012345;;;;;;N;;ITMIL;IDR;88899;P;*2345678901234567890123456789012345;VESSEL78901234567890123456789012345;Voyage789012345;KR;0208880;22/04/2005;XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX;yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy;ZZ;" + "23;BAG;1785;K;8256;87.89;1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111;2222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222;3333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333;4444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444;5555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555;6666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666;7777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777;8888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888;ZZ;TOTALPKG9012345678901234567890123456789012345678901234567890;JAS OCEAN SERVICES INC. AS CARRIER;13/12/2002;ISSUEPLACE1234567890123456789012345;AEST;RA345678901234567890123456789012345;RAADD678901234567890123456789012345;ReceivingAgentCity;XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX;yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy;ZZ;RCPTPLACE01234567890123456789012345;M&N456789012345678901234567890123456789012345;PAYABLEBY012345;PAYABLEBY012345;PAYABLEBY012345;3;RCPTDLV8901234567890123456789012345;1;;Y";
		const string ExpectedLineAsStringWhenReceivingAgentDoesNotExist = "OHBL3100;" + "N;SHP001;SO;HBL101;30;AUBNE;TESTSSL;Fast Shipper Inc.;30/111 Martin Place;Boogeyville;;Ashmore;OBLBLAH;Dummy Shipper;Dummy Add1;DummyADD2;;Dummyville;DMY;10222;DE;shipper@edi.com.au;N;40303059;DDNY;;Test Consignee;A01;Line 002;;Lidcombe;NSW;2031;AU;consignee@edi.com.au;N;1023827;TESTCO;;Fast Deliverer Co.;999 Fifth Avenue;Thompson Close;;Whidbey;;Brisbane;Test Notify;Addd1;LineAII;;Bangkok;email@editest.com.au;N;89888;;;;;;N;;ITMIL;IDR;88899;P;Milano;KOREAN PRIDE;VOY123;KR;0208880;22/04/2005;XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX;yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy;ZZ;" + "23;BAG;1785;K;8256;87.89;1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111;2222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222;3333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333;4444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444;5555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555;6666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666;7777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777777;8888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888888;ZZ;TWENTY THREE BAG(S);JAS OCEAN SERVICES INC. AS CARRIER;13/12/2002;Far far away land;AEST;;;;XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX;yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy;ZZ;Far East;MARKS&NUMS;ORIGIN;ORIGIN;ORIGIN;3;Far West;1;;Y";
		#endregion
		bool ExceedMaxLength;
		bool IsReceivingAgentNull;
		Mock<ExportHouseBillOfLading> fHouseBillOfLadingMock;
		Mock<JASForwardingShipment> fShipmentMock;
		#endregion
	}
}
