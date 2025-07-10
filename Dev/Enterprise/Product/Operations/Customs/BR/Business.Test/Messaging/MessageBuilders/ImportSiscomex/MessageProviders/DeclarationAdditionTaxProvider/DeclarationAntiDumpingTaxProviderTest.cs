using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationAntiDumpingTaxProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationAddtionTaxProvider()
		{
			var entryLine = CreateEntryLine(false);

			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.Antidumping).Single();

			CombineAssertions(() =>
			{
				AssertEquals("TaxType", "0003", taxProvider.TaxType);
				AssertEquals("RateType", "1", taxProvider.RateType);
				AssertEquals("RateType", 100m, taxProvider.BaseValue);
				AssertEquals("DirectTypeCode", "1", taxProvider.DirectTypeCode);
				AssertEquals("AgreementPercentualNormal", 6m, taxProvider.AgreementPercentualNormal);
				AssertEquals("SpecificRateUnitQuantity", 0, taxProvider.SpecificRateUnitQuantity);
				AssertEquals("SpecificIPTRateValue", 0m, taxProvider.SpecificIPTRateValue);
				AssertEquals("TariffACCalculatedValue", 0m, taxProvider.TariffACCalculatedValue);
				AssertEquals("IPTAmount", 18m, taxProvider.IPTAmount);
				AssertEquals("TaxPayable", 18m, taxProvider.TaxPayable);

				AssertNull("AgreementPercentual", taxProvider.AgreementPercentual);
				AssertNull("ReducedRatePercentage", taxProvider.ReducedRatePercentage);
				AssertNull("SpecificReducedIPTRateValue", taxProvider.SpecificReducedIPTRateValue);
				AssertNull("SpecificIPTCalculatedValue", taxProvider.SpecificIPTCalculatedValue);
				AssertNull("IPIComplementaryNote", taxProvider.IPIComplementaryNote);
				AssertNull("QuantityMLContainer", taxProvider.QuantityMLContainer);
				AssertNull("IPITaxRegime", taxProvider.IPITaxRegime);
				AssertNull("IPTCalculatedValue", taxProvider.IPTCalculatedValue);
				AssertNull("RecipientTypeCode", taxProvider.RecipientTypeCode);
				AssertNull("SpecificUQ", taxProvider.SpecificUQ);
			});
		}

		public void TestSpecialRate()
		{
			var refPackType = Factory.New<RefPackType>();
			refPackType.F3_Code = "XX";
			refPackType.F3_Description = "Description";

			var entryLine = CreateEntryLine(true);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.Antidumping).First();
			CombineAssertions(() =>
			{
				AssertEquals("SpecificRateUnitQuantity should be", 100, taxProvider.SpecificRateUnitQuantity);
				AssertEquals("SpecificIPTRateValue should be", 6m, taxProvider.SpecificIPTRateValue);
				AssertEquals("SpecificUQ should be", "DESCRIPTION", taxProvider.SpecificUQ);
				AssertEquals("RateType", "2", taxProvider.RateType);
			});

			entryLine.Fees[0].CF_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			CombineAssertions(() =>
			{
				AssertEquals("SpecificRateUnitQuantity should be", 0, taxProvider.SpecificRateUnitQuantity);
				AssertEquals("SpecificIPTRateValue should be", 0m, taxProvider.SpecificIPTRateValue);
				AssertNull("SpecificUQ should be", taxProvider.SpecificUQ);
				AssertEquals("RateType", "1", taxProvider.RateType);
			});
		}

		public void TestAgreementPercentualNormal()
		{
			var entryLine = CreateEntryLine(true);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.Antidumping).First();

			AssertEquals(0m, taxProvider.AgreementPercentualNormal);

			entryLine.Fees[0].CF_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			AssertEquals(6m, taxProvider.AgreementPercentualNormal);
		}

		CusEntryLine CreateEntryLine(bool specialRate)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 600m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "09022000";
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.JI_LinePrice = 600m;
			invoiceLine.JI_NetWeight = 100m;
			invoiceLine.JI_ComplementaryNote = "CN";
			invoiceLine.IPITaxRegime = ZString.Empty;
			invoiceLine.JI_CustomsSecondUnitQty = "01";

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = cusEntryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 100m;
			invoiceLine.JI_CL = entryLine.PK;

			var fee = entryLine.Fees.GetOrAddFeeByFeeType(Constants.RateTypes.Antidumping);

			fee.CF_BaseValue = 100m;
			fee.CF_ChargeAmount = 18m;
			fee.CF_Rate = 6m;

			if (specialRate)
			{
				var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
				specialCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
				specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
				specialCaseTax.RateOrUnitValue = 10m;
				specialCaseTax.CurrencyCode = "BRL";
				specialCaseTax.UnitOfMeasure = "XX";
				specialCaseTax.Quantity = 1m;

				fee.CF_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			}

			return entryLine;
		}
	}
}

