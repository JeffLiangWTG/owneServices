using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(DeclarationOtherDetailsLayoutBuilder))]
class DeclarationOtherDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<DeclarationOtherDetailsLayoutBuilder, JobDeclaration, DeclarationOtherDetailsControlBag>
{
	protected override int ExpectedMaxColumns => 1;

	protected override DeclarationOtherDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new DeclarationOtherDetailsLayoutBuilder();

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
}
