using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	sealed class JobComInvoiceGroupHeaderTest : BaseJobComInvoiceGroupHeaderTest
	{
		public void TestGroupCharges()
		{
			var groupHeader = Factory.New<JobDeclaration>().JobComInvoiceGroupHeaders[0];
			AssertType<Common.JobComInvChargeCollection<GroupInvoiceCharge>>(groupHeader.Charges);
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseJobComInvoiceGroupHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceGroupHeader)).GetType() == typeof(JobComInvoiceGroupHeader));
		}

		public override void TestChargeTypeList()
		{
			var dec = GetNewDeclarationForTest();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			ICommonInvoice commonInvoice = dec.TopGroupInvoice;
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));

			var customsChargeTypeList = new CodeDescriptionPairList();
			customsChargeTypeList.AddRange(new ImportChargeMethodOneCodeList());
			customsChargeTypeList.AddRange(new ImportChargeMethodTwoAndThreeCodeList());
			customsChargeTypeList.AddRange(new ImportChargeMethodFourCodeList());
			customsChargeTypeList.AddRange(new ImportChargeMethodFiveAndSixCodeList());
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));

			customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().JobComInvoiceGroupHeaders[0];

		protected override Type ExpectedTypeOfCharges => typeof(Common.JobComInvChargeCollection<GroupInvoiceCharge>);
	}
}
