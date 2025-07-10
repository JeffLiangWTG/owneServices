using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ProductCustoms))]
	sealed class ProductCustomsTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.ProductCustoms productCustoms = new Xsd.ProductCustoms();
			AssertEquals(false, productCustoms.IsSpecified);

			productCustoms.Classifications.AddNew();
			AssertEquals(true, productCustoms.IsSpecified);
		}
	}
}
