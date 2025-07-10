using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(SEDDetailsLayoutBuilder))]
	sealed class SEDDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<SEDDetailsLayoutBuilder, JobDeclaration, SEDDetailsControlBag>
	{
		protected override SEDDetailsLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new SEDDetailsLayoutBuilder();
		}

		protected override int ExpectedMaxColumns => 1;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
