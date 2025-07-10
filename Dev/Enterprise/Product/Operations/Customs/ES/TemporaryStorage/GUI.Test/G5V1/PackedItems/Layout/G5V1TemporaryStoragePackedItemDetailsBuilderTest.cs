using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(G5V1TemporaryStoragePackedItemDetailsBuilder<TemporaryStorageHeader>))]
	public sealed class G5V1TemporaryStoragePackedItemDetailsBuilderTest : ColumnLayoutBuilderAbstractTest<G5V1TemporaryStoragePackedItemDetailsBuilder<TemporaryStorageHeader>, TemporaryStorageHeader, UCC6TemporaryStoragePackedItemDetailsControlBag>
	{
		protected override G5V1TemporaryStoragePackedItemDetailsBuilder<TemporaryStorageHeader> GetColumnLayoutBuilderForTesting() => new G5V1TemporaryStoragePackedItemDetailsBuilder<TemporaryStorageHeader>();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int ExpectedMaxColumns => 3;
	}
}
