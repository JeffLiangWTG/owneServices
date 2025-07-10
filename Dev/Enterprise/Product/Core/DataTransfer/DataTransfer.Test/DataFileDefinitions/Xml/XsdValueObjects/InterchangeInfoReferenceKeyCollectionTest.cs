using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.InterchangeInfoReferenceKeyCollection))]
	sealed class InterchangeInfoReferenceKeyCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.InterchangeInfoReferenceKeyCollection value = null;
			value = new Xsd.InterchangeInfo().ReferenceKeys;
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
