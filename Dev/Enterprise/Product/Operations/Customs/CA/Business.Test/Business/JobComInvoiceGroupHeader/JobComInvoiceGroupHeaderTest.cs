using System;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	sealed class JobComInvoiceGroupHeaderTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTest
	{
		public void TestAsClaimed()
		{
			AssertEquals("As Claimed", JobComInvoiceGroupHeader.AsClaimed);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			ICommonInvoice commonInvoice = dec.TopGroupInvoice;
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			var customsChargeTypeList = new CAChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseJobComInvoiceGroupHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceGroupHeader)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestICurrencyConverterDataProvider()
		{
			var inv = Factory.New<JobComInvoiceGroupHeader>();
			var invAsProvider = inv as ICurrencyConverterDataProvider;
			AssertEquals("Fall back days", 365, invAsProvider.MaximumDaysToFallback);

			var dec = Factory.New<JobDeclaration>();
			var inv1 = dec.TopGroupInvoice;
			var inv1AsProvider = inv1 as ICurrencyConverterDataProvider;
			AssertEquals("Fall back days", 365, inv1AsProvider.MaximumDaysToFallback);
		}

		protected override Type ExpectedTypeOfCharges => typeof(Common.JobComInvChargeCollection<GroupInvoiceCharge>);
	}
}
