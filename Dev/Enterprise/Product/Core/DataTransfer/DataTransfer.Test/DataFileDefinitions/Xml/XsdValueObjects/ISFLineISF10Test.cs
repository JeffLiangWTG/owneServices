using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ISFLineISF10))]
	sealed class ISFLineISF10Test : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.ISFLineISF10 iSFLineISF10 = new Xsd.ISFLineISF10();
			AssertEquals("Should not be specified by default", false, iSFLineISF10.IsSpecified);

			iSFLineISF10.ManufacturerID = "ID1232";
			AssertEquals("ISFLineISF10.IsSpecified", true, iSFLineISF10.IsSpecified);
		}
	}
}
