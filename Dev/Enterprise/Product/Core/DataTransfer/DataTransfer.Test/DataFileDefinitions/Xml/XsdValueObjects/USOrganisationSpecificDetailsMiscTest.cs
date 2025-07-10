using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USOrganisationSpecificDetailsMisc))]
	sealed class USOrganisationSpecificDetailsMiscTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USOrganisationSpecificDetailsMisc misc = new Xsd.USOrganisationSpecificDetailsMisc();
			AssertEquals(false, misc.IsSpecified);

			misc.BIRDDefaultBranch = "CHI";
			AssertEquals(true, misc.IsSpecified);
		}
	}
}
