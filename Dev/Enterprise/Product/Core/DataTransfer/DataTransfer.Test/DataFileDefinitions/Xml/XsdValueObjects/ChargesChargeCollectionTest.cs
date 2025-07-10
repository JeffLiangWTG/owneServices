using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ChargesChargeCollection))]
	sealed class ChargesChargeCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.ChargesChargeCollection value = null;
			value = new Xsd.Charges().Charge;
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
