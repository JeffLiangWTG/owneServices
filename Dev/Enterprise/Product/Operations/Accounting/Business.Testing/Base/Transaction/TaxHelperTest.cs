using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.Base.Transaction
{
	public class TaxHelperTest : TestCaseWithFactory
	{
		public void TestIsGSTMandatory()
		{
			var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", creator.AUD, 1m, 100m, 0m, 100m, 0m, creator.LocalClient, creator.GLHeader1.PK);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			creator.LocalClient.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Assert(arInvoice.Lines[0].IsGSTMandatory);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			creator.LocalClient.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Assert(!arInvoice.Lines[0].IsGSTMandatory);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			creator.LocalClient.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			Assert(!arInvoice.Lines[0].IsGSTMandatory);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			creator.LocalClient.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			Assert(!arInvoice.Lines[0].IsGSTMandatory);
		}

		#region Implementation

		TestObjectCreator creator;

		protected override void SetUp()
		{
			base.SetUp();

			creator = new TestObjectCreator(Factory);
		}

		#endregion
	}
}

