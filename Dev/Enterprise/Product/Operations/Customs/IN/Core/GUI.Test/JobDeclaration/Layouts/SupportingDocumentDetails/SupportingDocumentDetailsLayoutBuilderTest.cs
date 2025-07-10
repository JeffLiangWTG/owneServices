using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SupportingDocumentDetailsLayoutBuilder<SupportingDocument>))]
sealed class SupportingDocumentDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<SupportingDocumentDetailsLayoutBuilder<SupportingDocument>, SupportingDocument, SupportingDocumentDetailsControlBag>
{
	protected override SupportingDocumentDetailsLayoutBuilder<SupportingDocument> GetColumnLayoutBuilderForTesting() => new SupportingDocumentDetailsLayoutBuilder<SupportingDocument>();

	protected override int ExpectedMaxColumns => 1;

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
}
