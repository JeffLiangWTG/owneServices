using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.AccountsReceivable))]
	sealed class AccountsReceivableTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.AccountsReceivable accountsReceivable = new Xsd.AccountsReceivable();
			AssertEquals("Should not be specified by default", false, accountsReceivable.IsSpecified);

			accountsReceivable.DefaultCurrency = "UAH";
			AssertEquals("Should be specified if there is a address", true, accountsReceivable.IsSpecified);
		}
	}
}
