using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Modules.Testing
{
	[TestedType(typeof(ProductImportRegistryBusinessObject))]
	public class ProductImportRegistryBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ProductImportRegistryBusinessObject();
		}
	}
}
