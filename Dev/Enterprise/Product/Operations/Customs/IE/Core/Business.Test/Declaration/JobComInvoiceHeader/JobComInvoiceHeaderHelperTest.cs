using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class JobComInvoiceHeaderHelperTest : TestCaseWithFactory
	{
		public void TestIsSellerOrBuyerDifferentFromDeclaration()
		{
			CombineAssertions(() =>
			{
				var buyer = Factory.New<OrgHeader>();
				var seller = Factory.New<OrgHeader>();
				declaration.JE_OH_Buyer = buyer.PK;
				invoiceHeader.BuyerOrgPK = buyer.PK;
				invoiceHeader.SellerOrgPK = seller.PK;
				declaration.SellerOrgPK = seller.PK;
				AssertEquals("Seller and Buyer are same", false, invoiceHeader.IsSellerOrBuyerDifferentFromDeclaration());

				invoiceHeader.SellerOrgPK = Factory.New<OrgHeader>().PK;
				AssertEquals("Seller is different", true, invoiceHeader.IsSellerOrBuyerDifferentFromDeclaration());

				invoiceHeader.SellerOrgPK = seller.PK;
				invoiceHeader.BuyerOrgPK = Factory.New<OrgHeader>().PK;
				AssertEquals("Buyer is different", true, invoiceHeader.IsSellerOrBuyerDifferentFromDeclaration());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
	}
}
