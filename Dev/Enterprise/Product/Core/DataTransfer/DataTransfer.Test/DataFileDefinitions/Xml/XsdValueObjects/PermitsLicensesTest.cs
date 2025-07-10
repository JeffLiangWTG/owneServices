using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.PermitAndLicenceType))]
	sealed class PermitsLicensesTest : ValueObjectTestCase
	{
		public void TestIsSpecifiedTrueByDefault()
		{
			Xsd.PermitAndLicenceType value = new Xsd.PermitAndLicenceType();
			AssertEquals(false, value.IsSpecified);

			value.AgricultureLicNo = "XYZ";
			AssertEquals(true, value.IsSpecified);

			value = new Xsd.PermitAndLicenceType();
			value.AgricultureLicNo = "XYZ";
			AssertEquals(true, value.IsSpecified);

			value = new Xsd.PermitAndLicenceType();
			value.CAExportCertificate = "XYZ";
			AssertEquals(true, value.IsSpecified);

			value = new Xsd.PermitAndLicenceType();
			value.CBTPACertificate = "XYZ";
			AssertEquals(true, value.IsSpecified);

			value = new Xsd.PermitAndLicenceType();
			value.MiscPermitNo = "XYZ";
			AssertEquals(true, value.IsSpecified);

			value = new Xsd.PermitAndLicenceType();
			value.WoolLicenceNo = "XYZ";
			AssertEquals(true, value.IsSpecified);

			value = new Xsd.PermitAndLicenceType();
			value.SWPMIndicator = "XYZ";
			AssertEquals(true, value.IsSpecified);

			value = new Xsd.PermitAndLicenceType();
			value.CottonCertificateNo = "XYZ";
			AssertEquals(true, value.IsSpecified);

			value = new Xsd.PermitAndLicenceType();
			value.CottonFeeExemptIndicator = "XYZ";
			AssertEquals(true, value.IsSpecified);
		}
	}
}
