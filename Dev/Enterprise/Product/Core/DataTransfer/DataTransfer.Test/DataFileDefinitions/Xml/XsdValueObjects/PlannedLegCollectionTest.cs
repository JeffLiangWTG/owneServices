using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.PlannedLegCollection))]
	sealed class PlannedLegCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.PlannedLegCollection value = null;
			value = new Xsd.ConsolConsolDetail().PlannedLegs;
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
