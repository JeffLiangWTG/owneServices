using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageBillPartiesBuilder))]
	sealed class UCC6TemporaryStorageBillPartiesBuilderTest : ColumnLayoutBuilderAbstractTest<UCC6TemporaryStorageBillPartiesBuilder, TemporaryStorageHeader, UCC6TemporaryStorageBillPartiesControlBag>
	{
		protected override UCC6TemporaryStorageBillPartiesBuilder GetColumnLayoutBuilderForTesting() => new UCC6TemporaryStorageBillPartiesBuilder();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int ExpectedMaxColumns => 3;
	}
}
