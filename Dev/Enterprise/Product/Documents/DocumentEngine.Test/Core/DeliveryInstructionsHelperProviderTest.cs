using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;
using static Enterprise.DocumentEngine.Testing.DocumentPackTest;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DeliveryInstructionsHelperProviderTest : TestCaseWithFactory
	{
		public void TestGet()
		{
			AssertType<DeliveryInstructions>("Should get a default DeliveryInstructions when the pack is null.", DeliveryInstructionsProvider.Get(null));

			var dummy = Factory.New<DummyDocManagerTestBizO>();
			dummy.SetupDocManagerObjects();

			var document = ((IDocManagerSupport)dummy).DocManagerInfo.Documents[1] as IStorageDocs;
			document.DocType = RefDocTypes.QuarantineRemotePrint;

			var mockDocSupportBizO = new MockDocSupportBizO();
			var menuItem = Factory.New<DocumentCommand>();

			using (var pack = new TestableDocumentPack(menuItem, mockDocSupportBizO, null))
			{
				pack.Add(document as IDeliverable);

				AssertEquals("Should get QuarantineDeliveryInstructions when the document type code is QRP.", @"Enterprise.Customs.AU.Declaration.Business.QuarantineDeliveryInstructions", DeliveryInstructionsProvider.Get(pack).GetType().FullName);
			}
		}
	}
}
