using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class TestGraphicalDisplayForm : TransactionedTestCase
	{
		public void TestSetDocument()
		{
			StorageDocs newDocument = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			Form.Document = newDocument;
			AssertEquals(newDocument, Form.GraphicalDisplayControlForTesting.Document);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			Form = new GUI.GraphicalDisplayForm(false);
		}

		protected override void TearDown()
		{
			Form.Dispose();
			base.TearDown();
		}

		DocumentFactory MasterFactory;
		GUI.GraphicalDisplayForm Form;
	}
}
