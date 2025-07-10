using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USOrganisationSpecificDetailsEntryDocumentPrinting))]
	sealed class USOrganisationSpecificDetailsEntryDocumentPrintingTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USOrganisationSpecificDetailsEntryDocumentPrinting prn = new Xsd.USOrganisationSpecificDetailsEntryDocumentPrinting();
			AssertEquals(false, prn.IsSpecified);

			prn.CustomAttribute1 = TrueFalse.@true;
			AssertEquals(true, prn.IsSpecified);
		}
	}
}
