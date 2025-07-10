using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	[TestedType(typeof(PreviousDocumentsFieldsLayoutBuilder<PreviousDocument>))]
	sealed class PreviousDocumentsFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<PreviousDocumentsFieldsLayoutBuilder<PreviousDocument>, PreviousDocument, PreviousDocumentsFieldsControlBag>
	{
		protected override PreviousDocumentsFieldsLayoutBuilder<PreviousDocument> GetColumnLayoutBuilderForTesting()
		{
			return new PreviousDocumentsFieldsLayoutBuilder<PreviousDocument>();
		}
	}
}
