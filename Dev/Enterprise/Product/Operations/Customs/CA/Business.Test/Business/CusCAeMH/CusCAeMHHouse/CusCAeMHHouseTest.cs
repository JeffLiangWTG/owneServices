using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageProcessors.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Edifact.D11B.Elements;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWise.Data;
using Enterprise.Messaging.Business;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHHouseSingleTransactionTest : TestCase
	{
		class CusCAeMHHouseForMessageDeleteTest : CusCAeMHHouse
		{
			public CusCAeMHHouseForMessageDeleteTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				shouldThrowException = true;
			}

			bool shouldThrowException;
			public ZString failedMessageNum { get; set; }

			public override void OnSaving()
			{
				base.OnSaving();
				if (shouldThrowException)
				{
					failedMessageNum = ((EDIMessage)Messages.LastOrDefault()).EM_MessageNum;
					shouldThrowException = false;
					throw new ConcurrencyConflictException();
				}
			}
		}

		public void TestDeleteUnsavedMessages()
		{
			var newFactory = new BusinessObjectFactory();
			var messageNum1 = ZString.Empty;

			Db.Connection.BeginTransaction();
			var helper = new CusCAeMHTestHelper(newFactory);
			var house = newFactory.New<CusCAeMHHouseForMessageDeleteTest>();
			house.BW_BP_Master = helper.MasterBill.PK;
			var message1 = house.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			try
			{
				newFactory.Save();
			}
			catch (Exception) { }
			messageNum1 = house.failedMessageNum;

			Db.Connection.RollbackTransaction();
			AssertEquals(false, message1.IsInDatabase);
			AssertEquals(true, message1.IsDeleted);

			Db.Connection.BeginTransaction();
			var message2 = house.Messages.AddNew();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newFactory.Save();

			AssertEquals(messageNum1, message2.EM_MessageNum);
			AssertEquals(false, message1.IsInDatabase);
			AssertEquals(true, message2.IsInDatabase);
			AssertEquals(true, message1.IsDeleted);
			AssertEquals(false, message2.IsDeleted);

			Db.Connection.RollbackTransaction();
			Db.Connection.CloseConnection();
		}
	}

	[TestedType(typeof(CusCAeMHHouse))]
	sealed class CusCAeMHHouseTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRoundingBW_Weight()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var house = helper.House;
			house.BW_Weight = -0.1m;
			AssertEquals(-0.1m, house.BW_Weight);
			house.BW_Weight = 0m;
			AssertEquals(0m, house.BW_Weight);
			house.BW_Weight = 0.1m;
			AssertEquals(1m, house.BW_Weight);
			house.BW_Weight = 0.5m;
			AssertEquals(1m, house.BW_Weight);
			house.BW_Weight = 1.1m;
			AssertEquals(1m, house.BW_Weight);
			house.BW_Weight = 1.5m;
			AssertEquals(2m, house.BW_Weight);
		}

		public void TestRecreateDocAddressesWhenBW_OverrideFreightDefaultsUnticked()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var consol = helper.Consol;
			var house = helper.House;
			var shipment = helper.Shipment;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_Code = "SNDFWD";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			house.ShipmentSynchroniser.SetEnabled(true, false);
			house.ShipmentSynchroniser.Synchronise();
			Factory.Save();

			var conAddress = house.DocAddresses.Cast<CAeMHDocAddress>().FirstOrDefault(x => x.E2_AddressType == DocAddressTypes.Codes.Consolidator);
			AssertEquals(sendingForwarder.OH_Code, conAddress.Organisation.OH_Code);

			house.BW_OverrideFreightDefaults = true;
			conAddress.Delete();

			house.BW_OverrideFreightDefaults = false;
			conAddress = house.DocAddresses.Cast<CAeMHDocAddress>().FirstOrDefault(x => x.E2_AddressType == DocAddressTypes.Codes.Consolidator);
			AssertEquals(sendingForwarder.OH_Code, conAddress.Organisation.OH_Code);
		}

		public void TestGivenCusCAeMHHouse_ThenBusinessObjectCollectionContainsOnlyLinkedEDIMessages()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house1 = master.HouseBills.AddNew();
			var house2 = master.HouseBills.AddNew();

			var message1 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			var message2 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			var message3 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			var message4 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			message1.EM_LinkedObject = house1;
			message2.EM_LinkedObject = house1;
			message3.EM_LinkedObject = house1;
			message4.EM_LinkedObject = house2;

			Factory.Save();

			var house1Messages = house1.MessagesForDisplay;
			var house2Messages = house2.MessagesForDisplay;

			AssertEquals("There should be three messages in MessagesForDisplay in house1", 3, house1Messages.Count);
			Assert("Expected house1 to contain message1", house1Messages.Contains(message1));
			Assert("Expected house1 to contain message2", house1Messages.Contains(message2));
			Assert("Expected house1 to contain message3", house1Messages.Contains(message3));

			AssertEquals("There should be one message in MessagesForDisplay in house2", 1, house2.MessagesForDisplay.Count);
			Assert("Expected house2 to contain message4", house2Messages.Contains(message4));
		}

		public void TestBW_MovementTypeDescription()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();

			house.BW_MovementType = eMHMovementTypeList.Codes.Import;
			AssertEquals(eMHMovementTypeList.Descriptions.Import, house.BW_MovementTypeDescription);

			house.BW_MovementType = eMHMovementTypeList.Codes.Inbond;
			AssertEquals(eMHMovementTypeList.Descriptions.Inbond, house.BW_MovementTypeDescription);
		}

		public void TestBW_WeightUQDescription()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();

			house.BW_WeightUQ = EManifestUnitOfWeightList.Codes.Kilogram;
			AssertEquals(EManifestUnitOfWeightList.Descriptions.Kilogram, house.BW_WeightUQDescription);

			house.BW_WeightUQ = EManifestUnitOfWeightList.Codes.MetricTon;
			AssertEquals(EManifestUnitOfWeightList.Descriptions.MetricTon, house.BW_WeightUQDescription);
		}

		public void TestBW_VolumeUQDesciption()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();

			house.BW_VolumeUQ = CustomsUnitOfMeasureList.Codes.CubicCentimetre;
			AssertEquals(CustomsUnitOfMeasureList.Descriptions.CubicCentimetre, house.BW_VolumeUQDesciption);

			house.BW_VolumeUQ = CustomsUnitOfMeasureList.Codes.CubicMillimetre;
			AssertEquals(CustomsUnitOfMeasureList.Descriptions.CubicMillimetre, house.BW_VolumeUQDesciption);
		}

		[UseSnapshotProtection]
		public void TestShouldDefaultUniqueMessageReference()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			var master1 = factory1.New<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			var dbConnection = ((IDbConnected)factory1).Connection;
			house1.OnSaving();
			var reference1 = house1.BW_MessageReference;
			dbConnection.RollbackTransaction();

			var master2 = factory2.New<CusCAeMHMaster>();
			var house2 = master2.HouseBills.AddNew();
			factory2.Save();
			var reference2 = house2.BW_MessageReference;

			AssertEquals(false, reference1.IsEmpty);
			AssertEquals(false, reference2.IsEmpty);
			AssertEquals(reference1, reference2);

			dbConnection.BeginTransaction();
			AssertEquals(reference1, house1.BW_MessageReference);

			factory1.Save();
			AssertNotEquals(reference1, house1.BW_MessageReference);
		}

		public void TestHouseBillNumberAndCCNReadOnly()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();

			foreach (var code in new MessageStatusList().GetAllCodesZString())
			{
				house.BW_MessageStatus = code;
				AssertEquals(house.DisableChangeHouseBillNumberAndCCN, house.BW_HouseBillInfo.ReadOnly);
				AssertEquals(house.DisableChangeHouseBillNumberAndCCN, house.BW_HouseCCNInfo.ReadOnly);
			}
		}

		public void TestSetDefaultValues()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			AssertEquals(eMHMovementTypeList.Codes.Import, house.BW_MovementType);
		}

		public void TestParentWorkflowProviders()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			Factory.Save();
			var prov = ((IWorkflowTriggerEventSource)house).ParentWorkflowProviders;
			AssertNull(house.Shipment);
			AssertNotNull(prov);
			AssertEquals(0, prov.Count);

			var shipment = Factory.New<ForwardingShipment>();
			house.BW_ParentID = shipment.PK;
			house.BW_ParentTableCode = shipment.TablePrefix;
			house.BW_OverrideFreightDefaults = true;
			Factory.Save();

			prov = ((IWorkflowTriggerEventSource)house).ParentWorkflowProviders;
			AssertNotNull(prov);
			AssertNotNull(house.Shipment);
			AssertEquals(1, prov.Count);
			AssertEquals(shipment, prov[0]);
		}

		[UseSnapshotProtection]
		public void TestCargoControlNumberGeneratedForStandaloneEmanifest()
		{
			Enterprise.Registry.Business.FreightDataRegistry.Instance.CanadaCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentCCNCustomizationTypes.Code.NumberFountain);

			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			Factory.Save();
			AssertEquals("Default prefix", "00000001", house.BW_HouseCCN);

			Enterprise.Registry.Business.FreightDataRegistry.Instance.CanadaCargoControlNumberBranchPrefix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			house = master.HouseBills.AddNew();
			Factory.Save();
			AssertEquals("Overrided prefix", "10000002", house.BW_HouseCCN);

			Enterprise.Registry.Business.FreightDataRegistry.Instance.CanadaCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentCCNCustomizationTypes.Code.ShipmentNumber);
			house = master.HouseBills.AddNew();
			Factory.Save();
			AssertEquals("CCNCustomizationTypes set to non-NFN", "", house.BW_HouseCCN);
		}

		public void TestOnFactorySavingBeforeTransactionCore()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			Factory.Save();

			AssertEquals("Message Status Change Log Count", master.Logs.Find(o => o.SL_SE_NKEvent == Events.MessageStatusChange.Code).Count(), 0);
			AssertEquals("Status Change Log Count", master.Logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).Count(), 0);
			house.BW_MessageStatus = "AWO";
			house.BW_CustomsStatus = "CAN";
			Factory.Save();
			var messagelog = house.Logs.Find(o => o.SL_SE_NKEvent == Events.MessageStatusChange.Code).FirstOrDefault();
			var customlog = house.Logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).FirstOrDefault();
			AssertNotNull(messagelog);
			AssertNotNull(customlog);
			AssertEquals("Message Status Change Log Reference", "AWO - Awaiting Original House", messagelog.SL_Reference);
			AssertEquals("Status Change Log Reference", "CAN - Canceled House", customlog.SL_Reference);
		}

		public void TestSomeReadOnlyProperties()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			Assert("Not readonly when no shipment", !house.BW_DGSpecialInstructionsInfo.ReadOnly);
			Assert("Not readonly when no shipment", !house.BW_HandlingInstructionsInfo.ReadOnly);
			var shipment = Factory.New<ForwardingShipment>();
			house.BW_ParentID = shipment.PK;
			house.BW_ParentTableCode = shipment.TablePrefix;
			house.BW_OverrideFreightDefaults = true;
			Assert(!house.ShouldSynchroniseWithShipment);
			Assert("Not readonly when not ShouldSynchroniseWithShipment", !house.BW_DGSpecialInstructionsInfo.ReadOnly);
			Assert("Not readonly when not ShouldSynchroniseWithShipment", !house.BW_HandlingInstructionsInfo.ReadOnly);
			house.BW_OverrideFreightDefaults = false;
			Assert(house.ShouldSynchroniseWithShipment);
			Assert("Is readonly when ShouldSynchroniseWithShipment", house.BW_DGSpecialInstructionsInfo.ReadOnly);
			Assert("Is readonly when ShouldSynchroniseWithShipment", house.BW_HandlingInstructionsInfo.ReadOnly);
			house.BW_MessageStatus = "CLR";
			Assert(!house.ShouldSynchroniseWithShipment);
			Assert("Not readonly when not ShouldSynchroniseWithShipment", !house.BW_DGSpecialInstructionsInfo.ReadOnly);
			Assert("Not readonly when not ShouldSynchroniseWithShipment", !house.BW_HandlingInstructionsInfo.ReadOnly);
		}

		public void TestHookShipmentSynchronisingEvent()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			house.BW_ParentID = shipment.PK;
			house.BW_ParentTableCode = shipment.TablePrefix;
			house.HookShipmentSynchronisingEvent();
			house.ShipmentSynchroniser.SetEnabled(true, false);
			house.BW_OverrideFreightDefaults = true;
			Assert(!house.ShipmentSynchroniser.IsEnabled);
			house.BW_OverrideFreightDefaults = false;
			house.BW_MessageStatus = "CLR";
			house.Messages.AddNew();
			Assert(!house.ShipmentSynchroniser.IsEnabled);
		}

		public void TestShouldSynchroniseWithShipment()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			house.BW_ParentID = shipment.PK;
			house.BW_ParentTableCode = shipment.TablePrefix;
			Assert(house.ShouldSynchroniseWithShipment);

			house.BW_OverrideFreightDefaults = true;
			Assert(!house.ShouldSynchroniseWithShipment);

			house.BW_OverrideFreightDefaults = false;
			house.BW_MessageStatus = "CLR";
			Assert(!house.ShouldSynchroniseWithShipment);

			house.BW_CustomsStatus = ZString.Empty;
			house.BW_ParentID = ZGuid.Empty;
			Assert(!house.ShouldSynchroniseWithShipment);
		}

		public void TestPopulateBW_MessageReference()
		{
			var master = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house1 = master.HouseBills.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			house1.BW_ParentID = shipment.PK;
			house1.BW_ParentTableCode = shipment.TablePrefix;
			house1.FillWithValidTestData();
			house1.BW_MessageReference = "";
			var house2 = master.HouseBills.AddNew();
			house2.FillWithValidTestData();
			house2.BW_MessageReference = "";

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S111144444888887777";
			house1.BW_ParentID = shipment.PK;
			house1.BW_ParentTableCode = shipment.TablePrefix;

			Factory.Save();

			AssertEquals("144444888887777", house1.BW_MessageReference);
			Assert(!house2.BW_MessageReference.IsEmpty);

			var shipment2 = Factory.New<ForwardingShipment>();
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			var house3 = master2.HouseBills.AddNew();
			house3.BW_ParentID = shipment2.PK;
			house3.BW_ParentTableCode = shipment2.TablePrefix;
			house3.FillWithValidTestData();
			house3.BW_MessageReference = "";

			shipment2.JS_UniqueConsignRef = "REF";
			Factory.Save();

			AssertEquals("REF", house3.BW_MessageReference);
		}

		public void TestMasterBill()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			AssertEquals(master, house.MasterBill);
		}

		public void TestIACIHouseBillProviderMembers()
		{
			CusCAeMHHouse house = (CusCAeMHHouse)GetNewBusinessObject();
			var line = house.Items.AddNew();
			line.BX_Description = "LINE1";
			var dgContact = Factory.New<OrgContact>();
			dgContact.OC_ContactName = "DG CONTACT";
			dgContact.OC_Phone = "12346789";
			line.UNDGs.AddNew().DI_OC_DGContact = dgContact.PK;
			var container = Master.Containers.AddNew();
			container.BQ_ContainerNumber = "C1";
			house.Pivots.AddNew().BPA_BQ_Container = container.PK;
			house.BW_MessageReference = "XXX123456";
			house.BW_MessageStatus = "AAA";
			house.BW_CustomsStatus = "BBB";
			Master.BP_IsActive = false;
			house.BW_HouseCCN = "CCC";
			house.BW_UCR = "DDD";
			Master.BP_PrimaryCCN = "EEE";
			house.BW_B2BComments = "FFF";
			house.BW_AmendReasonCode = "12";
			Master.BP_ModeOfTransport = "SEA";
			house.BW_IsMasterHouse = true;
			house.BW_Volume = 123;
			house.BW_VolumeUQ = "CBM";
			house.BW_HandlingInstructions = "GGG";
			house.BW_CBSAReleasePort = "HHH";
			house.BW_CBSAReleaseSubLocation = "III";
			Master.BP_CBSADischargePort = "JJJ";
			Master.BP_CBSADischargeSubLocation = "KKK";
			house.BW_DGSpecialInstructions = "LLL";
			house.BW_Weight = 456;
			house.BW_WeightUQ = "NMB";

			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.AccountSecurityCode, "54321");
			var caDocAddress = house.DocAddresses.AddNew();
			caDocAddress.DocAddressType = DocAddressType.ImportBroker;
			caDocAddress.OrganisationPK = org.PK;
			org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "8999");
			caDocAddress = house.DocAddresses.AddNew();
			caDocAddress.DocAddressType = DocAddressType.ReceivingForwarderAddress;
			caDocAddress.OrganisationPK = org.PK;
			org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "9123");
			caDocAddress = house.DocAddresses.AddNew();
			caDocAddress.DocAddressType = DocAddressType.Carrier;
			caDocAddress.OrganisationPK = org.PK;
			org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "7777");
			caDocAddress = house.DocAddresses.AddNew();
			caDocAddress.DocAddressType = DocAddressType.Warehouse;
			caDocAddress.OrganisationPK = org.PK;
			org = Factory.New<OrgHeader>();
			org.OH_FullName = "CONSIGNEE";
			caDocAddress = house.DocAddresses.AddNew();
			caDocAddress.DocAddressType = DocAddressType.ConsigneeDocumentaryAddress;
			caDocAddress.OrganisationPK = org.PK;
			org = Factory.New<OrgHeader>();
			org.OH_FullName = "SHIPPER";
			caDocAddress = house.DocAddresses.AddNew();
			caDocAddress.DocAddressType = DocAddressType.ConsignorDocumentaryAddress;
			caDocAddress.OrganisationPK = org.PK;
			org = Factory.New<OrgHeader>();
			org.OH_FullName = "DELIVERY";
			caDocAddress = house.DocAddresses.AddNew();
			caDocAddress.DocAddressType = DocAddressType.ConsigneePickupDeliveryAddress;
			caDocAddress.OrganisationPK = org.PK;
			org = Factory.New<OrgHeader>();
			org.OH_FullName = "NOTIFY";
			caDocAddress = house.DocAddresses.AddNew();
			caDocAddress.DocAddressType = DocAddressType.NotifyParty;
			caDocAddress.OrganisationPK = org.PK;
			org = Factory.New<OrgHeader>();
			org.OH_FullName = "CONSOLIDATION PLACE";
			var docAddress = ((IDocAddresses)Master).DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.PlaceOfConsolidation);
			docAddress.OrganisationPK = org.PK;
			org = Factory.New<OrgHeader>();
			org.OH_FullName = "CONSOLIDATOR";
			docAddress = ((IDocAddresses)Master).DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Consolidator);
			docAddress.OrganisationPK = org.PK;
			AssertEquals(house.PK, ((IACIHouseBillProvider)house).TopLevelBusinessObject.PK);
			var message = Factory.New<EDIMessage>();
			((IACIHouseBillProvider)house).AddMessage(message);
			AssertEquals(1, house.Messages.Count);
			AssertEquals(1, ((IACIHouseBillProvider)house).Messages.Count);
			AssertEquals("HBL-XXX123456", ((IACIHouseBillProvider)house).JobIdentification);
			AssertEquals("AAA", ((IACIHouseBillProvider)house).MessageStatus);
			AssertEquals("BBB", ((IACIHouseBillProvider)house).JobStatus);
			Assert(((IACIHouseBillProvider)house).IsCancelled);
			AssertEquals("CCC", ((IACIHouseBillProvider)house).HouseCCN);
			AssertEquals("DDD", ((IACIHouseBillProvider)house).UCR);
			AssertEquals("24", ((IACIHouseBillProvider)house).MovementType);
			AssertEquals("EEE", ((IACIHouseBillProvider)house).PrimaryCCN);
			AssertEquals("FFF", ((IACIHouseBillProvider)house).B2BComments);
			AssertEquals("12", ((IACIHouseBillProvider)house).AmendmentReason);
			AssertEquals("1", ((IACIHouseBillProvider)house).TransportMode);
			Assert(((IACIHouseBillProvider)house).IsConsolidatedCargo);
			AssertEquals(123m, ((IACIHouseBillProvider)house).Volume);
			AssertEquals("CBM", ((IACIHouseBillProvider)house).VolumeUOM);
			AssertEquals("GGG", ((IACIHouseBillProvider)house).SpecialHandlingInstructions);
			AssertEquals("DG CONTACT", ((IACIHouseBillProvider)house).UNDGContact.OC_ContactName);
			AssertEquals("12346789", ((IACIHouseBillProvider)house).UNDGContact.OC_Phone);
			AssertEquals("HHH", ((IACIHouseBillProvider)house).ReleasePortCode);
			AssertEquals("III", ((IACIHouseBillProvider)house).ReleaseSubLocationCode);
			AssertEquals("0JJJ", ((IACIHouseBillProvider)house).DischargePortCode);
			AssertEquals("KKK", ((IACIHouseBillProvider)house).DischargeSubLocationCode);
			AssertEquals("LLL", ((IACIHouseBillProvider)house).DGSpecialInstructions);
			AssertEquals(456m, ((IACIHouseBillProvider)house).TotalWeight);
			AssertEquals("NMB", ((IACIHouseBillProvider)house).TotalWeightUOM);

			var snp = ((IACIHouseBillProvider)house).SecondaryNotifyParties.FirstOrDefault(x => x.SecondaryNotifyType == PartyFunctionCodeQualifierList.CustomsBroker);
			AssertNotNull(snp);
			AssertEquals("54321", snp.Identifier);
			snp = ((IACIHouseBillProvider)house).SecondaryNotifyParties.FirstOrDefault(x => x.SecondaryNotifyType == PartyFunctionCodeQualifierList.FreightForwarder);
			AssertNotNull(snp);
			AssertEquals("8999", snp.Identifier);
			AssertEquals("MF", snp.NoticeType);
			snp = ((IACIHouseBillProvider)house).SecondaryNotifyParties.FirstOrDefault(x => x.SecondaryNotifyType == PartyFunctionCodeQualifierList.Carrier);
			AssertNotNull(snp);
			AssertEquals("9123", snp.Identifier);
			snp = ((IACIHouseBillProvider)house).SecondaryNotifyParties.FirstOrDefault(x => x.SecondaryNotifyType == PartyFunctionCodeQualifierList.WarehouseKeeper);
			AssertNotNull(snp);
			AssertEquals("7777", snp.Identifier);

			var orgAddress = ((IACIHouseBillProvider)house).Consignee;
			AssertNotNull(orgAddress);
			AssertEquals("CONSIGNEE", orgAddress.E2_CompanyName);
			orgAddress = ((IACIHouseBillProvider)house).Shipper;
			AssertNotNull(orgAddress);
			AssertEquals("SHIPPER", orgAddress.E2_CompanyName);
			orgAddress = ((IACIHouseBillProvider)house).DeliveryAddresses.First();
			AssertNotNull(orgAddress);
			AssertEquals("DELIVERY", orgAddress.E2_CompanyName);
			orgAddress = ((IACIHouseBillProvider)house).NotifyParties.First();
			AssertNotNull(orgAddress);
			AssertEquals("NOTIFY", orgAddress.E2_CompanyName);
			orgAddress = ((IACIHouseBillProvider)house).PlaceOfConsolidation;
			AssertNotNull(orgAddress);
			AssertEquals("CONSOLIDATION PLACE", orgAddress.E2_CompanyName);
			orgAddress = ((IACIHouseBillProvider)house).Consolidator;
			AssertNotNull(orgAddress);
			AssertEquals("CONSOLIDATOR", orgAddress.E2_CompanyName);

			AssertEquals(1, ((IACIHouseBillProvider)house).Containers.Count());
			AssertEquals("C1", ((IACIHouseBillProvider)house).Containers.First().ContainerNumber);

			AssertEquals(1, ((IACIHouseBillProvider)house).Lines.Count());
			AssertEquals("LINE1", ((IACIHouseBillProvider)house).Lines.First().GoodsDescription);

			org = Factory.New<OrgHeader>();
			org.OH_FullName = "HOUSE CONSOLIDATOR";
			caDocAddress = house.DocAddresses.AddNew();
			caDocAddress.DocAddressType = DocAddressType.Consolidator;
			caDocAddress.OrganisationPK = org.PK;
			org = Factory.New<OrgHeader>();
			org.OH_FullName = "HOUSE PLACE OF CONSOLIDATION";
			caDocAddress = house.DocAddresses.AddNew();
			caDocAddress.DocAddressType = DocAddressType.PlaceOfConsolidation;
			caDocAddress.OrganisationPK = org.PK;
			orgAddress = ((IACIHouseBillProvider)house).Consolidator;
			AssertNotNull(orgAddress);
			AssertEquals("CONSOLIDATOR", orgAddress.E2_CompanyName);
			orgAddress = ((IACIHouseBillProvider)house).PlaceOfConsolidation;
			AssertNotNull(orgAddress);
			AssertEquals("CONSOLIDATION PLACE", orgAddress.E2_CompanyName);
		}

		public void TestIACIHouseBillProviderPOCAndCON()
		{
			var masterPOC = GetOrgHeaderForTest("MASPOC", "Master POC");
			var masterCON = GetOrgHeaderForTest("MASCON", "Master CON");
			var housePOC = GetOrgHeaderForTest("HOUPOC", "House POC");
			var houseCON = GetOrgHeaderForTest("HOUCON", "House CON");

			var house = Master.HouseBills.AddNew();
			var houPOCDocAddress = house.DocAddresses.AddNew();
			houPOCDocAddress.DocAddressType = DocAddressType.PlaceOfConsolidation;
			houPOCDocAddress.OrganisationPK = housePOC.PK;
			var houCONDocAddress = house.DocAddresses.AddNew();
			houCONDocAddress.DocAddressType = DocAddressType.Consolidator;
			houCONDocAddress.OrganisationPK = houseCON.PK;
			var provider = house as IACIHouseBillProvider;
			AssertEquals(null, provider.PlaceOfConsolidation);
			AssertEquals(null, provider.Consolidator);

			house.BW_IsMasterHouse = true;
			AssertEquals(houPOCDocAddress.PK, provider.PlaceOfConsolidation.PK);
			AssertEquals(houCONDocAddress.PK, provider.Consolidator.PK);

			var masPOCDocAddress = Master.PlaceOfConsolidation;
			masPOCDocAddress.OrganisationPK = masterPOC.PK;
			var masCONDocAddress = Master.Consolidator;
			masCONDocAddress.OrganisationPK = masterCON.PK;
			AssertEquals(houPOCDocAddress.PK, provider.PlaceOfConsolidation.PK);
			AssertEquals(houCONDocAddress.PK, provider.Consolidator.PK);

			house.BW_IsMasterHouse = false;
			AssertEquals(null, provider.PlaceOfConsolidation);
			AssertEquals(null, provider.Consolidator);

			house.DocAddresses.RemoveAndDeleteAll();
			AssertEquals(null, provider.PlaceOfConsolidation);
			AssertEquals(null, provider.Consolidator);

			house.BW_IsMasterHouse = true;
			AssertEquals(masPOCDocAddress.PK, provider.PlaceOfConsolidation.PK);
			AssertEquals(masCONDocAddress.PK, provider.Consolidator.PK);
		}

		OrgHeader GetOrgHeaderForTest(ZString orgCode, ZString companyName)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = orgCode;
			org.OH_FullName = companyName;
			var address = org.Addresses.AddNew();
			address.OA_Address1 = companyName;
			Factory.Save();
			return org;
		}

		public void TestNoLoggingACILicenceInTest()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			house.BW_MessageReference = "CAH0000004";
			var count = Factory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIeManifestReportingPerTransaction.Name)).Length;

			var outMessage = house.Messages.AddNew();
			outMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			outMessage.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_Status = EDIMessage.Status.Sent;
			outMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			outMessage.EM_IsTestMessage = ZBool.True;
			outMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			var message1 = (ACIForwarderMessage)house.Messages.AddNew(typeof(ACIForwarderMessage));
			message1.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI;
			message1.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			message1.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			message1.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message1.EM_MessageNum = "100";
			message1.EM_MessageText = EManifestResponseTest.ContentAcceptedMessageText.Replace("\r\n", "'");
			Factory.Save();
			AssertEquals("Should not be any activity logs", 0, Factory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIeManifestReportingPerTransaction.Name)).Length - count);
		}

		public void TestLoggingACILicence()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			house.BW_MessageReference = "CAH0000004";
			foreach (var log in Factory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIeManifestReportingPerTransaction.Name)))
			{
				log.Delete();
			}
			var count = Factory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIeManifestReportingPerTransaction.Name)).Length;
			AssertEquals("pre-condition, should not be any licence logs", 0, count);
			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = Constants.CountryCodes.HongKong;
			newCompany.GC_Code = "HKC";
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Constants.CountryCodes.HongKong)).RL_Code;
			newBranch.GB_Code = "HKB";
			master.BP_GB_Branch = newBranch.PK;
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "TZT";
			newStaff.GS_LoginName = "BOBB";
			newStaff.GS_FullName = "BOB BUILDER";
			var outMessage = house.Messages.AddNew();
			outMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			outMessage.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_Status = EDIMessage.Status.Sent;
			outMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			outMessage.EM_SystemCreateUser = newStaff.GS_Code;
			outMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			house.BW_CustomsStatus = "XXX";
			var message2 = (ACIForwarderMessage)house.Messages.AddNew(typeof(ACIForwarderMessage));
			message2.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI;
			message2.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			message2.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			message2.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message2.EM_MessageNum = "110";
			message2.EM_MessageText = EManifestResponseTest.NotMatchedMessageText.Replace("\r\n", "'");
			Factory.Save();
			AssertEquals("Should be one new activity log", 1, Factory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIeManifestReportingPerTransaction.Name)).Length);
			var newLog = Factory.LoadTop1<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIeManifestReportingPerTransaction.Name));
			AssertNotNull(newLog);
			AssertEquals("Licence should have been logged under branch of CusSCAHouse", newBranch.PK, newLog.S7_ParentID);
			AssertEquals("Licence should have been logged under branch of CusSCAHouse", GlbBranchSchema.Constants.Prefix, newLog.S7_ParentTableCode);
			AssertEquals("Licence should have been logged under user of last sent message", newStaff.GS_Code, newLog.S7_GS_NKUser);

			var message3 = (ACIForwarderMessage)house.Messages.AddNew(typeof(ACIForwarderMessage));
			message3.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI;
			message3.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			message3.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			message3.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message3.EM_MessageNum = "120";
			message3.EM_MessageText = EManifestResponseTest.MatchedMessageText.Replace("\r\n", "'");
			Factory.Save();
			AssertEquals("Should still be only one new activity log", 1, Factory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIeManifestReportingPerTransaction.Name)).Length);
		}

		public void TestHouseBillMessagesForDisplay()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			var eventTime = ZDateTime.Now.AddMinutes(1);

			var message1 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			message1.EM_LinkedObject = house;
			message1.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message2 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime);
			message2.EM_LinkedObject = house;
			message2.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message3 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7598", messageNumber: "4");
			message3.EM_LinkedObject = house;
			message3.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message4 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "1");
			message4.EM_LinkedObject = house;
			message4.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message5 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "2");
			message5.EM_LinkedObject = house;
			message5.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message6 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime.AddHours(-1), interchangeNumber: "7599", messageNumber: "3");
			message6.EM_LinkedObject = house;
			message6.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			Factory.Save();

			var messages = house.MessagesForDisplay;
			AssertEquals(typeof(EDIMessageForDisplayCollection<EDIMessage>), messages.GetType());
			AssertEquals("Messages Count", 6, messages.Count);
			Assert("Contains Message", messages.Contains(message1));
			Assert("Contains Message", messages.Contains(message2));
			Assert("Contains Message", messages.Contains(message3));
			Assert("Contains Message", messages.Contains(message4));
			Assert("Contains Message", messages.Contains(message5));
			Assert("Contains Message", messages.Contains(message6));

			var message7 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "7");
			UniversalEventMessageTest.LinkToBusinessObject(message7, house);
			message7.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			var message8 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "8");
			UniversalEventMessageTest.LinkToBusinessObject(message8, house);
			message8.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			Factory.Save();
			house.MessagesForDisplay.Reload(true);

			AssertEquals("Messages Count", 8, messages.Count);
		}

		public void TestHouseBillMessagesForDisplay_WhenItemAdded()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			var message = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			message.EM_LinkedObject = house;

			AssertEquals("Expected there to be no messages in this collection", 0, house.MessagesForDisplay.Count);

			house.MessagesForDisplay.Add(message);

			AssertEquals("Expected there to be one message in this collection", 1, house.MessagesForDisplay.Count);
		}

		public void TestPiggyBackedDocAddressValidation()
		{
			var house = Master.HouseBills.AddNew();
			AssertType<CusCAeMHHouseJobDocAddressValidation>(((IDocAddresses)house).PiggyBackedDocAddressValidation(master.Consolidator));
		}

		public void TestIsPostArrival()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house1 = master.HouseBills.AddNew();

			Assert("IsPostArrival", !house1.IsPostArrival);

			var message1 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, contextNodeXml: "<Context><Type>Status</Type><Value>0002</Value></Context><Context><Type>Status</Type><Value>0011</Value></Context>");
			message1.EM_LinkedObject = house1;

			Factory.Save();
			house1.MessagesForDisplay.Reload(true);

			Assert("IsPostArrival", house1.IsPostArrival);

			var house2 = master.HouseBills.AddNew();

			Assert("IsPostArrival", !house2.IsPostArrival);

			var message2 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, contextNodeXml: "<Context><Type>Status</Type><Value>0002</Value></Context><Context><Type>Status</Type><Value>0010</Value></Context>");
			message2.EM_LinkedObject = house2;

			Factory.Save();
			house2.MessagesForDisplay.Reload(true);

			var isPost1 = house2.IsPostArrival;
			var isPost2 = house2.IsPostArrival;

			Assert("IsPostArrival", isPost1);
			Assert("IsPostArrival", isPost2);
			AssertEquals(isPost1.GetHashCode(), isPost2.GetHashCode());

			message2.EM_MessageText = ZString.Empty;
			Assert("Expected there to be no 0002 status codes in message because we set message2.EM_MessageText to empty", !message2.StatusCodes.Contains("0011"));
			Assert("Expected there to be no 0010 status codes in message because we set message2.EM_MessageText to empty", !message2.StatusCodes.Contains("0010"));

			Factory.Save();
			message2.Reload();

			isPost1 = house2.IsPostArrival;

			Assert("IsPostArrival", !isPost1);
		}

		public void TestIsAccepted()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			Assert("IsAccepted", !house.IsAccepted);
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			Assert("IsAccepted", house.IsAccepted);
		}

		public void TestNumberOfUNDG()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			AssertEquals(0, house.NumberOfUNDG);

			var line01 = house.Items.AddNew();
			AssertEquals(0, house.NumberOfUNDG);

			var undg11 = line01.UNDGs.AddNew();
			AssertEquals(1, house.NumberOfUNDG);

			var line02 = house.Items.AddNew();
			var undg21 = line02.UNDGs.AddNew();
			AssertEquals(2, house.NumberOfUNDG);

			var line12 = line01.UNDGs.AddNew();
			AssertEquals(3, house.NumberOfUNDG);
		}

		public void TestAddressesCodeTypes()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();

			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "XXX");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "XXX1");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "XXX2");
			org.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.AccountSecurityCode, "XXX3");

			var address = house.DocAddresses.AddNew(org.MainAddress);
			address.E2_AddressType = DocAddressTypes.Codes.Warehouse;
			AssertEquals(OrgCusCode.CodeTypes.ControlledPremisesID, address.E2_GovRegNumType);
			AssertEquals("XXX1", address.E2_GovRegNum);

			var address2 = house.DocAddresses.AddNew(org.MainAddress);
			address2.E2_AddressType = DocAddressTypes.Codes.ImportBroker;
			AssertEquals(OrgCusCode.CACodeTypes.AccountSecurityCode, address2.E2_GovRegNumType);
			AssertEquals("XXX3", address2.E2_GovRegNum);

			var address3 = house.DocAddresses.AddNew(org.MainAddress);
			address3.E2_AddressType = DocAddressTypes.Codes.ReceivingForwarderAddress;
			AssertEquals(OrgCusCode.CodeTypes.CarrierCode, address3.E2_GovRegNumType);
			AssertEquals("XXX2", address3.E2_GovRegNum);

			var address4 = house.DocAddresses.AddNew(org.MainAddress);
			address4.E2_AddressType = DocAddressTypes.Codes.Carrier;
			AssertEquals(OrgCusCode.CodeTypes.CarrierCode, address4.E2_GovRegNumType);
			AssertEquals("XXX2", address4.E2_GovRegNum);

			var address5 = house.DocAddresses.AddNew(org.MainAddress);
			address5.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			AssertEquals(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, address5.E2_GovRegNumType);
			AssertEquals("XXX", address5.E2_GovRegNum);
		}

		public void TestRNSProcessingDate_ReadOnly()
		{
			var bo = Factory.New<CusCAeMHHouse>();
			Assert(bo.BW_RNSProcessingDateInfo.ReadOnly);
		}

		public void TestFormattedLatestD4MessageStatus()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "CA Notice Reason Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, codeType, "0003", "Test Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var bo = Factory.New<CusCAeMHHouse>();
			bo.BW_D4MessageStatus = "0003";
			AssertEquals("0003 - Test Description", bo.FormattedLatestD4MessageStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Enterprise.ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("CAHouseBilleManifest", Guid.Empty);
			Enterprise.ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("CAMasterBilleManifest", Guid.Empty);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Master.HouseBills.AddNew();
		}

		CusCAeMHMaster Master
		{
			get { return master ?? (master = Factory.NewWithValidTestData<CusCAeMHMaster>()); }
		}
		CusCAeMHMaster master;
	}
}
