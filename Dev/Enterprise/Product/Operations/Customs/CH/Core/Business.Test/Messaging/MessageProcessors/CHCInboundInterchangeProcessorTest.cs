using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public class CHCInboundInterchangeProcessorTest : TestCaseWithFactory
{
	public void TestApplicationCodes()
	{
		AssertEquals("Application codes length is: ", 3, processor.ApplicationCodesExposed.Length);
		Assert("Application codes contains CHC", processor.ApplicationCodesExposed.Contains(EDIInterchange.ApplicationCodes.CHCustomsEdec));
		Assert("Application codes contains CHP", processor.ApplicationCodesExposed.Contains(EDIInterchange.ApplicationCodes.CHCustomsPassar));
		Assert("Application codes contains CHO", processor.ApplicationCodesExposed.Contains(EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput));
	}

	protected override void SetUp()
	{
		base.SetUp();
		processor = new CHCInboundInterchangeProcessorForTest();
	}
	CHCInboundInterchangeProcessorForTest processor;

	class CHCInboundInterchangeProcessorForTest : CHCInboundInterchangeProcessor
	{
		public CHCInboundInterchangeProcessorForTest() : base()
		{
		}

		public string[] ApplicationCodesExposed => base.ApplicationCodes;
	}
}
