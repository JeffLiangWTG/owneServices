using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.CDS.Declaration;
using URCRateCodes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class CDSFeeCodeFromRateCodeMapperTest : TestCaseWithFactory
	{
		public void TestGetMapping_EUTariff()
		{
			addInfo.CSI_Code = "ABC";

			AssertMapping(entryLine, URCRateCodes.CustomsDutyOnIndustrialProducts, URCRateCodes.CustomsDutyOnIndustrialProducts, false);
		}

		public void TestGetMapping_CustomsDutyOnIndustrialProducts()
		{
			AssertMapping(entryLine, URCRateCodes.CustomsDutyOnIndustrialProducts, GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty);
		}

		public void TestGetMapping_AdditionalDutyCountervailingSafeguardChargeVariableCharge()
		{
			AssertMapping(entryLine, URCRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge, GBCommonConstants.NorthernIrelandDutyCodes.AdditionalDuty);
		}

		public void TestGetMapping_DefinitiveAntiDumpingDuty()
		{
			AssertMapping(entryLine, URCRateCodes.DefinitiveAntiDumpingDuty, GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveAntiDumpingDuty);
		}

		public void TestGetMapping_ProvisionalAntiDumpingDuty()
		{
			AssertMapping(entryLine, URCRateCodes.ProvisionalAntiDumpingDuty, GBCommonConstants.NorthernIrelandDutyCodes.ProvisionalAntiDumpingDuty);
		}

		public void TestGetMapping_DefinitiveCountervailingDuty()
		{
			AssertMapping(entryLine, URCRateCodes.DefinitiveCountervailingDuty, GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveCountervailingDuty);
		}

		public void TestGetMapping_ProvisionalCountervailingDuty()
		{
			AssertMapping(entryLine, URCRateCodes.ProvisionalCountervailingDuty, GBCommonConstants.NorthernIrelandDutyCodes.ProvisionalCountervailingDuty);
		}

		public void TestGetMapping_NoMapping()
		{
			AssertMapping(entryLine, "ZXY", "ZXY");
			AssertMapping(entryLine, URCRateCodes.ClimateChangeLevy, "990");
		}

		void AssertMapping(CusEntryLine entryLine, string rateCode, string expectedRateCode, bool expectedIsUsingEuTariffForNI = true)
		{
			var actualValue = CDSFeeCodeFromRateCodeMapper.GetMapping(entryLine, rateCode);

			CombineAssertions(() =>
			{
				AssertEquals("IsUsingEuTariffForNI", expectedIsUsingEuTariffForNI, entryLine.IsEuTariffToBeUsedForNorthernIreland);
				AssertEquals("RateCode", expectedRateCode, actualValue);
			});
		}

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();

			addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIIMP;

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
		}

		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
		AdditionalInfo addInfo;
	}
}
