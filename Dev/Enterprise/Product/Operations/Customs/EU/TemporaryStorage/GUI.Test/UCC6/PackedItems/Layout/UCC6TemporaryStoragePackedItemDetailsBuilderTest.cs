using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStoragePackedItemDetailsBuilder<TemporaryStorageHeader>))]
	public class UCC6TemporaryStoragePackedItemDetailsBuilderTest : ColumnLayoutBuilderAbstractTest<UCC6TemporaryStoragePackedItemDetailsBuilder<TemporaryStorageHeader>, TemporaryStorageHeader, UCC6TemporaryStoragePackedItemDetailsControlBag>
	{
		protected override UCC6TemporaryStoragePackedItemDetailsBuilder<TemporaryStorageHeader> GetColumnLayoutBuilderForTesting() => new UCC6TemporaryStoragePackedItemDetailsBuilder<TemporaryStorageHeader>();

		protected override int ExpectedMaxColumns => 1;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
