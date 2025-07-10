using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.LandedCosting.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocLandedCostHistory : DocBaseWrapper
	{
		DocLandedCostHistory(LandedCostHistory lCHistory, BusinessObjectFactory factoryToWrap)
			: base(lCHistory, factoryToWrap)
		{
		}

		public static DocLandedCostHistory New(LandedCostHistory lCHistory, BusinessObjectFactory factoryForWrapper)
		{
			if (lCHistory == null)
			{
				return null;
			}
			else
			{
				return new DocLandedCostHistory(lCHistory, factoryForWrapper);
			}
		}

		#region ZDecimal Fields

		public ZDecimal DutyPercent
		{
			get { return LCHistory.LH_DutyPercent; }
		}

		public ZDecimal MarkUpPercentage1
		{
			get { return LCHistory.EffectiveMarkUpPercentage1; }
		}

		public ZDecimal MarkUpPercentage2
		{
			get { return LCHistory.EffectiveMarkUpPercentage2; }
		}

		public ZDecimal MarkUpPercentage3
		{
			get { return LCHistory.EffectiveMarkUpPercentage3; }
		}

		public ZDecimal LandedCostPercentage
		{
			get { return ZArchitecture.Core.Utilities.Round(LCHistory.LandedCostPercentage, 2); }
		}

		public ZDecimal LandedTotalCostPercentage
		{
			get { return ZArchitecture.Core.Utilities.Round(LCHistory.LandedTotalCostPercentage, 2); }
		}

		#endregion

		#region ZString Fields

		public ZString ProductCode
		{
			get { return LCHistory.ProductCode; }
		}

		public ZString ProductDepartment
		{
			get { return LCHistory.ProductDepartment; }
		}

		public ZString ProductDivision
		{
			get { return LCHistory.ProductDivision; }
		}

		public ZString LineDescription
		{
			get { return LCHistory.LineDescription; }
		}

		public ZString OrderNumber
		{
			get { return LCHistory.OrderNumber; }
		}

		public ZInt OrderLineNumber
		{
			get { return LCHistory.OrderLineNumber; }
		}

		public ZDecimal InvoiceQuantity
		{
			get { return LCHistory.InvoiceQuantity; }
		}

		public ZString InvoiceUQ
		{
			get { return LCHistory.InvoiceUQ; }
		}

		public ZDecimal CustomsQuantity
		{
			get { return LCHistory.CustomsQuantity; }
		}

		public ZString CustomsUQ
		{
			get { return LCHistory.CustomsUQ; }
		}

		public ZString TariffNumber
		{
			get { return LCHistory.TariffNumber; }
		}

		public ZString CountryOfOriginCode
		{
			get { return LCHistory.CountryOfOriginCode; }
		}

		public ZDecimal Weight
		{
			get { return LCHistory.Weight; }
		}

		public ZString WeightUQ
		{
			get { return LCHistory.WeightUQ; }
		}

		public ZDecimal Volume
		{
			get { return LCHistory.Volume; }
		}

		public ZString VolumeUQ
		{
			get { return LCHistory.VolumeUQ; }
		}

		public ZString InvoiceCurrencyCode
		{
			get { return LCHistory.InvoiceCurrencyCode; }
		}

		public ZDecimal LandedCostingExRateFallBackToJobExRate
		{
			get { return LCHistory.LandedCostingExRateFallingBackToJobInvoicingForInvoiceCurrency; }
		}

		public ZString InvoiceNumberSupplierNameGroupByString
		{
			get { return LCHistory.InvoiceNumberSupplierNameGroupByString; }
		}

		public ZString SupplierName
		{
			get { return LCHistory.SupplierName; }
		}

		public ZString InvoiceNumber
		{
			get { return LCHistory.InvoiceNumber; }
		}
		public ZDecimal LinePriceInLocalCurrency
		{
			get { return RoundingHelper.Round(LCHistory.InvoiceCostInLocalCurrency, LCHistory.LocalCurrencyDecimals); }
		}

		public ZDecimal LinePriceInInvoiceCurrency
		{
			get { return RoundingHelper.Round(LCHistory.LinePriceInInvoiceCurrency, LCHistory.LocalCurrencyDecimals); }
		}

		public ZDecimal EntryGSTVATAmount
		{
			get { return RoundingHelper.Round(LCHistory.EntryGSTVATAmount, LCHistory.LocalCurrencyDecimals); }
		}

		public ZDecimal UnitPriceInInvoiceCurrency
		{
			get { return RoundingHelper.Round(LCHistory.UnitPriceInInvoiceCurrency, LandedCostHistory.PerUnitDecimals); }
		}

		public ZDecimal UnitPriceInLocalCurrency
		{
			get { return RoundingHelper.Round(LCHistory.UnitPriceInLocalCurrency, LandedCostHistory.PerUnitDecimals); }
		}

		public ZDecimal LandedCostGroup1
		{
			get
			{
				return RoundingHelper.Round(LCHistory.RoundedGroup1, LCHistory.LocalCurrencyDecimals);
			}
		}

		public ZDecimal LandedCostGroup2
		{
			get
			{
				return RoundingHelper.Round(LCHistory.RoundedGroup2, LCHistory.LocalCurrencyDecimals);
			}
		}

		public ZDecimal LandedCostGroup3
		{
			get
			{
				return RoundingHelper.Round(LCHistory.RoundedGroup3, LCHistory.LocalCurrencyDecimals);
			}
		}

		public ZDecimal LandedCostGroup4
		{
			get
			{
				return RoundingHelper.Round(LCHistory.RoundedGroup4, LCHistory.LocalCurrencyDecimals);
			}
		}

		public ZDecimal LandedCostGroup5
		{
			get
			{
				return RoundingHelper.Round(LCHistory.RoundedGroup5, LCHistory.LocalCurrencyDecimals);
			}
		}

		public ZDecimal LandedCostGroup6
		{
			get
			{
				return RoundingHelper.Round(LCHistory.RoundedGroup6, LCHistory.LocalCurrencyDecimals);
			}
		}

		public ZDecimal LandedCostGroupMisc
		{
			get
			{
				return RoundingHelper.Round(LCHistory.RoundedGroupMisc, LCHistory.LocalCurrencyDecimals);
			}
		}

		public ZDecimal CustomsValue
		{
			get { return RoundingHelper.Round(LCHistory.CustomsValue, LCHistory.LocalCurrencyDecimals); }
		}

		public ZDecimal CustomsDisbursementCharges
		{
			get
			{
				return RoundingHelper.Round(LCHistory.CustomsDisbursementCharges, LCHistory.LocalCurrencyDecimals);
			}
		}

		public DocCustomsDisbursementChargeCollection DocCustomsDisbursementCharges
		{
			get
			{
				if (fCustomsDisbursementCharges == null)
				{
					fCustomsDisbursementCharges = new DocCustomsDisbursementChargeCollection(Factory);
					var customsChargeLCItemSettings = LCHistory.LCHeader?.CustomsChargeLCItemSettings;
					if (customsChargeLCItemSettings != null)
					{
						foreach (var setting in customsChargeLCItemSettings)
						{
							fCustomsDisbursementCharges.Add(DocCustomsDisbursementCharge.New(new CustomsDisbursementCharge(setting, LCHistory.GetRoundedLineValue(setting.CostType)), Factory));
						}
					}
				}
				return fCustomsDisbursementCharges;
			}
		}
		DocCustomsDisbursementChargeCollection fCustomsDisbursementCharges;

		public ZDecimal PerUnitCustomsDisbursementCharges
		{
			get { return RoundingHelper.Round(LCHistory.RoundedPerUnitCustomsDisbursementCharges, LandedCostHistory.PerUnitDecimals); }
		}

		public ZDecimal TotalLandingCost
		{
			get
			{
				return RoundingHelper.Round(LCHistory.TotalLandingCost, LCHistory.LocalCurrencyDecimals);
			}
		}

		public ZDecimal PerUnitLandingCost
		{
			get { return RoundingHelper.Round(LCHistory.RoundedPerUnitLandingCost, LandedCostHistory.PerUnitDecimals); }
		}

		public ZDecimal TotalCost
		{
			get
			{
				return RoundingHelper.Round(LCHistory.TotalCost, LCHistory.LocalCurrencyDecimals);
			}
		}

		public ZDecimal TotalCostWithMarkup1Applied
		{
			get
			{
				return RoundingHelper.Round(LCHistory.TotalCostWithMarkup1Applied, LCHistory.LocalCurrencyDecimals);
			}
		}

		public ZDecimal ExGSTSellPrice1To4DP
		{
			get { return RoundingHelper.Round(LCHistory.SellPrice1ExGST, 4); }
		}

		public ZDecimal PerUnitTotalCost
		{
			get { return RoundingHelper.Round(LCHistory.RoundedPerUnitTotalCost, LandedCostHistory.PerUnitDecimals); }
		}

		public ZDecimal ExGSTSellPrice1
		{
			get { return RoundingHelper.Round(LCHistory.RoundedSellPrice1ExGST, LCHistory.LocalCurrencyDecimals); }
		}

		public ZDecimal ExGSTSellPrice2
		{
			get { return RoundingHelper.Round(LCHistory.RoundedSellPrice2ExGST, LCHistory.LocalCurrencyDecimals); }
		}

		public ZDecimal ExGSTSellPrice3
		{
			get { return RoundingHelper.Round(LCHistory.RoundedSellPrice3ExGST, LCHistory.LocalCurrencyDecimals); }
		}

		public ZDecimal IncGSTSellPrice1
		{
			get { return RoundingHelper.Round(LCHistory.RoundedSellPrice1IncGST, LCHistory.LocalCurrencyDecimals); }
		}

		public ZDecimal IncGSTSellPrice2
		{
			get { return RoundingHelper.Round(LCHistory.RoundedSellPrice2IncGST, LCHistory.LocalCurrencyDecimals); }
		}

		public ZDecimal IncGSTSellPrice3
		{
			get { return RoundingHelper.Round(LCHistory.RoundedSellPrice3IncGST, LCHistory.LocalCurrencyDecimals); }
		}

		#endregion

		#region Implementation

		RoundingHelper RoundingHelper
		{
			get
			{
				if (fRoundingHelper == null)
				{
					fRoundingHelper = LCHistory.LCHeader.RoundingHelper;
				}
				return fRoundingHelper;
			}
		}
		RoundingHelper fRoundingHelper;

		internal LandedCostHistory LCHistory
		{
			get { return (LandedCostHistory)WrappedObject; }
		}

		public DocBaseJobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					BaseJobComInvoiceLine wrapperObject = LCHistory.UltimateDistributee as BaseJobComInvoiceLine;
					if (wrapperObject != null)
					{
						invoiceLine = DocBaseJobComInvoiceLine.New(wrapperObject, Factory);
					}
				}
				return invoiceLine;
			}
		}
		DocBaseJobComInvoiceLine invoiceLine;

		#endregion
	}
}
