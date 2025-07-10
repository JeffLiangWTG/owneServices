using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.InterchangeInfoSourceCustom))]
	sealed class InterchangeInfoSourceCustomTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.InterchangeInfoSourceCustom value = new Xsd.InterchangeInfoSourceCustom();
			Assert(!value.IsSpecified);
			value.Value1 = "SDG";
			Assert(value.IsSpecified);
		}
	}
}
