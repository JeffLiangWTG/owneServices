using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.GB.Business.Test.Declaration
{
	[TestedType(typeof(CusEntryLineFee))]
	public class CusEntryLineFeeTest : EU.Business.Declaration.Testing.CusEntryLineFeeTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
	{
		public void TestIsInNorthernIrelandDuty()
		{
			var entryLineFee = SetEntryLineFeeData().entryLineFee;
			Assert(!entryLineFee.IsInNorthernIrelandDuty);

			entryLineFee.CF_ChargeType = GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty;
			Assert(entryLineFee.IsInNorthernIrelandDuty);

			entryLineFee.CF_ChargeType = GBCommonConstants.NorthernIrelandDutyCodes.AdditionalDuty;
			Assert(entryLineFee.IsInNorthernIrelandDuty);

			entryLineFee.CF_ChargeType = GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveAntiDumpingDuty;
			Assert(entryLineFee.IsInNorthernIrelandDuty);

			entryLineFee.CF_ChargeType = GBCommonConstants.NorthernIrelandDutyCodes.ProvisionalAntiDumpingDuty;
			Assert(entryLineFee.IsInNorthernIrelandDuty);

			entryLineFee.CF_ChargeType = GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveCountervailingDuty;
			Assert(entryLineFee.IsInNorthernIrelandDuty);

			entryLineFee.CF_ChargeType = GBCommonConstants.NorthernIrelandDutyCodes.ProvisionalCountervailingDuty;
			Assert(entryLineFee.IsInNorthernIrelandDuty);

			entryLineFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.Vat;
			Assert(!entryLineFee.IsInNorthernIrelandDuty);
		}

		protected override (JobDeclaration declaration, CusEntryLine entryLine, CusEntryLineFee entryLineFee) SetEntryLineFeeData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First().MergedLines.Cast<CusEntryLine>()
				.First();
			var entryLineFee = entryLine.Fees.AddNew();
			return (declaration, entryLine, entryLineFee);
		}

		public void TestCF_ChargeAmountValueIsTruncatedWhenSaved_CDS()
		{
			var entryLineFeeData = SetEntryLineFeeData();
			var entryLineFee = entryLineFeeData.entryLineFee;
			var dec = entryLineFeeData.declaration;
			dec.JE_ApplicationCode = "CDS";
			entryLineFee.CF_ChargeAmount = 5.6789;
			AssertEquals(5.67m, entryLineFee.CF_ChargeAmount);
		}

		public void TestCF_ChargeAmountValueIsNotTruncatedWhenSaved_CHF()
		{
			var entryLineFeeData = SetEntryLineFeeData();
			var entryLineFee = entryLineFeeData.entryLineFee;
			var dec = entryLineFeeData.declaration;
			dec.JE_ApplicationCode = "CHF";
			entryLineFee.CF_ChargeAmount = 5.6789;
			AssertEquals(5.6789m, entryLineFee.CF_ChargeAmount);
		}

		public void TestChargeAmountCDS()
		{
			var entryLineFeeData = SetEntryLineFeeData();
			var entryLineFee = entryLineFeeData.entryLineFee;
			var dec = entryLineFeeData.declaration;
			dec.JE_ApplicationCode = "CDS";
			entryLineFee.CF_ChargeAmount = 20;
			entryLineFee.G4_Amount = "2.00";
			AssertEquals(nameof(CusEntryLineFee.G4_Amount), "2.00", entryLineFee.G4_Amount);
			AssertEquals(nameof(CusEntryLineFee.CF_ChargeAmount), (ZDecimal)2.00, entryLineFee.CF_ChargeAmount);

			entryLineFee.CF_ChargeAmount = 20.1234;
			AssertEquals(nameof(CusEntryLineFee.CF_ChargeAmount), (ZDecimal)20.12, entryLineFee.CF_ChargeAmount);

			entryLineFee.CF_ChargeAmount = 20.49998;
			AssertEquals($"Default rounding for {nameof(CusEntryLineFee.CF_ChargeAmount)}", (ZDecimal)20.49, entryLineFee.CF_ChargeAmount);
		}

		public void TestChargeAmountCHIEF()
		{
			var entryLineFeeData = SetEntryLineFeeData();
			var entryLineFee = entryLineFeeData.entryLineFee;
			var dec = entryLineFeeData.declaration;
			dec.JE_ApplicationCode = "CHF";
			entryLineFee.CF_ChargeAmount = 20;
			entryLineFee.G4_Amount = "2.00";
			AssertEquals(nameof(CusEntryLineFee.G4_Amount), "2.00", entryLineFee.G4_Amount);
			AssertEquals(nameof(CusEntryLineFee.CF_ChargeAmount), (ZDecimal)2.00, entryLineFee.CF_ChargeAmount);

			entryLineFee.CF_ChargeAmount = 20.1234;
			AssertEquals(nameof(CusEntryLineFee.CF_ChargeAmount), (ZDecimal)20.1234, entryLineFee.CF_ChargeAmount);

			entryLineFee.CF_ChargeAmount = 20.49998;
			AssertEquals($"Default rounding for {nameof(CusEntryLineFee.CF_ChargeAmount)}", (ZDecimal)20.5000, entryLineFee.CF_ChargeAmount);
		}

		public void TestShouldResetDataOnMergingCore()
		{
			var dec = Factory.New<JobDeclaration>();
			var header = dec.CustomsEntryHeaders.AddNew();
			var line = header.MergedLines.AddNew();
			var fee = line.Fees.AddNew();

			fee.CF_ChargeType = "DTY";
			fee.CF_ChargeAmount = 69m;
			fee.CF_MethodOfPayment = "ABC";
			dec.JE_ApplicationCode = "CDS";
			fee.CF_Source = "CW1";
			fee.CF_RateOverrideReasonCode = ZString.Empty;
			using (GBCustomsDataRegistry.Instance.ShouldResetCDSCalculatedEntryLineFeesAtMerger.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				line.ResetTotalsAndCachedValues();
				Assert(fee.ShouldResetDataOnMerging);
				AssertEquals(0m, fee.CF_ChargeAmount);
				AssertEquals(ZString.Empty, fee.CF_MethodOfPayment);
			}

			fee.CF_ChargeAmount = 69m;
			fee.CF_MethodOfPayment = "ABC";
			fee.CF_Source = "CW1";
			dec.JE_ApplicationCode = "CHF";
			fee.CF_RateOverrideReasonCode = ZString.Empty;
			using (GBCustomsDataRegistry.Instance.ShouldResetCDSCalculatedEntryLineFeesAtMerger.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				line.ResetTotalsAndCachedValues();
				AssertEquals(false, fee.ShouldResetDataOnMerging);
				AssertEquals(69m, fee.CF_ChargeAmount);
				AssertEquals("ABC", fee.CF_MethodOfPayment);
			}

			fee.CF_ChargeAmount = 69m;
			fee.CF_MethodOfPayment = "ABC";
			fee.CF_Source = "ABC";
			dec.JE_ApplicationCode = "CDS";
			fee.CF_RateOverrideReasonCode = ZString.Empty;
			using (GBCustomsDataRegistry.Instance.ShouldResetCDSCalculatedEntryLineFeesAtMerger.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				line.ResetTotalsAndCachedValues();
				AssertEquals(false, fee.ShouldResetDataOnMerging);
				AssertEquals(69m, fee.CF_ChargeAmount);
				AssertEquals("ABC", fee.CF_MethodOfPayment);
			}

			fee.CF_ChargeAmount = 69m;
			fee.CF_MethodOfPayment = "ABC";
			fee.CF_Source = "CW1";
			dec.JE_ApplicationCode = "CDS";
			fee.CF_RateOverrideReasonCode = "ADD";
			using (GBCustomsDataRegistry.Instance.ShouldResetCDSCalculatedEntryLineFeesAtMerger.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				line.ResetTotalsAndCachedValues();
				AssertEquals(false, fee.ShouldResetDataOnMerging);
				AssertEquals(69m, fee.CF_ChargeAmount);
				AssertEquals("ABC", fee.CF_MethodOfPayment);
			}

			fee.CF_ChargeAmount = 69m;
			fee.CF_MethodOfPayment = "ABC";
			dec.JE_ApplicationCode = "CDS";
			fee.CF_Source = "CW1";
			fee.CF_RateOverrideReasonCode = ZString.Empty;
			using (GBCustomsDataRegistry.Instance.ShouldResetCDSCalculatedEntryLineFeesAtMerger.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				line.ResetTotalsAndCachedValues();
				AssertEquals(false, fee.ShouldResetDataOnMerging);
				AssertEquals(69m, fee.CF_ChargeAmount);
				AssertEquals("ABC", fee.CF_MethodOfPayment);
			}
		}

		public void TestVATBaseValueOnlyIncludeExciseFeeOnce()
		{
			var (declaration, entryLine, dutyFee) = SetEntryLineFeeData();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.UnitedKingdom);
			helper.CreateNewOrGetExistingDataGrouping(declaration.GetDefaultDataGroupingCode(), parent: parentDataGrouping);
			var dutyRateType = helper.CreateCusRateType(CountryCodes.UnitedKingdom, rateType: RefCusRateTypes.Dty);
			_ = helper.CreateCusRateCode(Factory, zy1RateCode: UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dutyRateType.PK);
			var exciseRateType = helper.CreateCusRateType(CountryCodes.UnitedKingdom, rateType: RefCusRateTypes.Excise);
			_ = helper.CreateCusRateCode(Factory, zy1RateCode: "321", exciseRateType.PK);
			var ordVat = helper.CreateTaxOrFee("ORD", 0.2m, CountryCodes.UnitedKingdom);

			entryLine.RandomLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
			dutyFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			dutyFee.CF_ChargeAmount = 20m;
			var exciseFee = entryLine.Fees.AddNew();
			exciseFee.CF_ChargeType = "321";
			exciseFee.CF_ChargeAmount = 30m;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			CombineAssertions("Calculated VAT", () =>
			{
				var vatFee = entryLine.Fees[2];
				AssertEquals(UniversalReferenceConstants.RefCusRateCodes.Vat, vatFee.CF_ChargeType);
				AssertEquals(50m, vatFee.CF_BaseValue);
				AssertEquals(10m, vatFee.CF_ChargeAmount);
			});
		}
	}
}
