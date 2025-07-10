using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USOrganisationSpecificDetailsReconciliation))]
	sealed class USOrganisationSpecificDetailsReconciliationTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USOrganisationSpecificDetailsReconciliation recon = new Xsd.USOrganisationSpecificDetailsReconciliation();
			AssertEquals(false, recon.IsSpecified);

			recon.FileTheirOwnRecon = TrueFalse.@true;
			AssertEquals(true, recon.IsSpecified);

			recon.NAFTA = TrueFalse.@true;
			AssertEquals(true, recon.IsSpecified);
		}
	}
}
