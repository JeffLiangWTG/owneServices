using System;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	class JobComInvoiceGroupHeaderTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseJobComInvoiceGroupHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceGroupHeader)).GetType() == GetExpectedBusinessObjectType());
		}

		protected override Type ExpectedTypeOfCharges => typeof(Common.JobComInvChargeCollection<GroupInvoiceCharge>);

		public override void TestChargeTypeList()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			ICommonInvoice commonInvoice = dec.JobComInvoiceGroupHeaders[0];
			var chargeTypeList = commonInvoice.ChargeTypeList;
			AssertNotNullOrEmpty("ChargeTypeList has 'RYT'", chargeTypeList.GetDescriptionFromCode("RYT"));
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList = commonInvoice.ChargeTypeList;
			AssertNullOrEmpty("ChargeTypeList not has 'RYT'", chargeTypeList.GetDescriptionFromCode("RYT"));
		}
	}
}
