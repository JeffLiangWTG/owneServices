using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class LVSDeclarationRatingAdapterTest : TestCaseWithFactory
	{
		public void TestConditionsSupporter()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var adapter = new LVSDeclarationRatingAdapter(org, new[] { invoiceHeader }, invoiceHeader.IncoTerm, invoiceHeader.EffectiveImportClearanceProvince, declaration);
			AssertType<DeclarationRateLineConditionsSupporter>(((IAutoRatingFreightConditionsSupportable)adapter).ConditionsSupporter);
		}
	}
}
