using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationDutyFeeProviderTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var entryLine = CreateEntryLine();
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, ChargeTypesList.Codes.DTY).First();

			CombineAssertions(() =>
			{
				AssertEquals("DTY ChargeType should be", "0001", taxProvider.TaxType);
				AssertEquals("DTY RateType should be", "1", taxProvider.RateType);
				AssertEquals("DTY BaseValue should be", 100m, taxProvider.BaseValue);
				AssertNull("DTY IPTAmount should be", taxProvider.IPTAmount);
				AssertEquals("DTY AgreementPercentual should be", 0m, taxProvider.AgreementPercentual);
				AssertEquals("DTY AgreementPercentualNormal should be", 0m, taxProvider.AgreementPercentualNormal);
				AssertEquals("DTY TaxPayable should be", 0m, taxProvider.TaxPayable);
				AssertEquals("DTY ReducedRatePercentage should be", 0m, taxProvider.ReducedRatePercentage);
				AssertNull("DTY SpecificRateUnitQuantity should be Null", taxProvider.SpecificRateUnitQuantity);
				AssertNull("DTY SpecificUQ should be Null", taxProvider.SpecificUQ);
				AssertNull("DTY SpecificIPTRateValue should be Null", taxProvider.SpecificIPTRateValue);
				AssertEquals("DTY TariffACCalculatedValue should be", 0m, taxProvider.TariffACCalculatedValue);
				AssertNull("DTY SpecificIPTCalculatedValue should be Null", taxProvider.SpecificIPTCalculatedValue);
				AssertNull("SpecificReducedIPTRateValue", taxProvider.SpecificReducedIPTRateValue);
				AssertNull("SpecificIPTCalculatedValue", taxProvider.SpecificIPTCalculatedValue);
				AssertNull("IPIComplementaryNote", taxProvider.IPIComplementaryNote);
				AssertNull("QuantityMLContainer", taxProvider.QuantityMLContainer);
				AssertNull("IPITaxRegime", taxProvider.IPITaxRegime);
				AssertEquals("IPTCalculatedValue should be", 0m, taxProvider.IPTCalculatedValue);
				AssertNull("RecipientTypeCode", taxProvider.RecipientTypeCode);
			});
		}

		public void TestIPTAmount()
		{
			var entryLine = CreateEntryLine();
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, ChargeTypesList.Codes.DTY).First();

			CombineAssertions(() =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Reduction }.Contains(taxRegime) ? 10m : (decimal?)null;
					AssertEquals(taxRegime, expected, taxProvider.IPTAmount);
				}
			});
		}

		public void TestAgreementPercentual()
		{
			var entryLine = CreateEntryLine();
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, ChargeTypesList.Codes.DTY).First();

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.FreeTradeAgreement;

			CombineAssertions("FTA", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Reduction, TaxRegimeList.Codes.Suspension }.Contains(taxRegime) ? 5m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.AgreementPercentual);
				}
			});

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.Normal;
			CombineAssertions("Normal", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;
					AssertEquals(taxRegime, 0m, taxProvider.AgreementPercentual);
				}
			});
		}

		public void TestAgreementPercentualNormal()
		{
			var entryLine = CreateEntryLine();
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, ChargeTypesList.Codes.DTY).First();

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.FreeTradeAgreement;
			CombineAssertions("FTA", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Reduction, TaxRegimeList.Codes.Suspension, TaxRegimeList.Codes.Exemption }.Contains(taxRegime) ? 60m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.AgreementPercentualNormal);
				}
			});

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.ReducedRate;
			CombineAssertions("Reduced", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Reduction, TaxRegimeList.Codes.Suspension, TaxRegimeList.Codes.Exemption }.Contains(taxRegime) ? 60m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.AgreementPercentualNormal);
				}
			});

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.ReductionMargin;
			CombineAssertions("Reduction", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Reduction, TaxRegimeList.Codes.Suspension, TaxRegimeList.Codes.Exemption }.Contains(taxRegime) ? 60m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.AgreementPercentualNormal);
				}
			});

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.Normal;
			CombineAssertions("Normal", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Reduction, TaxRegimeList.Codes.Suspension, TaxRegimeList.Codes.Exemption }.Contains(taxRegime) ? 5m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.AgreementPercentualNormal);
				}
			});
		}

		public void TestTaxPayable()
		{
			var entryLine = CreateEntryLine();
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, ChargeTypesList.Codes.DTY).First();

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.FreeTradeAgreement;
			CombineAssertions("FTA", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.Suspension, TaxRegimeList.Codes.Exemption, TaxRegimeList.Codes.FullCollection }.Contains(taxRegime) ? 5m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.TaxPayable);
				}
			});

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.Normal;
			CombineAssertions("Normal", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Suspension, TaxRegimeList.Codes.Exemption }.Contains(taxRegime) ? 5m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.TaxPayable);
				}
			});

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.ExTariff;
			CombineAssertions("ExTariff", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Suspension, TaxRegimeList.Codes.Exemption }.Contains(taxRegime) ? 5m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.TaxPayable);
				}
			});

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.ReducedRate;
			CombineAssertions("Reduced", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.Reduction, TaxRegimeList.Codes.Suspension, TaxRegimeList.Codes.Exemption }.Contains(taxRegime) ? 5m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.TaxPayable);
				}
			});

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.ReductionMargin;
			CombineAssertions("Reduction", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.Reduction, TaxRegimeList.Codes.Suspension }.Contains(taxRegime) ? 60m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.TaxPayable);
				}
			});
		}

		public void TestTariffACCalculatedValue()
		{
			var entryLine = CreateEntryLine();
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, ChargeTypesList.Codes.DTY).First();

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.FreeTradeAgreement;
			CombineAssertions("FTA", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Reduction, TaxRegimeList.Codes.Suspension }.Contains(taxRegime) ? 5m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.TariffACCalculatedValue);
				}
			});

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.Normal;
			CombineAssertions("Normal", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;
					AssertEquals(taxRegime, 0m, taxProvider.TariffACCalculatedValue);
				}
			});

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.ReducedRate;
			CombineAssertions("Reduced", () =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.Reduction, TaxRegimeList.Codes.Suspension }.Contains(taxRegime) ? 5m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.TariffACCalculatedValue);
				}
			});
		}

		public void TestIPTCalculatedValue()
		{
			var entryLine = CreateEntryLine();
			var invoiceLine = entryLine.RandomLine;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, ChargeTypesList.Codes.DTY).First();

			CombineAssertions(() =>
			{
				foreach (var taxRegime in new TaxRegimeList().GetAllCodes())
				{
					invoiceLine.DutyTaxRegime = taxRegime;

					var expected = new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Reduction, TaxRegimeList.Codes.Suspension, TaxRegimeList.Codes.Exemption }.Contains(taxRegime) ? 60m : 0m;
					AssertEquals(taxRegime, expected, taxProvider.IPTCalculatedValue);
				}
			});
		}

		public void TestReducedRatePercentage()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "09022000", 60m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 600m;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "09022000";
			invoiceLine1.JI_CountryOfOrigin = "CA";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.JI_NetWeight = 100m;
			invoiceLine1.DutyTaxRegime = TaxRegimeList.Codes.Reduction;
			invoiceLine1.ICMSTaxRegime = TaxRegimeList.Codes.Reduction;
			invoiceLine1.JI_PrimaryPreference = Constants.RatePreferenceType.ReducedRate;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();

			var entryLine = cusEntryHeader.MergedLines[0];
			entryLine.CL_CustomsValue = 100m;

			var fee = entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypesList.Codes.DTY);
			fee.CF_BaseValue = 100m;
			fee.CF_ChargeAmount = 10m;
			fee.CF_Rate = 5m;

			var taxes = DeclarationAdditionTaxProvider.New(entryLine, ChargeTypesList.Codes.DTY).ToArray();
			var dutyTax = taxes.First(e => e.TaxType == "0001");

			AssertEquals("DTY ReducedRatePercentage should be", 5m, dutyTax.ReducedRatePercentage);
		}

		public void TestIPTReductionPercentage()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "09022000", 60m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 600m;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "09022000";
			invoiceLine1.JI_CountryOfOrigin = "CA";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.JI_NetWeight = 100m;
			invoiceLine1.DutyTaxRegime = TaxRegimeList.Codes.Reduction;
			invoiceLine1.ICMSTaxRegime = TaxRegimeList.Codes.Reduction;
			invoiceLine1.JI_PrimaryPreference = Constants.RatePreferenceType.ReductionMargin;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();

			var entryLine = cusEntryHeader.MergedLines[0];
			entryLine.CL_CustomsValue = 100m;

			var fee = entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypesList.Codes.DTY);
			fee.CF_BaseValue = 100m;
			fee.CF_ChargeAmount = 10m;
			fee.CF_Rate = 5m;

			invoiceLine1.DutyRateIsOverridden = true;
			invoiceLine1.ReductionMarginRateValue = 50m;

			var taxes = DeclarationAdditionTaxProvider.New(entryLine, ChargeTypesList.Codes.DTY).ToArray();
			var dutyTax = taxes.First(e => e.TaxType == "0001");

			AssertEquals("DTY IPTReductionPercentage should be", 50m, dutyTax.IPTReductionPercentage);
		}

		public void TestSpecialRate()
		{
			var entryLine = CreateEntryLine();
			entryLine.Fees[0].CF_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			var taxProvider = DeclarationAdditionTaxProvider.New(entryLine, ChargeTypesList.Codes.DTY).First();

			AssertEquals("RateType", "2", taxProvider.RateType);
		}

		CusEntryLine CreateEntryLine()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "09022000", 60m);

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

			var fee = entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypesList.Codes.DTY);
			fee.CF_BaseValue = 100m;
			fee.CF_ChargeAmount = 10m;
			fee.CF_Rate = 5m;

			return entryLine;
		}
	}
}
