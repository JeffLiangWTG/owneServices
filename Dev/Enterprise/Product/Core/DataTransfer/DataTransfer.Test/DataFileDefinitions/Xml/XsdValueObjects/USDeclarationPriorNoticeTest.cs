using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USDeclarationPriorNotice))]
	sealed class USDeclarationPriorNoticeTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USDeclarationPriorNotice uSDeclarationPriorNotice = new Xsd.USDeclarationPriorNotice();
			AssertEquals(false, uSDeclarationPriorNotice.IsSpecified);

			uSDeclarationPriorNotice.ActualTimeOfArrival = ZDateTime.Now;
			AssertEquals(true, uSDeclarationPriorNotice.IsSpecified);

			uSDeclarationPriorNotice.ActualTimeOfArrival = ZDateTime.Empty;
			uSDeclarationPriorNotice.PortOfCrossing = "Test";
			AssertEquals(true, uSDeclarationPriorNotice.IsSpecified);

			uSDeclarationPriorNotice.PortOfCrossing = "";
			AssertEquals(false, uSDeclarationPriorNotice.IsSpecified);

			uSDeclarationPriorNotice.Submitter.OwnerCode = "XYZ";
			AssertEquals(true, uSDeclarationPriorNotice.IsSpecified);

			uSDeclarationPriorNotice.Submitter.OwnerCode = "";
			uSDeclarationPriorNotice.ContactName = "JOHN";
			AssertEquals(true, uSDeclarationPriorNotice.IsSpecified);

			uSDeclarationPriorNotice.ContactName = "";
			AssertEquals(false, uSDeclarationPriorNotice.IsSpecified);

			uSDeclarationPriorNotice.ContactPhoneNo = "3297000500";
			AssertEquals(true, uSDeclarationPriorNotice.IsSpecified);

			uSDeclarationPriorNotice.ContactPhoneNo = "";
			AssertEquals(false, uSDeclarationPriorNotice.IsSpecified);
		}
	}
}
