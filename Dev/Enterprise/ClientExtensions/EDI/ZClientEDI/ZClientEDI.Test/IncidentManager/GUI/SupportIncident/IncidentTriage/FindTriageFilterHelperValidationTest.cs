using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	public class FindTriageFilterHelperValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNodeType()
		{
			Helper.NodeType = IncidentTriageTypes.Codes.Service;
			AssertNoErrors(Helper.NodeTypeInfo);

			Helper.NodeType = "ZZZ";
			AssertHasError(Helper.NodeTypeInfo, "Enter a valid Node Type.");

			Helper.NodeType = IncidentTriageTypes.Codes.Compliance;
			AssertNoErrors(Helper.NodeTypeInfo);
		}

		public void TestProduct()
		{
			var productCollection = new SystemProductCollection();
			var product1 = productCollection.AddNew();
			product1.Code = "ZZZ";
			product1.Description = "AAA Aardvark";
			product1.Enabled = true;

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productCollection);

			Helper.Product = ProductTypes.Codes.Enterprise;
			AssertNoErrors(Helper.ProductInfo);

			Helper.Product = "XXX";
			AssertHasError(Helper.ProductInfo, "Enter a valid Product.");

			Helper.Product = product1.Code;
			AssertNoErrors(Helper.ProductInfo);
		}

		public void TestProductArea()
		{
			Helper.ProductArea = ProductAreaList.Codes.ARC;
			AssertNoErrors(Helper.ProductAreaInfo);

			Helper.ProductArea = "XXX";
			AssertHasError(Helper.ProductAreaInfo, "Enter a valid Product Area.");

			Helper.ProductArea = ProductAreaList.Codes.CIL;
			AssertNoErrors(Helper.ProductAreaInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Helper = new FindTriageFilterHelper(incident);
		}

		FindTriageFilterHelper Helper;
	}
}
