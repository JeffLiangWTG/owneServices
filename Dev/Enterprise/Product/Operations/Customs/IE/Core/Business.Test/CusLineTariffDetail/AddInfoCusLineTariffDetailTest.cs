using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AddInfoCusLineTariffDetail))]
	sealed class AddInfoCusLineTariffDetailTest : EU.Business.Testing.AddInfoCusLineTariffDetailTest
	{
		public void TestAddInfoCusLineTariffDetailValidation()
		{
			AssertType<AddInfoCusLineTariffDetailValidation>(addInfoCusLineTariffDetail.Validation);
		}

		public void TestAddInfoCusLineTariffDetailLookups()
		{
			AssertType<AddInfoCusLineTariffDetailLookups>(addInfoCusLineTariffDetail.Lookups);
		}

		public void TestParent()
		{
			CombineAssertions(() =>
			{
				AssertType<CusLineTariffDetail>(addInfoCusLineTariffDetail.Parent);
				AssertSame(cusLineTariffDetail, addInfoCusLineTariffDetail.Parent);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => addInfoCusLineTariffDetail;

		protected override void SetUp()
		{
			base.SetUp();
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			cusLineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			addInfoCusLineTariffDetail = new AddInfoCusLineTariffDetail(cusLineTariffDetail.BZ_NAddInfoInfo);
		}

		AddInfoCusLineTariffDetail addInfoCusLineTariffDetail;
		CusLineTariffDetail cusLineTariffDetail;
	}
}
