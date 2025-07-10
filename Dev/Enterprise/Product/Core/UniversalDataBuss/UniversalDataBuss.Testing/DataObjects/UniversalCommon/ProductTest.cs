using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(Product))]
	class ProductTest : DataObjectTestCase<Product>
	{
		protected override bool ShouldBeFlattenedIntoAttributes
		{
			get { return true; }
		}
	}
}
