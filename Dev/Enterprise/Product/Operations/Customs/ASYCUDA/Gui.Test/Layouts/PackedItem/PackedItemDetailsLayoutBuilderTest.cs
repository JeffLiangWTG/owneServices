using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(PackedItemDetailsLayoutBuilder<AsycudaPack>))]
	sealed class PackedItemDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<PackedItemDetailsLayoutBuilder<AsycudaPack>, AsycudaPack, CommonPackedItemDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 3;

		protected override PackedItemDetailsLayoutBuilder<AsycudaPack> GetColumnLayoutBuilderForTesting() => new PackedItemDetailsLayoutBuilder<AsycudaPack>();
	}
}
