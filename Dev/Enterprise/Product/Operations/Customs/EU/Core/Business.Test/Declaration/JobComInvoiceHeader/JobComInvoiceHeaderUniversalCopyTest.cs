using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

sealed class JobComInvoiceHeaderUniversalCopyTest : BaseAddInfoUniversalCopyTest
{
	protected override void AssertHasOtherNodes(string[] allNodeNames)
	{
		CombineAssertions(() =>
		{
			AssertCollectionContains("CustomFields", "CustomFields", allNodeNames);
			AssertCollectionContains("HeaderDescriptions", "HeaderDescriptions", allNodeNames);
		});
	}

	protected override IAddInfoManager GetManager()
	{
		var declaration = Factory.New<JobDeclaration>();
		return declaration.Invoices.AddNew();
	}
}
