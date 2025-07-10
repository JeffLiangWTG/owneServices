using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageBillDetailBuilder))]
	sealed class UCC6TemporaryStorageBillDetailBuilderTest : ColumnLayoutBuilderAbstractTest<UCC6TemporaryStorageBillDetailBuilder, TemporaryStorageHeader, UCC6TemporaryStorageBillDetailControlBag>
	{
		protected override UCC6TemporaryStorageBillDetailBuilder GetColumnLayoutBuilderForTesting() => new UCC6TemporaryStorageBillDetailBuilder();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
