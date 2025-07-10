using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI
{
	[TestedType(typeof(FinalPriceExtensionRequestNewLayoutsBuilder))]
	sealed class FinalPriceExtensionRequestLayoutsBuilderTest : ColumnLayoutBuilderAbstractTest<FinalPriceExtensionRequestNewLayoutsBuilder, FinalPriceReportByDateExtensionHeader, FinalPriceExtensionRequestNewControlBag>
	{
		protected override int ExpectedMaxColumns => 1;
		protected override FinalPriceExtensionRequestNewLayoutsBuilder GetColumnLayoutBuilderForTesting() => new FinalPriceExtensionRequestNewLayoutsBuilder();
	}
}
