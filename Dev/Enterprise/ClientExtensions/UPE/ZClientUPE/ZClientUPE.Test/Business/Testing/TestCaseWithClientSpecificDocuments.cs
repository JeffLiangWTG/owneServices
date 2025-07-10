using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific;

namespace Enterprise.Client.UPE.Business.Testing
{
	class TestCaseWithClientSpecificDocuments : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			LoadUPESpecificDocuments();
		}

		internal static void LoadUPESpecificDocuments()
		{
			ClientDocumentTestHelper.SetupClientDocumentsFromSupplementaryContentPath("UPE");
		}
	}
}
