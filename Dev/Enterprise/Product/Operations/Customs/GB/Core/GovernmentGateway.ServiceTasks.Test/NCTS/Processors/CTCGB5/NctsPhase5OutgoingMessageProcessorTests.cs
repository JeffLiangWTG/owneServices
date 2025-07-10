using System.Threading;
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

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Testing.NCTS.Processors.CTCGB5
{
	public class NctsPhase5OutgoingMessageProcessorTests : TestCaseWithFactory
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
			validMsg.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIInterchange.ApplicationCodes.GbCustomsNCTS);
			validMsg.EM_ApplicationCode = EDIInterchange.ApplicationCodes.GbCustomsNCTS;
			validMsg.EM_ReceiveTransmit = "TRX";
			validMsg.EM_MessageText = "<CC015/>";
			validMsg.EM_MessageType = "015";
			validMsg.EM_MessageNum = "1";
			validMsg.EM_MessageOwner = "ABC";
			validMsg.EM_LinkedObject = nctsHeader;

			var invalidMsg1 = Factory.New<EDIMessage>();
			invalidMsg1.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIInterchange.ApplicationCodes.GbCustomsNCTS);
			invalidMsg1.EM_ApplicationCode = EDIInterchange.ApplicationCodes.GbCcsuk;
			invalidMsg1.EM_ReceiveTransmit = "TRX";
			invalidMsg1.EM_MessageText = "Wrong Application Code";
			invalidMsg1.EM_MessageType = "015";
			invalidMsg1.EM_MessageNum = "2";
			invalidMsg1.EM_MessageOwner = "ABC";
			invalidMsg1.EM_LinkedObject = nctsHeader;

			var invalidMsg2 = Factory.New<EDIMessage>();
			invalidMsg2.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIInterchange.ApplicationCodes.GbCustomsNCTS);
			invalidMsg2.EM_ApplicationCode = EDIInterchange.ApplicationCodes.GbCustomsNCTS;
			invalidMsg2.EM_ReceiveTransmit = "TRX";
			invalidMsg2.EM_HeldUntilDate = ZDateTime.Now.AddMinutes(15);
			invalidMsg2.EM_MessageText = "Future Held Date";
			invalidMsg2.EM_MessageType = "015";
			invalidMsg2.EM_MessageNum = "3";
			invalidMsg2.EM_MessageOwner = "ABC";
			invalidMsg2.EM_LinkedObject = nctsHeader;

			Factory.Save();

			var processor = new NctsPhase5OutgoingMessageProcessorForTest(new LoggingInformation());
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

		public void TestMessageFilter()
		{
			var processor = new NctsPhase5OutgoingMessageProcessorForTest(new LoggingInformation());

			var expectedFilter = System.FormattableString.Invariant($"{AutoEDIMessage.Schema.EM_ApplicationCode} = @CWO1_");
			var actualFilter = processor.MessageFilter_Exposed.FilterString;
			AssertEquals("Message Filter", expectedFilter, actualFilter);
			AssertEquals("Message Filter EM_ApplicationCode", "EM_ApplicationCode = 'GBN'", processor.MessageFilter_Exposed.FilterPartsHashKey);
		}

		public void TestInterchangeProvider()
		{
			var processor = new NctsPhase5OutgoingMessageProcessorForTest(new LoggingInformation());
			var collection = new NonDependentEDIMessageCollection(Factory);
			AssertType<CTCGB5InterchangeProvider>("CTCGB5 Interchange Provider", processor.CreateNewInterchangeProvider_Exposed(collection));
		}

		class NctsPhase5OutgoingMessageProcessorForTest : NctsPhase5OutgoingMessageProcessor
		{
			public NctsPhase5OutgoingMessageProcessorForTest(LoggingInformation logger) : base(logger)
			{
			}

			public ZQuery MessageFilter_Exposed => base.MessageFilter;
			public InterchangeProviderBase CreateNewInterchangeProvider_Exposed(NonDependentEDIMessageCollection readyMessages) => base.CreateNewInterchangeProvider(readyMessages);
		}
	}
}
