using CargoWise.Types;
using Enterprise.Accounting.Business.ChequeTransaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ChequeTransaction
{
	[TestedType(typeof(ChequeTransactionHeader))]
	public abstract class ChequeTransactionHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFountain()
		{
			Assert("Currently not support", true);
		}

		protected virtual ChequeTransactionHeader GetFirstCreatedAccPaymentBatch() => Factory.New<ChequeTransactionHeader>();

		public void TestDefaultValue()
		{
			var batch = Factory.New<ChequeTransactionHeader>();

			CombineAssertions("Default value asserting", () =>
			{
				AssertEquals(nameof(ChequeTransactionHeader.APB_GC), GlbCompany.CurrentCompany.PK, batch.APB_GC);
				AssertEquals(nameof(ChequeTransactionHeader.APB_GB), GlbBranch.CurrentBranch.PK, batch.APB_GB);
				AssertEquals(nameof(ChequeTransactionHeader.APB_PaymentDate), ZDateTime.Today, batch.APB_PaymentDate);
				AssertEquals(nameof(ChequeTransactionHeader.APB_PostDate), ZDateTime.Today, batch.APB_PostDate);
				AssertEquals(nameof(ChequeTransactionHeader.APB_PaymentType), ChequeTransactionTypes.ChequeEntryTransaction, batch.APB_PaymentType);
			});
		}

		public void TestAmountTotal()
		{
			Assert("Currently not support", true);
		}

		public void TestLocalAmountTotal()
		{
			Assert("Currently not support", true);
		}
	}
}
