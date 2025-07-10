using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Events = Enterprise.ZArchitecture.Business.Events;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class CustomsManifestStatusMessageProcessorTest_Declaration : CAUniversalEventMessageProcessorTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestD4NoticeLogged()
		{
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "555");
			var number = declaration.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "12345";

			var message = (UniversalEventMessage)Message.Clone();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			Factory.Save();
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, declaration);
			processor.Process();
			Factory.Save();

			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
			filter.AddToFilter(StmALogSchema.SL_Parent, declaration.PK);
			var logs = Factory.Load<StmALog>(filter);
			AssertEquals("Event Created", 2, logs.Length);
			AssertEquals("Event time should from Universal Event", new ZDateTime(2015, 11, 23, 9, 31, 0), logs[0].SL_EventTime);
			AssertEquals("Event time should from Universal Event", new ZDateTime(2015, 11, 23, 9, 31, 0), logs[1].SL_EventTime);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoSendRNSQueryIfNeeded()
		{
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "555");
			var number = declaration.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "12345";

			var message = (UniversalEventMessage)Message.Clone();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			Factory.Save();
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, declaration);
			processor.Process();
			Factory.Save();
			AssertEquals("No RNS query should be sent", 0, declaration.Messages.Count(m => m is RNSRequestMessage));
			AssertEquals(string.Empty, logger.ToString());

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			message = (UniversalEventMessage)message.Clone();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			Factory.Save();
			universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, declaration);
			processor.Process();
			Factory.Save();
			AssertEquals("An RNS query should be sent", 1, entryHeader.Messages.Count(m => m is RNSRequestMessage));
			AssertEquals("Request RNS Status Query for Declaration Job001 message queued for sending.", logger.ToString());

			message = (UniversalEventMessage)message.Clone();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			Factory.Save();
			universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, declaration);
			processor.Process();
			Factory.Save();
			AssertEquals("No more RNS query should be sent", 1, entryHeader.Messages.Count(m => m is RNSRequestMessage));
			AssertEquals(string.Empty, logger.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoSendRNSQueryIfNeeded_BOIsModifiedInOtherFactory()
		{
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "555");
			var number = declaration.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "12345";

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			declaration.JE_EntryStatus = EDIReleaseImportEntryStatusList.Codes.AwaitingCustomsProcessing;
			var message = (UniversalEventMessage)Message.Clone();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var declarationLoadedInFactory1 = factory1.Load<JobDeclaration>(declaration.PK);
			var entryLoadedInFactory1 = factory1.Load<CusEntryHeader>(entryHeader.PK);

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var declarationLoadedInFactory2 = factory2.Load<JobDeclaration>(declaration.PK);
			declarationLoadedInFactory2.JE_EntryStatus = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;
			factory2.Save();

			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, declarationLoadedInFactory1);
			processor.Process();
			Factory.Save();
			AssertEquals("An RNS query should be sent", 1, entryLoadedInFactory1.Messages.Count(m => m is RNSRequestMessage));
			AssertEquals("Request RNS Status Query for Declaration Job001 message queued for sending.", logger.ToString());
		}

		protected override string UniversalEventFileName => "CustomsManifestStatus.xml";

		protected override string MessageInterpretationFileName => "CustomsManifestStatus.html";

		protected override string MessageTypeDescription => "Customs Manifest Status";

		protected override CAUniversalEventMessageProcessor GetMessageProcesser() => new CustomsManifestStatusMessageProcessor(logger, UniversalEvent, Message, entryHeader);
	}
}
