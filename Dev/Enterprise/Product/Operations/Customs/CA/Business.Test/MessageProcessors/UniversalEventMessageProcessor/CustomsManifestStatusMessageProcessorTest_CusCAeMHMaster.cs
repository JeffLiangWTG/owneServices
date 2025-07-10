using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.Shared;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class CustomsManifestStatusMessageProcessorTest_CusCAeMHMaster : CAUniversalEventMessageProcessorTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAssertD4MessageStatusAndRNSProcessingDate()
		{
			var factory1 = new BusinessObjectFactory();
			var master1 = factory1.New<CusCAeMHMaster>();
			master1.BP_PrimaryCCN = "12345";
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var message = (UniversalEventMessage)Message.Clone();
			var master2 = factory2.Load<CusCAeMHMaster>(master1.PK);
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, master2);
			processor.Process();
			factory2.Save();

			var factory3 = new BusinessObjectFactory();
			var master3 = factory3.Load<CusCAeMHMaster>(master1.PK);
			AssertEquals("0001", master3.BP_D4MessageStatus);
			AssertEquals(new ZDateTime(2015, 11, 23, 9, 31, 0), master3.BP_RNSProcessingDate);
			master3.BP_D4MessageStatus = "0002";
			master3.BP_RNSProcessingDate = new ZDateTime(2015, 11, 20, 9, 31, 0);
			factory3.Save();

			var factory4 = new BusinessObjectFactory();
			message = (UniversalEventMessage)message.Clone();
			var master4 = factory4.Load<CusCAeMHMaster>(master1.PK);
			universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, master4);
			processor.Process();
			factory4.Save();

			var factory5 = new BusinessObjectFactory();
			var master5 = factory5.Load<CusCAeMHMaster>(master1.PK);
			AssertEquals("0001", master5.BP_D4MessageStatus);
			AssertEquals(new ZDateTime(2015, 11, 23, 9, 31, 0), master5.BP_RNSProcessingDate);
			master5.BP_D4MessageStatus = "0002";
			master5.BP_RNSProcessingDate = new ZDateTime(2015, 11, 25, 9, 31, 0);
			factory5.Save();

			var factory6 = new BusinessObjectFactory();
			message = (UniversalEventMessage)message.Clone();
			var master6 = factory6.Load<CusCAeMHMaster>(master1.PK);
			universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, master6);
			processor.Process();
			factory6.Save();

			var factory7 = new BusinessObjectFactory();
			var master7 = factory7.Load<CusCAeMHMaster>(master1.PK);
			AssertEquals("0002", master7.BP_D4MessageStatus);
			AssertEquals(new ZDateTime(2015, 11, 25, 9, 31, 0), master7.BP_RNSProcessingDate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAssertCustomsStatus()
		{
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_PrimaryCCN = "12345";

			var message = Factory.New<UniversalEventMessage>();
			message.EM_LinkedObject = master;
			message.EM_MessageText = File.ReadAllText(universalEventFilePath + "CustomsManifestStatus0001.xml");
			Factory.Save();

			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, master);
			processor.Process();

			Factory.Save();
			AssertEquals("CLR", master.BP_CustomsStatus);

			message = Factory.New<UniversalEventMessage>();
			message.EM_MessageText = File.ReadAllText(universalEventFilePath + "CustomsManifestStatus0002.xml");
			universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, master);
			processor.Process();

			Factory.Save();
			AssertEquals("NOM", master.BP_CustomsStatus);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoConcurrencyErrorExceptionThrow()
		{
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_PrimaryCCN = "12345";
			Factory.Save();
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var house = factory1.New<CusCAeMHHouse>();
			house.BW_BP_Master = master.PK;
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Validated;
			var house1 = factory1.New<CusCAeMHHouse>();
			house1.BW_BP_Master = master.PK;
			house1.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Validated;
			factory1.Save();
			var housePK = house.PK;
			house = null;

			var message = Factory.New<ACIForwarderMessage>();
			message.EM_LinkUniqueID = housePK;
			message.EM_LinkTable = CusCAeMHHouseSchema.Constants.TableName;
			message.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI;
			message.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message.EM_MessageNum = "110";
			message.EM_MessageText = EManifestResponseTest.MatchedMessageText.Replace("\r\n", "'");

			var message1 = Factory.New<UniversalEventMessage>();
			message1.EM_MessageText = File.ReadAllText(universalEventFilePath + "CustomsManifestStatus0002.xml");
			var universalEvent = message1.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message1, master);
			Factory.Saving += delegate
			{
				var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var house1 = anotherFactory.Load<CusCAeMHHouse>(housePK);
				house1.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.NotMatched;
				anotherFactory.Save();
				house1 = null;
			};
			processor.Process();
			AssertNoExceptionThrown(() => Factory.Save());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoCloseSetting_WhenProcessD4message()
		{
			CACustomsDataRegistry.Instance.AutoSendCloseMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var master = Factory.New<CusCAeMHMaster>();
			master.BP_MessageReference = "C10001000";
			master.BP_PrimaryCCN = "12345000000011";
			master.BP_MessageStatus = "AWD";

			var house = master.HouseBills.AddNew();
			house.BW_MessageReference = "CAH1000001";
			house.BW_HouseCCN = "8036 CAH1000001";

			var sentMessage = Factory.New<ACIHouseBillMessage>();
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			house.Messages.Add(sentMessage);

			var imcomingMessage = Factory.New<UniversalEventMessage>();
			imcomingMessage.EM_MessageText = File.ReadAllText(universalEventFilePath + "CustomsManifestStatus0002.xml");
			var universalEvent = imcomingMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, imcomingMessage, house);

			Factory.Save();
			processor.Process();

			AssertEquals("Close message should link to master", 1, master.Messages.Count);
			AssertEquals("Log created for close message", 1, logger.Logs.Count());
			AssertContains("LastLog", "The auto close report messages have been generated for C10001000", logger.Logs.FirstOrDefault().ToString());
		}

		protected override string UniversalEventFileName => "CustomsManifestStatus.xml";

		protected override string MessageInterpretationFileName => "CustomsManifestStatus.html";

		protected override string MessageTypeDescription => "Customs Manifest Status";

		protected override CAUniversalEventMessageProcessor GetMessageProcesser() => new CustomsManifestStatusMessageProcessor(logger, UniversalEvent, Message, entryHeader);
	}
}
