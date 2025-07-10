using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USInvoiceLineOGAIndicators))]
	sealed class USInvoiceLineOGAIndicatorsTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			USInvoiceLineOGAIndicators ind = new USInvoiceLineOGAIndicators();
			ind.DOTIndicator = OGAIndicator.Declared;
			AssertEquals(true, ind.IsSpecified);
		}
	}
}
