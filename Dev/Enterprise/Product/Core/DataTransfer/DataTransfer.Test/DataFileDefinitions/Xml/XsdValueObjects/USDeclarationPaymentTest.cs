using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USDeclarationPayment))]
	sealed class USDeclarationPaymentTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USDeclarationPayment uSDeclarationPayment = new Xsd.USDeclarationPayment();
			AssertEquals(false, uSDeclarationPayment.IsSpecified);

			uSDeclarationPayment.ClientBranchDesignation = "33";
			AssertEquals(true, uSDeclarationPayment.IsSpecified);

			uSDeclarationPayment.ClientBranchDesignation = "";
			uSDeclarationPayment.PreliminaryStatementPrintDate = ZDateTime.Today.Date;
			AssertEquals(true, uSDeclarationPayment.IsSpecified);

			uSDeclarationPayment.PreliminaryStatementPrintDate = ZDate.Empty;
			AssertEquals(false, uSDeclarationPayment.IsSpecified);
		}
	}
}
