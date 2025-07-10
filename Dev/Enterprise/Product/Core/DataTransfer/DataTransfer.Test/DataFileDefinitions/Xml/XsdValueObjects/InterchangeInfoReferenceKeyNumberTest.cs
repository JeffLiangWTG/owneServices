using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.InterchangeInfoReferenceKey))]
	sealed class InterchangeInfoReferenceKeyNumberTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.InterchangeInfoReferenceKey value = null;
			value = new Xsd.InterchangeInfoReferenceKeyCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
