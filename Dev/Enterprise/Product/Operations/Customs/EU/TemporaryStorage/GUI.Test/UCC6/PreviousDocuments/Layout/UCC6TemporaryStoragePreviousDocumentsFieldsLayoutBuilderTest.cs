using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStoragePreviousDocumentsDetailsLayoutBuilder<TemporaryStoragePreviousDocument>))]
	sealed class UCC6TemporaryStoragePreviousDocumentsFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<UCC6TemporaryStoragePreviousDocumentsDetailsLayoutBuilder<TemporaryStoragePreviousDocument>, TemporaryStoragePreviousDocument, UCC6TemporaryStoragePreviousDocumentsDetailsControlBag>
	{
		protected override UCC6TemporaryStoragePreviousDocumentsDetailsLayoutBuilder<TemporaryStoragePreviousDocument> GetColumnLayoutBuilderForTesting()
		{
			return new UCC6TemporaryStoragePreviousDocumentsDetailsLayoutBuilder<TemporaryStoragePreviousDocument>();
		}
	}
}
