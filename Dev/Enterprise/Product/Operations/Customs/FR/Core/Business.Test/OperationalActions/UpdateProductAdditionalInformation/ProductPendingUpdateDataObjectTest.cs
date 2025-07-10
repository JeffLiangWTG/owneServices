using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	[TestedType(typeof(ProductPendingUpdateDataObject))]
	public class ProductPendingUpdateDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetDescriptionByProductPk()
		{
			var dataObject = new ProductPendingUpdateDataObject(Factory);
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_Desc = "TEST DESC";
			dataObject.ProductPk = product.PK;
			AssertEquals("When the Product changes, the Description should update accordingly.", "TEST DESC", dataObject.Description);

			product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_Desc = "TEST DESC 2";
			dataObject.ProductPk = product.PK;
			AssertEquals("When the Product changes, the Description should update accordingly.", "TEST DESC 2", dataObject.Description);
		}

		public void TestDescription_ReadOnly()
		{
			var dataObject = new ProductPendingUpdateDataObject(Factory);
			AssertEquals(true, dataObject.DescriptionInfo.ReadOnly);
		}

		public void TestTypeOfLookups()
		{
			var dataObject = new ProductPendingUpdateDataObject(Factory);
			AssertType(typeof(ProductPendingUpdateDataObjectLookups), dataObject.Lookups);
		}

		public void TestTypeOfValidation()
		{
			var dataObject = new ProductPendingUpdateDataObject(Factory);
			AssertType(typeof(ProductPendingUpdateDataObjectValidation), dataObject.Validation);
		}
	}
}
