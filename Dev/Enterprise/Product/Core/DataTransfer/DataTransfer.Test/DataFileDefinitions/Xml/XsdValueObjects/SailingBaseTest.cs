using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.SailingBase))]
	sealed class SailingBaseTest : ValueObjectTestCase
	{
		public void TestGetVesselName()
		{
			AssertEquals("Expecting GetVesselName() to return empty by default.", ZString.Empty, new SailingBase().GetVesselName(null));
		}
	}
}
