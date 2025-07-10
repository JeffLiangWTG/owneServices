using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	public class IncomingInterchangeProcessorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestExecute()
		{
			CustomsDataRegistry.Instance.CustomsWareCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "AAAA");
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CustomsWare;
			interchange.EI_BodyText = "BODYTEXT";
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_To = "TO";
			interchange.EI_From = "FROM";
			Factory.Save();
			new IncomingInterchangeProcessor().ExecuteBatch();
			interchange.Reload();
			NUnit.Framework.Assert.That(interchange.EI_Status, Is.EqualTo(EDIInterchangeStatusList.Codes.Received).Using(CustomComparers.TypeComparison), "EI_Status");
			NUnit.Framework.Assert.That(interchange.ContainedMessages.Count, Is.EqualTo(1));
			var message = interchange.ContainedMessages[0];
			NUnit.Framework.Assert.That(message.EM_ApplicationCode, Is.EqualTo(interchange.EI_ApplicationCode));
			NUnit.Framework.Assert.That(message.EM_MessageType, Is.EqualTo(ApplicationCodeList.Codes.CustomsWare).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EM_MessageText, Is.EqualTo("BODYTEXT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(EDIMessageStatusList.Codes.Queued).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EM_ReceiveTransmit, Is.EqualTo(EDIInterchange.Direction.Receive).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestExecute_FalseArePrerequisiteReceivingConditionsMet()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CustomsWare;
			interchange.EI_BodyText = "BODYTEXT";
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_To = "TO";
			interchange.EI_From = "FROM";
			Factory.Save();
			var logger = new LoggingInformation();
			var processor = new MockIncomingInterchangeProcessor(logger);
			processor.ExecuteBatch();
			interchange.Reload();
			NUnit.Framework.Assert.That(interchange.EI_Status, Is.EqualTo("CAN").Using(CustomComparers.TypeComparison), "EI_Status");
			NUnit.Framework.Assert.That(logger.UserLogStrings.Count, Is.EqualTo(1), "One log should be created");
			NUnit.Framework.Assert.That(logger.UserLogStrings[0], Does.Contain("Interchange '1' has not been processed for company 'Eagle Datamation International': Please set up the ABM Web Service Company (code 'EDI') in the registry (Customs -> Integration -> ABM -> Web Service -> Company)"), "Log message");
			var ediMessageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CustomsWare);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Queued);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIInterchange.Direction.Receive);
			ediMessageQuery.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));
			var msgs = Factory.Load<EDIMessage>(ediMessageQuery);
			NUnit.Framework.Assert.That(msgs.Length, Is.EqualTo(0), "No messages created");
		}
	}
	class MockIncomingInterchangeProcessor : IncomingInterchangeProcessor
	{
		public MockIncomingInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}
	}
}
