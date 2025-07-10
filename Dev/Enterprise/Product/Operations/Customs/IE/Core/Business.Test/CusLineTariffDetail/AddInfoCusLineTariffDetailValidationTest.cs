using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class AddInfoCusLineTariffDetailValidationTest : EU.Business.Testing.AddInfoCusLineTariffDetailValidationTest
	{
		public void TestCheckMethodOfPayment()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(addInfoCusLineTariffDetail.ZG_MethodOfPaymentInfo);
				ValidationTestHelper.AssertInvalidCodeMessageError(addInfoCusLineTariffDetail.ZG_MethodOfPaymentInfo, "Z", "A");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			cusLineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			addInfoCusLineTariffDetail = (AddInfoCusLineTariffDetail)((IAddInfoManager)cusLineTariffDetail).AddInfo;
		}

		CusLineTariffDetail cusLineTariffDetail;
		AddInfoCusLineTariffDetail addInfoCusLineTariffDetail;
	}
}
