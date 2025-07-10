using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
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
	sealed class CusCAeMHMasterSingleTransactionTest : TestCase
	{
		class CusCAeMHMasterForMessageDeleteTest : CusCAeMHMaster
		{
			public CusCAeMHMasterForMessageDeleteTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
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
			var master = newFactory.New<CusCAeMHMasterForMessageDeleteTest>();
			var message1 = master.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			try
			{
				message1.OnSaving();
				newFactory.Save();
			}
			catch (Exception) { }
			messageNum1 = master.failedMessageNum;

			Db.Connection.RollbackTransaction();
			AssertEquals(false, message1.IsInDatabase);
			AssertEquals(true, message1.IsDeleted);

			Db.Connection.BeginTransaction();
			var message2 = master.Messages.AddNew();
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

	[TestedType(typeof(CusCAeMHMaster))]
	sealed class CusCAeMHMasterTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGivenCusCAeMHMaster_ThenBusinessObjectCollectionContainsOnlyLinkedEDIMessages()
		{
			var master1 = Factory.New<CusCAeMHMaster>();
			var master2 = Factory.New<CusCAeMHMaster>();

			var message1 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			var message2 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			var message3 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			var message4 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			message1.EM_LinkedObject = master1;
			message2.EM_LinkedObject = master1;
			message3.EM_LinkedObject = master1;
			message4.EM_LinkedObject = master2;
			message1.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			message2.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			message3.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			message4.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			Factory.Save();

			var master1Messages = master1.MessagesForDisplay;
			var master2Messages = master2.MessagesForDisplay;

			AssertEquals("There should be three messages in MessagesForDisplay in master1", 3, master1.MessagesForDisplay.Count);
			Assert("Expected master1 to contain message1", master1Messages.Contains(message1));
			Assert("Expected master1 to contain message2", master1Messages.Contains(message2));
			Assert("Expected master1 to contain message3", master1Messages.Contains(message3));

			AssertEquals("There should be one message in MessagesForDisplay in master2", 1, master2.MessagesForDisplay.Count);
			Assert("Expected master2 to contain message4", master2Messages.Contains(message4));
		}

		public void TestD4MessagesOnMasterBillAndHouseBills()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var houseBill1 = master.HouseBills.AddNew();
			houseBill1.BW_MessageReference = "CAH0000010";
			var houseBill2 = master.HouseBills.AddNew();
			houseBill2.BW_MessageReference = "CAH0000011";

			var eventTime = ZDateTime.Now.AddMinutes(1);

			var message1 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "5");
			message1.EM_LinkedObject = master;
			message1.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message2 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "5");
			message2.EM_LinkedObject = houseBill1;
			message2.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message3 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "5");
			message3.EM_LinkedObject = houseBill2;
			message3.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message4 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			message4.EM_LinkedObject = master;

			var message5 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime);
			message5.EM_LinkedObject = houseBill1;

			var message6 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7598", messageNumber: "4");
			message6.EM_LinkedObject = houseBill2;

			Factory.Save();

			var messages = master.D4MessagesOnMasterBillAndHouseBills;
			AssertEquals("Messages Count", 3, messages.Count());
			Assert("Contains Message 1", messages.Contains(message1));
			Assert("Contains Message 2", messages.Contains(message2));
			Assert("Contains Message 3", messages.Contains(message3));
			Assert("Contains Message 4", !messages.Contains(message4));
			Assert("Contains Message 5", !messages.Contains(message5));
			Assert("Contains Message 6", !messages.Contains(message6));
		}

		public void TestGetNewCusHAWBProcessTaskCollection()
		{
			var master = Factory.New<CusCAeMHMaster>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<CusCAeMHMasterProcessTask, CusCAeMHMaster>", typeof(ProcessTaskCollection<CusCAeMHMasterProcessTask, CusCAeMHMaster>), ((IWorkflowProvider)master).WorkflowItems);
		}

		[UseSnapshotProtection]
		public void TestShouldDefaultUniqueMessageReference()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			var master1 = factory1.New<CusCAeMHMaster>();
			var dbConnection = ((IDbConnected)factory1).Connection;
			master1.OnSaving();
			var reference1 = master1.BP_MessageReference;
			dbConnection.RollbackTransaction();

			var master2 = factory2.New<CusCAeMHMaster>();
			factory2.Save();
			var reference2 = master2.BP_MessageReference;

			AssertEquals(false, reference1.IsEmpty);
			AssertEquals(false, reference2.IsEmpty);
			AssertEquals(reference1, reference2);

			dbConnection.BeginTransaction();
			AssertEquals(reference1, master1.BP_MessageReference);

			factory1.Save();
			AssertNotEquals(reference1, master1.BP_MessageReference);
		}

		public void TestBP_CBSACarrierName()
		{
			var carrier = Factory.New<Universal.ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "4646";
			carrier.ZZ4_Description = "4646 Carrier Name";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.ZZ4_IsAir = true;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();

			var master = Factory.New<CusCAeMHMaster>();
			master.BP_CBSACarrierCode = "4646";
			AssertEquals("4646 Carrier Name", master.BP_CBSACarrierName);

			master.BP_CBSACarrierCode = "4647";
			AssertEquals(ZString.Empty, master.BP_CBSACarrierName);
		}

		public void TestReadyToClose()
		{
			var master = Factory.New<CusCAeMHMaster>();
			Assert(master.ReadyToClose);
			var house = master.HouseBills.AddNew();
			Assert(!master.ReadyToClose);
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Cancelled;
			Assert(!master.ReadyToClose);
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			Assert(master.ReadyToClose);
			var house2 = master.HouseBills.AddNew();
			Assert(!master.ReadyToClose);
			house2.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			Assert(master.ReadyToClose);
		}

		public void TestParentWorkflowProviders()
		{
			var master = Factory.New<CusCAeMHMaster>();
			Factory.Save();
			var prov = ((IWorkflowTriggerEventSource)master).ParentWorkflowProviders;
			AssertNull(master.Consol);
			AssertNotNull(prov);
			AssertEquals(0, prov.Count);

			var consol = Factory.New<ForwardingConsol>();
			master.BP_ParentID = consol.PK;
			master.BP_ParentTableCode = consol.TablePrefix;
			Factory.Save();

			prov = ((IWorkflowTriggerEventSource)master).ParentWorkflowProviders;
			AssertNotNull(prov);
			AssertNotNull(master.Consol);
			AssertEquals(1, prov.Count);
			AssertEquals(consol, prov[0]);
		}

		public void TestOnFactorySavingBeforeTransactionCore()
		{
			var master = Factory.New<CusCAeMHMaster>();
			Factory.Save();

			AssertEquals("Message Status Change Log Count", master.Logs.Find(o => o.SL_SE_NKEvent == Events.MessageStatusChange.Code).Count(), 0);
			AssertEquals("Status Change Log Count", master.Logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).Count(), 0);
			master.BP_MessageStatus = "AWO";
			master.BP_CustomsStatus = "CAN";
			Factory.Save();
			var messagelog = master.Logs.Find(o => o.SL_SE_NKEvent == Events.MessageStatusChange.Code).FirstOrDefault();
			var customlog = master.Logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).FirstOrDefault();
			AssertNotNull(messagelog);
			AssertNotNull(customlog);
			AssertEquals("Message Status Change Log  Reference", "AWO - Awaiting Original Close", messagelog.SL_Reference);
			AssertEquals("Status Change Log Reference", "CAN - Canceled Close", customlog.SL_Reference);
		}

		public void TestTemplateCopy()
		{
			RefUNLOCO catorUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "CATOR"));
			if (catorUNLOCO == null)
			{
				catorUNLOCO = Factory.New<RefUNLOCO>();
				catorUNLOCO.RL_Code = "CATOR";
			}
			var locoMap = Factory.New<RefLocoMap>();
			locoMap.RY_LocalPortCode = "EEEE";
			locoMap.RY_RL_NKLocoPort = "CATOR";
			locoMap.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMap.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sub;

			var master = Factory.New<CusCAeMHMaster>();
			master.BP_ModeOfTransport = TransportTypeList.Codes.Sea;
			master.BP_MasterBill = "MASTER";
			master.BP_PrimaryCCN = "CCN";
			master.BP_MasterHouseBill = "SUBMASTER";
			master.BP_MasterHouseCCN = "PCN";
			master.BP_CBSACarrierCode = "9999";
			master.BP_CBSADischargePort = "8888";
			master.BP_CBSADischargeSubLocation = "7777";
			master.BP_CustomsStatus = "AAA";
			master.BP_MessageStatus = "BBB";
			master.BP_RL_NKDiscPort = "CATOR";
			master.BP_ATA = ZDateTime.Now;
			master.BP_AmendReasonCode = "CCC";

			var house = master.HouseBills.AddNew();
			house.BW_HouseBill = "HOUSE";
			house.BW_HouseCCN = "HCCN";
			house.BW_MovementType = "24";
			house.BW_B2BComments = "AAAAA";
			house.BW_DGSpecialInstructions = "BBBBB";
			house.BW_HandlingInstructions = "CCCCC";
			house.BW_IsMasterHouse = true;
			house.BW_Volume = 1m;
			house.BW_VolumeUQ = "MTQ";
			house.BW_Weight = 1m;
			house.BW_WeightUQ = "KGM";
			house.BW_CBSAReleasePort = "1111";
			house.BW_CBSAReleaseSubLocation = "2222";
			house.BW_CustomsStatus = "DDD";
			house.BW_MessageStatus = "EEE";
			house.BW_ATDCode = "FFF";
			house.BW_UCR = "GGG";
			house.BW_AmendReasonCode = "HHD";

			var item = house.Items.AddNew();
			item.BX_LineNumber = 1;
			item.BX_HSCode = "12345678";
			item.BX_Description = "DESC";
			item.BX_Quantity = 2m;
			item.BX_QuantityUQ = "KGM";
			item.BX_Marks = "JJJJJJJ";
			item.BX_IsDangerousInBulk = true;

			var undg = item.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;

			Factory.Save();

			var templateCopy = (CusCAeMHMaster)master.TemplateCopy();
			AssertPropertyCopied(master.BP_ModeOfTransportInfo, templateCopy.BP_ModeOfTransportInfo);
			AssertPropertyCopied(master.BP_MasterBillInfo, templateCopy.BP_MasterBillInfo, shouldNotCopy: true);
			AssertPropertyCopied(master.BP_PrimaryCCNInfo, templateCopy.BP_PrimaryCCNInfo, shouldNotCopy: true);
			AssertPropertyCopied(master.BP_MasterHouseBillInfo, templateCopy.BP_MasterHouseBillInfo, shouldNotCopy: true);
			AssertPropertyCopied(master.BP_MasterHouseCCNInfo, templateCopy.BP_MasterHouseCCNInfo, shouldNotCopy: true);
			AssertPropertyCopied(master.BP_CBSACarrierCodeInfo, templateCopy.BP_CBSACarrierCodeInfo);
			AssertPropertyCopied(master.BP_CBSADischargePortInfo, templateCopy.BP_CBSADischargePortInfo);
			AssertPropertyCopied(master.BP_CBSADischargeSubLocationInfo, templateCopy.BP_CBSADischargeSubLocationInfo);
			AssertPropertyCopied(master.BP_CustomsStatusInfo, templateCopy.BP_CustomsStatusInfo, shouldNotCopy: true);
			AssertPropertyCopied(master.BP_MessageStatusInfo, templateCopy.BP_MessageStatusInfo, shouldNotCopy: true);
			AssertPropertyCopied(master.BP_MessageReferenceInfo, templateCopy.BP_MessageReferenceInfo, shouldNotCopy: true);
			AssertPropertyCopied(master.BP_RL_NKDiscPortInfo, templateCopy.BP_RL_NKDiscPortInfo);
			AssertPropertyCopied(master.BP_ATAInfo, templateCopy.BP_ATAInfo, shouldNotCopy: true);
			AssertPropertyCopied(master.BP_AmendReasonCodeInfo, templateCopy.BP_AmendReasonCodeInfo, shouldNotCopy: true);

			AssertEquals("One house copied", 1, templateCopy.HouseBills.Count);
			var houseCopy = templateCopy.HouseBills[0];
			AssertPropertyCopied(house.BW_HouseBillInfo, houseCopy.BW_HouseBillInfo, shouldNotCopy: true);
			AssertPropertyCopied(house.BW_HouseCCNInfo, houseCopy.BW_HouseCCNInfo, shouldNotCopy: true);
			AssertPropertyCopied(house.BW_MovementTypeInfo, houseCopy.BW_MovementTypeInfo);
			AssertPropertyCopied(house.BW_B2BCommentsInfo, houseCopy.BW_B2BCommentsInfo);
			Assert("BW_IsMasterHouse", houseCopy.BW_IsMasterHouse);
			AssertPropertyCopied(house.BW_VolumeInfo, houseCopy.BW_VolumeInfo);
			AssertPropertyCopied(house.BW_VolumeUQInfo, houseCopy.BW_VolumeUQInfo);
			AssertPropertyCopied(house.BW_WeightInfo, houseCopy.BW_WeightInfo);
			AssertPropertyCopied(house.BW_WeightUQInfo, houseCopy.BW_WeightUQInfo);
			AssertPropertyCopied(house.BW_CBSAReleasePortInfo, houseCopy.BW_CBSAReleasePortInfo);
			AssertPropertyCopied(house.BW_CBSAReleaseSubLocationInfo, houseCopy.BW_CBSAReleaseSubLocationInfo);
			AssertPropertyCopied(house.BW_CustomsStatusInfo, houseCopy.BW_CustomsStatusInfo, shouldNotCopy: true);
			AssertPropertyCopied(house.BW_MessageStatusInfo, houseCopy.BW_MessageStatusInfo, shouldNotCopy: true);
			AssertPropertyCopied(house.BW_MessageReferenceInfo, houseCopy.BW_MessageReferenceInfo, shouldNotCopy: true);
			AssertPropertyCopied(house.BW_ATDCodeInfo, houseCopy.BW_ATDCodeInfo);
			AssertPropertyCopied(house.BW_UCRInfo, houseCopy.BW_UCRInfo, shouldNotCopy: true);
			AssertPropertyCopied(house.BW_AmendReasonCodeInfo, houseCopy.BW_AmendReasonCodeInfo, shouldNotCopy: true);

			AssertEquals("One item copied", 1, houseCopy.Items.Count);
			var itemCopy = houseCopy.Items[0];
			AssertPropertyCopied(item.BX_LineNumberInfo, itemCopy.BX_LineNumberInfo);
			AssertPropertyCopied(item.BX_HSCodeInfo, itemCopy.BX_HSCodeInfo);
			AssertPropertyCopied(item.BX_DescriptionInfo, itemCopy.BX_DescriptionInfo);
			AssertPropertyCopied(item.BX_QuantityInfo, itemCopy.BX_QuantityInfo);
			AssertPropertyCopied(item.BX_QuantityUQInfo, itemCopy.BX_QuantityUQInfo);
			AssertPropertyCopied(item.BX_MarksInfo, itemCopy.BX_MarksInfo);
			Assert("BX_IsDangerousInBulk", itemCopy.BX_IsDangerousInBulk);

			AssertEquals("One undg copied", 1, itemCopy.UNDGs.Count);
			var undgCopy = itemCopy.UNDGs[0];
			AssertPropertyCopied(undg.DI_DGInfo, undgCopy.DI_DGInfo);
		}

		void AssertPropertyCopied(ZPropertyInfo originalProperty, ZPropertyInfo templatCopyProperty, bool shouldNotCopy = false)
		{
			Assert(string.Format("Re-condition, original property ({0}) should have a value", originalProperty.Name), !originalProperty.Value.IsEmpty);
			if (shouldNotCopy)
			{
				Assert(string.Format("Property ({0}) should NOT be copied", originalProperty.Name), templatCopyProperty.Value.IsEmpty);
			}
			else
			{
				AssertEquals(string.Format("Template copy property ({0}) should have the same value as the original", originalProperty.Name), originalProperty.Value, templatCopyProperty.Value);
			}
		}

		public void TestDefaultingFromCustomsPort()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "!ZZ";
			var locoMap = Factory.New<RefLocoMap>();
			locoMap.RY_LocalPortCode = "PZZ!";
			locoMap.RY_RL_NKLocoPort = "!ZZ";
			locoMap.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMap.RY_SystemUsage = CALocoMapSystemUsageList.Codes.All;

			var job = Factory.New<CusCAeMHMaster>();
			job.BP_ModeOfTransport = TransportTypeList.Codes.Sea;
			job.BP_CBSADischargePort = "PZZ!";
			AssertEquals("Arrival port defaults", "!ZZ", job.BP_RL_NKDiscPort);

			job.BP_CBSADischargePort = ZString.Empty;
			AssertEquals("Clearing customs port does no change existing arrival port", "!ZZ", job.BP_RL_NKDiscPort);

			job.BP_RL_NKDiscPort = "!QQ";
			job.BP_CBSADischargePort = "PZZ!";
			AssertEquals("Not saved into DB yet, will be reset", "!ZZ", job.BP_RL_NKDiscPort);
			Factory.Save();
			job.BP_RL_NKDiscPort = "!QQ";
			job.BP_CBSADischargePort = "";
			job.BP_CBSADischargePort = "PZZ!";
			AssertEquals("Saved into DB yet, will not change", "!QQ", job.BP_RL_NKDiscPort);

			job.BP_RL_NKDiscPort = ZString.Empty;
			job.BP_CBSADischargePort = ZString.Empty;
			var consol = Factory.New<ForwardingConsol>();
			job.BP_ParentID = consol.PK;
			job.BP_CBSADischargePort = "PZZ!";
			Assert("Arrival port does not default when attached to shipment", job.BP_RL_NKDiscPort.IsEmpty);
		}

		public void TestPortOfDischarge()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0497", "Nanaimo", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var master = Factory.New<CusCAeMHMaster>();
			master.BP_CBSADischargePort = "497";
			AssertEquals("0497", master.BP_CBSADischargePort);
			AssertEquals("Nanaimo", master.BP_CBSADischargePortName);
		}

		public void TestEnableAndSynchronise()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			master.BP_ParentID = consol.PK;
			master.BP_ParentTableCode = consol.TablePrefix;
			house.BW_ParentID = shipment.PK;
			house.BW_ParentTableCode = shipment.TablePrefix;

			Assert(!master.ConsolSynchroniser.IsEnabled);
			master.EnableAndSynchronise();
			Assert(master.ConsolSynchroniser.IsEnabled);
		}

		public void TestShouldSynchroniseWithConsol()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house1 = master.HouseBills.AddNew();
			var house2 = master.HouseBills.AddNew();
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			master.BP_ParentID = consol.PK;
			master.BP_ParentTableCode = consol.TablePrefix;
			house1.BW_ParentID = shipment1.PK;
			house1.BW_ParentTableCode = shipment1.TablePrefix;
			house2.BW_ParentID = shipment2.PK;
			house2.BW_ParentTableCode = shipment2.TablePrefix;

			Assert(master.ShouldSynchroniseWithConsol);
			house1.BW_OverrideFreightDefaults = true;
			Assert(!master.ShouldSynchroniseWithConsol);
			house1.BW_OverrideFreightDefaults = false;
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.NotMatched;
			Assert(!master.ShouldSynchroniseWithConsol);
		}

		public void TestBP_MessageReference()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C111166666777774444";
			var master = Factory.NewWithValidTestData<CusCAeMHMaster>();
			master.BP_ParentID = consol.PK;
			master.BP_ParentTableCode = JobConsolSchema.Constants.Prefix;
			master.BP_MessageStatus = "AAA";

			Factory.Save();
			AssertEquals("166666777774444", master.BP_MessageReference);

			master.BP_MessageReference = "X123456";
			master.BP_MessageStatus = "BBB";
			master.OnSaved(false);
			AssertEquals("X123456", master.BP_MessageReference);
			AssertEquals("AAA", master.BP_MessageStatus);

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "C123456";
			var master2 = Factory.NewWithValidTestData<CusCAeMHMaster>();
			master2.BP_ParentID = consol2.PK;
			master2.BP_ParentTableCode = JobConsolSchema.Constants.Prefix;
			master2.BP_MessageStatus = "AAA";

			Factory.Save();
			AssertEquals("C123456", master2.BP_MessageReference);
		}

		public void TestICAEDIFACTMessageAttacheeMembers()
		{
			Master.BP_IsActive = false;
			Master.BP_MessageReference = "JOBREF";
			Master.BP_CustomsStatus = "XXX";
			Master.BP_MessageStatus = "AAA";

			Assert(((ICAEDIFACTMessageAttachee)Master).IsCancelled);
			Assert(((ICAEDIFACTMessageAttachee)Master).HasChanges);
			AssertEquals("CLS-JOBREF", ((ICAEDIFACTMessageAttachee)Master).JobIdentification);
			AssertEquals("XXX", ((ICAEDIFACTMessageAttachee)Master).JobStatus);
			((ICAEDIFACTMessageAttachee)Master).JobStatus = "YYY";
			AssertEquals("YYY", Master.BP_CustomsStatus);
			AssertEquals("AAA", ((ICAEDIFACTMessageAttachee)Master).MessageStatus);
			((ICAEDIFACTMessageAttachee)Master).MessageStatus = "BBB";
			AssertEquals("BBB", Master.BP_MessageStatus);
			AssertEquals(Master.PK, ((ICAEDIFACTMessageAttachee)Master).TopLevelBusinessObject.PK);
			((ICAEDIFACTMessageAttachee)Master).AddMessage(Factory.New<EDIMessage>());
			AssertEquals(1, Master.Messages.Count);
		}

		public void TestIControllerIDProviderMembers()
		{
			AssertEquals(Master.PK, ((IControllerIDProvider)Master).BusinessObjectPK);
			AssertEquals(ControllerIDs.Customs.CA.CAHouseBilleManifest, ((IControllerIDProvider)Master).ControllerID);
		}

		public void TestDefaultValues()
		{
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, Master.BP_GB_Branch);
			AssertEquals("Carrier Code", "9090", Master.BP_CBSACarrierCode);
			AssertEquals("NCT", 1, Master.Containers.Count);
			AssertEquals("NCT", "NCT", Master.Containers[0].BQ_ContainerNumber);

			var currentBranch = GlbBranch.CurrentBranch;
			var oldOrgProxy = currentBranch.GB_OH_OrgProxy;
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			currentBranch.GB_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var orgCusCode = currentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "2345", Core.Constants.CountryCodes.Canada);
			var master = Factory.New<CusCAeMHMaster>();
			AssertEquals("Carrier Code", "2345", master.BP_CBSACarrierCode);

			currentBranch.GB_OH_OrgProxy = oldOrgProxy;
			orgProxy.Delete();
			Factory.Save();
		}

		public void TestHumanReadableShortcutNameCore()
		{
			var ccn = "803612345678";
			var master = Factory.New<CusCAeMHMaster>();
			AssertEquals("Default HumanReadableShortcutName", string.Empty, master.HumanReadableShortcutName);
			Factory.Save();
			AssertEquals("HumanReadableShortcutName without ccn", master.BP_MessageReference, master.HumanReadableShortcutName);
			master.BP_PrimaryCCN = ccn;
			AssertEquals("HumanReadableShortcutName with ccn", master.BP_MessageReference + " - CCN: " + ccn, master.HumanReadableShortcutName);
		}

		public void TestHumanReadableNameCore()
		{
			var master = Factory.New<CusCAeMHMaster>();
			AssertEquals("HumanReadableShortcutName without ref number", "eManifest", master.HumanReadableName);
			Factory.Save();
			AssertEquals("HumanReadableShortcutName with ccn", "eManifest " + master.BP_MessageReference, master.HumanReadableName);
		}

		public void TestMessagesForDisplay()
		{
			var master = Factory.New<CusCAeMHMaster>();

			var message1 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			message1.EM_LinkedObject = master;
			var eventTime = ZDateTime.Now.AddMinutes(1);

			var message2 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime);
			message2.EM_LinkedObject = master;

			var message3 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7598", messageNumber: "4");
			message3.EM_LinkedObject = master;

			var message4 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "1");
			message4.EM_LinkedObject = master;

			var message5 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "2");
			message5.EM_LinkedObject = master;

			var message6 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime.AddHours(-1), interchangeNumber: "7599", messageNumber: "3");
			message6.EM_LinkedObject = master;

			var message7 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime.AddHours(-1), interchangeNumber: "7599", messageNumber: "5");
			message7.EM_LinkedObject = master;
			message7.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message8 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "5");
			message8.EM_LinkedObject = master;
			message8.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			Factory.Save();

			var messages = master.MessagesForDisplay;
			AssertEquals(typeof(EDIMessageForDisplayCollection<EDIMessage>), messages.GetType());
			AssertEquals("Messages Count", 8, messages.Count);
			Assert("Contains Message 1", messages.Contains(message1));
			Assert("Contains Message 2", messages.Contains(message2));
			Assert("Contains Message 3", messages.Contains(message3));
			Assert("Contains Message 4", messages.Contains(message4));
			Assert("Contains Message 5", messages.Contains(message5));
			Assert("Contains Message 6", messages.Contains(message6));
			Assert("Contains Message 7", messages.Contains(message7));
			Assert("Contains Message 8", messages.Contains(message8));
		}

		public void TestMessagesForDisplay_WhenItemAdded()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var message = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);

			AssertEquals("Expected there to be no messages in this collection ", 0, master.MessagesForDisplay.Count);

			master.MessagesForDisplay.Add(message);

			AssertEquals("Expected there to be one message in this collection", 1, master.MessagesForDisplay.Count);
		}

		public void TestPiggyBackedDocAddressValidation()
		{
			AssertType<CusCAeMHMasterJobDocAddressValidation>(((IDocAddresses)Master).PiggyBackedDocAddressValidation(master.Consolidator));
		}

		public void TestIsPostArrival()
		{
			var master1 = Factory.New<CusCAeMHMaster>();

			Assert("IsPostArrival", !master1.IsPostArrival);

			var message1 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, contextNodeXml: "<Context><Type>Status</Type><Value>0002</Value></Context><Context><Type>Status</Type><Value>0011</Value></Context>");
			var stmAlog1 = Factory.New<StmALog>();

			using (stmAlog1.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog1.SL_Parent = master1.PK;
				stmAlog1.SL_Table = master1.TableName;
			}

			var genPivot1 = Factory.New<GenPivot>();
			genPivot1.XX_Relation1ID = stmAlog1.PK;
			genPivot1.XX_Relation2ID = message1.PK;
			genPivot1.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();
			master1.MessagesForDisplay.Reload(true);

			Assert("IsPostArrival", master1.IsPostArrival);

			var master2 = Factory.New<CusCAeMHMaster>();

			Assert("IsPostArrival", !master2.IsPostArrival);

			var message2 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, contextNodeXml: "<Context><Type>Status</Type><Value>0002</Value></Context><Context><Type>Status</Type><Value>0010</Value></Context>");
			var stmAlog2 = Factory.New<StmALog>();

			using (stmAlog2.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog2.SL_Parent = master2.PK;
				stmAlog2.SL_Table = master2.TableName;
			}

			var genPivot2 = Factory.New<GenPivot>();
			genPivot2.XX_Relation1ID = stmAlog2.PK;
			genPivot2.XX_Relation2ID = message2.PK;
			genPivot2.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();
			master2.MessagesForDisplay.Reload(true);

			var isPost1 = master2.IsPostArrival;
			var isPost2 = master2.IsPostArrival;
			Assert("IsPostArrival", isPost1);
			Assert("IsPostArrival", isPost2);
			AssertEquals(isPost1.GetHashCode(), isPost2.GetHashCode());

			message2.EM_MessageText = ZString.Empty;
			Assert("Expected there to be no 0002 status codes in message because we set message2.EM_MessageText to empty", !message2.StatusCodes.Contains("0011"));
			Assert("Expected there to be no 0010 status codes in message because we set message2.EM_MessageText to empty", !message2.StatusCodes.Contains("0010"));

			Factory.Save();

			isPost1 = master2.IsPostArrival;

			Assert("IsPostArrival", !isPost1);
		}

		public void TestRNSProcessingDate_ReadOnly()
		{
			var bo = Factory.New<CusCAeMHMaster>();
			Assert(bo.BP_RNSProcessingDateInfo.ReadOnly);
		}

		public void TestFormattedLatestD4MessageStatus()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "CA Notice Reason Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, codeType, "0003", "Test Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var bo = Factory.New<CusCAeMHMaster>();
			bo.BP_D4MessageStatus = "0003";
			AssertEquals("0003 - Test Description", bo.FormattedLatestD4MessageStatus);
		}

		public void TestLatestD4MessageStatusAndDescriptions()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "CA Notice Reason Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, codeType, "S001", "Positive Functional Acknowledgement.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, codeType, "S002", "Negative Functional Acknowledgement.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var master = Factory.New<CusCAeMHMaster>();
			var house1 = master.HouseBills.AddNew();
			house1.BW_D4MessageStatus = "S001";
			var house2 = master.HouseBills.AddNew();
			house2.BW_D4MessageStatus = "S002";
			master.HouseBills.AddNew();

			AssertEquals("S001| S002", master.HouseBillLatestD4MessageStatus);
			AssertEquals("Positive Functional Acknowledgement.| Negative Functional Acknowledgement.", master.HouseBillLatestD4MessageStatusDescription);
		}

		#region Job Header Deactivation Tests

		public void TestJobHeaderIsNotDeactivatedWhenFactoryHasInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(true);
		}

		public void TestJobHeaderIsDeactivatedWhenFactoryDoesNotHaveInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(false);
		}

		void AssertJobHeaderDeactivation(bool hasInvoicingPlugInGUIContext)
		{
			var master = Factory.New<CusCAeMHMaster>();
			var jobLoader = new JobHeader.Loader(master);
			var job = jobLoader.TryCreate();
			Factory.Save();

			master.IsCancelled = true;
			if (hasInvoicingPlugInGUIContext)
			{
				Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			}

			var assertionMessage1 = string.Format("master {0} have InvoicingPluginGUI Business Context", hasInvoicingPlugInGUIContext ? "should" : "should not");
			var assertionMessage2 = string.Format("Job {0} be deactivated by OnSaving method", hasInvoicingPlugInGUIContext ? "should not" : "should");

			Assert("Deactivating master, IsCancelled flag should be set to true", master.IsCancelled);
			Assert("Deactivating master, IsCancelledInfo should have changes", master.IsCancelledHasChanged);
			AssertEquals(assertionMessage1, hasInvoicingPlugInGUIContext, master.HasContext(BusinessContext.InvoicingPlugInGUI));
			Assert("Job is not yet deactivated", !job.IsCancelled);

			Factory.Save();

			AssertEquals(assertionMessage2, !hasInvoicingPlugInGUIContext, job.IsCancelled);
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return Master;
		}

		CusCAeMHMaster Master
		{
			get { return master ?? (master = Factory.New<CusCAeMHMaster>()); }
		}
		CusCAeMHMaster master;

		protected override void SetUp()
		{
			base.SetUp();
			ValidationTestHelper.AddCarrierCodeToCurrentCompany(Factory, "9090");

			Enterprise.ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("CAHouseBilleManifest", Guid.Empty);
			Enterprise.ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("CAMasterBilleManifest", Guid.Empty);
		}
	}
}
