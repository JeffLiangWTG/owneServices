using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(DocumentStmMenuEDocsDependentCollectionView))]
	sealed class DocumentStmMenuEDocsDependentCollectionViewTest : BusinessObjectCollectionViewTestCase<DocumentStmMenuEDocsDependentCollectionView>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			DocumentStmMenuEDocs stmMenuEDocs = Factory.New<DocumentStmMenuEDocs>();
			return stmMenuEDocs;
		}
		protected override DocumentStmMenuEDocsDependentCollectionView GetCollectionToTest()
		{
			DocumentCommand parentItem = Factory.New<DocumentCommand>();
			parentItem.SU_BusinessContext = nameof(BusinessContext.Shipment);
			return new DocumentStmMenuEDocsDependentCollectionView(new DocumentStmMenuEDocsDependentCollection(parentItem, Factory));
		}

		public void TestIsThisPartOfTheCollection()
		{
			DocumentCommand parentItem = Factory.New<DocumentCommand>();
			parentItem.SU_BusinessContext = nameof(BusinessContext.Shipment);

			RefDocType docType1 = Factory.New<RefDocType>();
			docType1.RT_ReferenceType = "SHP";
			docType1.RT_DocType = "AAA";

			RefDocType docType2 = Factory.New<RefDocType>();
			docType2.RT_ReferenceType = "SHP";
			docType2.RT_DocType = "BBB";

			StmMenuEDocs clientSupressedMenu = Factory.New<StmMenuEDocs>();
			clientSupressedMenu.SX_IsClientSupressed = true;
			clientSupressedMenu.SX_IsSystemDefined = true;
			clientSupressedMenu.SX_SU = parentItem.PK;
			clientSupressedMenu.SX_RT_DocType = docType1.PK;

			StmMenuEDocs userMenu = Factory.New<StmMenuEDocs>();
			userMenu.SX_IsClientSupressed = false;
			userMenu.SX_IsSystemDefined = false;
			userMenu.SX_SU = parentItem.PK;
			userMenu.SX_RT_DocType = docType2.PK;

			Factory.Save();

			parentItem.Reload();
			DocumentStmMenuEDocsDependentCollection collection = parentItem.EDocs;
			DocumentStmMenuEDocsDependentCollectionView view = parentItem.EDocsView;

			AssertEquals("Collection should have two elements", 2, collection.Count);
			AssertEquals("View should have just one element (excluding IsClientSupressed values)", 1, view.Count);
			AssertEquals("View's element should be the UserMenu", userMenu.PK, view[0].PK);

			clientSupressedMenu.SX_IsClientSupressed = false;
			view.Rebuild();
			AssertEquals("View should now have two elements", 2, view.Count);
		}
	}
}
