using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	sealed class InvoiceLineChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<InvoiceLineChargeLookups>(invoiceLineCharge.Lookups);
		}

		public void TestGetNewValidation_Export()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertType<ExportInvoiceLineChargeValidation>(invoiceLineCharge.Validation);
		}

		public void TestGetNewValidation_Import()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			AssertType<ImportInvoiceLineChargeValidation>(invoiceLineCharge.Validation);
		}

		public void TestGetNewValidation_Miscellaneous()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<InvoiceLineChargeValidation>(invoiceLineCharge.Validation);
		}

		public void TestChargeFlags_AdditionCharge()
		{
			var charge = SetupChargeWithType(ChargeCodeList.Codes.AdditionCharge);
			AssertChargeFlags(charge, false, false, false, true, false);
		}

		public void TestChargeFlags_DeductionCharge()
		{
			var charge = SetupChargeWithType(ChargeCodeList.Codes.DeductionCharge);
			AssertChargeFlags(charge, false, false, false, false, false);
		}

		public void TestChargeFlags_EUBorderFreight()
		{
			var charge = SetupChargeWithType(ChargeCodeList.Codes.EUBorderFreight);
			AssertChargeFlags(charge, true, false, false, true, false);
		}

		public void TestChargeFlags_EUBorderInsurance()
		{
			var charge = SetupChargeWithType(ChargeCodeList.Codes.EUBorderInsurance);
			AssertChargeFlags(charge, true, false, false, true, false);
		}

		public void TestChargeFlags_OverseasFreight()
		{
			var charge = SetupChargeWithType(ChargeCodeList.Codes.OverseasFreight);
			AssertChargeFlags(charge, true, false, false, false, false);
		}

		public void TestChargeFlags_OverseasInsurance()
		{
			var charge = SetupChargeWithType(ChargeCodeList.Codes.OverseasInsurance);
			AssertChargeFlags(charge, true, false, false, false, false);
		}

		public void TestJ7_RX_NKCurrency_DiscountCharge()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, invoiceLineCharge.J7_RX_NKCurrency);
		}

		public void TestJ7_RX_NKCurrency_ReadOnly_DiscountCharge()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			AssertEquals(true, invoiceLineCharge.J7_RX_NKCurrencyInfo.ReadOnly);
		}

		public void TestJ7_RX_NKCurrency_ReadOnly_IsImportSpecialRate()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes.TCE;
				AssertEquals("TCE", true, invoiceLineCharge.J7_RX_NKCurrencyInfo.ReadOnly);
				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes.INP;
				AssertEquals("INP", false, invoiceLineCharge.J7_RX_NKCurrencyInfo.ReadOnly);
				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes.OPF;
				AssertEquals("OPF", true, invoiceLineCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			});
		}

		public void TestJ7_IsDutiable_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes._001, invoiceLineCharge.J7_IsDutiableInfo);
		}

		public void TestJ7_IsStatisticalValueApplicable_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes._002, invoiceLineCharge.J7_IsStatisticalValueApplicableInfo);
		}

		public void TestJ7_IsGSTApplicable_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes._003, invoiceLineCharge.J7_IsGSTApplicableInfo);
		}

		public void TestJ7_Calc_IsIncludedInInvoiceAmount_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(EU.Business.ChargeTypeList.Codes.StatisticalValue, invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo);
		}

		public void TestJ7_IsIncludedInITOT_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes.TCE, invoiceLineCharge.J7_IsIncludedInITOTInfo);
		}

		public void TestJ7_Percentage_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(CustomsChargeTypeList.Codes.Discount, invoiceLineCharge.J7_PercentageInfo);
		}

		public void TestIsJ7_ExchangeRateIATA_ReadOnly()
		{
			AssertReadOnlyForImportAndHighValueOvrd(ImportChargeCodeList.Codes.INP, invoiceLineCharge.IsJ7_ExchangeRateIATAInfo);
		}

		public void TestIsJ7_ExchangeRateIATA()
		{
			CombineAssertions(() =>
			{
				invoiceLineCharge.IsJ7_ExchangeRateIATA = true;
				AssertEquals("Set", invoiceLineCharge.J7_ExchangeRateType, ChargeExchangeRateTypeList.Codes.IATARate);
				invoiceLineCharge.IsJ7_ExchangeRateIATA = false;
				AssertEquals("Empty", invoiceLineCharge.J7_ExchangeRateType, ZString.Empty);
			});
		}

		public void TestIATAAndFixedRateAreMutuallyExclusive()
		{
			CombineAssertions(() =>
			{
				invoiceLineCharge.IsJ7_ExchangeRateUserEnterable = true;
				invoiceLineCharge.J7_ExchangeRate = 1.25m;
				AssertEquals("Fixed Rate = true -> IATA = false", false, invoiceLineCharge.IsJ7_ExchangeRateIATA);

				invoiceLineCharge.IsJ7_ExchangeRateIATA = true;
				AssertEquals("IATA = true -> Fixed Rate = false", false, invoiceLineCharge.IsJ7_ExchangeRateUserEnterable);
				AssertEquals("IATA = true -> Exchange Rate = 0", 0m, invoiceLineCharge.J7_ExchangeRate);
			});
		}

		public void TestJ7_ExchangeRate_IATARateUsed()
		{
			CombineAssertions(() =>
			{
				ChargeValidationHelperTest.TestJ7_ExchangeRate_IATARateUsed(invoiceLineCharge, invoiceLineCharge.IsJ7_ExchangeRateIATAInfo);
			});
		}

		public void TestChangingChargeTypeResetsIATA()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default value", false, invoiceLineCharge.IsJ7_ExchangeRateIATA);
				invoiceLineCharge.IsJ7_ExchangeRateIATA = true;
				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
				AssertEquals("Updated charge code supports IATA", true, invoiceLineCharge.IsJ7_ExchangeRateIATA);
				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes._011;
				AssertEquals("Updated another charge code supports IATA", true, invoiceLineCharge.IsJ7_ExchangeRateIATA);
				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes._012;
				AssertEquals("Updated charge code doesn't support IATA", false, invoiceLineCharge.IsJ7_ExchangeRateIATA);
			});
		}

		public void TestClearAmountIfChargeTypeIsSetToBlank()
		{
			invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			invoiceLineCharge.J7_Amount = 11.0m;
			CombineAssertions(() =>
			{
				AssertNotEquals("Pre-condition", ZDecimal.Zero, invoiceLineCharge.J7_Amount);
				invoiceLineCharge.J7_ChargeType = ZString.Empty;
				AssertEquals("Cleared", ZDecimal.Zero, invoiceLineCharge.J7_Amount);
			});
		}

		public void TestClearCurrencyIfChargeTypeIsSetToBlank()
		{
			invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			invoiceLineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			CombineAssertions(() =>
			{
				AssertNotEquals("Pre-condition", ZString.Empty, invoiceLineCharge.J7_RX_NKCurrency);
				invoiceLineCharge.J7_ChargeType = ZString.Empty;
				AssertEquals("Cleared", ZString.Empty, invoiceLineCharge.J7_RX_NKCurrency);
			});
		}

		public void TestJ7_ChargeDescription_ReadOnly()
		{
			CombineAssertions(() =>
			{
				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes._012;
				AssertEquals("ReadOnly", true, invoiceLineCharge.J7_ChargeDescriptionInfo.ReadOnly);
				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes._016;
				AssertEquals("Editable", false, invoiceLineCharge.J7_ChargeDescriptionInfo.ReadOnly);
			});
		}

		public void TestDefaultChargeDescription()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes._012;
				AssertEquals("Description from code", ImportChargeCodeList.Descriptions._012, invoiceLineCharge.J7_ChargeDescription);
				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes._016;
				AssertEquals("Empty", ZString.Empty, invoiceLineCharge.J7_ChargeDescription);
			});
		}

		public void TestIsImportSpecialRate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, invoiceLineCharge.IsImportSpecialRate);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes.SRC;
				AssertEquals("Is true when J7_ChargeType is Special Rate", true, invoiceLineCharge.IsImportSpecialRate);

				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes._001;
				AssertEquals("Is false when J7_ChargeType is other value", false, invoiceLineCharge.IsImportSpecialRate);
			});
		}

		public void TestDefaultCurrency()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes.SRC;
			AssertEquals("IsImportSpecialRate", "EUR", invoiceLineCharge.J7_RX_NKCurrency);
		}

		public void TestJ7_ChargeType_SetValuesIfNeeded()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				invoiceLineCharge.J7_ChargeType = ImportChargeCodeList.Codes.AIR;
				AssertEquals("J7_IsDutiable", true, invoiceLineCharge.J7_IsDutiable);
				AssertEquals("J7_IsStatisticalValueApplicable", true, invoiceLineCharge.J7_IsStatisticalValueApplicable);
				AssertEquals("J7_IsGSTApplicable", true, invoiceLineCharge.J7_IsGSTApplicable);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", false, invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_IsIncludedInITOT", false, invoiceLineCharge.J7_IsIncludedInITOT);
				AssertEquals("IsJ7_ExchangeRateIATA", true, invoiceLineCharge.IsJ7_ExchangeRateIATA);
			});
		}

		public void TestJ7_Amount_DiscountCharge_UpdateJI_NetPrice()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_LinePrice = 200m;

			CombineAssertions(() =>
			{
				invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 100m);
				AssertEquals("JI_NetPrice is updated", 100m, invoiceLine.JI_NetPrice);

				invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 50m);
				AssertEquals("Multiple Discount charges, JI_NetPrice isn't updated", 100m, invoiceLine.JI_NetPrice);
			});
		}

		public void TestDelete_DiscountCharge_UpdateJI_NetPrice()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_LinePrice = 200m;
			var charge1 = invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 60m);
			var charge2 = invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 80m);

			CombineAssertions(() =>
			{
				invoiceLine.JI_NetPrice = 150m;

				charge1.Delete();
				AssertEquals("JI_NetPrice is updated after charge1 is deleted", 120m, invoiceLine.JI_NetPrice);
				AssertEquals("charge1.IsDeleted", true, charge1.IsDeleted);
				AssertEquals("charge2.J7_Amount", 80m, charge2.J7_Amount);

				charge2.Delete();
				AssertEquals("JI_NetPrice is updated after charge2 is deleted", 200m, invoiceLine.JI_NetPrice);
				AssertEquals("charge2.IsDeleted", true, charge2.IsDeleted);
			});
		}

		public void TestOnSaving_SetValuesForApportionChargesIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			var invoiceLineCharge = invoiceLine.Charges.AddNew(ImportChargeCodeList.Codes._010, 1000m, declaration.LocalCurrencyCode);
			invoiceLineCharge.IsJ7_ExchangeRateIATA = true;
			declaration.ResumeApportionment();

			invoiceLineCharge.OnSaving();
			AssertEquals(true, invoice.GroupCharges.Cast<InvoiceApportionCharge>().Single(x => x.J7_ChargeType == ImportChargeCodeList.Codes._010).IsJ7_ExchangeRateIATA);
		}

		public void TestDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var invoiceLineCharge = invoiceLine.Charges.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("IsDutiable", false, invoiceLineCharge.J7_IsDutiable);
				AssertEquals("IsGSTApplicable", false, invoiceLineCharge.J7_IsGSTApplicable);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLineCharge = invoiceLine.Charges.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		InvoiceLineCharge invoiceLineCharge;

		protected override BusinessObject GetNewBusinessObject()
		{
			SetUp();
			return invoiceLineCharge;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.New<InvoiceLineCharge>();
		}

		void AssertReadOnlyForImportAndHighValueOvrd(ZString chargeType, ZPropertyInfo propertyInfo)
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				invoiceLineCharge.J7_ChargeType = chargeType;
				AssertEquals("ZPropertyInfo ReadOnly is false", false, propertyInfo.ReadOnly);

				declaration.ZG_IsHighValueOvrd = ZBool.True;
				AssertEquals("ZPropertyInfo ReadOnly is true", true, propertyInfo.ReadOnly);
			});
		}

		InvoiceLineCharge SetupChargeWithType(string chargeType)
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = chargeType;
			return charge;
		}

		void AssertChargeFlags(InvoiceLineCharge charge, bool isIncludedInInvoice, bool isIncludedInInvoiceLine, bool isDutiable, bool isStatisticalValueApplicable, bool isVATApplicable)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Included In Invoice", isIncludedInInvoice, !charge.J7_IsNotIncludedInInvoice);
				AssertEquals("Included In Invoice Line", isIncludedInInvoiceLine, charge.J7_IsIncludedInITOT);
				AssertEquals("Dutiable", isDutiable, charge.J7_IsDutiable);
				AssertEquals("Statistical Value Applicable", isStatisticalValueApplicable, charge.J7_IsStatisticalValueApplicable);
				AssertEquals("VAT Applicable", isVATApplicable, charge.J7_IsGSTApplicable);
			});
		}
	}
}
