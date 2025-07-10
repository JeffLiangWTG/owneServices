using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USFDAEstablismentID))]
	sealed class USFDAEstablismentIDTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			var fDAEstablismentID = new Xsd.USFDAEstablismentID();
			AssertEquals(false, fDAEstablismentID.IsSpecified);

			fDAEstablismentID.Item = "TEST";
			AssertEquals(true, fDAEstablismentID.IsSpecified);

			fDAEstablismentID.Item = "";
			var organisation = new Xsd.Organisation();
			organisation.EDICode = "TST";
			fDAEstablismentID.Item = organisation;
			AssertEquals(true, fDAEstablismentID.IsSpecified);

			organisation.EDICode = "";
			AssertEquals(false, fDAEstablismentID.IsSpecified);
		}
	}
}
