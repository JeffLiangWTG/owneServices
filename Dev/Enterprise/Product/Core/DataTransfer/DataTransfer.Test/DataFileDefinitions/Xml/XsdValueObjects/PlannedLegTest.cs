using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.PlannedLeg))]
	sealed class PlannedLegTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.PlannedLeg value = null;
			value = new Xsd.PlannedLegCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
