using Enterprise.Accounting.Netting;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Universal.Netting.Testing
{
	[TestedType(typeof(NettingTransactionDataContextManager))]
	public class NettingTransactionDataContextManagerTest : UniversalDataBuss.Management.Testing.DataContextManagerTestCase<NettingTransactionDataContextManager, NettingReceivableTransaction>
	{
		protected override NettingReceivableTransaction GetNewBusinessObjectForTesting()
		{
			NettingReceivableTransaction result = Factory.NewWithValidTestData<NettingReceivableTransaction>();
			Factory.SaveForTesting();
			return result;
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("NettingReceivableTransaction doesn't support IJobNumber", true);
		}
	}
}
