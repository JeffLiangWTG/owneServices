using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USOrganisationSpecificDetailsImporterOfRecordDetails))]
	sealed class USOrganisationSpecificDetailsImporterOfRecordDetailsTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USOrganisationSpecificDetailsImporterOfRecordDetails ior = new Xsd.USOrganisationSpecificDetailsImporterOfRecordDetails();

			ior.PaymentType = USImporterOfRecordDetailsPaymentType.Item2;
			AssertEquals(true, ior.IsSpecified);

			ior.TaxDeferredInd = USImporterOfRecordDetailsTaxDeferredInd.Item1;
			AssertEquals(true, ior.IsSpecified);

			ior.PaymentTypeSpecified = false;
			ior.TaxDeferredIndSpecified = false;
			AssertEquals(false, ior.IsSpecified);
			ior.NotifyParty.Item = "Sdsds";
			AssertEquals(true, ior.IsSpecified);
		}
	}
}
