using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.DebtorBalance))]
	sealed class DebtorBalanceTest : ValueObjectTestCase
	{
		public void TestIsSpecified_UpdatedIfNewFieldsAdded()
		{
			AssertEquals(
				"If the number of properties changes, you should update IsSpecified",
				8, typeof(DebtorBalance).GetProperties().Length);
		}

		public void TestIsSpecified()
		{
			Xsd.DebtorBalance debtorBalance = new Xsd.DebtorBalance();
			AssertEquals("Should not be specified by default", false, debtorBalance.IsSpecified);

			debtorBalance.Debtor.EDICode = "xxx";
			AssertEquals("Should not be specified", false, debtorBalance.IsSpecified);

			debtorBalance.OutstandingBalance.Value = 100m;
			debtorBalance.OutstandingBalanceSpecified = true;
			AssertEquals("Should be specified if outstaning balance is specified", true, debtorBalance.IsSpecified);

			debtorBalance.OutstandingBalance.Value = 0m;
			debtorBalance.OutstandingWIPAmount.Value = 100m;
			AssertEquals("Should be specified if outstaning balance is specified", true, debtorBalance.IsSpecified);

			debtorBalance.OutstandingWIPAmountSpecified = false;
			AssertEquals("Should not be specified ", false, debtorBalance.IsSpecified);
		}
	}
}
