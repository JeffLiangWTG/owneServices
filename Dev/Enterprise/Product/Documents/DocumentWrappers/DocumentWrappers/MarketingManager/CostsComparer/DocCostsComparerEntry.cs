using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCostsComparerEntry : DocEntryQuotation
	{
		DocCostsComparerEntry(PricingPage page, CostsComparerEntry entry, BusinessObjectFactory factoryToWrap)
			: base(page, factoryToWrap)
		{
			comparerEntry = entry;
		}

		public static DocCostsComparerEntry New(CostsComparerEntry entry, BusinessObjectFactory factoryToWrap)
		{
			if (entry == null)
			{
				return null;
			}
			else
			{
				PricingPage page = new PricingPage(entry.Entry, entry.Factory, PricingPageStyle.Standard);
				return new DocCostsComparerEntry(page, entry, factoryToWrap);
			}
		}

		public ZGuid Identifier
		{
			get { return FirstEntry != null ? FirstEntry.PK : ZGuid.Empty; }
		}

		public ZString SvcProvider
		{
			get
			{
				RateEntry entry = FirstEntry;

				return entry != null && !entry.Organisation.IsEmpty
					? string.Format("{0}{1}({2})", entry.Parent.Header.OH_FullName, System.Environment.NewLine, entry.Organisation)
					: string.Empty;
			}
		}

		public ZString Container
		{
			get
			{
				RateEntry entry = FirstEntry;

				if (entry != null)
				{
					RefContainer container = Factory.Load<RefContainer>(entry.TI_RC);
					return container != null ? container.RC_Code : ZString.Empty;
				}

				return ZString.Empty;
			}
		}

		public ZString Commodity
		{
			get
			{
				RateEntry entry = FirstEntry;
				return entry != null ? entry.TI_RH_NKCommodityCode : ZString.Empty;
			}
		}

		public ZString WeightVolume
		{
			get
			{
				RateEntry entry = FirstEntry;
				return entry != null ? entry.Unit : ZString.Empty;
			}
		}

		public new ZString Provider
		{
			get
			{
				ZString result = "";
				RateEntry entry = FirstEntry;

				if (entry != null)
				{
					result = entry.TransportProvider?.OH_FullName ?? "";
					var providerCode = entry.TransportProviderCarrierCode;
					if (!providerCode.IsEmpty)
					{
						result += " (" + providerCode.Trim() + ")";
					}
				}
				return result;
			}
		}

		public new ZString TransitTime
		{
			get
			{
				RateEntry entry = FirstEntry;
				if (entry != null && !entry.TI_TransitTime.IsEmpty)
				{
					return base.TransitTime;
				}

				return "";
			}
		}

		public new ZString Frequency
		{
			get
			{
				RateEntry entry = FirstEntry;
				if (entry != null && !entry.TI_FrequencyUnit.IsEmpty)
				{
					return base.Frequency;
				}
				return "";
			}
		}

		public ZString SummaryColumn1
		{
			get { return GetValue(1); }
		}

		public ZString SummaryColumn2
		{
			get { return GetValue(2); }
		}

		public ZString SummaryColumn3
		{
			get { return GetValue(3); }
		}

		public ZString SummaryColumn4
		{
			get { return GetValue(4); }
		}

		public ZString SummaryColumn5
		{
			get { return GetValue(5); }
		}

		public ZString SummaryColumn6
		{
			get { return GetValue(6); }
		}

		public ZString SummaryColumn7
		{
			get { return GetValue(7); }
		}

		public ZString SummaryColumn8
		{
			get { return GetValue(8); }
		}

		public ZString SummaryColumn9
		{
			get { return GetValue(9); }
		}

		public ZString SummaryColumn10
		{
			get { return GetValue(10); }
		}

		#region Implementation

		ZString GetValue(int columnNumber)
		{
			return comparerEntry != null ? (ZString)comparerEntry.GetSummaryColumn(columnNumber, typeof(ZString)).ToString().Replace("/", System.Environment.NewLine) : ZString.Empty;
		}

		internal ZString GetRateLineColumnValue(int columnNumber, RateLine rateLine)
		{
			ZString result = ZString.Empty;

			if (comparerEntry != null && comparerEntry.Parent != null && comparerEntry.Parent.SummaryItems.Count >= columnNumber)
			{
				foreach (ChargesSummaryItem chargesSummaryItem in comparerEntry.GetChargesSummaryItems(rateLine))
				{
					ChargesSummaryItem comparerSummaryItem = comparerEntry.Parent.SummaryItems.Keys[columnNumber - 1];
					bool isSameTypeAndBreak = chargesSummaryItem.Type == comparerSummaryItem.Type && chargesSummaryItem.Break == comparerSummaryItem.Break;
					bool isGenericUnitType = chargesSummaryItem.Type == Calculator.Items.Operator.UNT && (comparerSummaryItem.Type == Calculator.Items.Operator.Plus || comparerSummaryItem.Type == Calculator.Items.Operator.Minus);

					if (isSameTypeAndBreak || isGenericUnitType)
					{
						result = chargesSummaryItem.GetStringValue(comparerEntry.Parent.CurrencyObj);
						break;
					}
				}
			}

			return result.Replace("/", System.Environment.NewLine);
		}

		readonly CostsComparerEntry comparerEntry;

		#endregion
	}
}
