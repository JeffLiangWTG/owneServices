using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class CustomsManifestStatusMessageProcessorTest_CusCAeMHHouse : CAUniversalEventMessageProcessorTest
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAssertD4MessageStatusAndRNSProcessingDate()
		{
			var factory1 = new BusinessObjectFactory();
			var master1 = factory1.New<CusCAeMHMaster>();
			var house1 = master1.HouseBills.AddNew();
			house1.BW_HouseCCN = "12345";
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var message = (UniversalEventMessage)Message.Clone();
			var house2 = factory2.Load<CusCAeMHHouse>(house1.PK);
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, house2);
			processor.Process();
			factory2.Save();

			var factory3 = new BusinessObjectFactory();
			var house3 = factory3.Load<CusCAeMHHouse>(house1.PK);
			AssertEquals("0001", house3.BW_D4MessageStatus);
			AssertEquals(new ZDateTime(2015, 11, 23, 9, 31, 0), house3.BW_RNSProcessingDate);
			house3.BW_D4MessageStatus = "0002";
			house3.BW_RNSProcessingDate = new ZDateTime(2015, 11, 20, 9, 31, 0);
			factory3.Save();

			var factory4 = new BusinessObjectFactory();
			message = (UniversalEventMessage)message.Clone();
			var house4 = factory4.Load<CusCAeMHHouse>(house1.PK);
			universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, house4);
			processor.Process();
			factory4.Save();

			var factory5 = new BusinessObjectFactory();
			var house5 = factory5.Load<CusCAeMHHouse>(house1.PK);
			AssertEquals("0001", house5.BW_D4MessageStatus);
			AssertEquals(new ZDateTime(2015, 11, 23, 9, 31, 0), house5.BW_RNSProcessingDate);
			house5.BW_D4MessageStatus = "0002";
			house5.BW_RNSProcessingDate = new ZDateTime(2015, 11, 25, 9, 31, 0);
			factory5.Save();

			var factory6 = new BusinessObjectFactory();
			message = (UniversalEventMessage)message.Clone();
			var house6 = factory6.Load<CusCAeMHHouse>(house1.PK);
			universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, house6);
			processor.Process();
			factory6.Save();

			var factory7 = new BusinessObjectFactory();
			var house7 = factory7.Load<CusCAeMHHouse>(house1.PK);
			AssertEquals("0002", house7.BW_D4MessageStatus);
			AssertEquals(new ZDateTime(2015, 11, 25, 9, 31, 0), house7.BW_RNSProcessingDate);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEM_MessageSubTypeShouldBeUpdatedToD4()
		{
			var message = (UniversalEventMessage)Message.Clone();
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, house);
			processor.Process();
			AssertEquals(UniversalEventMessageTypes.Codes.D4Notices, message.EM_MessageSubType);
			AssertEquals(ZString.Empty, message.GetSystemDefinedValue<ZString>(message.XMLCustomsMessageType));
		}

		protected override string UniversalEventFileName => "CustomsManifestStatus.xml";

		protected override string MessageInterpretationFileName => "CustomsManifestStatus.html";

		protected override string MessageTypeDescription => "Customs Manifest Status";

		protected override CAUniversalEventMessageProcessor GetMessageProcesser() => new CustomsManifestStatusMessageProcessor(logger, UniversalEvent, Message, entryHeader);
	}
}
