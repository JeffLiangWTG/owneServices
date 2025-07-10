using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocChargeSummaryLineTest : TestCaseWithFactory
	{
		public void TestChargeDescription()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			DocTaxRate docTaxRate = DocTaxRate.New(taxRate, Factory);

			ChargeSummaryLine chargeSummaryLine = new ChargeSummaryLine(docTaxRate, 0, 0, "this is a test", 100, 100, 110, 110, 10, 10);
			DocChargeSummaryLine docSummaryLine = DocChargeSummaryLine.New(chargeSummaryLine, Factory);
			AssertEquals("this is a test", docSummaryLine.ChargeDescription);
		}

		public void TestChargeMainTaxDescription()
		{
			var collection = AccountingConfigurationRegistry.Instance.ZeroAmountTaxTypesDescription.Value;
			var notApplicableDescription = collection.Cast<ZeroAmountTaxTypesDescriptions>().FirstOrDefault(x => x.TaxType == AccTaxRate.Types.NotReportable);
			notApplicableDescription.OverrideValue = (NoResString)"NOT Override";
			var exemptDescription = collection.Cast<ZeroAmountTaxTypesDescriptions>().FirstOrDefault(x => x.TaxType == AccTaxRate.Types.Exempt);
			exemptDescription.OverrideValue = (NoResString)"Exempt Override";

			AccountingConfigurationRegistry.Instance.ZeroAmountTaxTypesDescription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AssertEquals("Exempt Override", AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.Exempt));
			AssertEquals("NOT Override", AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.NotReportable));
			AssertEquals("Reverse Charge", AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.ReverseRated));

			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.SetRate_ForTestOnly(105, 10);
			DocTaxRate docTaxRate = DocTaxRate.New(taxRate, Factory);

			ChargeSummaryLine chargeSummaryLine = new ChargeSummaryLine(docTaxRate, taxRate.GetRate(ZDate.Today), taxRate.GetEffectiveExtraRate(ZDate.Today), "this is a test", 100, 100, 110, 110, 10, 10);
			DocChargeSummaryLine docSummaryLine = DocChargeSummaryLine.New(chargeSummaryLine, Factory);
			AssertEquals("10.5%", docSummaryLine.ChargeMainTaxDescription);

			taxRate.AT_Type = AccTaxRate.Types.CapitalRated;
			AssertEquals("10.5%", docSummaryLine.ChargeMainTaxDescription);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				taxRate.AT_Type = AccTaxRate.Types.ServiceTax;
				AssertEquals("10.5%", docSummaryLine.ChargeMainTaxDescription);
			}

			taxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			AssertEquals("Reverse Charge", docSummaryLine.ChargeMainTaxDescription);

			taxRate.AT_Type = AccTaxRate.Types.Exempt;
			AssertEquals("Exempt Override", docSummaryLine.ChargeMainTaxDescription);

			taxRate.AT_Type = AccTaxRate.Types.Suspended;
			AssertEquals("Suspended", docSummaryLine.ChargeMainTaxDescription);

			taxRate.AT_Type = AccTaxRate.Types.NotReportable;
			AssertEquals("NOT Override", docSummaryLine.ChargeMainTaxDescription);
		}

		public void TestChargeExtraTaxDescription()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = "RAT";
			taxRate.SetRate_ForTestOnly(105, 10);
			taxRate.AT_ExtraTaxRateType = "QST";
			DocTaxRate docTaxRate = DocTaxRate.New(taxRate, Factory);

			ChargeSummaryLine chargeSummaryLine = new ChargeSummaryLine(docTaxRate, 0, 0, "this is a test", 100, 100, 110, 110, 10, 10);
			DocChargeSummaryLine docSummaryLine = DocChargeSummaryLine.New(chargeSummaryLine, Factory);
			AssertEquals("QST", docSummaryLine.ChargeExtraTaxDescription);

			taxRate.AT_ExtraTaxRateType = "QCT";
			AssertEquals("QST", docSummaryLine.ChargeExtraTaxDescription);

			taxRate.AT_ExtraTaxRateType = "RET";
			AssertEquals("Retention", docSummaryLine.ChargeExtraTaxDescription);

			taxRate.AT_ExtraTaxRateType = "EDU";
			AssertEquals("CESS", docSummaryLine.ChargeExtraTaxDescription);

			taxRate.AT_ExtraTaxRateType = "INP";
			AssertEquals("Input", docSummaryLine.ChargeExtraTaxDescription);
		}

		public void TestChargeExtraTaxRate()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = "RAT";
			taxRate.SetRate_ForTestOnly(105, 10);
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			taxRate.SetExtraRate_ForTestOnly(35, 10);
			DocTaxRate docTaxRate = DocTaxRate.New(taxRate, Factory);

			ChargeSummaryLine chargeSummaryLine = new ChargeSummaryLine(docTaxRate, taxRate.GetRate(ZDate.Today), taxRate.GetEffectiveExtraRate(ZDate.Today), "this is a test", 100, 100, 110, 110, 10, 10);
			DocChargeSummaryLine docSummaryLine = DocChargeSummaryLine.New(chargeSummaryLine, Factory);
			AssertEquals("3.5%", docSummaryLine.ChargeExtraTaxRate);
		}

		public void TestVairousAmount()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = "RAT";
			taxRate.SetRateNumerator_ForTestOnly(10);
			DocTaxRate docTaxRate = DocTaxRate.New(taxRate, Factory);
			ChargeSummaryLine chargeSummaryLine = new ChargeSummaryLine(docTaxRate, 0, 0, "this is a test", 100, 120, 110, 122, 10, 12);
			DocChargeSummaryLine docSummaryLine = DocChargeSummaryLine.New(chargeSummaryLine, Factory);

			AssertEquals(120m, docSummaryLine.TotalChargeAmountExcludeTaxInLocalCurrency);
			AssertEquals(100m, docSummaryLine.TotalChargeAmountExcludeTaxInOSCurrency);
			AssertEquals(122m, docSummaryLine.TotalChargeAmountInLocalCurrency);
			AssertEquals(110m, docSummaryLine.TotalChargeAmountInOSCurrency);
			AssertEquals(12m, docSummaryLine.TotalChargeTaxAmountInLocalCurrency);
			AssertEquals(10m, docSummaryLine.TotalChargeTaxAmountInOSCurrency);
		}
	}
}
