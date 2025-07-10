using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.AccountsPayable))]
	sealed class AccountsPayableTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.AccountsPayable accountsPayable = new Xsd.AccountsPayable();
			AssertEquals("Should not be specified by default", false, accountsPayable.IsSpecified);

			accountsPayable.DefaultCurrency = "UAH";
			AssertEquals("Should be specified if there is a address", true, accountsPayable.IsSpecified);
		}
	}
}
