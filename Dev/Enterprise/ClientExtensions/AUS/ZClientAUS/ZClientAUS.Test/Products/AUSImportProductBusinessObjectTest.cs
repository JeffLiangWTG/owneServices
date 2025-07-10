using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Products.Testing
{
	[TestedType(typeof(AUSImportProductBusinessObject))]
	public class AUSImportProductBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "AAAAAA";
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "BBBBBB";
			Factory.Save();
			AUSImportProductBusinessObject obj = new AUSImportProductBusinessObject(Factory);
			obj.ImporterPK = importer.PK;
			obj.SupplierPK = supplier.PK;
			AssertEquals(importer, obj.Importer);
			AssertEquals(supplier, obj.Supplier);
		}
	}
}
