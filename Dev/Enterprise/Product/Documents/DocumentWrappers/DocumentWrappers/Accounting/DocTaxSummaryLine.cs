using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class TaxSummaryLine
	{
		public TaxSummaryLine(ZString taxRateType, ZDecimal taxRate, ZString taxExtraRateType,
								ZInt taxExtraRateNumerator, ZInt taxExtraRateDenominator,
								ZDecimal totalExcludeTaxAmountInOSCurrency, ZDecimal totalExcludeTaxAmountInLocalCurrency,
								ZDecimal totalTaxAmountInOSCurrency, ZDecimal totalTaxAmountInLocalCurrency,
								ZDecimal totalIncludeTaxAmountInOSCurrency, ZDecimal totalIncludeTaxAmountInLocalCurrency,
								ZDecimal totalExtraTaxAmountInOSCurrency, ZDecimal totalExtraTaxAmountInLocalCurrency)
		{
			this.TaxRateType = taxRateType;
			this.TaxRate = taxRate;
			this.TaxExtraRateType = taxExtraRateType;
			this.TaxExtraRateNumerator = taxExtraRateNumerator;
			this.TaxExtraRateDenominator = taxExtraRateDenominator;
			this.TotalExcludeTaxAmountInOSCurrency = totalExcludeTaxAmountInOSCurrency;
			this.TotalExcludeTaxAmountInLocalCurrency = totalExcludeTaxAmountInLocalCurrency;
			this.TotalTaxAmountInOSCurrency = totalTaxAmountInOSCurrency;
			this.TotalTaxAmountInLocalCurrency = totalTaxAmountInLocalCurrency;
			this.TotalIncludeTaxAmountInOSCurrency = totalIncludeTaxAmountInOSCurrency;
			this.TotalIncludeTaxAmountInLocalCurrency = totalIncludeTaxAmountInLocalCurrency;
			this.TotalExtraTaxAmountInOSCurrency = totalExtraTaxAmountInOSCurrency;
			this.TotalExtraTaxAmountInLocalCurrency = totalExtraTaxAmountInLocalCurrency;
		}

		public ZString TaxRateType { get; set; }

		public ZDecimal TaxRate { get; set; }

		public ZString TaxExtraRateType { get; set; }

		public ZDecimal TaxExtraRate
		{
			get
			{
				if (TaxExtraRateType == AccTaxRate.ExtraTypes.VATRetentionFraction)
				{
					return TaxRate * (ZDecimal)TaxExtraRateNumerator / (ZDecimal)TaxExtraRateDenominator;
				}
				return (ZDecimal)TaxExtraRateNumerator / (ZDecimal)TaxExtraRateDenominator;
			}
		}

		public ZInt TaxExtraRateNumerator { get; set; }

		public ZInt TaxExtraRateDenominator { get; set; }

		public ZDecimal TotalExcludeTaxAmountInOSCurrency { get; set; }

		public ZDecimal TotalExcludeTaxAmountInLocalCurrency { get; set; }

		public ZDecimal TotalTaxAmountInOSCurrency { get; set; }

		public ZDecimal TotalTaxAmountInLocalCurrency { get; set; }

		public ZDecimal TotalIncludeTaxAmountInOSCurrency { get; set; }

		public ZDecimal TotalIncludeTaxAmountInLocalCurrency { get; set; }

		public ZDecimal TotalExtraTaxAmountInOSCurrency { get; set; }

		public ZDecimal TotalExtraTaxAmountInLocalCurrency { get; set; }
	}

	public class DocTaxSummaryLine : DocBaseWrapper
	{
		protected DocTaxSummaryLine(TaxSummaryLine taxSummaryLine, BusinessObjectFactory factoryToWrap)
			: base(taxSummaryLine, factoryToWrap)
		{
			this.TaxSummaryLine = taxSummaryLine;
		}

		internal readonly TaxSummaryLine TaxSummaryLine;

		public static DocTaxSummaryLine New(TaxSummaryLine taxSummaryLine, BusinessObjectFactory factoryToWrap)
		{
			DocTaxSummaryLine result = null;
			if (taxSummaryLine != null)
			{
				result = new DocTaxSummaryLine(taxSummaryLine, factoryToWrap);
			}
			return result;
		}

		public ZString TaxDescription
		{
			get
			{
				ZString result = ZString.Empty;
				if (TaxSummaryLine != null)
				{
					var taxType = TaxSummaryLine.TaxRateType;
					if (DocTaxRate.IsRatedTax(taxType) || taxType == AccTaxRate.Types.CapitalRated)
					{
						result = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(TaxSummaryLine.TaxRate);
					}
					else
					{
						switch (taxType)
						{
							case AccTaxRate.Types.ReverseRated:
								result = Res.GetString("40114c7a-455a-4ec6-8f5a-ff272520a8d3", "Reverse");
								break;
							case AccTaxRate.Types.Exempt:
								result = Res.GetString("9307e9e6-fd4f-4cde-9d4e-576a74852c39", "Exempt");
								break;
							case AccTaxRate.Types.Suspended:
								result = Res.GetString("01aa2b47-6263-4b5d-8069-ff2b685adfa2", "Suspended");
								break;
							case AccTaxRate.Types.NotReportable:
								result = Res.GetString("f9efe9be-6ff7-4cef-8dde-0605a3ee29bd", "N/A");
								break;
							default:
								result = Res.GetString("00810eae-5260-463b-8b3b-636b740d6a4e", "N/A");
								break;
						}
					}
				}
				return result;
			}
		}

		public ZString TaxDescriptionWithDescriptionOverride
		{
			get
			{
				ZString result = ZString.Empty;
				if (TaxSummaryLine != null)
				{
					var taxRateType = TaxSummaryLine.TaxRateType;
					if (DocTaxRate.IsRatedTax(taxRateType) || taxRateType == AccTaxRate.Types.CapitalRated)
					{
						result = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(TaxSummaryLine.TaxRate);
					}
					else if (taxRateType == AccTaxRate.ExtraTypes.VATRemittedByCustomer)
					{
						result = DocARInvoiceLineTaxDisplayExt.GetSPVTaxLabel(CurrentCompany.Country.Code);
					}
					else
					{
						result = AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(taxRateType);
					}

					if (result == ZString.Empty)
					{
						result = Res.GetString("1eb72a32-cc2d-4ac0-a249-f35c47a75ed2", "N/A");
					}
				}
				return result;
			}
		}

		public ZString ExtraTaxDescription
		{
			get
			{
				ZString result = ZString.Empty;
				if (TaxSummaryLine != null)
				{
					switch (TaxSummaryLine.TaxExtraRateType)
					{
						case AccTaxRate.ExtraTypes.QuebecQST:
						case AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase:
							result = Res.GetString("28ba7710-4958-4b1c-bde0-83426cf97ea6", "QST");
							break;
						case AccTaxRate.ExtraTypes.VATRetention:
						case AccTaxRate.ExtraTypes.VATRetentionFraction:
							result = Res.GetString("b248d491-6ddf-43bf-9318-50b5431448e1", "Retention");
							break;
						case AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax:
							result = Res.GetString("36c10b85-3a60-4db6-91f2-01ed3665d5ca", "CESS");
							break;
						case AccTaxRate.ExtraTypes.ChinaInputVATClaimed:
							result = Res.GetString("5b050e1f-6f7f-4f65-92b4-92493a139965", "Input");
							break;
						default:
							break;
					}
				}
				return result;
			}
		}

		public ZString ExtraTaxRate
		{
			get
			{
				ZString result = ZString.Empty;
				if (TaxSummaryLine != null)
				{
					result = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(TaxSummaryLine.TaxExtraRate);
				}
				return result;
			}
		}

		public ZDecimal TotalExcludeTaxAmountInOSCurrency
		{
			get { return TaxSummaryLine == null ? 0 : TaxSummaryLine.TotalExcludeTaxAmountInOSCurrency; }
		}

		public ZDecimal TotalExcludeTaxAmountInLocalCurrency
		{
			get { return TaxSummaryLine == null ? 0 : TaxSummaryLine.TotalExcludeTaxAmountInLocalCurrency; }
		}

		public ZDecimal TotalTaxAmountInOSCurrency
		{
			get { return TaxSummaryLine == null ? 0 : TaxSummaryLine.TotalTaxAmountInOSCurrency; }
		}

		public ZDecimal TotalTaxAmountInLocalCurrency
		{
			get { return TaxSummaryLine == null ? 0 : TaxSummaryLine.TotalTaxAmountInLocalCurrency; }
		}

		public ZDecimal TotalIncludeTaxAmountInOSCurrency
		{
			get { return TaxSummaryLine == null ? 0 : TaxSummaryLine.TotalIncludeTaxAmountInOSCurrency; }
		}

		public ZDecimal TotalIncludeTaxAmountInLocalCurrency
		{
			get { return TaxSummaryLine == null ? 0 : TaxSummaryLine.TotalIncludeTaxAmountInLocalCurrency; }
		}

		public ZDecimal TotalExtraTaxAmountInOSCurrency
		{
			get { return TaxSummaryLine == null ? 0 : TaxSummaryLine.TotalExtraTaxAmountInOSCurrency; }
		}

		public ZDecimal TotalExtraTaxAmountInLocalCurrency
		{
			get { return TaxSummaryLine == null ? 0 : TaxSummaryLine.TotalExtraTaxAmountInLocalCurrency; }
		}

		public ZBool IsSPVLine
		{
			get
			{
				return TaxSummaryLine == null || TaxSummaryLine.TaxRateType == AccTaxRate.ExtraTypes.VATRemittedByCustomer;
			}
		}

		public ZDecimal TotalTaxAmountExcludeExtraTaxAmountInOSCurrency
		{
			get
			{
				return TotalTaxAmountInOSCurrency - TotalExtraTaxAmountInOSCurrency;
			}
		}

		public ZDecimal TotalTaxAmountExcludeExtraTaxAmountInLocalCurrency
		{
			get
			{
				return TotalTaxAmountInLocalCurrency - TotalExtraTaxAmountInLocalCurrency;
			}
		}
	}
}
