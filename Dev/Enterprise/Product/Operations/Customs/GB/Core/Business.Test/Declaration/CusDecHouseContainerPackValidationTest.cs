using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.Test.Declaration
{
	public class CusDecHouseContainerPackValidationTest : Customs.Business.Testing.CusDecHouseContainerPackValidationTest
	{
		protected override void MakeAssertionsOnCW_PackQtyValidationNotificationType(BaseJobDeclaration declaration)
		{
			var message = "The total number of packs included in invoice(s) and invoice line(s) is 20, it exceeds this package quantity.";

			var invoiceLine1 = declaration.InvoiceLines[0];
			var package = declaration.Packages[0];

			AssertNoWarning("Package.CW_PackQty when Filled in Incorrectly", package.CW_PackQtyInfo, message);

			invoiceLine1.PackagesPivot[0].CHC_NumberOfPacks = 20;
			AssertHasWarning("Package.CW_PackQty when Filled in Incorrectly", package.CW_PackQtyInfo, message);
		}
	}
}
