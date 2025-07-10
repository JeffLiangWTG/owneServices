using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(FinalPriceReportByDateExtensionHeader))]
	sealed class FinalPriceReportByDateExtensionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new FinalPriceReportByDateExtensionHeader(Factory);

		public void TestCustomsOfficeList()
		{
			AssertEquals("Lookups.CustomsOfficeList", new FinalPriceReportByDateExtensionHeader(Factory).CustomsOfficeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestIsPossibleSend()
		{
			var header = new FinalPriceReportByDateExtensionHeader(Factory);
			Assert(!header.HasExtensionLines);

			header.FinalPriceReportByDateExtensionLines.AddNew();
			Assert(header.HasExtensionLines);
		}
	}
}
