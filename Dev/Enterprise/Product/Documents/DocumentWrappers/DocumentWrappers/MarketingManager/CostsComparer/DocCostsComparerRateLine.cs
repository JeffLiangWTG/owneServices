using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Quotation;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCostsComparerRateLine : DocRateLineItem
	{
		protected DocCostsComparerRateLine(QuotationLine quotationLine, DocQuotation quotation, BusinessObjectFactory factoryToWrap)
			: base(quotationLine, quotation, factoryToWrap) { }

		internal static new DocCostsComparerRateLine New(QuotationLine quotationLine, DocQuotation quotation, BusinessObjectFactory factoryToWrap)
		{
			return quotationLine != null ? new DocCostsComparerRateLine(quotationLine, quotation, factoryToWrap) : null;
		}

		public static new DocCostsComparerRateLine New(QuotationLine quotationLine, BusinessObjectFactory factoryToWrap)
		{
			return new DocCostsComparerRateLine(quotationLine, null, factoryToWrap);
		}

		internal RateLine Master
		{
			get { return QuotationLine != null ? QuotationLine.Master : null; }
		}

		#region Warnings

		internal bool HasCNwarning
		{
			get
			{
				if (Master.HasRowWarnings)
				{
					foreach (INotification warning in Master.RowWarnings)
					{
						if (warning.Message == CostsComparerEntry.GetComplexNatureRowWarning())
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		internal bool HasExRateWarning
		{
			get
			{
				if (Master.HasRowWarnings)
				{
					foreach (INotification warning in Master.RowWarnings)
					{
						if (warning.Message == CostsComparerEntry.GetNoBuyExchangeRateWarning(Master))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		internal bool HasUDwarnings
		{
			get
			{
				if (Master.HasRowWarnings)
				{
					foreach (INotification warning in Master.RowWarnings)
					{
						if (warning.Message == CostsComparerEntry.GetUnitIsDifferenToFreightChargeRowWarning())
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		#endregion

		#region Properties

		public ZString WarningStar
		{
			get { return HasCNwarning ? "*" : HasExRateWarning ? "**" : HasUDwarnings ? "***" : ""; }
		}

		public DocCostsComparerEntry ComparerEntry
		{
			get { return (Quotation as DocCostsComparerEntry) ?? DocCostsComparerEntry.New(null, Factory); }
		}

		public ZString WeightVolumeMultipleAsString
		{
			get { return Master != null ? Master.UnitMultipleAsString : ZString.Empty; }
		}

		public ZString WeightVolumeUnits
		{
			get { return Master != null ? Master.TL_WeightVolume : ZString.Empty; }
		}

		public ZBool UseOnlyActualWeightMeasure
		{
			get { return Master.UseOnlyActualWeightMeasure; }
		}

		protected override ZString GetCurrencyCore()
		{
			return Master != null ? Master.TL_RX_NKCurrency : ZString.Empty;
		}

		#endregion

		#region RateColumns

		ZString GetValue(int columnNumber)
		{
			if (ComparerEntry != null)
			{
				return ComparerEntry.GetRateLineColumnValue(columnNumber, QuotationLine.Master);
			}
			return ZString.Empty;
		}

		public ZString RateColumn1
		{
			get { return GetValue(1); }
		}

		public ZString RateColumn2
		{
			get { return GetValue(2); }
		}

		public ZString RateColumn3
		{
			get { return GetValue(3); }
		}

		public ZString RateColumn4
		{
			get { return GetValue(4); }
		}

		public ZString RateColumn5
		{
			get { return GetValue(5); }
		}

		public ZString RateColumn6
		{
			get { return GetValue(6); }
		}

		public ZString RateColumn7
		{
			get { return GetValue(7); }
		}

		public ZString RateColumn8
		{
			get { return GetValue(8); }
		}

		public ZString RateColumn9
		{
			get { return GetValue(9); }
		}

		public ZString RateColumn10
		{
			get { return GetValue(10); }
		}

		#endregion
	}
}
