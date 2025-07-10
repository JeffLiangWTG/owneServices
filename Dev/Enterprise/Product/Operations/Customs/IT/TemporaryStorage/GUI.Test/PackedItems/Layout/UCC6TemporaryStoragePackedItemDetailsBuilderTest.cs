using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(UCC6TemporaryStoragePackedItemDetailsBuilder))]
sealed class UCC6TemporaryStoragePackedItemDetailsBuilderTest : ColumnLayoutBuilderAbstractTest<UCC6TemporaryStoragePackedItemDetailsBuilder, TemporaryStorageHeader, UCC6TemporaryStoragePackedItemDetailsControlBag>
{
	protected override UCC6TemporaryStoragePackedItemDetailsBuilder GetColumnLayoutBuilderForTesting() => new UCC6TemporaryStoragePackedItemDetailsBuilder();

	protected override int ExpectedMaxColumns => 2;

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
}

