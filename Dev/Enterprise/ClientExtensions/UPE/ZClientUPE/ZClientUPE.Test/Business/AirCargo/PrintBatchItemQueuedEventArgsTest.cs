using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class PrintBatchItemQueuedEventArgsTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestConstructor()
		{
			UPEPrintBatchItem printBatchItem = Factory.New<UPEPrintBatchItem>();
			AssertEquals("PrintItem", printBatchItem, new PrintBatchItemQueuedEventArgs(printBatchItem, true).PrintItem);
			AssertEquals("NotifyUser", true, new PrintBatchItemQueuedEventArgs(printBatchItem, true).NotifyUser);
		}
	}
}
