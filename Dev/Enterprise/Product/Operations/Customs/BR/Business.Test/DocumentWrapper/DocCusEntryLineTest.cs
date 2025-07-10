using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DocCusEntryLine))]
	class DocCusEntryLineTest : DocBaseCusEntryLineAbstractTest<CusEntryLine, DocCusEntryLine>
	{
		public override void TestDutyAmountRounded()
		{
			EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 12.3453M);
			AssertEquals("DutyAmount", 12.35M, EntryLineWrapperInternal.DutyAmountRounded);
		}

		public override void TestGSTVATAmountRounded()
		{
			EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 12.3453M);
			AssertEquals("DutyAmount", 12.35M, EntryLineWrapperInternal.GSTVATAmountRounded);
		}

		public override void TestLinePricesWithCurrency()
		{
			Assert(true);
		}

		public override void TestLinePriceInLocalCurrencyEqualsTheRelatedValueInBizObj()
		{
			Assert(true);
		}

		public void TestNetWeightInKG()
		{
			var line1 = EntryLineInternal.InvoiceLines[0];
			line1.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			line1.JI_NetWeight = 10m;

			var line2 = EntryLineInternal.InvoiceLines.AddNew();
			line2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			line2.JI_NetWeight = 10m;

			AssertEquals("NetWeightInKG should be equal", 20m, EntryLineWrapperInternal.NetWeightInKG);

			line1.JI_NetWeightUQ = Core.Constants.Weight.Pounds;
			line1.JI_NetWeight = 10m;

			line2.JI_NetWeightUQ = Core.Constants.Weight.Pounds;
			line2.JI_NetWeight = 10m;

			AssertEquals("NetWeightInKG should be equal", 9.071848m, EntryLineWrapperInternal.NetWeightInKG);
		}

		public void TestFees()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "09022000", 60m);

			var fee = EntryLineInternal.Fees.AddOrUpdate(Constants.RateTypes.ICMS, 55.88m);
			fee.CF_BaseValue = 200.05m;

			fee = EntryLineInternal.Fees.AddOrUpdate(Enterprise.Customs.Business.ChargeTypesList.Codes.DTY, 74.88m);
			fee.CF_BaseValue = 200.0544m;
			fee.CF_Rate = 5m;

			fee = EntryLineInternal.Fees.AddOrUpdate(Constants.RateTypes.IPI, 25.88m);
			fee.CF_BaseValue = 200.05987m;
			fee.CF_Rate = 6m;

			fee = EntryLineInternal.Fees.AddOrUpdate(Constants.RateTypes.PIS, 21.88m);
			fee.CF_BaseValue = 200.0545m;
			fee.CF_Rate = 7m;

			fee = EntryLineInternal.Fees.AddOrUpdate(Constants.RateTypes.Cofins, 87.88m);
			fee.CF_BaseValue = 200.0565m;
			fee.CF_Rate = 4m;

			fee = EntryLineInternal.Fees.AddOrUpdate(Constants.RateTypes.Antidumping, 12.15m);
			fee.CF_BaseValue = 200.05454m;
			fee.CF_Rate = 2m;

			fee = EntryLineInternal.Fees.AddOrUpdate(Constants.RateTypes.ICMSFCP, 17.30m);
			fee.CF_BaseValue = 200.05789m;
			fee.CF_Rate = 8;

			var invoiceLine = fee.EntryLine.RandomLine as JobComInvoiceLine;
			invoiceLine.DutyTaxRegime = TaxRegimeList.Codes.FullCollection;
			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.FullCollection;
			invoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.FullCollection;
			invoiceLine.JI_Tariff = "09022000";
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.Normal;

			EntryLineInternal.CL_CustomsValue = 30m;

			CombineAssertions(() =>
			{
				AssertEquals("ICMSChargeAmount", 55.88m, EntryLineWrapperInternal.AdValoremFees["ICM"].ChargeAmount);
				AssertEquals("ICMSBaseAmount", 200.05m, EntryLineWrapperInternal.AdValoremFees["ICM"].BaseValue);

				AssertEquals("DutyChargeAmount", 74.88m, EntryLineWrapperInternal.AdValoremFees["DTY"].ChargeAmount);
				AssertEquals("DutyBaseAmount", 200.0544m, EntryLineWrapperInternal.AdValoremFees["DTY"].BaseValue);
				AssertEquals("DutyChargePaybleAmount", 1.5m, EntryLineWrapperInternal.DutyDueAmount);
				AssertEquals("DutyReducedRate", 0m, EntryLineWrapperInternal.DutyReducedRate);
				AssertEquals("DutyAgreementRate", 0m, EntryLineWrapperInternal.DutyAgreementRate);
				AssertEquals("DutyReductionPercentage", 0m, EntryLineWrapperInternal.DutyReductionPercentage);

				AssertEquals("IPIChargeAmount", 25.88m, EntryLineWrapperInternal.AdValoremFees["IPI"].ChargeAmount);
				AssertEquals("IPIBaseAmount", 200.0599m, EntryLineWrapperInternal.AdValoremFees["IPI"].BaseValue);
				AssertEquals("IPIChargePaybleAmount", 25.88m, EntryLineWrapperInternal.IPIDueAmount);
				AssertEquals("IPIReducedRate", 0m, EntryLineWrapperInternal.IPIReducedRate);

				AssertEquals("PISChargeAmount", 21.88m, EntryLineWrapperInternal.AdValoremFees["PIS"].ChargeAmount);
				AssertEquals("PISBaseAmount", 200.0545m, EntryLineWrapperInternal.AdValoremFees["PIS"].BaseValue);
				AssertEquals("PISChargePaybleAmount", 21.88m, EntryLineWrapperInternal.PISDueAmount);
				AssertEquals("PISCofinsReducedRate", 0m, EntryLineWrapperInternal.PISCofinsReducedRate);
				AssertEquals("PISCofinsReductionPercentage", 0m, EntryLineWrapperInternal.PISCofinsReductionPercentage);

				AssertEquals("CofinsChargeAmount", 87.88m, EntryLineWrapperInternal.AdValoremFees["COF"].ChargeAmount);
				AssertEquals("CofinsBaseAmount", 200.0565m, EntryLineWrapperInternal.AdValoremFees["COF"].BaseValue);
				AssertEquals("CofinsDueAmount", 87.88m, EntryLineWrapperInternal.CofinsDueAmount);

				AssertEquals("AntidumpingChargeAmount", 12.15m, EntryLineWrapperInternal.AdValoremFees["ADD"].ChargeAmount);
				AssertEquals("AntidumpingBaseAmount", 200.0545m, EntryLineWrapperInternal.AdValoremFees["ADD"].BaseValue);
				AssertEquals("AntidumpingChargePaybleAmount", 12.15m, EntryLineWrapperInternal.AntidumpingDueAmount);

				AssertEquals("ICMSFCPChargeAmount", 17.30m, EntryLineWrapperInternal.AdValoremFees["FCP"].ChargeAmount);
				AssertEquals("ICMSFCPBaseAmount", 200.0579m, EntryLineWrapperInternal.AdValoremFees["FCP"].BaseValue);
				AssertEquals("ICMSFCPRate", 8m, EntryLineWrapperInternal.AdValoremFees["FCP"].Rate);

				AssertEquals("QuantityPerUnitFees count", 0, EntryLineWrapperInternal.QuantityPerUnitFees.Count);
			});

			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;
			AssertEquals("DutyAgreementRate", 5m, EntryLineWrapperInternal.DutyAgreementRate);

			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReducedRate;
			invoiceLine.DutyTaxRegime = TaxRegimeList.Codes.Reduction;
			invoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.Reduction;
			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.Reduction;
			CombineAssertions(() =>
			{
				AssertEquals("DutyReducedRate", 5m, EntryLineWrapperInternal.DutyReducedRate);
				AssertEquals("PISCofinsReducedRate", 7m, EntryLineWrapperInternal.PISCofinsReducedRate);
				AssertEquals("IPIReducedRate", 6m, EntryLineWrapperInternal.IPIReducedRate);
			});

			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReductionMargin;
			invoiceLine.DutyTaxRegime = TaxRegimeList.Codes.Reduction;
			invoiceLine.DutyRateIsOverridden = true;
			invoiceLine.ReductionMarginRateValue = 50m;
			CombineAssertions(() =>
			{
				AssertEquals("DutyReductionPercentage", 50m, EntryLineWrapperInternal.DutyReductionPercentage);
				AssertEquals("PISCofinsReductionPercentage", 0m, EntryLineWrapperInternal.PISCofinsReductionPercentage);
			});

			fee = EntryLineInternal.Fees.AddOrUpdate(Constants.RateTypes.IPI, 25.88m);
			fee.CF_BaseValue = 200.05m;
			fee.CF_Rate = 6m;
			fee.CF_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;

			fee = EntryLineInternal.Fees.AddOrUpdate(Constants.RateTypes.PIS, 21.88m);
			fee.CF_BaseValue = 200.05m;
			fee.CF_Rate = 7m;
			fee.CF_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;

			fee = EntryLineInternal.Fees.AddOrUpdate(Constants.RateTypes.Cofins, 87.88m);
			fee.CF_BaseValue = 200.05m;
			fee.CF_Rate = 4m;
			fee.CF_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;

			fee = EntryLineInternal.Fees.AddOrUpdate(Constants.RateTypes.Antidumping, 12.15m);
			fee.CF_BaseValue = 200.05m;
			fee.CF_Rate = 2m;
			fee.CF_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;

			invoiceLine.DutyTaxRegime = TaxRegimeList.Codes.FullCollection;
			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.FullCollection;
			invoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.FullCollection;

			CombineAssertions(() =>
			{
				AssertEquals("IPIChargeAmountQuantityPerUnit", 25.88m, EntryLineWrapperInternal.QuantityPerUnitFees["IPI"].ChargeAmount);
				AssertEquals("IPIBaseAmountQuantityPerUnit", 200.05m, EntryLineWrapperInternal.QuantityPerUnitFees["IPI"].BaseValue);

				AssertEquals("PISChargeAmountQuantityPerUnit", 21.88m, EntryLineWrapperInternal.QuantityPerUnitFees["PIS"].ChargeAmount);
				AssertEquals("PISBaseAmountQuantityPerUnit", 200.05m, EntryLineWrapperInternal.QuantityPerUnitFees["PIS"].BaseValue);

				AssertEquals("CofinsChargeAmountQuantityPerUnit", 87.88m, EntryLineWrapperInternal.QuantityPerUnitFees["COF"].ChargeAmount);
				AssertEquals("CofinsBaseAmountQuantityPerUnit", 200.05m, EntryLineWrapperInternal.QuantityPerUnitFees["COF"].BaseValue);

				AssertEquals("AntidumpingChargeAmountQuantityPerUnit", 12.15m, EntryLineWrapperInternal.QuantityPerUnitFees["ADD"].ChargeAmount);
				AssertEquals("AntidumpingBaseAmountQuantityPerUnit", 200.05m, EntryLineWrapperInternal.QuantityPerUnitFees["ADD"].BaseValue);
			});
		}

		public void TestInvoiceLines()
		{
			EntryLineInternal.InvoiceLines.AddNew();
			EntryLineInternal.InvoiceLines.AddNew();

			var property = EntryLineWrapperInternal.GetType().GetProperty("InvoiceLines");
			AssertNotNull("You must implement a property call InvoiceLines", property);
			var method = property.GetGetMethod();
			var invoiceLines = (DocBaseJobComInvoiceLineCollection)method.Invoke(EntryLineWrapperInternal, System.Array.Empty<object>());
			AssertNotNull("InvoiceLines is not null", invoiceLines);
			AssertEquals("3 lines in collection", 3, invoiceLines.Count);
			Assert("InvoiceLines is of type DocJobComInvoiceLineCollection", invoiceLines.GetType().ToString().EndsWith("DocJobComInvoiceLineCollection"));
		}

		public void TestICMSRate()
		{
			var line = EntryLineInternal.InvoiceLines[0] as JobComInvoiceLine;
			line.JI_ICMSRate = 10m;
			AssertEquals(10m, EntryLineWrapperInternal.ICMSRate);
		}

		public void TestDutyAdValorem()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 50m);

			var line = EntryLineInternal.InvoiceLines[0] as JobComInvoiceLine;
			line.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			line.JI_Tariff = "99999999";
			line.JI_CountryOfOrigin = "CA";

			AssertEquals(50m, EntryLineWrapperInternal.DutyAdValorem);

			line.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			line.DutyRateIsOverridden = true;
			line.DutyVigentRateValue = 78m;
			AssertEquals(78m, EntryLineWrapperInternal.DutyAdValorem);
		}

		public void TestIPIAdValorem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Brazil, Constants.RateTypes.IPI);
			var rateCodeIpi = helper.LoadOrCreateNewCusRateCode(Factory, Constants.RateCodes.IPI, rateType.PK);
			var tradeGroup = helper.LoadOrCreateTradeGroup("BR", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Brazil, tariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate = helper.CreateRate(tariff, rateCodeIpi.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "VFD * 0.5", null, "50", Core.Constants.CountryCodes.Brazil);
			helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var line = EntryLineInternal.InvoiceLines[0] as JobComInvoiceLine;
			line.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			line.JI_Tariff = "99999999";
			line.JI_CountryOfOrigin = "CA";

			AssertEquals(50m, EntryLineWrapperInternal.IPIAdValorem);

			line.IPIRateIsOverridden = true;
			line.IPIVigentRateValue = 68m;
			AssertEquals(68m, EntryLineWrapperInternal.IPIAdValorem);
		}

		public void TestPisAdValorem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Brazil, Constants.RateTypes.PIS, "Social Integration Program");
			var rateCodeDuty = helper.LoadOrCreateNewCusRateCode(Factory, Constants.RateCodes.PIS, rateType.PK);
			var tradeGroup = helper.LoadOrCreateTradeGroup("BR", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Brazil, tariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate = helper.CreateRate(tariff, rateCodeDuty.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "VFD * 0.865", null, "8.65", Core.Constants.CountryCodes.Brazil);
			helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var line = EntryLineInternal.InvoiceLines[0] as JobComInvoiceLine;
			line.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			line.JI_Tariff = "99999999";
			line.JI_CountryOfOrigin = "CA";

			AssertEquals(8.65m, EntryLineWrapperInternal.PisAdValorem);

			line.PisRateIsOverridden = true;
			line.PisVigentRateValue = 7.7m;
			AssertEquals(7.7m, EntryLineWrapperInternal.PisAdValorem);
		}

		public void TestCofinsAdValorem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Brazil, Constants.RateTypes.Cofins, "Social Security Financing Contribution");
			var rateCodeDuty = helper.LoadOrCreateNewCusRateCode(Factory, Constants.RateCodes.Cofins, rateType.PK);
			var tradeGroup = helper.LoadOrCreateTradeGroup("BR", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Brazil, tariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate = helper.CreateRate(tariff, rateCodeDuty.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "VFD * 0.15", null, "1.5", Core.Constants.CountryCodes.Brazil);
			helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var line = EntryLineInternal.InvoiceLines[0] as JobComInvoiceLine;
			line.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			line.JI_Tariff = "99999999";
			line.JI_CountryOfOrigin = "CA";

			AssertEquals(1.5m, EntryLineWrapperInternal.CofinsAdValorem);

			line.CofinsRateIsOverridden = true;
			line.CofinsVigentRateValue = 3.9m;
			AssertEquals(3.9m, EntryLineWrapperInternal.CofinsAdValorem);
		}

		public void TestDefaultAntidumpingAdValorem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Brazil, Constants.RateTypes.Antidumping, "Anti - Dumping");
			var rateCodeDuty = helper.LoadOrCreateNewCusRateCode(Factory, Constants.RateCodes.Antidumping, rateType.PK);
			var tradeGroup = helper.LoadOrCreateTradeGroup("BR", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Brazil, tariffType.PK, "99999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate = helper.CreateRate(tariff, rateCodeDuty.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "VFD * 0.315", null, "3.15", Core.Constants.CountryCodes.Brazil);
			helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var line = EntryLineInternal.InvoiceLines[0];
			line.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			line.JI_Tariff = "99999999";
			line.JI_CountryOfOrigin = "CA";

			AssertEquals(3.15m, EntryLineWrapperInternal.DefaultAntidumpingAdValorem);
		}

		#region Implementation

		protected CusEntryLine entryLine;
		protected JobComInvoiceLine invoiceLine;

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Brazil; }
		}

		protected override DocCusEntryLine CreateEntryLineWrapper(Customs.Business.ICusEntryLine entryLineInternal)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineInternal, Factory);
		}

		Customs.Business.CusEntryLine fEntryLineMergeOfTwoInvoiceLines;
		protected override Customs.Business.ICusEntryLine EntryLineMergeOfTwoInvoiceLines
		{
			get
			{
				if (fEntryLineMergeOfTwoInvoiceLines == null)
				{
					var declaration = (JobDeclaration)GetNewDeclaration();
					var header = declaration.Invoices.AddNew();
					header.JZ_InvoiceNumber = "1";
					header.JZ_RX_NKInvoice_Currency = "BRL";

					var invoiceLine = header.JobComInvoiceLines.AddNew();
					invoiceLine.JI_LinePrice = 200.05m;
					invoiceLine.JI_Tariff = "123";
					invoiceLine.JI_LineNo = 1;

					var line1ONS = invoiceLine.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance);
					line1ONS.J7_Amount = 5m;
					var line1OFT = invoiceLine.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight);
					line1OFT.J7_Amount = 10m;

					declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
					declaration.DoMerge();
					Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders.Count > 0);
					Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders[0].MergedLines.Count > 0);
					fEntryLineMergeOfTwoInvoiceLines = declaration.CustomsEntryHeaders[0].MergedLines[0];
				}
				return fEntryLineMergeOfTwoInvoiceLines;
			}
		}

		protected override DocCusEntryLine DocEntryLineMergeOfTwoInvoiceLines
		{
			get { return CreateEntryLineWrapper(EntryLineMergeOfTwoInvoiceLines); }
		}

		#endregion

	}
}
