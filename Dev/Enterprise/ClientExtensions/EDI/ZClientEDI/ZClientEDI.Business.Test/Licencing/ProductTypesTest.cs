using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class ProductTypesTest : TestCaseWithFactory
	{
		public void TestENTDescription()
		{
			var types = new ProductTypes();
			AssertEquals("CargoWise", types[ProductTypes.Codes.Enterprise].Description);

			types = new ProductTypes(true);
			AssertEquals("ediEnterprise", types[ProductTypes.Codes.Enterprise].Description);
		}

		public void TestCargoWiseNext()
		{
			var types = new ProductTypes(true);
			AssertEquals("CargoWise Next", types[ProductTypes.Codes.CargoWiseNext].Description);
		}

		public void TestEnterpriseFamilyProducts()
		{
			AssertEquals("Enterprise Family", 5, ProductTypes.EnterpriseFamilyProducts.Count());
			AssertContainsExactElementsInAnyOrder("Enterprise Family", 
				new[] { ProductTypes.Codes.CargoWiseOne, ProductTypes.Codes.CargoWiseNext, ProductTypes.Codes.CargoWise, ProductTypes.Codes.Enterprise, ProductTypes.Codes.ProductivityWise }, 
				ProductTypes.EnterpriseFamilyProducts);
		}
	}
}
