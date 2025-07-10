using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	internal class AddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_CustomsAuthorisationReferenceForExportFallback()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				invoice.ZG_CustomsAuthorisationReferenceForExportFallback = "EFB01-XYZ-D1234";
				AssertNoMessageErrorContaining(invoice.ZG_CustomsAuthorisationReferenceForExportFallbackInfo, "match required pattern");
				AssertNoMessageErrorContaining(invoice.ZG_CustomsAuthorisationReferenceForExportFallbackInfo, "CAR must start with");
				invoice.ZG_CustomsAuthorisationReferenceForExportFallback = "IFB01-XYZ-D1234";
				AssertNoMessageErrorContaining(invoice.ZG_CustomsAuthorisationReferenceForExportFallbackInfo, "match required pattern");
				AssertHasMessageErrorContaining(invoice.ZG_CustomsAuthorisationReferenceForExportFallbackInfo, "CAR must start with");
				invoice.ZG_CustomsAuthorisationReferenceForExportFallback = "POOP";
				AssertHasMessageErrorContaining(invoice.ZG_CustomsAuthorisationReferenceForExportFallbackInfo, "match required pattern");
			}
		}
	}
}
