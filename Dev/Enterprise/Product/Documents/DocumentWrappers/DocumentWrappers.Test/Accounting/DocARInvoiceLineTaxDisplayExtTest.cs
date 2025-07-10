using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.DocumentWrappers.Accounting.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest("AU")]
	sealed class DocARInvoiceLineTaxDisplayExtTest : TestCaseWithFactory
	{
		public void TestFormatOSTaxWithNoCurrencySymbolFormat()
		{
			var lineMock = new Mock<IDocARInvoiceLine>();
			IDocARInvoiceLine line = lineMock.Object;
			DocCurrency currency = DocCurrency.New(Factory, GlbCompany.CurrentCompany.LocalCurrency);
			lineMock.Setup(m => m.Currency).Returns(currency);
			lineMock.Setup(m => m.OSTaxAmount).Returns(1234.56m);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("1,234.56", line.FormatOSTaxWithNoCurrencySymbolFormat());
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("1.234,56", line.FormatOSTaxWithNoCurrencySymbolFormat());
			}
		}

		public void TestGetSPVTaxLabel()
		{
			AssertEquals("SPV", DocARInvoiceLineTaxDisplayExt.GetSPVTaxLabel(Core.Constants.CountryCodes.Italy));
			AssertEquals("Exon.", DocARInvoiceLineTaxDisplayExt.GetSPVTaxLabel(Core.Constants.CountryCodes.CostaRica));
			AssertEquals("N/A", DocARInvoiceLineTaxDisplayExt.GetSPVTaxLabel(Core.Constants.CountryCodes.Australia));
		}

		public void TestTaxAmountDisplayForReverseRateHasTaxCodeTranslated()
		{
			var lineMock = new Mock<IDocARInvoiceLine>();
			IDocARInvoiceLine line = lineMock.Object;
			lineMock.Setup(m => m.IsSpacerLine).Returns(false);
			lineMock.Setup(m => m.IsSubTotalLine).Returns(false);
			lineMock.Setup(m => m.DisplayTaxGroupCode).Returns(false);

			Assert("Pre-condition: CurrentCompany.GC_IsGSTRegistered", GlbCompany.CurrentCompany.GC_IsGSTRegistered);
			DocTaxRate ratedDocRate = GetDocTaxRate("FREEGST", AccTaxRate.Types.Rated);
			lineMock.Setup(m => m.TaxRate).Returns(ratedDocRate);
			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			AssertEquals("Reverse GST", line.GetTaxAmountDisplay());

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "SE";

			AssertEquals("Reverse VAT", line.GetTaxAmountDisplay());
			AssertEquals("Reverse Charge", line.GetOSTaxAmountDisplay());
			AssertEquals("Reverse Charge", line.GetOSTaxAmountDisplayWithRegistryRule());

			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.French))
			{
				AssertEquals("Exonération TVA", line.GetTaxAmountDisplay());
				AssertEquals("Autoliquidation", line.GetOSTaxAmountDisplay());
				AssertEquals("Autoliquidation", line.GetOSTaxAmountDisplayWithRegistryRule());
			}
		}

		public void TestGetTaxAmountDisplay()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			var lineMock = new Mock<IDocARInvoiceLine> { CallBase = true };
			lineMock.Setup(m => m.TaxRate).Returns(() => null);
			Assert("Should be empty", lineMock.Object.GetTaxAmountDisplay().IsEmpty);

			DocTaxRate ratedDocRate = GetDocTaxRate("FREEGST", AccTaxRate.Types.Rated);
			lineMock.Setup(m => m.TaxRate).Returns(ratedDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(0);
			AssertEquals("Zero Rated", lineMock.Object.GetTaxAmountDisplay());

			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(10);
			DocCurrency currency = DocCurrency.New(Factory, GlbCompany.CurrentCompany.LocalCurrency);
			lineMock.Setup(m => m.Currency).Returns(currency);
			lineMock.Setup(m => m.OSTaxAmount).Returns(1234.56m);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("1,234.56", lineMock.Object.GetTaxAmountDisplay());
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("1.234,56", lineMock.Object.GetTaxAmountDisplay());
			}

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.CapitalRated;
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(0);
			AssertEquals("Zero Rated", lineMock.Object.GetTaxAmountDisplay());
			AssertEquals("Zero Rated", lineMock.Object.GetTaxAmountDisplayWithRegistryRule());
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.CapitalRated, "Zero Rated CAP");
			AssertEquals("Zero Rated", lineMock.Object.GetTaxAmountDisplay());
			AssertEquals("Zero Rated CAP", lineMock.Object.GetTaxAmountDisplayWithRegistryRule());

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, string.Empty);

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			AssertEquals("Reverse GST", lineMock.Object.GetTaxAmountDisplay());
			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.Suspended;
			AssertEquals("Suspended", lineMock.Object.GetTaxAmountDisplay());

			DocTaxRate exemptDocRate = GetDocTaxRate("EXEMPT", AccTaxRate.Types.Exempt);
			lineMock.Setup(m => m.TaxRate).Returns(exemptDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(exemptDocRate.AccTaxRate.GetRateRaw_ForTestOnly());
			AssertEquals("Exempt Rated", lineMock.Object.GetTaxAmountDisplay());

			AccTaxRate rate;
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("IT"))
			{
				rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "ESCLUSE", AccTaxRate.Types.ExcludedFromTheTaxBase, 0);
			}
			var excludedDocRate = DocTaxRate.New(Factory.Load<AccTaxRate>(rate.PK), Factory);
			lineMock.Setup(m => m.TaxRate).Returns(excludedDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(excludedDocRate.AccTaxRate.GetRateRaw_ForTestOnly());
			AssertEquals("Excluded", lineMock.Object.GetTaxAmountDisplay());
			AssertEquals("Excluded", lineMock.Object.GetTaxAmountDisplayWithRegistryRule());

			DocTaxRate notReportableDocRate = GetDocTaxRate("NOTREPORT", AccTaxRate.Types.NotReportable);
			lineMock.Setup(m => m.TaxRate).Returns(notReportableDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(notReportableDocRate.AccTaxRate.GetRateRaw_ForTestOnly());
			AssertEquals("Not Applicable", lineMock.Object.GetTaxAmountDisplay());

			var taxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "NOTREPORT", AccTaxRate.Types.NotReportable, 0);
			AccChargeCode chargeCode = CreateAccChargeCode("BOLLO", taxRate.PK, "NON", "NGC");
			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
			DocChargeCode docChargeCode = DocChargeCode.New(chargeCode, Factory);
			lineMock.Setup(m => m.ChargeCode).Returns(docChargeCode);
			AssertEquals("Not Applicable", lineMock.Object.GetTaxAmountDisplay());

			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.NewGuid());
			AssertEquals("Not Applicable", lineMock.Object.GetTaxAmountDisplay());
		}

		public void TestGetOSTaxMainRateDisplay()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			var lineMock = new Mock<IDocARInvoiceLine>();
			IDocARInvoiceLine line = lineMock.Object;

			lineMock.Setup(m => m.IsSpacerLine).Returns(false);
			lineMock.Setup(m => m.IsSubTotalLine).Returns(false);
			lineMock.Setup(m => m.ChargeCode).Returns(() => null);

			lineMock.Setup(m => m.TaxRate).Returns(() => null);
			AssertEquals("N/A", line.GetOSTaxMainRateDisplay());

			DocTaxRate ratedDocRate = GetDocTaxRate("FREEGST", AccTaxRate.Types.Rated);
			lineMock.Setup(m => m.TaxRate).Returns(ratedDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(0);
			AssertEquals("0%", line.GetOSTaxMainRateDisplay());

			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(10.5);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("10.5%", line.GetOSTaxMainRateDisplay());
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("10,5%", line.GetOSTaxMainRateDisplay());
			}

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.CapitalRated;
			AssertEquals("10.5%", line.GetOSTaxMainRateDisplay());

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			AssertEquals("Reverse", line.GetOSTaxMainRateDisplay());

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.Exempt;
			AssertEquals("Exempt", line.GetOSTaxMainRateDisplay());

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.NotReportable;
			AssertEquals("N/A", line.GetOSTaxMainRateDisplay());

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.Suspended;
			AssertEquals("Suspended", line.GetOSTaxMainRateDisplay());

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.ExcludedFromTheTaxBase;
			AssertEquals("Excluded", line.GetOSTaxMainRateDisplay());
		}

		public void TestGetOSTaxExtraRateDisplay()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			var lineMock = new Mock<IDocARInvoiceLine>();
			IDocARInvoiceLine line = lineMock.Object;

			lineMock.Setup(m => m.IsSpacerLine).Returns(false);
			lineMock.Setup(m => m.IsSubTotalLine).Returns(false);
			lineMock.Setup(m => m.ChargeCode).Returns(() => null);

			lineMock.Setup(m => m.TaxRate).Returns(() => null);
			AssertEquals("N/A", line.GetOSTaxMainRateDisplay());

			DocTaxRate ratedDocRate = GetDocTaxRate("EXEMPT", AccTaxRate.Types.Exempt);
			lineMock.Setup(m => m.TaxRate).Returns(ratedDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(ratedDocRate.AccTaxRate.GetRateRaw_ForTestOnly());
			AssertEquals("N/A", line.GetOSTaxExtraRateDisplay());

			ratedDocRate = GetDocTaxRate("FREEGST", AccTaxRate.Types.Rated);
			lineMock.Setup(m => m.TaxRate).Returns(ratedDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(10);
			lineMock.Setup(m => m.TaxExtraRateAmount).Returns(1.23);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
				AssertEquals("1.23%", line.GetOSTaxExtraRateDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				AssertEquals("1.23%", line.GetOSTaxExtraRateDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				AssertEquals("1.23%", line.GetOSTaxExtraRateDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
				AssertEquals("1.23%", line.GetOSTaxExtraRateDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
				lineMock.Setup(m => m.TaxExtraRateAmount).Returns(12.3);
				AssertEquals("12.3%", line.GetOSTaxExtraRateDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATClaimed;
				lineMock.Setup(m => m.TaxExtraRateAmount).Returns(0);
				AssertEquals("N/A", line.GetOSTaxExtraRateDisplay());
			}

			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(10);
			lineMock.Setup(m => m.TaxExtraRateAmount).Returns(1.23);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
				AssertEquals("1,23%", line.GetOSTaxExtraRateDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				AssertEquals("1,23%", line.GetOSTaxExtraRateDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				AssertEquals("1,23%", line.GetOSTaxExtraRateDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
				AssertEquals("1,23%", line.GetOSTaxExtraRateDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
				lineMock.Setup(m => m.TaxExtraRateAmount).Returns(12.3);
				AssertEquals("12,3%", line.GetOSTaxExtraRateDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATClaimed;
				lineMock.Setup(m => m.TaxExtraRateAmount).Returns(0);
				AssertEquals("N/A", line.GetOSTaxExtraRateDisplay());
			}
		}

		public void TestGetOSTaxMainAmountDisplay()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			var lineMock = new Mock<IDocARInvoiceLine>();
			IDocARInvoiceLine line = lineMock.Object;

			lineMock.Setup(m => m.IsSpacerLine).Returns(false);
			lineMock.Setup(m => m.IsSubTotalLine).Returns(false);
			lineMock.Setup(m => m.ChargeCode).Returns(() => null);

			lineMock.Setup(m => m.TaxRate).Returns(() => null);
			AssertEquals("", line.GetOSTaxMainAmountDisplay());

			DocTaxRate ratedDocRate = GetDocTaxRate("FREEGST", AccTaxRate.Types.Rated);
			lineMock.Setup(m => m.TaxRate).Returns(ratedDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(10);

			DocCurrency aUD = DocCurrency.New("AUD", Factory);
			DocCurrency vND = DocCurrency.New("VND", Factory);

			lineMock.Setup(m => m.DisplayCurrency).Returns(aUD);
			lineMock.Setup(m => m.OSTaxAmount).Returns(1234.56m);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("1,234.56", line.GetOSTaxMainAmountDisplay());
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("1.234,56", line.GetOSTaxMainAmountDisplay());
			}

			lineMock.Setup(m => m.DisplayCurrency).Returns(vND);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("1,235", line.GetOSTaxMainAmountDisplay());
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("1.235", line.GetOSTaxMainAmountDisplay());
			}

			lineMock.Setup(m => m.DisplayCurrency).Returns(aUD);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.CapitalRated;
				AssertEquals("1,234.56", line.GetOSTaxMainAmountDisplay());

				ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.ReverseRated;
				AssertEquals("", line.GetOSTaxMainAmountDisplay());

				ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.Exempt;
				AssertEquals("", line.GetOSTaxMainAmountDisplay());

				ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.NotReportable;
				AssertEquals("", line.GetOSTaxMainAmountDisplay());

				ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.Suspended;
				AssertEquals("", line.GetOSTaxMainAmountDisplay());

				ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.ExcludedFromTheTaxBase;
				AssertEquals("", line.GetOSTaxMainAmountDisplay());
			}
		}

		public void TestGetOSTaxExtraAmountDisplay()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			var lineMock = new Mock<IDocARInvoiceLine>();
			IDocARInvoiceLine line = lineMock.Object;

			lineMock.Setup(m => m.IsSpacerLine).Returns(false);
			lineMock.Setup(m => m.IsSubTotalLine).Returns(false);
			lineMock.Setup(m => m.ChargeCode).Returns(() => null);

			lineMock.Setup(m => m.TaxRate).Returns(() => null);
			AssertEquals("", line.GetOSTaxExtraAmountDisplay());

			DocTaxRate ratedDocRate = GetDocTaxRate("FREEGST", AccTaxRate.Types.Rated);
			lineMock.Setup(m => m.TaxRate).Returns(ratedDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(10);
			lineMock.Setup(m => m.TaxExtraRateAmount).Returns(1);
			lineMock.Setup(m => m.OSTaxAmount).Returns(1234.56m);

			DocCurrency aUD = DocCurrency.New("AUD", Factory);
			DocCurrency vND = DocCurrency.New("VND", Factory);

			lineMock.Setup(m => m.DisplayCurrency).Returns(aUD);
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
				lineMock.Setup(m => m.OSQSTAmount).Returns(1234.56m);
				AssertEquals("1,234.56", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				lineMock.Setup(m => m.OSQSTAmount).Returns(1234.56m);
				AssertEquals("1,234.56", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				lineMock.Setup(m => m.OSEDUAmount).Returns(1234.56m);
				AssertEquals("1,234.56", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
				lineMock.Setup(m => m.OSRETAmount).Returns(1234.56m);
				AssertEquals("1,234.56", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
				lineMock.Setup(m => m.OSRETAmount).Returns(1234.56m);
				AssertEquals("1,234.56", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATClaimed;
				AssertEquals("", line.GetOSTaxExtraAmountDisplay());
			}
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
				lineMock.Setup(m => m.OSQSTAmount).Returns(1234.56m);
				AssertEquals("1.234,56", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				lineMock.Setup(m => m.OSQSTAmount).Returns(1234.56m);
				AssertEquals("1.234,56", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				lineMock.Setup(m => m.OSEDUAmount).Returns(1234.56m);
				AssertEquals("1.234,56", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
				lineMock.Setup(m => m.OSRETAmount).Returns(1234.56m);
				AssertEquals("1.234,56", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
				lineMock.Setup(m => m.OSRETAmount).Returns(1234.56m);
				AssertEquals("1.234,56", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATClaimed;
				AssertEquals("", line.GetOSTaxExtraAmountDisplay());
			}

			lineMock.Setup(m => m.DisplayCurrency).Returns(vND);
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
				lineMock.Setup(m => m.OSQSTAmount).Returns(1234.56m);
				AssertEquals("1,235", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				lineMock.Setup(m => m.OSQSTAmount).Returns(1234.56m);
				AssertEquals("1,235", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				lineMock.Setup(m => m.OSEDUAmount).Returns(1234.56m);
				AssertEquals("1,235", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
				lineMock.Setup(m => m.OSRETAmount).Returns(1234.56m);
				AssertEquals("1,235", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
				lineMock.Setup(m => m.OSRETAmount).Returns(1234.56m);
				AssertEquals("1,235", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATClaimed;
				AssertEquals("", line.GetOSTaxExtraAmountDisplay());
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
				lineMock.Setup(m => m.OSQSTAmount).Returns(1234.56m);
				AssertEquals("1.235", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				lineMock.Setup(m => m.OSQSTAmount).Returns(1234.56m);
				AssertEquals("1.235", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				lineMock.Setup(m => m.OSEDUAmount).Returns(1234.56m);
				AssertEquals("1.235", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
				lineMock.Setup(m => m.OSRETAmount).Returns(1234.56m);
				AssertEquals("1.235", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
				lineMock.Setup(m => m.OSRETAmount).Returns(1234.56m);
				AssertEquals("1.235", line.GetOSTaxExtraAmountDisplay());

				ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATClaimed;
				AssertEquals("", line.GetOSTaxExtraAmountDisplay());
			}
		}

		public void TestGetOSTaxAmountDisplay()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			var lineMock = new Mock<IDocARInvoiceLine>();
			IDocARInvoiceLine line = lineMock.Object;

			lineMock.Setup(m => m.IsSpacerLine).Returns(false);
			lineMock.Setup(m => m.IsSubTotalLine).Returns(false);
			lineMock.Setup(m => m.ChargeCode).Returns(() => null);
			lineMock.Setup(m => m.DisplayTaxGroupCode).Returns(false);
			lineMock.Setup(m => m.TaxRate).Returns(() => null);

			AssertEquals("N/A", line.GetOSTaxAmountDisplay());
			AssertEquals("N/A", line.GetOSTaxAmountDisplayWithRegistryRule());

			DocTaxRate ratedDocRate = GetDocTaxRate("FREEGST", AccTaxRate.Types.Rated);
			lineMock.Setup(m => m.TaxRate).Returns(ratedDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(0);
			AssertEquals("Zero Rated", line.GetOSTaxAmountDisplay());
			AssertEquals("Zero Rated", line.GetOSTaxAmountDisplayWithRegistryRule());
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, "Override Zero Rated");
			AssertEquals("Override Zero Rated", line.GetOSTaxAmountDisplayWithRegistryRule());

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.CapitalRated;
			AssertEquals("Zero Rated", line.GetOSTaxAmountDisplay());
			AssertEquals("Zero Rated", line.GetOSTaxAmountDisplayWithRegistryRule());
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.CapitalRated, "Zero Rated CAP");
			AssertEquals("Zero Rated", line.GetOSTaxAmountDisplay());
			AssertEquals("Zero Rated CAP", line.GetOSTaxAmountDisplayWithRegistryRule());

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, string.Empty);

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.Rated;
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(10);
			DocCurrency currency = DocCurrency.New(Factory, GlbCompany.CurrentCompany.LocalCurrency);
			lineMock.Setup(m => m.Currency).Returns(currency);
			DocCurrency aUD = DocCurrency.New("AUD", Factory);
			DocCurrency vND = DocCurrency.New("VND", Factory);
			DocCurrency gHS = DocCurrency.New("GHS", Factory);
			DocCurrency mYR = DocCurrency.New("MYR", Factory);
			DocCurrency iNR = DocCurrency.New("INR", Factory);
			lineMock.Setup(m => m.DisplayCurrency).Returns(aUD);
			lineMock.Setup(m => m.OSTaxAmount).Returns(1234.56m);
			lineMock.Setup(m => m.OSSERAmount).Returns(1234.56m);
			lineMock.Setup(m => m.ShowPercentInGSTDisplay).Returns(false);
			lineMock.Setup(m => m.IncludeTaxAmountInOsTaxDisplay).Returns(false);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("1,234.56", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "10%=1,234.56", "1,234.56", "10%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("1.234,56", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "10%=1.234,56", "1.234,56", "10%");
			}

			lineMock.Setup(m => m.ShowPercentInGSTDisplay).Returns(true);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("10%", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "10%=1,234.56", "1,234.56", "10%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("10%", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "10%=1.234,56", "1.234,56", "10%");
			}

			lineMock.Setup(m => m.IncludeTaxAmountInOsTaxDisplay).Returns(true);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("10%=1,234.56", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "10%=1,234.56", "1,234.56", "10%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("10%=1.234,56", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "10%=1.234,56", "1.234,56", "10%");
			}

			lineMock.Setup(m => m.DisplayCurrency).Returns(vND);
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("10%=1,235", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "10%=1,235", "1,235", "10%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("10%=1.235", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "10%=1.235", "1.235", "10%");
			}

			lineMock.Setup(m => m.DisplayCurrency).Returns(mYR);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(6);
			ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ServiceTax;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
			{
				AssertEquals("SERVICE TAX\r\n6%=1,234.56", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "SERVICE TAX\r\n6%=1,234.56", "SERVICE TAX\r\n1,234.56", "SERVICE TAX\r\n6%");
			}

			lineMock.Setup(m => m.DisplayCurrency).Returns(aUD);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("6%=1.234,56", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "6%=1.234,56", "1.234,56", "6%");
			}

			lineMock.Setup(m => m.DisplayCurrency).Returns(vND);
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("6%=1,235", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "6%=1,235", "1,235", "6%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("6%=1.235", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "6%=1.235", "1.235", "6%");
			}
			lineMock.Setup(m => m.DisplayCurrency).Returns(aUD);

			lineMock.Setup(m => m.OSGSTAmount).Returns(1234.56m);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(10);
			ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			lineMock.Setup(m => m.TaxExtraRateAmount).Returns(1);
			lineMock.Setup(m => m.OSQSTAmount).Returns(123.46m);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("GST 10%=1,234.56,\r\nQST 1%=123.46", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "GST 10%=1,234.56,\r\nQST 1%=123.46", "GST 1,234.56,\r\nQST 123.46", "GST 10%,\r\nQST 1%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("GST 10%=1.234,56,\r\nQST 1%=123,46", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "GST 10%=1.234,56,\r\nQST 1%=123,46", "GST 1.234,56,\r\nQST 123,46", "GST 10%,\r\nQST 1%");
			}

			lineMock.Setup(m => m.DisplayCurrency).Returns(vND);
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("GST 10%=1,235,\r\nQST 1%=123", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "GST 10%=1,235,\r\nQST 1%=123", "GST 1,235,\r\nQST 123", "GST 10%,\r\nQST 1%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("GST 10%=1.235,\r\nQST 1%=123", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "GST 10%=1.235,\r\nQST 1%=123", "GST 1.235,\r\nQST 123", "GST 10%,\r\nQST 1%");
			}
			lineMock.Setup(m => m.DisplayCurrency).Returns(aUD);
			lineMock.Setup(m => m.OSGSTAmount).Returns(1234.56m);
			ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			lineMock.Setup(m => m.TaxExtraRateAmount).Returns(1);

			lineMock.Setup(m => m.OSQSTAmount).Returns(123.46m);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("GST 10%=1,234.56,\r\nQST 1%=123.46", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "GST 10%=1,234.56,\r\nQST 1%=123.46", "GST 1,234.56,\r\nQST 123.46", "GST 10%,\r\nQST 1%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("GST 10%=1.234,56,\r\nQST 1%=123,46", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "GST 10%=1.234,56,\r\nQST 1%=123,46", "GST 1.234,56,\r\nQST 123,46", "GST 10%,\r\nQST 1%");
			}

			lineMock.Setup(m => m.DisplayCurrency).Returns(vND);
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("GST 10%=1,235,\r\nQST 1%=123", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "GST 10%=1,235,\r\nQST 1%=123", "GST 1,235,\r\nQST 123", "GST 10%,\r\nQST 1%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("GST 10%=1.235,\r\nQST 1%=123", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "GST 10%=1.235,\r\nQST 1%=123", "GST 1.235,\r\nQST 123", "GST 10%,\r\nQST 1%");
			}

			lineMock.Setup(m => m.IsExtraTaxSBCAndKKC).Returns(true);
			lineMock.Setup(m => m.OSSBCAmount).Returns(61.5);
			lineMock.Setup(m => m.OSKKCAmount).Returns(61.5);
			ratedDocRate.AccTaxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.India))
			{
				AssertEquals("SER 10%=1,235,\r\nSBC.5% KKC.5%=123", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "SER 10%=1,235,\r\nSBC.5% KKC.5%=123", "SER 1,235,\r\nSBC KKC=123", "SER 10%,\r\nSBC.5% KKC.5%");
			}

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.ServiceTax;
			lineMock.Setup(m => m.IsExtraTaxSBCAndKKC).Returns(true);
			lineMock.Setup(m => m.OSSBCAmount).Returns(61.5);
			lineMock.Setup(m => m.OSKKCAmount).Returns(61.5);
			ratedDocRate.AccTaxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AssertEquals("SER 10%=1,235,\r\nSBC.5% KKC.5%=123", line.GetOSTaxAmountDisplay());
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
				AssertEquals("SER 10%=1,235,\r\nSBC.5% KKC.5%=123", line.GetOSTaxAmountDisplayWithRegistryRule());
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
				AssertEquals("SER 1,235,\r\nSBC KKC=123", line.GetOSTaxAmountDisplayWithRegistryRule());
				AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
				AssertEquals("SER 10%,\r\nSBC.5% KKC.5%", line.GetOSTaxAmountDisplayWithRegistryRule());
			}

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.Rated;
			ratedDocRate.AccTaxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Ghana;
			lineMock.Setup(m => m.DisplayCurrency).Returns(gHS);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ghana))
			{
				AssertEquals("VAT 10%=1,234.56,\r\nNHIL/GETFL 1%=123.46", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "VAT 10%=1,234.56,\r\nNHIL/GETFL 1%=123.46", "VAT 1,234.56,\r\nNHIL/GETFL 123.46", "VAT 10%,\r\nNHIL/GETFL 1%");
			}
			ratedDocRate.AccTaxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			lineMock.Setup(m => m.DisplayCurrency).Returns(aUD);
			ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
			lineMock.Setup(m => m.TaxExtraRateAmount).Returns(1);
			lineMock.Setup(m => m.OSEDUAmount).Returns(123.46m);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("SER 10%=1,234.56,\r\nEDU 1%=123.46", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "SER 10%=1,234.56,\r\nEDU 1%=123.46", "SER 1,234.56,\r\nEDU 123.46", "SER 10%,\r\nEDU 1%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("SER 10%=1.234,56,\r\nEDU 1%=123,46", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "SER 10%=1.234,56,\r\nEDU 1%=123,46", "SER 1.234,56,\r\nEDU 123,46", "SER 10%,\r\nEDU 1%");
			}

			lineMock.Setup(m => m.DisplayCurrency).Returns(vND);
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("SER 10%=1,235,\r\nEDU 1%=123", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "SER 10%=1,235,\r\nEDU 1%=123", "SER 1,235,\r\nEDU 123", "SER 10%,\r\nEDU 1%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("SER 10%=1.235,\r\nEDU 1%=123", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "SER 10%=1.235,\r\nEDU 1%=123", "SER 1.235,\r\nEDU 123", "SER 10%,\r\nEDU 1%");
			}
			lineMock.Setup(m => m.DisplayCurrency).Returns(aUD);

			ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
			lineMock.Setup(m => m.TaxExtraRateAmount).Returns(1);
			lineMock.Setup(m => m.OSRETAmount).Returns(123.46m);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("IVA 10%=1,234.56\r\n- Withheld 1%=123.46", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "IVA 10%=1,234.56\r\n- Withheld 1%=123.46", "IVA 1,234.56\r\n- Withheld 123.46", "IVA 10%\r\n- Withheld 1%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("IVA 10%=1.234,56\r\n- Withheld 1%=123,46", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "IVA 10%=1.234,56\r\n- Withheld 1%=123,46", "IVA 1.234,56\r\n- Withheld 123,46", "IVA 10%\r\n- Withheld 1%");
			}

			lineMock.Setup(m => m.DisplayCurrency).Returns(vND);
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("IVA 10%=1,235\r\n- Withheld 1%=123", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "IVA 10%=1,235\r\n- Withheld 1%=123", "IVA 1,235\r\n- Withheld 123", "IVA 10%\r\n- Withheld 1%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("IVA 10%=1.235\r\n- Withheld 1%=123", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "IVA 10%=1.235\r\n- Withheld 1%=123", "IVA 1.235\r\n- Withheld 123", "IVA 10%\r\n- Withheld 1%");
			}
			lineMock.Setup(m => m.DisplayCurrency).Returns(aUD);

			ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
			lineMock.Setup(m => m.TaxExtraRateAmount).Returns(5);
			lineMock.Setup(m => m.OSRETAmount).Returns(123.46m);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("IVA 10%=1,234.56\r\n- Withheld 5%=123.46", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "IVA 10%=1,234.56\r\n- Withheld 5%=123.46", "IVA 1,234.56\r\n- Withheld 123.46", "IVA 10%\r\n- Withheld 5%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("IVA 10%=1.234,56\r\n- Withheld 5%=123,46", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "IVA 10%=1.234,56\r\n- Withheld 5%=123,46", "IVA 1.234,56\r\n- Withheld 123,46", "IVA 10%\r\n- Withheld 5%");
			}

			lineMock.Setup(m => m.DisplayCurrency).Returns(vND);
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("IVA 10%=1,235\r\n- Withheld 5%=123", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "IVA 10%=1,235\r\n- Withheld 5%=123", "IVA 1,235\r\n- Withheld 123", "IVA 10%\r\n- Withheld 5%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("IVA 10%=1.235\r\n- Withheld 5%=123", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "IVA 10%=1.235\r\n- Withheld 5%=123", "IVA 1.235\r\n- Withheld 123", "IVA 10%\r\n- Withheld 5%");
			}
			lineMock.Setup(m => m.DisplayCurrency).Returns(aUD);

			ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;
			ratedDocRate.AccTaxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Italy;
			lineMock.Setup(m => m.TaxExtraRateAmount).Returns(0);
			lineMock.Setup(m => m.OSSPVAmount).Returns(123.46m);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("10%=-123.46", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "10%=-123.46", "-123.46", "10%");
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("10%=-123,46", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "10%=-123,46", "-123,46", "10%");
			}

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.IntegratedGST;
			ratedDocRate.AccTaxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = string.Empty;
			lineMock.Setup(m => m.TaxExtraRateAmount).Returns(1);
			lineMock.Setup(m => m.OSIntegratedGSTAmount).Returns(1.235);
			lineMock.Setup(m => m.DisplayCurrency).Returns(iNR);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.India))
			{
				AssertEquals("IGST 10%=1.24", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "IGST 10%=1.24", "IGST 1.24", "IGST 10%");
			}

			ratedDocRate.AccTaxRate.AT_Code = "STAGST";
			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.Rated;
			ratedDocRate.AccTaxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
			ratedDocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.StateGST;
			lineMock.Setup(m => m.TaxExtraRateAmount).Returns(10);

			lineMock.Setup(m => m.OSCentreGSTAmount).Returns(1.235);
			lineMock.Setup(m => m.OSStateGSTAmount).Returns(1.235);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.India))
			{
				AssertEquals("CGST 10%=1.24,\r\nSGST 10%=1.24", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "CGST 10%=1.24,\r\nSGST 10%=1.24", "CGST 1.24,\r\nSGST 1.24", "CGST 10%,\r\nSGST 10%");
			}

			lineMock.Setup(m => m.DisplayCurrency).Returns(aUD);

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			AssertEquals("Reverse Charge", line.GetOSTaxAmountDisplay());
			AssertEquals("Reverse Charge", line.GetOSTaxAmountDisplayWithRegistryRule());
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.ReverseRated, "Override Reverse");
			AssertEquals("Override Reverse", line.GetOSTaxAmountDisplayWithRegistryRule());

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.ReverseRated, string.Empty);

			ratedDocRate.AccTaxRate.AT_Type = AccTaxRate.Types.Suspended;
			AssertEquals("Suspended", line.GetTaxAmountDisplay());
			AssertEquals("Suspended", line.GetOSTaxAmountDisplayWithRegistryRule());
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Suspended, "Override Suspended");
			AssertEquals("Override Suspended", line.GetOSTaxAmountDisplayWithRegistryRule());

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Suspended, string.Empty);

			DocTaxRate exemptDocRate = GetDocTaxRate("EXEMPT", AccTaxRate.Types.Exempt);
			lineMock.Setup(m => m.TaxRate).Returns(exemptDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(exemptDocRate.AccTaxRate.GetRateRaw_ForTestOnly());
			AssertEquals("Exempt Rated", line.GetOSTaxAmountDisplay());
			AssertEquals("Exempt", line.GetOSTaxAmountDisplayWithRegistryRule());
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Exempt, "Override Exempt");
			AssertEquals("Override Exempt", line.GetOSTaxAmountDisplayWithRegistryRule());

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Exempt, string.Empty);

			AccTaxRate rate;
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("IT"))
			{
				rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "ESCLUSE", AccTaxRate.Types.ExcludedFromTheTaxBase, 0);
			}
			var excludedDocRate = DocTaxRate.New(Factory.Load<AccTaxRate>(rate.PK), Factory);
			lineMock.Setup(m => m.TaxRate).Returns(excludedDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(excludedDocRate.AccTaxRate.GetRateRaw_ForTestOnly());
			AssertEquals("Excluded", line.GetOSTaxAmountDisplay());
			AssertEquals("Excluded", line.GetOSTaxAmountDisplayWithRegistryRule());
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.ExcludedFromTheTaxBase, "Override Excluded");
			AssertEquals("Override Excluded", line.GetOSTaxAmountDisplayWithRegistryRule());

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.ExcludedFromTheTaxBase, string.Empty);

			DocTaxRate notReportableDocRate = GetDocTaxRate("NOTREPORT", AccTaxRate.Types.NotReportable);
			lineMock.Setup(m => m.TaxRate).Returns(notReportableDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(notReportableDocRate.AccTaxRate.GetRateRaw_ForTestOnly());
			AssertEquals("Not Applicable", line.GetOSTaxAmountDisplay());
			AssertEquals("Not Applicable", line.GetOSTaxAmountDisplayWithRegistryRule());
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.NotReportable, "Override Not Applicable");
			AssertEquals("Override Not Applicable", line.GetOSTaxAmountDisplayWithRegistryRule());

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.NotReportable, string.Empty);

			lineMock.Setup(m => m.IsSpacerLine).Returns(true);
			Assert("Should be empty", line.GetOSTaxAmountDisplay().IsEmpty);
			Assert("Should be empty", line.GetOSTaxAmountDisplayWithRegistryRule().IsEmpty);

			lineMock.Setup(m => m.IsSpacerLine).Returns(false);
			AssertEquals("Not Applicable", line.GetOSTaxAmountDisplay());
			AssertEquals("Not Applicable", line.GetOSTaxAmountDisplayWithRegistryRule());
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.NotReportable, "Override Not Applicable");
			AssertEquals("Override Not Applicable", line.GetOSTaxAmountDisplayWithRegistryRule());

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.NotReportable, string.Empty);

			lineMock.Setup(m => m.IsSubTotalLine).Returns(true);
			Assert("Should be empty", line.GetOSTaxAmountDisplay().IsEmpty);
			Assert("Should be empty", line.GetOSTaxAmountDisplayWithRegistryRule().IsEmpty);

			lineMock.Setup(m => m.IsSubTotalLine).Returns(false);
			AssertEquals("Not Applicable", line.GetOSTaxAmountDisplay());
			AssertEquals("Not Applicable", line.GetOSTaxAmountDisplayWithRegistryRule());
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.NotReportable, "Override Not Applicable");
			AssertEquals("Override Not Applicable", line.GetOSTaxAmountDisplayWithRegistryRule());

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.NotReportable, string.Empty);

			var taxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "NOTREPORT", AccTaxRate.Types.NotReportable, 0);
			AccChargeCode chargeCode = CreateAccChargeCode("BOLLO", taxRate.PK, "NON", "NGC");
			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
			DocChargeCode docChargeCode = DocChargeCode.New(chargeCode, Factory);
			lineMock.Setup(m => m.ChargeCode).Returns(docChargeCode);
			AssertEquals("Not Applicable", line.GetOSTaxAmountDisplay());
			AssertEquals("Not Applicable", line.GetOSTaxAmountDisplayWithRegistryRule());
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.NotReportable, "Override Not Applicable");
			AssertEquals("Override Not Applicable", line.GetOSTaxAmountDisplayWithRegistryRule());

			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.NotReportable, string.Empty);

			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.NewGuid());
			AssertEquals("Not Applicable", line.GetOSTaxAmountDisplay());
			AssertEquals("Not Applicable", line.GetOSTaxAmountDisplayWithRegistryRule());
			AccountingHelperClassForTest.SetZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.NotReportable, "Override Not Applicable");
			AssertEquals("Override Not Applicable", line.GetOSTaxAmountDisplayWithRegistryRule());
		}

		void AssertOSTaxAmountDisplayWithRegistryRule(IDocARInvoiceLine line, ZString expectedValueWhenBoth, ZString expectedValueWhenTaxAmount, ZString expectedValueWhenTaxRate)
		{
			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);
			AssertEquals(expectedValueWhenBoth, line.GetOSTaxAmountDisplayWithRegistryRule());
			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
			AssertEquals(expectedValueWhenTaxAmount, line.GetOSTaxAmountDisplayWithRegistryRule());
			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code);
			AssertEquals(expectedValueWhenTaxRate, line.GetOSTaxAmountDisplayWithRegistryRule());
		}

		public void TestGetTaxRateDisplay()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			var lineMock = new Mock<IDocARInvoiceLine>();
			IDocARInvoiceLine line = lineMock.Object;

			lineMock.Setup(m => m.TaxRate).Returns(() => null);
			Assert("Should be empty", line.GetTaxRateDisplay().IsEmpty);

			DocTaxRate exemptDocRate = GetDocTaxRate("EXEMPT", AccTaxRate.Types.Exempt);
			lineMock.Setup(m => m.TaxRate).Returns(exemptDocRate);
			Assert("Should be empty", line.GetTaxRateDisplay().IsEmpty);

			DocTaxRate ratedDocRate = GetDocTaxRate("FREEGST", AccTaxRate.Types.Rated);
			lineMock.Setup(m => m.TaxRate).Returns(ratedDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(0);
			AssertEquals("Zero Rated", "0%", line.GetTaxRateDisplay());

			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(10);
			lineMock.Setup(m => m.ShowPercentInGSTDisplay).Returns(false);
			Assert("Should be empty", line.GetTaxRateDisplay().IsEmpty);
			AssertEquals("Rated", "10%", line.GetTaxRateDisplay(true));

			lineMock.Setup(m => m.ShowPercentInGSTDisplay).Returns(true);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("Rated", "10%", line.GetTaxRateDisplay());
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("Rated", "10%", line.GetTaxRateDisplay());
			}

			DocTaxRate capitalRatedDocRate = GetDocTaxRate("FREECAPGST", AccTaxRate.Types.CapitalRated);
			lineMock.Setup(m => m.TaxRate).Returns(capitalRatedDocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(5);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("CapitalRated", "5%", line.GetTaxRateDisplay());
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("CapitalRated", "5%", line.GetTaxRateDisplay());
			}

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Portugal))
			{
				AccountingConfigurationRegistry.Instance.DisplayTaxRateInAllLinesOfTaxSummary.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				exemptDocRate = GetDocTaxRate("EXEMPT", AccTaxRate.Types.Exempt);
				lineMock.Setup(m => m.TaxRate).Returns(exemptDocRate);
				lineMock.Reset();
				lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(ZDecimal.Zero);
				Assert("Should be empty", line.GetTaxRateDisplay().IsEmpty);

				lineMock.Setup(m => m.TaxRate).Returns(() => null);
				Assert("Should be empty", line.GetTaxRateDisplay().IsEmpty);

				AccountingConfigurationRegistry.Instance.DisplayTaxRateInAllLinesOfTaxSummary.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				lineMock.Setup(m => m.TaxRate).Returns(exemptDocRate);
				AssertEquals("EXT", "0%", line.GetTaxRateDisplay());

				lineMock.Setup(m => m.TaxRate).Returns(() => null);
				AssertEquals("", "0%", line.GetTaxRateDisplay());
			}
		}

		public void TestGetOSTaxAmountDisplayTurkeyForVAT()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			var lineMock = new Mock<IDocARInvoiceLine>();
			IDocARInvoiceLine line = lineMock.Object;

			lineMock.Setup(m => m.IsSpacerLine).Returns(false);
			lineMock.Setup(m => m.IsSubTotalLine).Returns(false);
			lineMock.Setup(m => m.ChargeCode).Returns(() => null);
			lineMock.Setup(m => m.DisplayTaxGroupCode).Returns(false);
			lineMock.Reset();

			DocTaxRate vatt50DocRate = GetDocTaxRate("VATT50", AccTaxRate.Types.Rated);
			vatt50DocRate.AccTaxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			vatt50DocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
			lineMock.Setup(m => m.TaxRate).Returns(vatt50DocRate);
			lineMock.Setup(m => m.TaxRateAmount_Raw).Returns(18);
			lineMock.Setup(m => m.TaxExtraRateAmount).Returns(9);
			lineMock.Setup(m => m.OSGSTAmount).Returns(18m);
			lineMock.Setup(m => m.OSRETAmount).Returns(9m);
			lineMock.Setup(m => m.IncludeTaxAmountInOsTaxDisplay).Returns(true);
			lineMock.Setup(m => m.ShowPercentInGSTDisplay).Returns(true);

			vatt50DocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Turkey))
			{
				AssertEquals("VAT 18%=18,00\r\n- Withheld 9%=9,00", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "VAT 18%=18,00\r\n- Withheld 9%=9,00", "VAT 18,00\r\n- Withheld 9,00", "VAT 18%\r\n- Withheld 9%");
			}

			vatt50DocRate.AccTaxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Turkey))
			{
				AssertEquals("VAT 18%=18,00\r\n- Withheld 9%=9,00", line.GetOSTaxAmountDisplay());
				AssertOSTaxAmountDisplayWithRegistryRule(line, "VAT 18%=18,00\r\n- Withheld 9%=9,00", "VAT 18,00\r\n- Withheld 9,00", "VAT 18%\r\n- Withheld 9%");
			}
		}

		#region Implementation

		DocTaxRate GetDocTaxRate(string code, string rateType)
		{
			var taxId = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, code, rateType, 0);
			return DocTaxRate.New(Factory.Load<AccTaxRate>(taxId.PK), Factory);
		}

		AccChargeCode CreateAccChargeCode(string code, ZGuid taxRatePK, string chargeType, string chargeGroup)
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_AT_GSTRate = taxRatePK;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_ChargeGroup = chargeGroup;
			return chargeCode;
		}

		#endregion
	}
}
