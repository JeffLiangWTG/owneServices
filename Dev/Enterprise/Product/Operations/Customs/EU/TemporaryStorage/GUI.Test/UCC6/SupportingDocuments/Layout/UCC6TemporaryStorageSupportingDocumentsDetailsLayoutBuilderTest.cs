using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageSupportingDocumentsDetailsLayoutBuilder))]
	public class UCC6TemporaryStorageSupportingDocumentsDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<UCC6TemporaryStorageSupportingDocumentsDetailsLayoutBuilder, TemporaryStorageBill, UCC6TemporaryStorageSupportingDocumentsDetailsControlBag>
	{
		protected override UCC6TemporaryStorageSupportingDocumentsDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new UCC6TemporaryStorageSupportingDocumentsDetailsLayoutBuilder();

		protected override int ExpectedMaxColumns => 1;
		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
