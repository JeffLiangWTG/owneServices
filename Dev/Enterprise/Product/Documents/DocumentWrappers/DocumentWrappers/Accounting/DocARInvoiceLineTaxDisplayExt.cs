using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public static class DocARInvoiceLineTaxDisplayExt
	{
		public static ZString GetTaxAmountDisplayWithRegistryRule(this IDocARInvoiceLine line)
		{
			var result = ZString.Empty;

			if (line.TaxRate == null)
			{
				result = ZString.Empty;
			}
			else
			{
				var lineTaxRateType = line.TaxRate.Type.ToString().ToUpper();
				if (line.TaxRate.IsRatedTax() && line.TaxRateAmount_Raw == 0m)
				{
					result = GetOverrideDescriptionForZeroRated();
				}
				else if (lineTaxRateType == AccTaxRate.Types.CapitalRated && line.TaxRateAmount_Raw == 0m)
				{
					result = GetOverrideDescriptionForCapitalRated();
				}
				else if (lineTaxRateType == AccTaxRate.Types.Exempt)
				{
					result = GetOverrideDescriptionForExemptRated();
				}
				else if (lineTaxRateType == AccTaxRate.Types.NotReportable)
				{
					result = GetOverrideDescriptionForNotReportable();
				}
				else if (lineTaxRateType == AccTaxRate.Types.ReverseRated)
				{
					result = GetOverrideDescriptionForReverse();
				}
				else if (lineTaxRateType == AccTaxRate.Types.Suspended)
				{
					result = GetOverrideDescriptionForSuspended();
				}
				else if (lineTaxRateType == AccTaxRate.Types.ExcludedFromTheTaxBase)
				{
					result = GetOverrideDescriptionForExcludedFromTheTaxBase();
				}
				else
				{
					result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(line.OSTaxAmount, line.Currency);
				}
			}
			return result;
		}

		public static ZString GetTaxAmountDisplay(this IDocARInvoiceLine line)
		{
			var result = ZString.Empty;

			if (line.TaxRate == null)
			{
				result = ZString.Empty;
			}
			else
			{
				var lineTaxRateType = line.TaxRate.Type.ToString().ToUpper();
				if ((line.TaxRate.IsRatedTax() || lineTaxRateType == AccTaxRate.Types.CapitalRated) && line.TaxRateAmount_Raw == 0m)
				{
					result = GetDescriptionForZeroRated();
				}
				else if (lineTaxRateType == AccTaxRate.Types.Exempt)
				{
					result = GetDescriptionForExemptRated();
				}
				else if (lineTaxRateType == AccTaxRate.Types.NotReportable)
				{
					result = GetDescriptionForNotReportable();
				}
				else if (lineTaxRateType == AccTaxRate.Types.ReverseRated)
				{
					result = GetDescriptionForReverse() + " " + DocARInvoiceCommon.TranslatedTaxCode;
				}
				else if (lineTaxRateType == AccTaxRate.Types.Suspended)
				{
					result = GetDescriptionForSuspended();
				}
				else if (lineTaxRateType == AccTaxRate.Types.ExcludedFromTheTaxBase)
				{
					result = GetDescriptionForExcludedFromTheTaxBase();
				}
				else
				{
					result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(line.OSTaxAmount, line.Currency);
				}
			}
			return result;
		}

		public static ZString GetOSTaxMainRateDisplay(this IDocARInvoiceLine line)
		{
			ZString result = "N/A";
			if (line.NeedToDisplayTaxInfo())
			{
				if (line.TaxRate != null)
				{
					string taxRateType = line.TaxRate.Type.ToString().ToUpper();
					if (taxRateType == AccTaxRate.Types.Exempt)
					{
						result = Res.GetString("f1d983dd-5b5a-4b38-af86-a4e5a7e70f9c", "Exempt");
					}
					else if (taxRateType == AccTaxRate.Types.ReverseRated)
					{
						result = Res.GetString("eaadc318-6512-4e0d-b907-ade069567dbf", "Reverse");
					}
					else if (taxRateType == AccTaxRate.Types.Suspended)
					{
						result = Res.GetString("27416fb3-4283-46a3-9fe2-4645d85c5986", "Suspended");
					}
					else if (taxRateType == AccTaxRate.Types.NotReportable)
					{
						result = "N/A";
					}
					else if (taxRateType == AccTaxRate.Types.ExcludedFromTheTaxBase)
					{
						result = Res.GetString("20323bc2-114e-4fd6-894d-1a3aee9da4ec", "Excluded");
					}
					else if (line.TaxRate.IsRatedTax() || taxRateType == AccTaxRate.Types.CapitalRated)
					{
						result = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(line.TaxRateAmount_Raw);
					}
				}
			}
			return result;
		}

		public static ZString GetOSTaxExtraRateDisplay(this IDocARInvoiceLine line)
		{
			ZString result = "N/A";
			if (line.NeedToDisplayTaxInfo())
			{
				if (line.TaxRate != null)
				{
					string taxRateExtraType = line.TaxRate.ExtraType.ToString().ToUpper();
					if (line.TaxRate.IsRatedTax())
					{
						if (taxRateExtraType == AccTaxRate.ExtraTypes.QuebecQST ||
							taxRateExtraType == AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase ||
							taxRateExtraType == AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax ||
							taxRateExtraType == AccTaxRate.ExtraTypes.VATRetention)
						{
							result = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(line.TaxExtraRateAmount);
						}
						else if (taxRateExtraType == AccTaxRate.ExtraTypes.VATRetentionFraction)
						{
							result = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(Math.Round(line.TaxExtraRateAmount, 3)); // reduce accuracy for fraction by 3 digits
						}
						else if (taxRateExtraType == AccTaxRate.ExtraTypes.VATRemittedByCustomer)
						{
							result = GetSPVTaxLabel(line.TaxRate.AccTaxRate.AT_RN_NKCountry);
						}
					}
				}
			}
			return result;
		}

		public static ZString GetSPVTaxLabel(string country)
		{
			switch (country)
			{
				case Core.Constants.CountryCodes.CostaRica:
					return Res.GetString("51F790BD-01FB-4D41-B662-429F9084156D", "Exon.");
				case Core.Constants.CountryCodes.Italy:
					return Res.GetString("3C8A672E-60CE-4020-BF36-E7B862F6D434", "SPV");
				default:
					return "N/A";
			}
		}

		public static ZString GetOSTaxMainAmountDisplay(this IDocARInvoiceLine line)
		{
			ZString result = ZString.Empty;
			if (line.NeedToDisplayTaxInfo())
			{
				if (line.TaxRate != null)
				{
					string taxRateType = line.TaxRate.Type.ToString().ToUpper();
					if (taxRateType != AccTaxRate.Types.NotReportable &&
						taxRateType != AccTaxRate.Types.Exempt &&
						taxRateType != AccTaxRate.Types.ReverseRated &&
						taxRateType != AccTaxRate.Types.Suspended &&
						taxRateType != AccTaxRate.Types.ExcludedFromTheTaxBase)
					{
						result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(line.OSTaxAmount, line.DisplayCurrency);
					}
				}
			}
			return result;
		}

		public static ZString GetOSTaxExtraAmountDisplay(this IDocARInvoiceLine line)
		{
			ZString result = ZString.Empty;
			if (line.NeedToDisplayTaxInfo())
			{
				if (line.TaxRate != null)
				{
					string taxRateExtraType = line.TaxRate.ExtraType.ToString().ToUpper();
					if (line.TaxRate.IsRatedTax())
					{
						if (taxRateExtraType == AccTaxRate.ExtraTypes.QuebecQST ||
							taxRateExtraType == AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase)
						{
							result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(line.OSQSTAmount, line.DisplayCurrency);
						}
						else if (taxRateExtraType == AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax)
						{
							result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(line.OSEDUAmount, line.DisplayCurrency);
						}
						else if (taxRateExtraType == AccTaxRate.ExtraTypes.VATRetention || taxRateExtraType == AccTaxRate.ExtraTypes.VATRetentionFraction)
						{
							result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(line.OSRETAmount, line.DisplayCurrency);
						}
						else if (line.TaxRate.AccTaxRate.IsVATRemittedByCustomer)
						{
							result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(-line.OSSPVAmount, line.DisplayCurrency);
						}
					}
				}
			}
			return result;
		}

		static bool NeedToDisplayTaxInfo(this IDocARInvoiceLine line)
		{
			return (!(line.IsSpacerLine || line.IsSubTotalLine) && (line.ChargeCode == null || line.ChargeCode.ChargeType != "CMT"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ZString GetOSTaxAmountDisplay(this IDocARInvoiceLine line)
		{
			ZString result = ZString.Empty;

			if (line.NeedToDisplayTaxInfo())
			{
				string taxRateType = String.Empty;
				string taxRateExtraType = String.Empty;

				if (line.TaxRate != null)
				{
					taxRateType = line.TaxRate.Type.ToString().ToUpper();
					taxRateExtraType = line.TaxRate.ExtraType.ToString().ToUpper();
				}

				if (line.TaxRate == null)
				{
					result = "N/A";
				}
				else if ((line.TaxRate.IsRatedTax() || taxRateType == AccTaxRate.Types.CapitalRated) && line.TaxRateAmount_Raw == 0m)
				{
					result = GetDescriptionForZeroRated();
				}
				else if (taxRateType == AccTaxRate.Types.Exempt)
				{
					result = GetDescriptionForExemptRated();
				}
				else if (taxRateType == AccTaxRate.Types.NotReportable)
				{
					result = GetDescriptionForNotReportable();
				}
				else if ((line.TaxRate.IsRatedTax()
							&& (taxRateExtraType == AccTaxRate.ExtraTypes.QuebecQST || taxRateExtraType == AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase)))
				{
					if (line.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India)
					{
						var extraRate = line.IsExtraTaxSBCAndKKC ? new ZDecimal(line.TaxExtraRateAmount / 2m) : line.TaxExtraRateAmount;
						var oSExtraTax = FormatOSTax(extraRate, line.OSQSTAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay).TrimStart('0');
						var oSExtraRate = ZString.Empty;
						if (!line.ShowPercentInGSTDisplay)
						{
							oSExtraTax = oSExtraTax.Insert(0, "=");
						}
						else
						{
							oSExtraRate = FormatOSTax(extraRate, line.OSQSTAmount, line.DisplayCurrency, true, false).TrimStart('0');
						}

						result = Res.GetString("81f77068-f8d7-48f9-a917-cc805e4d610c", "{0} {1},\r\n{2}{3}{4}",
							"SER", FormatOSTax(line.TaxRateAmount_Raw, line.OSGSTAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay),
							line.OSSBCAmount != ZDecimal.Zero ? "SBC" : "",
							line.IsExtraTaxSBCAndKKC ? oSExtraRate : line.OSSBCAmount != ZDecimal.Zero ? oSExtraTax : ZString.Empty,
							line.OSKKCAmount != ZDecimal.Zero ? " KKC" + oSExtraTax : "");
					}
					else
					{
						result = Res.GetString("96acd196-7a70-4631-8329-5098d660fc4a", "{0} {1},\r\n{2} {3}",
							DocARInvoiceCommon.GetTranslatedCodeWithFallback(),
							FormatOSTax(line.TaxRateAmount_Raw, line.OSGSTAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay),
							DocARBaseInvoice.GetExtraTaxCodeFromCountryCode(line.TaxRate?.ExtraType),
							FormatOSTax(line.TaxExtraRateAmount, line.OSQSTAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay));
					}
				}
				else if (taxRateType == AccTaxRate.Types.ReverseRated)
				{
					result = GetOverrideDescriptionForReverse();
				}
				else if (taxRateType == AccTaxRate.Types.Suspended)
				{
					result = GetDescriptionForSuspended();
				}
				else if (line.TaxRate.IsRatedTax() && taxRateExtraType == AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax)
				{
					result = Res.GetString("ad18bed6-f7ce-415d-ad90-7389d735c7c8", "SER {0},\r\nEDU {1}",
						FormatOSTax(line.TaxRateAmount_Raw, line.OSGSTAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay),
						FormatOSTax(line.TaxExtraRateAmount, line.OSEDUAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay));
				}
				else if (taxRateType == AccTaxRate.Types.Rated && taxRateExtraType == AccTaxRate.ExtraTypes.VATRetention)
				{
					result = string.Format(CultureInfo.InvariantCulture, "{0} {1}\r\n- {2} {3}",
						GetTaxCodeByCountry(line.TaxRate.AccTaxRate.AT_RN_NKCountry),
						FormatOSTax(line.TaxRateAmount_Raw, line.OSGSTAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay),
						Res.GetString("a2adf242-f274-45e0-b93f-05db9df4b77f", "Withheld"),
						FormatOSTax(line.TaxExtraRateAmount, line.OSRETAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay));
				}
				else if (taxRateType == AccTaxRate.Types.Rated && taxRateExtraType == AccTaxRate.ExtraTypes.VATRetentionFraction)
				{
					result = string.Format(CultureInfo.InvariantCulture, "{0} {1}\r\n- {2} {3}",
						GetTaxCodeByCountry(line.TaxRate.AccTaxRate.AT_RN_NKCountry),
						FormatOSTax(line.TaxRateAmount_Raw, line.OSGSTAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay),
						Res.GetString("a2adf242 -f274-45e0-b93f-05db9df4b77f", "Withheld"),
						FormatOSTax(Math.Round(line.TaxExtraRateAmount, 3), line.OSRETAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay));
				}
				else if (taxRateType == AccTaxRate.Types.IntegratedGST && line.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India)
				{
					result = Res.GetString("b9b25aeb-b7e7-46b2-86bb-cca842fc8224", "IGST {0}",
						FormatOSTax(line.TaxRateAmount_Raw, line.OSIntegratedGSTAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay));
				}
				else if (line.TaxRate.ExtraType == AccTaxRate.ExtraTypes.StateGST && line.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India)
				{
					result = Res.GetString("8ec24043-a187-45b1-b46c-8f117318c39f", "CGST {0},\r\nSGST {1}",
									FormatOSTax(line.TaxRateAmount_Raw, line.OSCentreGSTAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay),
									FormatOSTax(line.TaxExtraRateAmount, line.OSStateGSTAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay));
				}
				else if (line.TaxRate.AccTaxRate.IsVATRemittedByCustomer)
				{
					result = FormatOSTax(line.TaxRateAmount_Raw, -line.OSSPVAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay);
				}
				else if (taxRateExtraType == AccTaxRate.ExtraTypes.ServiceTax && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Malaysia)
				{
					result = Res.GetString("e8eb1ee5-d196-476c-af40-da01d667b482", "SERVICE TAX\r\n{0}", FormatOSTax(line.TaxRateAmount_Raw, line.OSSERAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay));
				}
				else if (taxRateType == AccTaxRate.Types.ExcludedFromTheTaxBase)
				{
					result = GetDescriptionForExcludedFromTheTaxBase();
				}
				else if (taxRateExtraType == AccTaxRate.ExtraTypes.RegionalTax)
				{
					result = FormatOSTax(line.TaxRateAmount_Raw, line.OSGSTAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay);
				}
				else
				{
					result = FormatOSTax(line.TaxRateAmount_Raw, line.OSTaxAmount, line.DisplayCurrency, line.ShowPercentInGSTDisplay, line.IncludeTaxAmountInOsTaxDisplay);
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ZString GetOSTaxAmountDisplayWithRegistryRule(this IDocARInvoiceLine line)
		{
			ZString result = ZString.Empty;

			if (line.NeedToDisplayTaxInfo())
			{
				string taxRateType = String.Empty;
				string taxRateExtraType = String.Empty;

				if (line.TaxRate != null)
				{
					taxRateType = line.TaxRate.Type.ToString().ToUpper();
					taxRateExtraType = line.TaxRate.ExtraType.ToString().ToUpper();
				}

				if (line.TaxRate == null)
				{
					result = "N/A";
				}
				else if (line.TaxRate.IsRatedTax() && line.TaxRateAmount_Raw == 0m)
				{
					result = GetOverrideDescriptionForZeroRated();
				}
				else if (taxRateType == AccTaxRate.Types.CapitalRated && line.TaxRateAmount_Raw == 0m)
				{
					result = GetOverrideDescriptionForCapitalRated();
				}
				else if (taxRateType == AccTaxRate.Types.Exempt)
				{
					result = GetOverrideDescriptionForExemptRated();
				}
				else if (taxRateType == AccTaxRate.Types.NotReportable)
				{
					result = GetOverrideDescriptionForNotReportable();
				}
				else if (line.TaxRate.IsRatedTax()
					&& (taxRateExtraType == AccTaxRate.ExtraTypes.QuebecQST || taxRateExtraType == AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase))
				{
					if (line.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India)
					{
						var extraRate = line.IsExtraTaxSBCAndKKC ? new ZDecimal(line.TaxExtraRateAmount / 2m) : line.TaxExtraRateAmount;
						var oSExtraTaxWithRegistryRule = FormatOSTaxWithRegistryRule(extraRate, line.OSQSTAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule).TrimStart('0');
						var oSExtraRateWithRegistryRule = ZString.Empty;
						if (AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule == AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code)
						{
							oSExtraTaxWithRegistryRule = oSExtraTaxWithRegistryRule.Insert(0, "=");
						}
						else
						{
							oSExtraRateWithRegistryRule = FormatOSTaxWithRegistryRule(extraRate, line.OSQSTAmount, line.DisplayCurrency, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code).TrimStart('0');
						}
						result = Res.GetString("81f77068-f8d7-48f9-a917-cc805e4d610c", "{0} {1},\r\n{2}{3}{4}",
							"SER", FormatOSTaxWithRegistryRule(line.TaxRateAmount_Raw, line.OSGSTAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule),
							line.OSSBCAmount != ZDecimal.Zero ? "SBC" : "",
							line.IsExtraTaxSBCAndKKC ? oSExtraRateWithRegistryRule : line.OSSBCAmount != ZDecimal.Zero ? oSExtraTaxWithRegistryRule : ZString.Empty,
							line.OSKKCAmount != ZDecimal.Zero ? " KKC" + oSExtraTaxWithRegistryRule : "");
					}
					else
					{
						result = Res.GetString("96acd196-7a70-4631-8329-5098d660fc4a", "{0} {1},\r\n{2} {3}",
							DocARInvoiceCommon.GetTranslatedCodeWithFallback(),
							FormatOSTaxWithRegistryRule(line.TaxRateAmount_Raw, line.OSGSTAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule),
							DocARBaseInvoice.GetExtraTaxCodeFromCountryCode(line.TaxRate?.ExtraType),
							FormatOSTaxWithRegistryRule(line.TaxExtraRateAmount, line.OSQSTAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule));
					}
				}
				else if (taxRateType == AccTaxRate.Types.ReverseRated)
				{
					result = GetOverrideDescriptionForReverse();
				}
				else if (taxRateType == AccTaxRate.Types.Suspended)
				{
					result = GetOverrideDescriptionForSuspended();
				}
				else if (line.TaxRate.IsRatedTax() && taxRateExtraType == AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax)
				{
					result = Res.GetString("ad18bed6-f7ce-415d-ad90-7389d735c7c8", "SER {0},\r\nEDU {1}",
						FormatOSTaxWithRegistryRule(line.TaxRateAmount_Raw, line.OSGSTAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule),
						FormatOSTaxWithRegistryRule(line.TaxExtraRateAmount, line.OSEDUAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule));
				}
				else if (taxRateType == AccTaxRate.Types.Rated && taxRateExtraType == AccTaxRate.ExtraTypes.VATRetention)
				{
					result = string.Format(CultureInfo.InvariantCulture, "{0} {1}\r\n- {2} {3}",
						GetTaxCodeByCountry(line.TaxRate.AccTaxRate.AT_RN_NKCountry),
						FormatOSTaxWithRegistryRule(line.TaxRateAmount_Raw, line.OSGSTAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule),
						Res.GetString("a2adf242 -f274-45e0-b93f-05db9df4b77f", "Withheld"),
						FormatOSTaxWithRegistryRule(line.TaxExtraRateAmount, line.OSRETAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule));
				}
				else if (taxRateType == AccTaxRate.Types.Rated && taxRateExtraType == AccTaxRate.ExtraTypes.VATRetentionFraction)
				{
					result = string.Format(CultureInfo.InvariantCulture, "{0} {1}\r\n- {2} {3}",
						GetTaxCodeByCountry(line.TaxRate.AccTaxRate.AT_RN_NKCountry),
						FormatOSTaxWithRegistryRule(line.TaxRateAmount_Raw, line.OSGSTAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule),
						Res.GetString("a2adf242 -f274-45e0-b93f-05db9df4b77f", "Withheld"),
						FormatOSTaxWithRegistryRule(Math.Round(line.TaxExtraRateAmount, 3), line.OSRETAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule));
				}
				else if (taxRateType == AccTaxRate.Types.IntegratedGST && line.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India)
				{
					result = Res.GetString("b9b25aeb-b7e7-46b2-86bb-cca842fc8224", "IGST {0}",
						FormatOSTaxWithRegistryRule(line.TaxRateAmount_Raw, line.OSIntegratedGSTAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule));
				}
				else if (line.TaxRate.ExtraType == AccTaxRate.ExtraTypes.StateGST && line.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India)
				{
					result = Res.GetString("8ec24043-a187-45b1-b46c-8f117318c39f", "CGST {0},\r\nSGST {1}",
									FormatOSTaxWithRegistryRule(line.TaxRateAmount_Raw, line.OSCentreGSTAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule),
									FormatOSTaxWithRegistryRule(line.TaxExtraRateAmount, line.OSStateGSTAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule));
				}
				else if (line.TaxRate.AccTaxRate.IsVATRemittedByCustomer)
				{
					result = FormatOSTaxWithRegistryRule(line.TaxRateAmount_Raw, -line.OSSPVAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule);
				}
				else if (taxRateExtraType == AccTaxRate.ExtraTypes.ServiceTax && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Malaysia)
				{
					result = Res.GetString("e8eb1ee5-d196-476c-af40-da01d667b482", "SERVICE TAX\r\n{0}", FormatOSTaxWithRegistryRule(line.TaxRateAmount_Raw, line.OSSERAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule));
				}
				else if (taxRateType == AccTaxRate.Types.ExcludedFromTheTaxBase)
				{
					result = GetOverrideDescriptionForExcludedFromTheTaxBase();
				}
				else if (taxRateExtraType == AccTaxRate.ExtraTypes.RegionalTax)
				{
					result = FormatOSTaxWithRegistryRule(line.TaxRateAmount_Raw, line.OSGSTAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule);
				}
				else
				{
					result = FormatOSTaxWithRegistryRule(line.TaxRateAmount_Raw, line.OSTaxAmount, line.DisplayCurrency, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule);
				}
			}

			if (line.DisplayTaxGroupCode)
			{
				var taxGroupCode = line.TaxGroupCode;
				if (!string.IsNullOrEmpty(taxGroupCode) && !result.IsEmpty)
				{
					result = string.Format(CultureInfo.InvariantCulture, "({0}) {1}", taxGroupCode, result);
				}
			}

			return result;
		}

		public static ZString GetTaxCodeByCountry(ZString countryCode)
		{
			var result = "";

			if (countryCode == Core.Constants.CountryCodes.Turkey)
			{
				result = Res.GetString("VATTranslation|EU", "VAT");
			}
			else
			{
				result = Res.GetString("057C492C-5E55-4E2C-8141-216BB9EE7B6D", "IVA");
			}

			return result;
		}

		public static ZString GetTaxRateDisplay(this IDocARInvoiceLine line, bool alwaysDisplayTaxRate = false)
		{
			var result = ZString.Empty;
			var displayZeroPercentageTaxRate = AccountingConfigurationRegistry.Instance.DisplayTaxRateInAllLinesOfTaxSummary.Value;

			var lineTaxRateType = string.Empty;
			var isRatedTax = false;
			if (line.TaxRate != null)
			{
				lineTaxRateType = line.TaxRate.Type.ToString().ToUpper();
				isRatedTax = line.TaxRate.IsRatedTax();
			}

			if (!displayZeroPercentageTaxRate && (string.IsNullOrEmpty(lineTaxRateType) || lineTaxRateType == AccTaxRate.Types.Exempt))
			{
				result = "";
			}
			else if (((isRatedTax || displayZeroPercentageTaxRate) && line.TaxRateAmount_Raw == 0m) || (displayZeroPercentageTaxRate && lineTaxRateType == AccTaxRate.Types.Suspended))
			{
				result = "0%";
			}
			else if (line.ShowPercentInGSTDisplay || alwaysDisplayTaxRate)
			{
				result = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(line.TaxRateAmount_Raw);
			}

			return result;
		}

		public static ZString FormatOSTaxWithNoCurrencySymbolFormat(this IDocARInvoiceLine line)
		{
			return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(line.OSTaxAmount, line.Currency);
		}

		static ZString FormatOSTax(ZDecimal rate, ZDecimal amount, DocCurrency currency, bool showPercentInGSTDisplay, bool includeTaxAmountInOsTaxDisplay)
		{
			ZString result = ZString.Empty;

			if (showPercentInGSTDisplay)
			{
				result = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(rate);
				if (includeTaxAmountInOsTaxDisplay)
				{
					result += "=" + FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(amount, currency);
				}
			}
			else
			{
				result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(amount, currency);
			}
			return result;
		}

		static ZString FormatOSTaxWithRegistryRule(ZDecimal rate, ZDecimal amount, DocCurrency currency, ZString descriptionInDocumentsForTaxAmountsRule)
		{
			ZString result = ZString.Empty;

			if (descriptionInDocumentsForTaxAmountsRule == AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code)
			{
				result = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(rate) + "=" + FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(amount, currency);
			}
			else if (descriptionInDocumentsForTaxAmountsRule == AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxRate.Code)
			{
				result = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(rate);
			}
			else
			{
				result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(amount, currency);
			}

			return result;
		}

		public static ZBool IsRatedTax(this DocTaxRate docTaxRate)
		{
			return DocTaxRate.IsRatedTax(docTaxRate.Type);
		}

		public static ZBool IsRatedTax(this AccTaxRate taxRate)
		{
			return DocTaxRate.IsRatedTax(taxRate.AT_Type);
		}

		static ZString GetDescriptionForZeroRated()
		{
			return Res.GetString("63b24924-f75e-4890-92f1-c3a5752e8a68", "Zero Rated");
		}

		static ZString GetDescriptionForExemptRated()
		{
			return Res.GetString("cd6b4ad9-5004-49de-ba34-a9b7d1cd6761", "Exempt Rated");
		}

		static ZString GetDescriptionForNotReportable()
		{
			return Res.GetString("9df75b95-9d4c-49ff-9e81-c532a35cf353", "Not Applicable");
		}

		static ZString GetDescriptionForReverse()
		{
			return Res.GetString("eaadc318-6512-4e0d-b907-ade069567dbf", "Reverse");
		}

		static ZString GetDescriptionForSuspended()
		{
			return Res.GetString("27416fb3-4283-46a3-9fe2-4645d85c5986", "Suspended");
		}

		static ZString GetDescriptionForExcludedFromTheTaxBase()
		{
			return Res.GetString("6e5afb20-56a3-4991-a610-5698828ff62a", "Excluded");
		}

		static ZString GetOverrideDescriptionForZeroRated()
		{
			return AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.Rated);
		}

		static ZString GetOverrideDescriptionForExemptRated()
		{
			return AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.Exempt);
		}

		static ZString GetOverrideDescriptionForNotReportable()
		{
			return AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.NotReportable);
		}

		static ZString GetOverrideDescriptionForReverse()
		{
			return AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.ReverseRated);
		}

		static ZString GetOverrideDescriptionForCapitalRated()
		{
			return AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.CapitalRated);
		}

		static ZString GetOverrideDescriptionForSuspended()
		{
			return AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.Suspended);
		}

		static ZString GetOverrideDescriptionForExcludedFromTheTaxBase()
		{
			return AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.ExcludedFromTheTaxBase);
		}
	}
}
