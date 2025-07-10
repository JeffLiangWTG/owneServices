using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	[TestedType(typeof(EDIOrgSupplierPartCollection))]
	public class EDIOrgSupplierPartCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollection()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "123";
			part.OP_Desc = "Description";
			Factory.Save();

			EDIOrgSupplierPartCollection coll = new EDIOrgSupplierPartCollection(Factory);
			coll.Load();
			AssertEquals("Count is 1", 1, coll.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EDIOrgSupplierPartCollection(Factory);
		}
	}
}
