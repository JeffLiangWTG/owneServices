using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(FinalPriceReportByDateExtensionLineCollection))]
	sealed class FinalPriceReportByDateExtensionLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FinalPriceReportByDateExtensionLineCollection>
	{
		protected override FinalPriceReportByDateExtensionLineCollection GetCollectionToTest() => new FinalPriceReportByDateExtensionLineCollection(new FinalPriceReportByDateExtensionHeader(Factory));
		protected override BusinessObject GetNewElementToAddToTheCollection() => new FinalPriceReportByDateExtensionLine(new FinalPriceReportByDateExtensionHeader(Factory));
	}
}

