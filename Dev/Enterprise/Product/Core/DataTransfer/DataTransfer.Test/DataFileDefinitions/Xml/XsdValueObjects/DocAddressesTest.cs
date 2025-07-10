using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.DocAddresses))]
	sealed class DocAddressesTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			DocAddresses docAddresses = new DocAddresses();

			AssertEquals("Precondition - 0 DocAddress objects.", 0, docAddresses.DocAddress.Count);
			AssertEquals("With 0 docAddress, IsSpecified should be false.", false, docAddresses.IsSpecified);

			docAddresses.DocAddress.AddNew();
			AssertEquals("With 1 or more docAddress, IsSpecified should be true.", true, docAddresses.IsSpecified);
		}
	}
}
