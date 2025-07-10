using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.ServiceTask.Testing
{
	public class CcsukNonChiefResponseBaseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestOrderAndHint()
		{
			var processor = new CcsukNonChiefResponseBaseMessageProcessorForTest();
			var query = processor.GetMessageProcessorQueryExposed();
			var hint = query.TableIndexHints.Single();

			AssertEquals("EM_SystemCreateTimeUtc, EM_MessageNum", query.OrderBy);
			AssertEquals("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum", hint.IndexName);
		}

		protected class CcsukNonChiefResponseBaseMessageProcessorForTest : CcsukNonChiefResponseBaseMessageProcessor
		{
			public CcsukNonChiefResponseBaseMessageProcessorForTest() : base(new Logger())
			{
			}

			public ZQuery GetMessageProcessorQueryExposed() => GetMessageProcessorQuery();
		}
	}
}
