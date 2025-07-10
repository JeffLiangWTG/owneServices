using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	public class CusCodeDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			CusCodeData cusCode = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew().CustomsOfficers.AddNew();
			cusCode.CY_Code = ZString.Empty;
			AssertNoMessageErrors(cusCode.CY_CodeInfo);

			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			invoice.ContractNumber = ZString.Empty;
			AssertNoMessageErrors(invoice.ContractDateInfo);
			AssertNoMessageErrors(invoice.ContractNumberInfo);

			invoice.PurchaseOrderNumber = ZString.Empty;
			AssertNoMessageErrors(invoice.PurchaseOrderDateInfo);
			AssertNoMessageErrors(invoice.PurchaseOrderNumberInfo);

			cusCode = Factory.New<JobDeclaration>().Invoices.AddNew().ValuationDeclarationCodes.AddNew();
			cusCode.CY_Code = ZString.Empty;
			AssertNoMessageErrors(cusCode.CY_CodeInfo);

			cusCode = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().VehicleNumbers.AddNew();
			cusCode.CY_Code = ZString.Empty;
			AssertNoMessageErrors(cusCode.CY_CodeInfo);
		}
	}
}
