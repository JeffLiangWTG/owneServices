using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationPISCofinsTaxProviderTest : TestCaseWithFactory
	{
		public void TestPropertiesPis()
		{
			var entryLine = CreateEntryLine(Constants.RateTypes.PIS);
			entryLine.Fees.AddOrUpdate(Constants.RateTypes.Cofins, 10m).CF_BaseValue = 100m;

			AssertDeclarationAdditionTaxProvider(entryLine, Constants.RateTypes.PIS, "0005", Constants.RateCodes.PIS);
		}

		public void TestPropertiesCofins()
		{
			var entryLine = CreateEntryLine(Constants.RateTypes.Cofins);
			AssertDeclarationAdditionTaxProvider(entryLine, Constants.RateTypes.Cofins, "0006", Constants.RateCodes.Cofins);
		}

		public void TestIPTAmount()
		{
			var entryLine = CreateEntryLine(Constants.RateTypes.PIS);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.PIS).First();

			CombineAssertions(() =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.PisCofinsTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Reduction }.Contains(taxRegime) ? 10m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.IPTAmount);
				}
			});
		}

		public void TestAgreementPercentualNormal()
		{
			var entryLine = CreateEntryLine(Constants.RateTypes.PIS);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.PIS).First();

			CombineAssertions(() =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.PisCofinsTaxRegime = taxRegime;
					invoiceLine.PisVigentRateValue = 0.2;
					invoiceLine.PisRateIsOverridden = true;

					var expected = new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Suspension, TaxRegimeList.Codes.Exemption }.Contains(taxRegime) ? 5m :
						new[] { TaxRegimeList.Codes.Reduction }.Contains(taxRegime) ? 0.2m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.AgreementPercentualNormal);
				}
			});
		}

		public void TestTaxPayable()
		{
			var entryLine = CreateEntryLine(Constants.RateTypes.PIS);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.PIS).First();

			CombineAssertions(() =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.PisCofinsTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Reduction }.Contains(taxRegime) ? 10m
												: new[] { TaxRegimeList.Codes.Suspension, TaxRegimeList.Codes.Exemption }.Contains(taxRegime) ? 5m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.TaxPayable);
				}
			});
		}

		public void TestIPTCalculatedValue()
		{
			var entryLine = CreateEntryLine(Constants.RateTypes.PIS);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.PIS).First();

			CombineAssertions(() =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.PisCofinsTaxRegime = taxRegime;
					invoiceLine.PisVigentRateValue = 0.2;
					invoiceLine.PisRateIsOverridden = true;

					var expected = new[] { TaxRegimeList.Codes.FullCollection }.Contains(taxRegime) ? 10m
										: new[] { TaxRegimeList.Codes.Reduction }.Contains(taxRegime) ? 20m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.IPTCalculatedValue);
				}
			});
		}

		public void TestSpecialRatePis()
		{
			var entryLine = CreateEntryLine(Constants.RateTypes.PIS, Constants.RateCodes.PIS);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.PIS).First();

			CombineAssertions(() =>
			{
				AssertEquals("SpecificRateUnitQuantity should be", 100, taxProvider.SpecificRateUnitQuantity);
				AssertEquals("SpecificIPTRateValue should be", 10m, taxProvider.SpecificIPTRateValue);
				AssertEquals("SpecificUQ should be", "PKG", taxProvider.SpecificUQ);
				AssertEquals("RateType", "2", taxProvider.RateType);
			});
		}

		public void TestSpecialRateCofins()
		{
			var entryLine = CreateEntryLine(Constants.RateTypes.Cofins, Constants.RateCodes.Cofins);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.Cofins).First();

			CombineAssertions(() =>
			{
				AssertEquals("SpecificRateUnitQuantity should be", 100, taxProvider.SpecificRateUnitQuantity);
				AssertEquals("SpecificIPTRateValue should be", 10m, taxProvider.SpecificIPTRateValue);
				AssertEquals("SpecificUQ should be", "PKG", taxProvider.SpecificUQ);
				AssertEquals("RateType", "2", taxProvider.RateType);
			});
		}

		public void TestReducedRatePercentage()
		{
			var entryLine = CreateEntryLine(Constants.RateTypes.PIS);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.PIS).First();

			CombineAssertions(() =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.PisCofinsTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.Reduction }.Contains(taxRegime) ? 5m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.ReducedRatePercentage);
				}
			});
		}

		CusEntryLine CreateEntryLine(string rateType, string rateCode = null)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 600m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "09022000";
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.JI_LinePrice = 600m;
			invoiceLine.JI_NetWeight = 100m;

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = cusEntryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 100m;
			invoiceLine.JI_CL = entryLine.PK;

			var fee = entryLine.Fees.GetOrAddFeeByFeeType(rateType);
			fee.CF_BaseValue = 100m;
			fee.CF_ChargeAmount = 10m;
			fee.CF_Rate = 5m;

			if (rateCode != null)
			{
				var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
				specialCaseTax.TaxGroup = rateCode;
				specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
				specialCaseTax.RateOrUnitValue = 10m;
				specialCaseTax.CurrencyCode = "BRL";
				specialCaseTax.UnitOfMeasure = "PKG";
				specialCaseTax.Quantity = 1m;

				fee.CF_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			}

			return entryLine;
		}

		void AssertDeclarationAdditionTaxProvider(CusEntryLine entryLine, string chargeType, string taxType, string specialRate)
		{
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, chargeType).First();

			CombineAssertions(chargeType, () =>
			{
				AssertEquals("ChargeType should be", taxType, taxProvider.TaxType);
				AssertEquals("RateType should be", "1", taxProvider.RateType);
				AssertEquals("BaseValue should be", 100m, taxProvider.BaseValue);
				AssertNull("TaxPayable should be", taxProvider.TaxPayable);
				AssertNull("IPTAmount should be", taxProvider.IPTAmount);
				AssertNull("AgreementPercentual should be Null", taxProvider.AgreementPercentual);
				AssertNull("AgreementPercentualNormal should be", taxProvider.AgreementPercentualNormal);
				AssertEquals("ReducedRatePercentage should be", 0m, taxProvider.ReducedRatePercentage);
				AssertNull("SpecificRateUnitQuantity should be", taxProvider.SpecificRateUnitQuantity);
				AssertNull("SpecificIPTRateValue should be", taxProvider.SpecificIPTRateValue);
				AssertEquals("TariffACCalculatedValue should be", 0m, taxProvider.TariffACCalculatedValue);
				AssertEquals("SpecificIPTCalculatedValue should be", 0m, taxProvider.SpecificIPTCalculatedValue);
				AssertEquals("SpecificReducedIPTRateValue", 0m, taxProvider.SpecificReducedIPTRateValue);
				AssertNull("IPIComplementaryNote", taxProvider.IPIComplementaryNote);
				AssertNull("QuantityMLContainer", taxProvider.QuantityMLContainer);
				AssertNull("IPITaxRegime", taxProvider.IPITaxRegime);
				AssertNull("IPTCalculatedValue", taxProvider.IPTCalculatedValue);
				AssertNull("RecipientTypeCode", taxProvider.RecipientTypeCode);
				AssertNull("SpecificUQ", taxProvider.SpecificUQ);
			});
		}
	}
}
