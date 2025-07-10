using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class JobComInvoiceLineUniversalCopyTest : Customs.Business.Testing.BaseAddInfoUniversalCopyTest
	{
		protected override void AssertHasOtherNodes(string[] allNodeNames)
		{
			AssertCollectionContains("CustomFields", allNodeNames);
			AssertCollectionContains("AdditionalProcedureCodes", allNodeNames);
			AssertCollectionContains("AdditionalSupplementaryCodes", allNodeNames);
		}

		protected override IAddInfoManager GetManager()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			return invoice.JobComInvoiceLines.AddNew();
		}
	}
}
