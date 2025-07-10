using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.EU.NCTS.Business.Documents.DocDataObjects.Testing
{
	class NctsHeaderVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestCustomizeFormCheckpoint()
		{
			AssertEquals(nameof(supporter.CustomizeFormCheckpoint), Env.Security.EuNcts, supporter.CustomizeFormCheckpoint);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			supporter = new NctsHeaderVisualizableDocumentSupporter(header);
		}

		NctsHeader header;
		NctsHeaderVisualizableDocumentSupporter supporter;
	}
}
