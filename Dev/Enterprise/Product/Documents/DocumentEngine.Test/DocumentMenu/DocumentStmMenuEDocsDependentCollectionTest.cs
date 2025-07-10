using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	[TestedType(typeof(DocumentStmMenuEDocsDependentCollection))]
	sealed class DocumentStmMenuEDocsDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			DocumentCommand parentItem = Factory.New<DocumentCommand>();
			parentItem.SU_BusinessContext = nameof(BusinessContext.Shipment);

			return parentItem.EDocs;
		}

		public void TestContains()
		{
			RefDocType docType1 = Factory.New<RefDocType>();
			docType1.RT_ReferenceType = "SHP";
			docType1.RT_DocType = "AAA";

			RefDocType docType2 = Factory.New<RefDocType>();
			docType2.RT_ReferenceType = "SHP";
			docType2.RT_DocType = "BBB";

			DocumentCommand parentItem = Factory.New<DocumentCommand>();
			parentItem.SU_BusinessContext = nameof(BusinessContext.Shipment);
			DocumentStmMenuEDocsDependentCollection collection = parentItem.EDocs;

			DocumentStmMenuEDocs menu1 = Factory.New<DocumentStmMenuEDocs>();
			menu1.SX_IsClientSupressed = true;
			menu1.SX_IsSystemDefined = true;
			menu1.SX_SU = Factory.New(typeof(StmMenuItem)).PK;
			menu1.SX_RT_DocType = docType1.PK;

			DocumentStmMenuEDocs menu2 = collection.AddNew();
			menu2.SX_IsClientSupressed = false;
			menu2.SX_IsSystemDefined = false;
			menu2.SX_SU = parentItem.PK;
			menu2.SX_RT_DocType = docType2.PK;

			Factory.Save();

			AssertNull("Collection does not contain doctype AAA - indexer should return null", collection["AAA"]);
			AssertNotNull("Collection contains doctype BBB - indexer should return object", collection["BBB"]);
		}
	}
}
