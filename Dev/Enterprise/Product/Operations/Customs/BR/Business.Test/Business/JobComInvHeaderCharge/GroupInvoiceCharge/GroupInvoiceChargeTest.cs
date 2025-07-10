using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	class GroupInvoiceChargeTest : Customs.Business.Testing.BaseGroupInvoiceChargeTest
	{
		public void TestJ7_PrepaidCollectReadOnly()
		{
			TestDec.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var groupCharge = GroupHeader.Charges.AddNew();

			groupCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightCollect;
			Assert("J7_PrepaidCollect must be ReadOnly", groupCharge.J7_PrepaidCollectInfo.ReadOnly);

			groupCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory;
			Assert("J7_PrepaidCollect must NOT be ReadOnly", !groupCharge.J7_PrepaidCollectInfo.ReadOnly);

			groupCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid;
			Assert("J7_PrepaidCollect must be ReadOnly", groupCharge.J7_PrepaidCollectInfo.ReadOnly);
		}

		public void TestPrepaidOrCollect()
		{
			TestDec.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var groupCharge = GroupHeader.Charges.AddNew();

			groupCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightCollect;
			AssertEquals("J7_PrepaidCollect must Collect (CCX)", Core.Constants.PaymentType.Collect, groupCharge.J7_PrepaidCollect);

			groupCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid;
			AssertEquals("J7_PrepaidCollect must Prepaid (PPD)", Core.Constants.PaymentType.Prepaid, groupCharge.J7_PrepaidCollect);
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseGroupInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestImportLicenseDistributeBy()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var groupInvoiceHeader = declaration.Invoices.AddNew();
			groupInvoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var charge = groupInvoiceHeader.GroupCharges.AddNew(ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, 100m, groupInvoiceHeader.Invoice_Currency.RX_Code);
			AssertEquals("Distribute by should be NWT", ChargeDistributeByList.Codes.NetWeight, charge.J7_DistributeBy);

			charge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightCollect;
			AssertEquals("Distribute by should be NWT", ChargeDistributeByList.Codes.NetWeight, charge.J7_DistributeBy);

			charge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid;
			AssertEquals("Distribute by should be NWT", ChargeDistributeByList.Codes.NetWeight, charge.J7_DistributeBy);

			charge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasInsurance;
			AssertEquals("Distribute by should be FOB", ChargeDistributeByList.Codes.FOB, charge.J7_DistributeBy);
		}

		public void TestDefaultCurrencyAndReadOnly()
		{
			TestDec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var groupCharge = GroupHeader.Charges.AddNew();

			groupCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Zimbabwe;
			groupCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory;
			Assert("J7_RX_NKCurrencyInfo must NOT be ReadOnly for FNT", !groupCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			AssertEquals("J7_RX_NKCurrency must be BRL for FCO", Core.Constants.CurrencyCodes.Zimbabwe, groupCharge.J7_RX_NKCurrency);

			groupCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightComponents;
			Assert("J7_RX_NKCurrency must NOT be ReadOnly for FCO", !groupCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			AssertEquals("J7_RX_NKCurrency must be BRL for FCO", Core.Constants.CurrencyCodes.Brazil, groupCharge.J7_RX_NKCurrency);

			groupCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Zimbabwe;
			groupCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OtherExpensesICMS;
			Assert("J7_RX_NKCurrency must be ReadOnly for EIC", groupCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			AssertEquals("J7_RX_NKCurrency must be BRL for EIC", Core.Constants.CurrencyCodes.Brazil, groupCharge.J7_RX_NKCurrency);
		}

		public void TestIsFreightComponents()
		{
			TestDec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var groupCharge = GroupHeader.Charges.AddNew() as GroupInvoiceCharge;

			groupCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory;
			Assert("IsFreightComponents must be False", !groupCharge.IsFreightComponents);

			groupCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightComponents;
			Assert("IsFreightComponents must be True", groupCharge.IsFreightComponents);
		}

		public void TestIsOtherExpensesICMS()
		{
			TestDec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var groupCharge = GroupHeader.Charges.AddNew() as GroupInvoiceCharge;

			groupCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory;
			Assert("IsOtherExpensesICMS must be False", !groupCharge.IsOtherExpensesICMS);

			groupCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OtherExpensesICMS;
			Assert("IsOtherExpensesICMS must be True", groupCharge.IsOtherExpensesICMS);
		}

		protected override ICustomsChargeCode GetOverseasFreightCharge()
		{
			return ImportCommonChargesProvider.OverseasFreightCollect;
		}

		protected override string GetOverseasFreightChargeCodeForTest()
		{
			if (TestDec.IsImport)
			{
				return ImportCustomsChargeTypeList.Codes.OverseasFreightCollect;
			}
			return base.GetOverseasFreightChargeCodeForTest();
		}

		protected override BaseJobDeclaration GetNewDeclarationForTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			return declaration;
		}
	}
}
