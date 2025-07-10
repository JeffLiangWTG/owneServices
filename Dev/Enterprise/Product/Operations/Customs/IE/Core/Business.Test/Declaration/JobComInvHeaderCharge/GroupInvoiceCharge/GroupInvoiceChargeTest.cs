using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	sealed class GroupInvoiceChargeTest : EU.Business.Declaration.Testing.GroupInvoiceChargeTest
	{
		public void TestLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var groupInvoiceCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();

			AssertType<ImportGroupInvoiceChargeLookups>("Lookups for IMP", groupInvoiceCharge.Lookups);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertType<EU.Business.Declaration.GroupInvoiceChargeLookups>("Lookups for EXP", declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew().Lookups);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
			AssertType<EU.Business.Declaration.GroupInvoiceChargeLookups>("Lookups for non-IMP or EXP", declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew().Lookups);
		}

		public override void TestChargePrepaidCollectCommittedToApportionedCharge()
		{
			var invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_IncoTerm = GetOverseasFreightIncoTermForTest();
			invoice1.JZ_InvoiceAmount = 1000;
			invoice1.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			var invoice2 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_IncoTerm = "CIF";
			invoice2.JZ_InvoiceAmount = 1000;
			invoice2.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			var oFT = GroupHeader.Charges.AddNew();
			oFT.J7_ChargeType = GetOverseasFreightChargeCodeForTest();
			oFT.J7_Amount = 100;
			oFT.J7_RX_NKCurrency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			TestDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition:OFT is Prepaid", Core.Constants.PaymentType.Collect, oFT.J7_PrepaidCollect);
				AssertEquals("1 apportioned Charge", 1, invoice1.GroupCharges.Count);
				AssertEquals("1 apportioned Charge", 1, invoice2.GroupCharges.Count);

				oFT.J7_PrepaidCollect = Core.Constants.PaymentType.Collect;
				TestDec.ResumeApportionment();
				AssertEquals("Now it is collect", Core.Constants.PaymentType.Collect, invoice1.GroupCharges[0].J7_PrepaidCollect);
				AssertEquals("Now it is collect", Core.Constants.PaymentType.Collect, invoice2.GroupCharges[0].J7_PrepaidCollect);
			});
		}

		public override void TestDefaultPrepaidCollectForGroupCharge2()
		{
			var invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_IncoTerm = GetOverseasFreightIncoTermForTest();
			var invoice2 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_IncoTerm = "CIF";

			var oFT = GroupHeader.Charges.AddNew();
			oFT.J7_ChargeType = GetOverseasFreightChargeCodeForTest();
			AssertEquals("Overseas freight charge is Prepaid", Core.Constants.PaymentType.Collect, oFT.J7_PrepaidCollect);
		}

		protected override string GetOverseasFreightIncoTermForTest() => Core.Constants.IncoTerms.FreeAlongsideShip;

		protected override string GetOverseasFreightChargeCodeForTest() => AISChargeCodeList.Codes.AK;

		protected override string ChargeCodeForTestApportionChargeWithSameChargeTypeWithDifferntKeys => AISChargeCodeList.Codes.AK;

		protected override ICustomsChargeCode GetOverseasFreightCharge() => new ImportIncoTermAndCustomsChargeFactory().GetCharge(AISChargeCodeList.Codes.BA);
	}
}
