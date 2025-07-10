using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class MercosulForeignDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_RN_NKCountryCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var mercosulForeignDeclaration = invoiceLine.MercosulForeignDeclarations.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(mercosulForeignDeclaration.CSI_RN_NKCountryCodeInfo, "XX", MercosulCountriesList.Codes.AR);
		}
	}
}
