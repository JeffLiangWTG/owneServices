using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;
using Enterprise.Rating.Business.RatingEnums;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <summary>
	///	Only for Freight charges, not Origin and Destination i.e. OriginRates, DestinationRates
	/// Origin/Destination charges are using PricingPageLineWrapperCollection
	/// Only used in 'Forwarding Landscape Pricing Page - Loose' that doesn't have container and MatchContainerRateClass
	/// </summary>
	sealed partial class ChargeableTableStrategy : BaseTableStrategy
	{
		public ChargeableTableStrategy(BusinessObjectFactory factory)
			: base(factory, new PricingPageRateLineFactory(EntryTypes.Freight))
		{
		}

		public override PricingPageTableRowWrapperCollection Extract(PricingPage pricingPage)
		{
			var result = new PricingPageTableRowWrapperCollection(Factory);

			foreach (var entry in pricingPage.RateEntries)
			{
				if (entry.IsFreightEntry())
				{
					var wrapper = Extract(pricingPage, entry);
					bool hasLines = wrapper.SubRows.Any() || wrapper.OtherCharges.Any();
					if (hasLines)
					{
						wrapper.Index = result.Count;
						result.Add(wrapper);
					}
				}
			}

			return result;
		}

		PricingPageTableRowWrapper Extract(PricingPage pricingPage, RateEntry rateEntry)
		{
			var helper = new QuotationLineListHelper();
			var groups = new List<SubRowGroup>();
			int setIndex = 0;

			var result = new PricingPageTableRowWrapper(new RateEntry[] { rateEntry }, Factory);

			var pricingPageLineSets = PricingPageRateLineFactory.LoadLineSets(pricingPage, new RateEntry[] { rateEntry }, exactMatch: true);
			foreach (var set in pricingPageLineSets)
			{
				var write = 0;
				for (var read = 0; read < set.Count; read++)
				{
					var line = set[read];
					bool lineViewAgentRates = line.ViewAgentRates;
					line.ViewAgentRates = pricingPage.ViewAgentRates;

					bool? isLocalClient = null;

					if (line?.Parent?.Parent != null
						&& pricingPage?.FirstRateEntry?.Parent != null
						&& line.Parent.Parent.PK != pricingPage.FirstRateEntry.Parent.PK
						&& pricingPage.FirstRateEntry.Parent.Header != null)
					{
						isLocalClient = pricingPage.FirstRateEntry.Parent.Header.IsLocalCountry;
					}

					var group = ExtractSubRowGroup(line, isLocalClient);
					if (group != null)
					{
						groups.Add(group);
					}
					else
					{
						// keep the line for "Other Charges"
						set[write] = line;
						write++;
					}

					line.ViewAgentRates = lineViewAgentRates;
				}

				if (write > 0)
				{
					if (write < set.Count)
					{
						// remove lines not explicitly kept above.
						set.RemoveRange(write, set.Count - write);
					}

					var list = helper.GetLines(set, pricingPage.ViewAgentRates);
					var lineIndex = 0;

					if (list.Count > 0)
					{
						setIndex++;

						foreach (QuotationLine line in list)
						{
							lineIndex++;
							result.OtherCharges.Add(new PricingPageLineWrapper(pricingPage, line, setIndex, lineIndex, Factory));
						}
					}
				}
			}

			CollapseGroups(ref groups);

			for (var i = 0; i < groups.Count; i++)
			{
				AddSubRows(result, groups[i], i + 1);
			}

			return result;
		}

		void CollapseGroups(ref List<SubRowGroup> groups)
		{
			// if the headings used by row A are a subset of the headings used by row B, then group them
			// together under the one set of headings.

			if (groups.Count > 1)
			{
				// sort in descending order of count to reduce the number of negative checks,
				// no point trying to merge A into B if A has more columns than B.
				groups = groups.OrderByDescending(group => group.Columns.Count).ToList();

				var write = 1;

				for (var read = 1; read < groups.Count; read++)
				{
					var group = groups[read];
					var absorbed = false;

					for (var potential = 0; potential < write; potential++)
					{
						if (groups[potential].TryAbsorb(group))
						{
							absorbed = true;
							break;
						}
					}

					if (!absorbed)
					{
						groups[write] = group;
						write++;
					}
				}

				if (write < groups.Count)
				{
					groups.RemoveRange(write, groups.Count - write);
				}
			}
		}

		void AddSubRows(PricingPageTableRowWrapper rowWrapper, SubRowGroup group, int captionGroup)
		{
			var headings = new ZString[group.Columns.Count];

			for (int i = 0; i < group.Columns.Count; i++)
			{
				headings[i] = group.Columns[i].ToString();
			}

			foreach (var subRow in group)
			{
				var subRowWrapper = new PricingPageTableSubRowWrapper(Factory);
				subRowWrapper.Charge = subRow.ChargeDescription == ZString.Empty
					? new CodeAndDescriptionWrapper(subRow.ChargeCode.AC_Code, new AccChargeCodeCollection(Factory, new ZQuery(), subRow.ChargeCode.AC_GC.ToGuid()), Factory)
					: new CodeAndDescriptionWrapper(subRow.ChargeCode.AC_Code, subRow.ChargeDescription, Factory);
				subRowWrapper.Currency = new CurrencyWrapper(subRow.Currency, Factory);
				subRowWrapper.UseOnlyActualWeightMeasure = subRow.UseOnlyActualWeightMeasure;
				subRowWrapper.ConversionFactor = new RatingConversionFactorWrapper(subRow.ConversionFactor, Factory);
				subRowWrapper.Index = rowWrapper.SubRows.Count;
				subRowWrapper.CaptionGroup = captionGroup;

				for (int i = 0; i < subRow.Count; i++)
				{
					subRowWrapper.Columns.Add(new PricingPageColumnWrapper(headings[i], subRow[i], Factory));
				}

				rowWrapper.SubRows.Add(subRowWrapper);
			}
		}

		SubRowGroup ExtractSubRowGroup(RateLine rateLine, bool? isLocalClient)
		{
			switch (rateLine.TL_RateCalculator)
			{
				case CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode:
				case CompanyTariffOrCostBasedCalculator.CostBasedCode:
					throw new DeveloperNotificationException("Cost or Company Tariff based calculators should not reach this place and should be all replaced by base rate lines by this moment.");

				case UnitCalculator.Code:
					return ExtractSubRowGroup(rateLine, (UnitCalculator)rateLine.Calculator, isLocalClient);

				case FlatCalculator.Code:
					return ExtractSubRowGroup(rateLine, (FlatCalculator)rateLine.Calculator, isLocalClient);

				case FlatPlusPerUnitCalculator.Code:
					return ExtractSubRowGroup(rateLine, (FlatPlusPerUnitCalculator)rateLine.Calculator, isLocalClient);

				case FirstPlusAdditionalCalculator.Code:
					return ExtractSubRowGroup(rateLine, (FirstPlusAdditionalCalculator)rateLine.Calculator, isLocalClient);

				case MinimumOrPerUnitCalculator.Code:
					return ExtractSubRowGroup(rateLine, (MinimumOrPerUnitCalculator)rateLine.Calculator, isLocalClient);

				case CombinedCalculator.Code:
					return ExtractSubRowGroup(rateLine, (CombinedCalculator)rateLine.Calculator, isLocalClient);

				default:
					return null;
			}
		}

		SubRowGroup ExtractSubRowGroup(RateLine rateLine, UnitCalculator calculator, bool? isLocalClient)
		{
			if (UnitValid(rateLine))
			{
				var columns = new List<Column>();
				var values = new List<DocAmount>();

				SetValue(columns, values, rateLine, ColumnType.Unit, 0m, calculator.PerUnit);

				return new SubRowGroup(rateLine, columns.ToArray(), values.ToArray(), isLocalClient);
			}
			else
			{
				return null;
			}
		}

		SubRowGroup ExtractSubRowGroup(RateLine rateLine, FlatCalculator calculator, bool? isLocalClient)
		{
			var columns = new List<Column>();
			var values = new List<DocAmount>();

			SetValue(columns, values, rateLine, ColumnType.Flat, calculator.BaseRate);

			return new SubRowGroup(rateLine, columns.ToArray(), values.ToArray(), isLocalClient);
		}

		SubRowGroup ExtractSubRowGroup(RateLine rateLine, FlatPlusPerUnitCalculator calculator, bool? isLocalClient)
		{
			if (UnitValid(rateLine))
			{
				var columns = new List<Column>();
				var values = new List<DocAmount>();

				SetValueIfNonZero(columns, values, rateLine, ColumnType.Flat, calculator.BaseRate);
				SetValue(columns, values, rateLine, ColumnType.Unit, 0m, calculator.PerUnit);

				return new SubRowGroup(rateLine, columns.ToArray(), values.ToArray(), isLocalClient);
			}
			else
			{
				return null;
			}
		}

		SubRowGroup ExtractSubRowGroup(RateLine rateLine, FirstPlusAdditionalCalculator calculator, bool? isLocalClient)
		{
			if (UnitValid(rateLine))
			{
				var columns = new List<Column>();
				var values = new List<DocAmount>();

				SetValue(columns, values, rateLine, ColumnType.First, calculator.First);
				SetValue(columns, values, rateLine, ColumnType.Additional, calculator.Additional);

				return new SubRowGroup(rateLine, columns.ToArray(), values.ToArray(), isLocalClient);
			}
			else
			{
				return null;
			}
		}

		SubRowGroup ExtractSubRowGroup(RateLine rateLine, MinimumOrPerUnitCalculator calculator, bool? isLocalClient)
		{
			if (UnitValid(rateLine))
			{
				var columns = new List<Column>();
				var values = new List<DocAmount>();

				SetValueIfNonZero(columns, values, rateLine, ColumnType.Min, calculator.Minimum);
				SetValue(columns, values, rateLine, ColumnType.Unit, 0m, calculator.PerUnit);

				return new SubRowGroup(rateLine, columns.ToArray(), values.ToArray(), isLocalClient);
			}
			else
			{
				return null;
			}
		}

		SubRowGroup ExtractSubRowGroup(RateLine rateLine, CombinedCalculator calculator, bool? isLocalClient)
		{
			if (!UnitValid(rateLine))
			{
				return null;
			}
			else
			{
				var columns = new List<Column>();
				var values = new List<DocAmount>();

				SetValueIfNonZero(columns, values, rateLine, ColumnType.Min, calculator.Minimum);
				SetValueIfNonZero(columns, values, rateLine, ColumnType.Base, calculator.BaseRate);

				if (calculator.IsSliding())
				{
					foreach (RateLineItem item in rateLine.RateLineItems)
					{
						if (item.RateOperatorIsPlus())
						{
							SetValue(columns, values, rateLine, ColumnType.Plus, item.TM_Break, item.TM_RelevantValue);
						}
						else if (item.RateOperatorIsMinus())
						{
							SetValue(columns, values, rateLine, ColumnType.Minus, item.TM_Break, item.TM_RelevantValue);
						}
					}
				}
				else
				{
					SetValue(columns, values, rateLine, ColumnType.Unit, 0m, calculator.PerUnit);
				}

				SetValueIfNonZero(columns, values, rateLine, ColumnType.Max, calculator.Maximum);

				return new SubRowGroup(rateLine, columns.ToArray(), values.ToArray(), isLocalClient);
			}
		}

		void SetValue(List<Column> columns, List<DocAmount> values, RateLine rateLine, ColumnType type, ZDecimal valueBreak, ZDecimal value)
		{
			columns.Add(new Column(type, valueBreak, rateLine.TL_WeightVolume));
			values.Add(DocAmount.Create(value, rateLine.Currency.Decimals));
		}
		void SetValue(List<Column> columns, List<DocAmount> values, RateLine rateLine, ColumnType type, ZDecimal value)
		{
			columns.Add(new Column(type));
			values.Add(DocAmount.Create(value, rateLine.Currency.Decimals));
		}

		void SetValueIfNonZero(List<Column> columns, List<DocAmount> values, RateLine rateLine, ColumnType type, ZDecimal value)
		{
			if (value != 0)
			{
				SetValue(columns, values, rateLine, type, value);
			}
		}

		bool UnitValid(RateLine rateLine) => rateLine.TL_WeightVolumeMultiple == 1m || rateLine.TL_WeightVolumeMultiple == 0;
	}
}
