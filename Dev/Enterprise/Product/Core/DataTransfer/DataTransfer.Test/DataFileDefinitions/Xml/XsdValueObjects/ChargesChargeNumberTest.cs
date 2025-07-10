using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ChargesCharge))]
	sealed class ChargesChargeNumberTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.ChargesCharge value = null;
			value = new Xsd.ChargesChargeCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
