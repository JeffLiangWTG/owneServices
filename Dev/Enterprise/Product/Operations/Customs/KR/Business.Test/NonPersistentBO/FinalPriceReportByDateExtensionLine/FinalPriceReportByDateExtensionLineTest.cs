using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(FinalPriceReportByDateExtensionLine))]
	sealed class FinalPriceReportByDateExtensionLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new FinalPriceReportByDateExtensionLine(new FinalPriceReportByDateExtensionHeader(Factory));
	}
}
