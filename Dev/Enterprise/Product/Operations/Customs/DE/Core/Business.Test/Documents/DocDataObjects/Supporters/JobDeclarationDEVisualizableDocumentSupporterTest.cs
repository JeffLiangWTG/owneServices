using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.DE.Business.Documents.DocDataObjects.Testing
{
	sealed class JobDeclarationDEVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestCustomizeFormCheckpoint()
		{
			AssertEquals(nameof(supporter.CustomizeFormCheckpoint), Env.Security.MaintainJobDeclarationCustomiseForms, supporter.CustomizeFormCheckpoint);
		}

		public void TestGetDocDataObject()
		{
			var docDataObject = supporter.GetDocDataObject(declaration, DataContext.CMRWayBill, null);
			AssertNotNull(nameof(docDataObject.Right), docDataObject.Right);
			AssertType<CMRConsignmentNoteDocDataObject>($"{nameof(docDataObject.Right)} type", docDataObject.Right);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			supporter = new JobDeclarationDEVisualizableDocumentSupporter(declaration);
		}

		JobDeclaration declaration;
		JobDeclarationDEVisualizableDocumentSupporter supporter;
	}
}
