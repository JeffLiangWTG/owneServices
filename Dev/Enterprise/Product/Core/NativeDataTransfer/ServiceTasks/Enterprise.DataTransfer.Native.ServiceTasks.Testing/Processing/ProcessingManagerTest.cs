using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.ServiceTasks.Testing
{
	public class ProcessingManagerTest : TestCase
	{
		public void TestOrderAndHint()
		{
			var processor = new ProcessingManagerForTest();
			var query = processor.GetMessageProcessorQueryExposed();
			var hint = query.TableIndexHints.Single();

			AssertEquals("EM_MessageNum, EM_SystemCreateTimeUtc", query.OrderBy);
			AssertEquals("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc", hint.IndexName);
		}

		class ProcessingManagerForTest : NativeProcessingManager
		{
			public ZQuery GetMessageProcessorQueryExposed() => GetMessageProcessorQuery();
		}
	}
}
