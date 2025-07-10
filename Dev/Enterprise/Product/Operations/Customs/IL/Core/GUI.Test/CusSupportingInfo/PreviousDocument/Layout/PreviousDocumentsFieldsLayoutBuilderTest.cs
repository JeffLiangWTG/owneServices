using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(PreviousDocumentsFieldsLayoutBuilder))]
	sealed class PreviousDocumentsFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<PreviousDocumentsFieldsLayoutBuilder, PreviousDocument, PreviousDocumentsFieldsControlBag>
	{
		protected override PreviousDocumentsFieldsLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new PreviousDocumentsFieldsLayoutBuilder();
		}
	}
}
