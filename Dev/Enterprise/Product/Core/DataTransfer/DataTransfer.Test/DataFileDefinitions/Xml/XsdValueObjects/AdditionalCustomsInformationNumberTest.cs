using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.AdditionalCustomsInformation))]
	sealed class AdditionalCustomsInformationNumberTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.AdditionalCustomsInformation value = null;
			value = new Xsd.AdditionalCustomsInformationCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
