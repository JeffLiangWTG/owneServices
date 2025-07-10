using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC037CIndividualGuaranteeVoucherProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("IndividualGuaranteeVoucherType missing", () => new CC037CIndividualGuaranteeVoucherProvider(null));
			});
		}
		public void TestIssueDate()
		{
			AssertEquals("IssueDate", new ZDate(2023, 01, 24), provider.IssueDate);
		}

		public void TestExpiryDate()
		{
			AssertEquals("ExpiryDate", new ZDate(2023, 01, 28), provider.ExpiryDate);
		}

		public void TestCopyGiven()
		{
			AssertEquals("CopyGiven", "1", provider.CopyGiven);
		}

		public void TestTIRCarnet()
		{
			AssertEquals("TIRCarnet", "1", provider.TIRCarnet);
		}

		public void TestVoucherAmount()
		{
			AssertEquals("VoucherAmount", 999M, provider.VoucherAmount);
		}

		public void TestCurrency()
		{
			AssertEquals("Currency", "AUD", provider.Currency);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC037CIndividualGuaranteeVoucherProvider(new IndividualGuaranteeVoucherType
			{
				IssueDate = new DateTime(2023, 01, 24, 2, 4, 6),
				ExpiryDate = new DateTime(2023, 01, 28, 2, 4, 8),
				CopyGiven = Flag.Item1,
				TirCarnet = Flag.Item1,
				VoucherAmount = 999,
				Currency = "AUD",
			});
		}
		CC037CIndividualGuaranteeVoucherProvider provider;
	}
}
