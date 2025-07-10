using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class ChargeSummaryLine
	{
		public ChargeSummaryLine(DocTaxRate chargeTaxRate, ZDecimal taxRateAmount_Raw, ZDecimal taxExtraRateAmount, ZString chargeDescription,
								ZDecimal totalChargeAmountExcludeTaxInOSCurrency, ZDecimal totalChargeAmountExcludeTaxInLocalCurrency,
								ZDecimal totalChargeAmountInOSCurrency, ZDecimal totalChargeAmountInLocalCurrency,
								ZDecimal totalChargeTaxAmountInOSCurrency, ZDecimal totalChargeTaxAmountInLocalCurrency)
		{
			this.ChargeTaxRate = chargeTaxRate;
			this.TaxRateAmount_Raw = taxRateAmount_Raw;
			this.TaxExtraRateAmount = taxExtraRateAmount;
			this.ChargeDescription = chargeDescription;
			this.TotalChargeAmountExcludeTaxInOSCurrency = totalChargeAmountExcludeTaxInOSCurrency;
			this.TotalChargeAmountExcludeTaxInLocalCurrency = totalChargeAmountExcludeTaxInLocalCurrency;
			this.TotalChargeAmountInOSCurrency = totalChargeAmountInOSCurrency;
			this.TotalChargeAmountInLocalCurrency = totalChargeAmountInLocalCurrency;
			this.TotalChargeTaxAmountInOSCurrency = totalChargeTaxAmountInOSCurrency;
			this.TotalChargeTaxAmountInLocalCurrency = totalChargeTaxAmountInLocalCurrency;
		}

		public DocTaxRate ChargeTaxRate
		{ get; }

		public ZDecimal TaxRateAmount_Raw
		{ get; }

		public ZDecimal TaxExtraRateAmount
		{ get; }

		public ZString ChargeDescription
		{ get; }

		public ZDecimal TotalChargeAmountExcludeTaxInOSCurrency
		{ get; }

		public ZDecimal TotalChargeAmountExcludeTaxInLocalCurrency
		{ get; }

		public ZDecimal TotalChargeAmountInOSCurrency
		{ get; }

		public ZDecimal TotalChargeAmountInLocalCurrency
		{ get; }

		public ZDecimal TotalChargeTaxAmountInOSCurrency
		{ get; }

		public ZDecimal TotalChargeTaxAmountInLocalCurrency
		{ get; }
	}

	public class DocChargeSummaryLine : DocBaseWrapper
	{
		protected DocChargeSummaryLine(ChargeSummaryLine chargeSummaryLine, BusinessObjectFactory factoryToWrap)
			: base(chargeSummaryLine, factoryToWrap)
		{
			this.ChargeSummaryLine = chargeSummaryLine;
		}

		internal readonly ChargeSummaryLine ChargeSummaryLine;

		public static DocChargeSummaryLine New(ChargeSummaryLine chargeSummaryLine, BusinessObjectFactory factoryToWrap)
		{
			DocChargeSummaryLine result = null;
			if (chargeSummaryLine != null)
			{
				result = new DocChargeSummaryLine(chargeSummaryLine, factoryToWrap);
			}
			return result;
		}

		public ZString ChargeDescription
		{
			get
			{
				return ChargeSummaryLine.ChargeDescription;
			}
		}

		public ZString ChargeMainTaxDescription
		{
			get
			{
				ZString result = ZString.Empty;
				if (ChargeSummaryLine.ChargeTaxRate != null)
				{
					var taxType = ChargeSummaryLine.ChargeTaxRate.Type;
					if (DocTaxRate.IsRatedTax(taxType) || taxType == AccTaxRate.Types.CapitalRated)
					{
						result = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(ChargeSummaryLine.TaxRateAmount_Raw);
					}
					else
					{
						result = AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(taxType);
					}
				}
				return result;
			}
		}

		public ZString ChargeExtraTaxDescription
		{
			get
			{
				ZString result = ZString.Empty;
				if (ChargeSummaryLine.ChargeTaxRate != null)
				{
					switch (ChargeSummaryLine.ChargeTaxRate.ExtraType)
					{
						case AccTaxRate.ExtraTypes.QuebecQST:
						case AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase:
							result = Res.GetString("0a2a64b3-6140-4118-85e3-6480eb696f32", "QST");
							break;
						case AccTaxRate.ExtraTypes.VATRetention:
						case AccTaxRate.ExtraTypes.VATRetentionFraction:
							result = Res.GetString("437506d2-d36c-4968-975b-b22aa49d1038", "Retention");
							break;
						case AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax:
							result = Res.GetString("f7f320fa-2976-4420-9f0a-cf4973995993", "CESS");
							break;
						case AccTaxRate.ExtraTypes.ChinaInputVATClaimed:
							result = Res.GetString("cecf277d-813b-4987-8910-4487e3483d6c", "Input");
							break;
						default:
							break;
					}
				}
				return result;
			}
		}

		public ZString ChargeExtraTaxRate
		{
			get
			{
				ZString result = ZString.Empty;
				if (ChargeSummaryLine.ChargeTaxRate != null)
				{
					result = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(ChargeSummaryLine.TaxExtraRateAmount);
				}
				return result;
			}
		}

		public ZDecimal TotalChargeAmountExcludeTaxInOSCurrency
		{
			get { return ChargeSummaryLine.TotalChargeAmountExcludeTaxInOSCurrency; }
		}

		public ZDecimal TotalChargeAmountExcludeTaxInLocalCurrency
		{
			get { return ChargeSummaryLine.TotalChargeAmountExcludeTaxInLocalCurrency; }
		}

		public ZDecimal TotalChargeAmountInOSCurrency
		{
			get { return ChargeSummaryLine.TotalChargeAmountInOSCurrency; }
		}

		public ZDecimal TotalChargeAmountInLocalCurrency
		{
			get { return ChargeSummaryLine.TotalChargeAmountInLocalCurrency; }
		}

		public ZDecimal TotalChargeTaxAmountInOSCurrency
		{
			get { return ChargeSummaryLine.TotalChargeTaxAmountInOSCurrency; }
		}

		public ZDecimal TotalChargeTaxAmountInLocalCurrency
		{
			get { return ChargeSummaryLine.TotalChargeTaxAmountInLocalCurrency; }
		}
	}
}
