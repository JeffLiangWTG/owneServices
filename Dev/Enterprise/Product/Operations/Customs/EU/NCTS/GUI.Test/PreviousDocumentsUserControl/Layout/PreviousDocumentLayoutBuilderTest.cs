using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(PreviousDocumentLayoutBuilder<NctsPreviousDocument>))]
	class PreviousDocumentLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<PreviousDocumentLayoutBuilder<NctsPreviousDocument>, NctsPreviousDocument, PreviousDocumentControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override PreviousDocumentLayoutBuilder<NctsPreviousDocument> GetColumnLayoutBuilderForTesting() => new PreviousDocumentLayoutBuilder<NctsPreviousDocument>();
	}
}
