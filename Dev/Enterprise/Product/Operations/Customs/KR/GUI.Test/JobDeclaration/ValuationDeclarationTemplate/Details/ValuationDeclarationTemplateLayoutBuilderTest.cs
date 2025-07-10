using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ValuationDeclarationTemplateLayoutBuilder))]
	sealed class ValuationDeclarationTemplateLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ValuationDeclarationTemplateLayoutBuilder, JobDeclaration, ValuationDeclarationTemplateDetailsControlBag>
	{
		protected override ValuationDeclarationTemplateLayoutBuilder GetColumnLayoutBuilderForTesting() => new ValuationDeclarationTemplateLayoutBuilder();

		protected override int ExpectedMaxColumns => 1;
	}
}
