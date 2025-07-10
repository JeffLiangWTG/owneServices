using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.SubAccount))]
	sealed class SubAccountTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.SubAccount subaccount = new Xsd.SubAccount();
			AssertEquals("Should not be specified by default", false, subaccount.IsSpecified);

			subaccount.Code = "SUBACCOUNT";
			AssertEquals("Should be specified", true, subaccount.IsSpecified);

			subaccount.IsSpecified = false;
			AssertEquals("Should Not be specified", false, subaccount.IsSpecified);
		}
	}
}
