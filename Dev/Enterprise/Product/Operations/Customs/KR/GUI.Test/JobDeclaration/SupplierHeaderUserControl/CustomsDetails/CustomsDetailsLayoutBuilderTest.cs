using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CustomsDetailsLayoutBuilder))]
	sealed class CustomsDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CustomsDetailsLayoutBuilder, JobComInvoiceHeader, CustomsDetailsControlBag>
	{
		protected override CustomsDetailsLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new CustomsDetailsLayoutBuilder();
		}

		protected override int ExpectedMaxColumns => 2;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
