using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GovernmentGateway.NCTS.Testing;
using Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.NCTS.Testing
{
	public class CTCOutgoingMessageProcessorTests : TestCaseWithFactory
	{
		public void TestProcess()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = "NCT00000001";
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "Ncts001";
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG";
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "ABC";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			aaaBranch.GB_OH_OrgProxy = org1.PK;
			nctsHeader.Declarant.E2_OA_Address = org1.MainAddress.PK;
			nctsHeader.BH_GB = aaaBranch.PK;

			CTCMessagingTests.CreateCredential(nctsHeader, "GB123456789000", "CTC");

			var validMsg = Factory.New<EDIMessage>();
			validMsg.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCommonTransitConvention);
			validMsg.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCommonTransitConvention;
			validMsg.EM_ReceiveTransmit = "TRX";
			validMsg.EM_MessageText = "<CC015/>";
			validMsg.EM_MessageSubType = "015";
			validMsg.EM_MessageNum = "1";
			validMsg.EM_MessageOwner = "ABC";
			validMsg.EM_LinkedObject = nctsHeader;

			var invalidMsg1 = Factory.New<EDIMessage>();
			invalidMsg1.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCommonTransitConvention);
			invalidMsg1.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCcsuk;
			invalidMsg1.EM_ReceiveTransmit = "TRX";
			invalidMsg1.EM_MessageText = "Wrong Application Code";
			invalidMsg1.EM_MessageSubType = "015";
			invalidMsg1.EM_MessageNum = "2";
			invalidMsg1.EM_MessageOwner = "ABC";
			invalidMsg1.EM_LinkedObject = nctsHeader;

			var invalidMsg2 = Factory.New<EDIMessage>();
			invalidMsg2.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCommonTransitConvention);
			invalidMsg2.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCommonTransitConvention;
			invalidMsg2.EM_ReceiveTransmit = "TRX";
			invalidMsg2.EM_HeldUntilDate = ZDateTime.Now.AddMinutes(15);
			invalidMsg2.EM_MessageText = "Future Held Date";
			invalidMsg2.EM_MessageSubType = "015";
			invalidMsg2.EM_MessageNum = "3";
			invalidMsg2.EM_MessageOwner = "ABC";
			invalidMsg2.EM_LinkedObject = nctsHeader;

			Factory.Save();

			var processor = new CTCOutgoingMessageProcessorForTest(new LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			validMsg.Reload();
			invalidMsg1.Reload();
			invalidMsg2.Reload();
			var interchange = Factory.Load<EDIInterchange>(validMsg.EM_EI);

			CombineAssertions(() =>
			{
				AssertEquals("Valid msg Sent", "SNT", validMsg.EM_Status);
				AssertSame("Msg1 Still linked to Header", nctsHeader, validMsg.EM_LinkedObject);
				AssertNotNull("Interchange created and linked", interchange);
				AssertEquals("Interchange queued", "HQU", interchange.EI_Status);

				AssertEquals("Invalid Msg1 still Qeued", "QUE", invalidMsg1.EM_Status);
				AssertEquals("Invalid Msg2 still Qeued", "QUE", invalidMsg2.EM_Status);
			});
		}

		[TestDate(2021, 10, 14, 13, 15, 0)]
		public void TestProcessWithNoToken()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = "NCT00000001";
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "Ncts001";

			var message1 = Factory.New<EDIMessage>();
			message1.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCommonTransitConvention);
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCommonTransitConvention;
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_MessageText = "Msg1";
			message1.EM_MessageSubType = "015";
			message1.EM_MessageOwner = "ABC";
			message1.EM_LinkedObject = nctsHeader;
			message1.EM_MessageNum = "1";
			message1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;

			var message2 = Factory.New<EDIMessage>();
			message2.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCommonTransitConvention);
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCommonTransitConvention;
			message2.EM_ReceiveTransmit = "TRX";
			message2.EM_MessageText = "Msg2";
			message2.EM_MessageSubType = "015";
			message2.EM_MessageOwner = "DEF";
			message2.EM_LinkedObject = nctsHeader;
			message2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-65);
			message2.EM_HeldUntilDate = ZDateTime.UtcNow;
			message2.EM_MessageNum = "2";

			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new CTCOutgoingMessageProcessorForTest(logger);
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				processor.ProcessMessage(CancellationToken.None);
			}

			message1.Reload();
			message2.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("Msg1 Queued", EDIMessage.Status.Queued, message1.EM_Status);
				AssertNotNull("Msg1 Held Date filled", message1.EM_HeldUntilDate);

				var minutes = (message1.EM_HeldUntilDate - message1.EM_SystemCreateTimeUtc).TotalMinutes;
				Assert($"Held at least 10 minutes - Actual: {minutes} between {message1.EM_SystemCreateTimeUtc} and {message1.EM_HeldUntilDate}", minutes >= 10.0);
				AssertContains("Log for 1", "A valid access token was not found for message 1. Message will be held back until", string.Join("\n", logger.Logs));

				AssertEquals("Message 2 is failed", EDIMessage.Status.Failed, message2.EM_Status);
				AssertContains("Log", "Message sending failed: A valid access token was not found for message 2", string.Join("\n", logger.Logs));
			});
		}

		public void TestMessageFilter()
		{
			var processor = new CTCOutgoingMessageProcessorForTest(new LoggingInformation());

			var expectedFilter = System.FormattableString.Invariant($"{EDIMessage.Schema.EM_ApplicationCode} = @CWO1_");
			var actualFilter = processor.MessageFilter_Exposed.FilterString;
			AssertEquals("Message Filter", expectedFilter, actualFilter);
		}

		public void TestInterchangeProvider()
		{
			var processor = new CTCOutgoingMessageProcessorForTest(new LoggingInformation());
			var collection = new NonDependentEDIMessageCollection(Factory);
			AssertType<CTCInterchangeProvider>("CTC Interchange Provider", processor.CreateNewInterchangeProvider_Exposed(collection));
		}

		class CTCOutgoingMessageProcessorForTest : CTCOutgoingMessageProcessor
		{
			public CTCOutgoingMessageProcessorForTest(LoggingInformation logger) : base(logger)
			{
			}

			public ZQuery MessageFilter_Exposed => base.MessageFilter;
			public InterchangeProviderBase CreateNewInterchangeProvider_Exposed(NonDependentEDIMessageCollection readyMessages) => base.CreateNewInterchangeProvider(readyMessages);
		}
	}
}
