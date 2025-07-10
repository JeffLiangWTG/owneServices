using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestGetMessageCreator()
		{
			var processor = new BRCInboundInterchangeProcessorForTesting(new string[] { ApplicationCodeList.Codes.BRCustoms });
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeType = MessageTypeList.Codes.CDC;
			AssertType<BRCExportInboundMessageCreator>(processor.GetMessageCreator(interchange));
			interchange.EI_InterchangeType = MessageTypeList.Codes.CDE;
			AssertType<BRCExportInboundMessageCreator>(processor.GetMessageCreator(interchange));
			interchange.EI_InterchangeType = MessageTypeList.Codes.XER;
			AssertType<BRCResponseErrorInboundMessageCreator>(processor.GetMessageCreator(interchange));
			interchange.EI_InterchangeType = MessageTypeList.Codes.LIC;
			AssertType<BRCImportLicenseAcceptInboundMessageCreator>(processor.GetMessageCreator(interchange));
			interchange.EI_InterchangeType = MessageTypeList.Codes.LIS;
			AssertType<BRCImportLicenseStatusInboundMessageCreator>(processor.GetMessageCreator(interchange));
			interchange.EI_InterchangeType = MessageTypeList.Codes.CAT;
			AssertType<BRCCatalogInboundMessageCreator>(processor.GetMessageCreator(interchange));
			interchange.EI_InterchangeType = MessageTypeList.Codes.CDI;
			AssertType<BRCInboundMessageCreator>(processor.GetMessageCreator(interchange));
			interchange.EI_InterchangeType = MessageTypeList.Codes.SUB;
			AssertType<BRCInboundMessageCreator>(processor.GetMessageCreator(interchange));
			interchange.EI_InterchangeType = MessageTypeList.Codes.OPE;
			AssertType<BRCForeignOperatorInboundMessageCreator>(processor.GetMessageCreator(interchange));
			interchange.EI_InterchangeType = MessageTypeList.Codes.RTT;
			AssertType<BRCRequestTTCEReferenceFileInboundMessageCreator>(processor.GetMessageCreator(interchange));
			interchange.EI_InterchangeType = MessageTypeList.Codes.LPC;
			AssertType<BRCInboundMessageCreator>(processor.GetMessageCreator(interchange));
		}

		class BRCInboundInterchangeProcessorForTesting : BRCInboundInterchangeProcessor
		{
			public BRCInboundInterchangeProcessorForTesting(IEnumerable<string> applicationCodes) : base(applicationCodes)
			{
			}

			public new IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => base.GetMessageCreator(interchange);
		}
	}
}
