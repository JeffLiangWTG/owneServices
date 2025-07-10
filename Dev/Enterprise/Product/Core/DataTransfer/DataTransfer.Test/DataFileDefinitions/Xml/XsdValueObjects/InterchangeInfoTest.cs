using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.InterchangeInfo))]
	sealed class InterchangeInfoTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.InterchangeInfo value = null;
			value = new Xsd.InterchangeInfoCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
