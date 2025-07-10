using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	[TestedType(typeof(SupportingDocumentFieldsLayoutBuilder<SupportingDocument>))]
	class SupportingDocumentFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<SupportingDocumentFieldsLayoutBuilder<SupportingDocument>, SupportingDocument, SupportingDocumentFieldsControlBag>
	{
		protected override SupportingDocumentFieldsLayoutBuilder<SupportingDocument> GetColumnLayoutBuilderForTesting() => new SupportingDocumentFieldsLayoutBuilder<SupportingDocument>();
	}
}
