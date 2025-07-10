using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(SupportingDocumentLayoutBuilder<NctsSupportingDocument>))]
	class SupportingDocumentLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<SupportingDocumentLayoutBuilder<NctsSupportingDocument>, NctsSupportingDocument, SupportingDocumentControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override SupportingDocumentLayoutBuilder<NctsSupportingDocument> GetColumnLayoutBuilderForTesting() => new SupportingDocumentLayoutBuilder<NctsSupportingDocument>();
	}
}
