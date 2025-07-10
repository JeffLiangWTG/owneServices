using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(TransferDetailsLayoutBuilder<AsycudaTransferHeader>))]
	sealed class TransferDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransferDetailsLayoutBuilder<AsycudaTransferHeader>, AsycudaTransferHeader, CommonTransferDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 3;

		protected override TransferDetailsLayoutBuilder<AsycudaTransferHeader> GetColumnLayoutBuilderForTesting() => new TransferDetailsLayoutBuilder<AsycudaTransferHeader>();
	}
}
