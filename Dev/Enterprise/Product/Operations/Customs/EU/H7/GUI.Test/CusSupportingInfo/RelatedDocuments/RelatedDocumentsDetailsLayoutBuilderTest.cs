using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(RelatedDocumentsDetailsLayoutBuilder))]
	sealed class RelatedDocumentsDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<RelatedDocumentsDetailsLayoutBuilder, CusSupportingInfo, AdditionalInformationDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override RelatedDocumentsDetailsLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new RelatedDocumentsDetailsLayoutBuilder();
		}
	}
}
