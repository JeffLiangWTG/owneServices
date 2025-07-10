using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationIPITaxProviderTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var entryLine = CreateEntryLine(false);
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.IPI).First();

			CombineAssertions(() =>
			{
				AssertEquals("IPI ChargeType should be", "0002", taxProvider.TaxType);
				AssertEquals("IPI RateType should be", "1", taxProvider.RateType);
				AssertNull("IPI BaseValue should be", taxProvider.BaseValue);
				AssertNull("IPI IPTAmount should be", taxProvider.IPTAmount);
				AssertNull("IPI TaxPayable should be", taxProvider.TaxPayable);
				AssertNull("IPI AgreementPercentual should be Null", taxProvider.AgreementPercentual);
				AssertNull("IPI AgreementPercentualNormal should be", taxProvider.AgreementPercentualNormal);
				AssertEquals("SpecificReducedIPTRateValue", 0m, taxProvider.SpecificReducedIPTRateValue);
				AssertNull("IPTCalculatedValue", taxProvider.IPTCalculatedValue);
				AssertEquals("IPI ReducedRatePercentage should be", 0m, taxProvider.ReducedRatePercentage);
				AssertNull("IPI SpecificRateUnitQuantity should be", taxProvider.SpecificRateUnitQuantity);
				AssertNull("IPI SpecificIPTRateValue should be", taxProvider.SpecificIPTRateValue);
				AssertEquals("IPI TariffACCalculatedValue should be", 0m, taxProvider.TariffACCalculatedValue);
				AssertEquals("IPI SpecificIPTCalculatedValue should be", 0m, taxProvider.SpecificIPTCalculatedValue);
				AssertEquals("IPI QuantityMLContainer should be", 0, taxProvider.QuantityMLContainer);
				AssertEquals("IPI IPIComplementaryNote should be", "CN", taxProvider.IPIComplementaryNote);

				AssertEquals("IPI RecipientTypeCode should be", "01", taxProvider.RecipientTypeCode);
				AssertNull("SpecificUQ", taxProvider.SpecificUQ);
			});
		}

		public void TestIPTAmount()
		{
			var entryLine = CreateEntryLine(false);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.IPI).First();

			CombineAssertions(() =>
			{
				foreach (var taxRegime in new IPITaxRegimeList().GetAllCodes())
				{
					invoiceLine.IPITaxRegime = taxRegime;

					var expected = new[] { IPITaxRegimeList.Codes.FullCollection, IPITaxRegimeList.Codes.Reduction }.Contains(taxRegime) ? 10m : 0m;
					AssertEquals(taxRegime, taxRegime, taxProvider.IPITaxRegime);
					AssertEquals(taxRegime, expected, taxProvider.IPTAmount);
				}
			});
		}

		public void TestTaxPayable()
		{
			var entryLine = CreateEntryLine(false);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.IPI).First();

			CombineAssertions(() =>
			{
				foreach (var taxRegime in new IPITaxRegimeList().GetAllCodes())
				{
					invoiceLine.IPITaxRegime = taxRegime;

					var expected = new[] { IPITaxRegimeList.Codes.FullCollection, IPITaxRegimeList.Codes.Reduction }.Contains(taxRegime) ? 10m
												: new[] { IPITaxRegimeList.Codes.Suspension, IPITaxRegimeList.Codes.Exemption }.Contains(taxRegime) ? 5m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.TaxPayable);
				}
			});
		}

		public void TestIPTCalculatedValue()
		{
			var entryLine = CreateEntryLine(false);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.IPI).First();

			CombineAssertions(() =>
			{
				foreach (var taxRegime in new IPITaxRegimeList().GetAllCodes())
				{
					invoiceLine.IPITaxRegime = taxRegime;
					invoiceLine.IPIRateIsOverridden = true;
					invoiceLine.IPIVigentRateValue = 0.2;

					var expected = new[] { IPITaxRegimeList.Codes.FullCollection }.Contains(taxRegime) ? 10m
										: new[] { IPITaxRegimeList.Codes.Reduction }.Contains(taxRegime) ? 20m : 0m;
					AssertEquals(taxRegime, taxRegime, taxProvider.IPITaxRegime);
					AssertEquals(taxRegime, expected, taxProvider.IPTCalculatedValue);
				}
			});
		}

		public void TestAgreementPercentualNormal()
		{
			var entryLine = CreateEntryLine(false);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.IPI).First();

			CombineAssertions(() =>
			{
				foreach (var taxRegime in new IPITaxRegimeList().GetAllCodes())
				{
					invoiceLine.IPITaxRegime = taxRegime;
					invoiceLine.IPIRateIsOverridden = true;
					invoiceLine.IPIVigentRateValue = 0.2;

					var expected = new[] { IPITaxRegimeList.Codes.FullCollection, IPITaxRegimeList.Codes.Suspension, IPITaxRegimeList.Codes.Exemption }.Contains(taxRegime) ? 5m
											: new[] { IPITaxRegimeList.Codes.Reduction }.Contains(taxRegime) ? 0.2m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.AgreementPercentualNormal);
				}
			});
		}

		public void TestBaseValue()
		{
			var entryLine = CreateEntryLine(false);
			var invoiceLine = entryLine.RandomLine;
			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.FullCollection;
			invoiceLine.JI_PrimaryPreference = RatePreferenceType.FreeTradeAgreement;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.IPI).First();

			CombineAssertions(() =>
			{
				foreach (var taxRegime in new IPITaxRegimeList().GetAllCodes())
				{
					invoiceLine.IPITaxRegime = taxRegime;

					var expected = new[] { IPITaxRegimeList.Codes.FullCollection, IPITaxRegimeList.Codes.Reduction, IPITaxRegimeList.Codes.Suspension, IPITaxRegimeList.Codes.Exemption }.Contains(taxRegime) ? 100m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.BaseValue);
				}
			});
		}

		public void TestSpecialRate()
		{
			var entryLine = CreateEntryLine(true);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.IPI).First();

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
			var entryLine = CreateEntryLine(false);
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.IPI).First();

			CombineAssertions(() =>
			{
				foreach (var taxRegime in new IPITaxRegimeList().GetAllCodes())
				{
					invoiceLine.IPITaxRegime = taxRegime;

					var expected = new[] { IPITaxRegimeList.Codes.Reduction }.Contains(taxRegime) ? 5m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.ReducedRatePercentage);
				}
			});
		}

		CusEntryLine CreateEntryLine(bool specialRate)
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
			invoiceLine.JI_ComplementaryNote = "CN";
			invoiceLine.IPITaxRegime = ZString.Empty;
			invoiceLine.JI_CustomsSecondUnitQty = "01";

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = cusEntryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 100m;
			invoiceLine.JI_CL = entryLine.PK;

			var fee = entryLine.Fees.GetOrAddFeeByFeeType(Constants.RateTypes.IPI);
			fee.CF_BaseValue = 100m;
			fee.CF_ChargeAmount = 10m;
			fee.CF_Rate = 5m;

			if (specialRate)
			{
				var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
				specialCaseTax.TaxGroup = Constants.RateCodes.IPI;
				specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
				specialCaseTax.RateOrUnitValue = 10m;
				specialCaseTax.CurrencyCode = "BRL";
				specialCaseTax.UnitOfMeasure = "PKG";
				specialCaseTax.Quantity = 1m;

				fee.CF_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			}

			return entryLine;
		}
	}
}
