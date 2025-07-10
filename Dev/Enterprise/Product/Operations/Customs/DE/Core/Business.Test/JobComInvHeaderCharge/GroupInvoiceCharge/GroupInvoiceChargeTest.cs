using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	sealed class GroupInvoiceChargeTest : EU.Business.Declaration.Testing.GroupInvoiceChargeTest
	{
		public void TestLookups()
		{
			AssertType<GroupInvoiceChargeLookups>(groupInvoiceCharge.Lookups);
		}

		public void TestValidation()
		{
			AssertType<GroupInvoiceChargeValidation>(groupInvoiceCharge.Validation);
		}

		public override void TestChargePrepaidCollectCommittedToApportionedCharge()
		{
			var invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_InvoiceAmount = 1000;
			invoice1.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			var invoice2 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_IncoTerm = "CIF";
			invoice2.JZ_InvoiceAmount = 1000;
			invoice2.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			var overseasFreightCharge = GroupHeader.Charges.AddNew();
			overseasFreightCharge.J7_ChargeType = GetOverseasFreightChargeCodeForTest();
			overseasFreightCharge.J7_Amount = 100;
			overseasFreightCharge.J7_RX_NKCurrency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			TestDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("PreCondition:OFT is Collect", Core.Constants.PaymentType.Collect, overseasFreightCharge.J7_PrepaidCollect);
				AssertEquals("1 apportioned Charge", 1, invoice1.GroupCharges.Count);
				AssertEquals("1 apportioned Charge", 1, invoice2.GroupCharges.Count);

				overseasFreightCharge.J7_PrepaidCollect = Core.Constants.PaymentType.Collect;
				TestDec.ResumeApportionment();
				AssertEquals("Now it is collect", Core.Constants.PaymentType.Collect, invoice1.GroupCharges[0].J7_PrepaidCollect);
				AssertEquals("Now it is collect", Core.Constants.PaymentType.Collect, invoice2.GroupCharges[0].J7_PrepaidCollect);
			});
		}

		public override void TestDefaultPrepaidCollectForGroupCharge2()
		{
			var invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_IncoTerm = "FOB";
			var invoice2 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_IncoTerm = "CIF";

			var overseasFreightCharge = GroupHeader.Charges.AddNew();
			overseasFreightCharge.J7_ChargeType = GetOverseasFreightChargeCodeForTest();
			AssertEquals("Overseas freight charge is Collect", Core.Constants.PaymentType.Collect, overseasFreightCharge.J7_PrepaidCollect);
		}

		public void TestJ7_IsDutiable_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes._001, groupInvoiceCharge.J7_IsDutiableInfo);
		}

		public void TestJ7_IsStatisticalValueApplicable_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes._002, groupInvoiceCharge.J7_IsStatisticalValueApplicableInfo);
		}

		public void TestJ7_IsGSTApplicable_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes._003, groupInvoiceCharge.J7_IsGSTApplicableInfo);
		}

		public void TestJ7_IsIncludedInITOT_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes.TCE, groupInvoiceCharge.J7_IsIncludedInITOTInfo);
		}

		public void TestJ7_Percentage_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(CustomsChargeTypeList.Codes.Discount, groupInvoiceCharge.J7_PercentageInfo);
		}

		public void TestIsJ7_ExchangeRateIATA_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes.INP, groupInvoiceCharge.IsJ7_ExchangeRateIATAInfo);
		}

		public void TestIsJ7_ExchangeRateIATA()
		{
			CombineAssertions(() =>
			{
				groupInvoiceCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.IATARate;
				AssertEquals("Getter - true", true, groupInvoiceCharge.IsJ7_ExchangeRateIATA);
				groupInvoiceCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
				AssertEquals("Getter - false", false, groupInvoiceCharge.IsJ7_ExchangeRateIATA);

				groupInvoiceCharge.IsJ7_ExchangeRateIATA = true;
				AssertEquals("Setter - true", ChargeExchangeRateTypeList.Codes.IATARate, groupInvoiceCharge.J7_ExchangeRateType);
				groupInvoiceCharge.IsJ7_ExchangeRateIATA = false;
				AssertEquals("Setter - false", ZString.Empty, groupInvoiceCharge.J7_ExchangeRateType);
			});
		}

		public void TestIATAAndFixedRateAreMutuallyExclusive()
		{
			CombineAssertions(() =>
			{
				groupInvoiceCharge.IsJ7_ExchangeRateUserEnterable = true;
				groupInvoiceCharge.J7_ExchangeRate = 1.25m;
				AssertEquals("Fixed Rate = true -> IATA = false", false, groupInvoiceCharge.IsJ7_ExchangeRateIATA);

				groupInvoiceCharge.IsJ7_ExchangeRateIATA = true;
				AssertEquals("IATA = true -> Fixed Rate = false", false, groupInvoiceCharge.IsJ7_ExchangeRateUserEnterable);
				AssertEquals("IATA = true -> Exchange Rate = 0", 0m, groupInvoiceCharge.J7_ExchangeRate);
			});
		}

		public void TestJ7_ExchangeRate_IATARateUsed()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestJ7_ExchangeRate_IATARateUsed(groupInvoiceCharge, groupInvoiceCharge.IsJ7_ExchangeRateIATAInfo);
			});
		}

		public void TestChangingChargeTypeResetsIATA()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default value", false, groupInvoiceCharge.IsJ7_ExchangeRateIATA);
				groupInvoiceCharge.IsJ7_ExchangeRateIATA = true;
				groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
				AssertEquals("Updated charge code supports IATA", true, groupInvoiceCharge.IsJ7_ExchangeRateIATA);
				groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._011;
				AssertEquals("Updated another charge code supports IATA", true, groupInvoiceCharge.IsJ7_ExchangeRateIATA);
				groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._012;
				AssertEquals("Updated charge code doesn't support IATA", false, groupInvoiceCharge.IsJ7_ExchangeRateIATA);
			});
		}

		public void TestClearAmountIfChargeTypeIsSetToBlank()
		{
			groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			groupInvoiceCharge.J7_Amount = 11.0m;
			CombineAssertions(() =>
			{
				AssertNotEquals("Pre-condition", ZDecimal.Zero, groupInvoiceCharge.J7_Amount);
				groupInvoiceCharge.J7_ChargeType = ZString.Empty;
				AssertEquals("Cleared", ZDecimal.Zero, groupInvoiceCharge.J7_Amount);
			});
		}

		public void TestClearCurrencyIfChargeTypeIsSetToBlank()
		{
			groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			groupInvoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			CombineAssertions(() =>
			{
				AssertNotEquals("Pre-condition", ZString.Empty, groupInvoiceCharge.J7_RX_NKCurrency);
				groupInvoiceCharge.J7_ChargeType = ZString.Empty;
				AssertEquals("Cleared", ZString.Empty, groupInvoiceCharge.J7_RX_NKCurrency);
			});
		}

		public void TestJ7_ChargeType_SetValuesIfNeeded()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes._001;
				AssertEquals("J7_IsDutiable", true, groupInvoiceCharge.J7_IsDutiable);
				AssertEquals("J7_IsStatisticalValueApplicable", true, groupInvoiceCharge.J7_IsStatisticalValueApplicable);
				AssertEquals("J7_IsGSTApplicable", true, groupInvoiceCharge.J7_IsGSTApplicable);
				AssertEquals("J7_IsIncludedInITOT", false, groupInvoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("IsJ7_ExchangeRateIATA", false, groupInvoiceCharge.IsJ7_ExchangeRateIATA);
			});
		}

		public void TestOnSaving_SetValuesForApportionChargesIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var groupCharge = invoice.GroupHeader.Charges.AddNew(ImportChargeCodeList.Codes._010, 1000m, declaration.LocalCurrencyCode);
			groupCharge.IsJ7_ExchangeRateIATA = true;
			declaration.ResumeApportionment();

			groupCharge.OnSaving();
			AssertEquals(true, invoice.GroupCharges.Cast<InvoiceApportionCharge>().Single(x => x.J7_ChargeType == ImportChargeCodeList.Codes._010).IsJ7_ExchangeRateIATA);
		}

		public void TestJ7_ChargeType_DefaultCurrencyIfNeeded()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes.INP;
				AssertEquals("J7_RX_NKCurrency is empty", ZString.Empty, groupInvoiceCharge.J7_RX_NKCurrency);

				groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes.TCE;
				AssertEquals("J7_RX_NKCurrency is 'EUR'", Core.Constants.CurrencyCodes.EuropeanUnion, groupInvoiceCharge.J7_RX_NKCurrency);
			});
		}

		public void TestJ7_RX_NKCurrencyInfo_ReadOnly()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes.INP;
				AssertEquals("Not TCE type", false, groupInvoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);

				groupInvoiceCharge.J7_Percentage = 0.25m;
				AssertEquals("base.GetJ7_RX_NKCurrency_ReadOnly() is true", true, groupInvoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);

				groupInvoiceCharge.J7_ChargeType = ImportChargeCodeList.Codes.TCE;
				groupInvoiceCharge.J7_Percentage = ZDecimal.Zero;
				AssertEquals("Is TCE type", true, groupInvoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			});
		}

		protected override string ChargeCodeForTestApportionChargeWithSameChargeTypeWithDifferntKeys => ImportChargeCodeList.Codes._011;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var groupInvoice = declaration.JobComInvoiceGroupHeaders[0];
			groupInvoiceCharge = groupInvoice.Charges.AddNew();
		}

		JobDeclaration declaration;
		GroupInvoiceCharge groupInvoiceCharge;

		void AssertReadOnlyForImportAndHighValueOvrd(ZString chargeType, ZPropertyInfo propertyInfo)
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				groupInvoiceCharge.J7_ChargeType = chargeType;
				AssertEquals("ZPropertyInfo ReadOnly is false", false, propertyInfo.ReadOnly);

				declaration.ZG_IsHighValueOvrd = ZBool.True;
				AssertEquals("ZPropertyInfo ReadOnly is true", true, propertyInfo.ReadOnly);
			});
		}
	}
}
