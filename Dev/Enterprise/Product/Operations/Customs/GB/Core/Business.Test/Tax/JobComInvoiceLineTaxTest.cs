using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineTax))]
	public class JobComInvoiceLineTaxTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.ZG_MethodOfPayment = "M";
			var gbTax = invLine.Taxes.AddNew();
			AssertEquals("M", gbTax.JLT_MethodOfPayment);
		}

		public void TestDoIfTaxLinePaymentMethodIsNOrP_IsUCCCompliant()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "Declarant";
			declarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "5678", Core.Constants.CountryCodes.UnitedKingdom);
			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.Address1 = "Address";
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_OA_DeclarantAddress = declarantAddress.PK;
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var gbTax = invLine.Taxes.AddNew();
			gbTax.Data.JLT_MethodOfPayment = "N";

			Assert(dec.Guarantees.Cast<GBGuarantee>().Any(x =>
				x.PW_BondType == "G"
				&& x.PW_Password == "Y"
				&& x.PW_HolderIdentification == "GB5678"));

			dec.Guarantees.RemoveAndDeleteAll();
			gbTax.Data.JLT_MethodOfPayment = "P";

			Assert(dec.Guarantees.Cast<GBGuarantee>().Any(x =>
				x.PW_BondType == "G"
				&& x.PW_Password == "Y"
				&& x.PW_HolderIdentification == "GB5678"));
		}

		public void TestData()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var gbTax = invLine.Taxes.AddNew();
			AssertType<JobComInvoiceLineTax>(gbTax.Data);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var gbTax = invLine.Taxes.AddNew();
			return gbTax;
		}
	}
}
