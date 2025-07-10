using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(MiscOptionsLayoutBuilder))]
	public class MiscOptionsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<MiscOptionsLayoutBuilder, JobDeclaration, CommonMiscOptionsControlBag>
	{
		protected override MiscOptionsLayoutBuilder GetColumnLayoutBuilderForTesting() => new MiscOptionsLayoutBuilder();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int ExpectedMaxColumns => 1;
	}
}
