using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;
using Enterprise.ResourceStrings.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <summary>
	/// Quotation Forwarding Compact Pricing Page has the following layout:
	/// Origin		Destination		Tranship-Port		Container-Type		Charge1		Charge2
	/// ---------------------------------------------------------------------------------------
	/// Sydney		Los Angeles							20GP				10			11			=> This is subrow, and combined with next subrow to become row. Each row have subrow/s and each subrow have column/s
	///													40GP				20			21			=> This is subrow
	///																		^			^
	///																		|-----------|------------- This is column[n]
	///													Other Charges
	///														RateLine Info 1	xx
	///														RateLIne Info 2 xx
	/// </summary>
	sealed partial class CompactTableStrategy : BaseTableStrategy
	{
		public CompactTableStrategy(BusinessObjectFactory factory, PricingPageRateLineFactory pricingPageLineSetFactory)
			: base(factory, pricingPageLineSetFactory)
		{
			isFreight = pricingPageLineSetFactory.EntryType == Rating.Business.RatingEnums.EntryTypes.Freight;
			formatter = new CompactFormatter();
		}

		public override PricingPageTableRowWrapperCollection Extract(PricingPage pricingPage)
		{
			var columns = new ColumnKeyList();
			var result = new PricingPageTableRowWrapperCollection(Factory);
			var comparer = BasePricingPageRateLineListComparer.FromPage(pricingPage);
			var strategy = new GroupStrategy();

			var rateEntriesToGroup = GetRateEntriesToGroup(pricingPage);
			var groupedSimilarRateEntriesList = GroupSimilarRateEntries(rateEntriesToGroup);
			foreach (var similarRateEntries in groupedSimilarRateEntriesList)
			{
				strategy.Reset();
				var pricingPageTableRowWrapper = CreateTableRow(strategy, columns, comparer, pricingPage, similarRateEntries.ToArray());
				pricingPageTableRowWrapper.Index = result.Count;
				result.Add(pricingPageTableRowWrapper);
			}

			if (columns.Count > 0)
			{
				FillInMissingColumns(result, columns);
			}
			else
			{
				result.RemoveAndDeleteAll();
			}

			return result;
		}

		/// <summary>
		/// FRT: get Page.Entries that are freight, the related rates are retrieved in CreateTableRow through PricingPageRateLineFactory.GetRelatedLineSets with exactMatch.
		/// ORG/DST: get related rates without exactMatch because if quotation only has FRT, we still need to get related ORG/DST rates.
		/// </summary>
		RateEntry[] GetRateEntriesToGroup(PricingPage pricingPage)
			=> isFreight
				? pricingPage.RateEntries.Where(rateEntry => rateEntry.IsFreightEntry()).ToArray()
				: PricingPageRateLineFactory.GetRelatedEntries(pricingPage, pricingPage.RateEntries, exactMatch: false).Where(rateEntry => rateEntry.RateLines.Any()).ToArray();

		PricingPageTableRowWrapper CreateTableRow(GroupStrategy strategy, ColumnKeyList columns, BasePricingPageRateLineListComparer comparer, PricingPage pricingPage, RateEntry[] similarRateEntries)
		{
			var pricingPageTableRowWrapper = new PricingPageTableRowWrapper(similarRateEntries, Factory);
			var otherCharges = new List<PricingPageRateLineList>();

			//All similarRates have been found in Extract > GroupSimilarRateEntries.
			//This part is to print individual group with similarRates hence we should GetRelatedEntries with exactMatch to avoid overwriting with similarRates that exist in other groups.
			var pricingPageLineSetDict = PricingPageRateLineFactory.LoadLineSets(strategy, pricingPage, similarRateEntries, exactMatch: true);
			foreach (var pricingPageLineSetPair in pricingPageLineSetDict)
			{
				var subRow = CreateTableSubRow
				(
					pricingPageLineSetPair.Key,
					columns,
					pricingPageLineSetPair.Value,
					otherCharges
				);

				subRow.Index = pricingPageTableRowWrapper.SubRows.Count;
				pricingPageTableRowWrapper.SubRows.Add(subRow);
			}

			if (pricingPageTableRowWrapper.SubRows.Count == 0)
			{
				pricingPageTableRowWrapper.SubRows.Add(new PricingPageTableSubRowWrapper(Factory));
			}

			PricingPageRateLineList.RemoveDuplicateRateLineInSets(otherCharges);

			if (comparer != null)
			{
				otherCharges.Sort(comparer);
			}

			AddOtherCharges(pricingPageTableRowWrapper, pricingPage, otherCharges);

			return pricingPageTableRowWrapper;
		}

		/// <summary>
		/// Create PricingPageTableSubRowWrapper to be shown on Excel template.
		/// </summary>
		/// <param name="split">subRow identification i.e. ContainerType</param>
		[CodeStringFinderHint(typeof(PricingPageTableCellFormatter), "TryFormat")]
		PricingPageTableSubRowWrapper CreateTableSubRow(string split, ColumnKeyList columns, List<PricingPageRateLineList> sets, List<PricingPageRateLineList> otherCharges)
		{
			var subRow = new PricingPageTableSubRowWrapper(Factory);
			var formattedValues = DocAmount.Create(count: formatter.RequiredColumnCount);
			var values = new List<DocAmount>();

			foreach (var pricingPageLineSet in sets)
			{
				int write = 0;

				// don't use foreach because it cause error 'Collection was modified; enumeration operation may not execute.'
				for (int read = 0; read < pricingPageLineSet.Count; read++)
				{
					var rateLine = pricingPageLineSet[read];
					var columnKey = ColumnKey.New(rateLine);

					// rateLine that cannot be shown in column would go to otherCharges section i.e. shown per line
					if (columnKey == null || !formatter.TryFormat(rateLine, formattedValues))
					{
						pricingPageLineSet[write] = rateLine;
						write++;
					}

					InsertValue(values, columns.GetIndex(columnKey), formattedValues[PricingPageTableCellFormatter.UnitColumn]);
				}

				// if rateLines cannot be shown in the column, then add it to otherCharges section i.e. shown per line
				if (write > 0)
				{
					if (write < pricingPageLineSet.Count)
					{
						pricingPageLineSet.RemoveRange(write, pricingPageLineSet.Count - write);
					}

					otherCharges.Add(pricingPageLineSet);
				}
			}

			SetSubRowSplit(subRow, split);

			AddSubRowColumns(subRow, columns, values);

			return subRow;
		}

		void SetSubRowSplit(PricingPageTableSubRowWrapper subRow, string split)
		{
			switch (split)
			{
				case GroupStrategy.All:
					break;

				case GroupStrategy.Containerised:
					subRow.Split = Res.GetString("135498aa-b0c2-4da0-a598-d7dd525ade47", "Containerized");
					break;

				case GroupStrategy.NonContainerised:
					subRow.Split = Res.GetString("cc5b93d6-4e70-4417-848a-4a38e04922af", "Non-Containerized");
					break;

				default:
					subRow.Split = split;
					break;
			}
		}

		void AddSubRowColumns(PricingPageTableSubRowWrapper subRow, ColumnKeyList columns, List<DocAmount> values)
		{
			for (int i = 0; i < values.Count; i++)
			{
				var column = columns[i];
				subRow.Columns.Add
				(
					new PricingPageColumnWrapper
					(
						column.ToString(),
						values[i],
						Factory,
						column.Currency
					)
				);
			}
		}

		/// <summary>
		/// Group similar rateEntries that match all fields except container
		/// </summary>
		IEnumerable<List<RateEntry>> GroupSimilarRateEntries(RateEntry[] rateEntries)
		{
			var result = new List<List<RateEntry>>();

			foreach (var rateEntry in rateEntries)
			{
				var isDistinct = true;

				foreach (var rateEntryListResult in result)
				{
					// if the first rateEntry in the group match
					if (rateEntry.IsDuplicateForPricingPageGrouping(rateEntryListResult[0], ignoreCategoryAndMode: true, checkMatchContainerRateClass: true))
					{
						// if rateEntry doesn't match any rateEntries in the group
						if (!rateEntryListResult.Any(otherEntry => rateEntry.IsDuplicateForPricingPageGrouping(otherEntry, ignoreCategoryAndMode: false, checkMatchContainerRateClass: true)))
						{
							rateEntryListResult.Add(rateEntry);
						}
						isDistinct = false;
						break;
					}
				}

				if (isDistinct)
				{
					result.Add(new List<RateEntry>() { rateEntry });
				}
			}

			return result;
		}

		static void AddOtherCharges(PricingPageTableRowWrapper row, PricingPage pricingPage, IEnumerable<PricingPageRateLineList> pricingPageLineSets)
		{
			int setIndex = 0;

			foreach (var pricingPageLineSet in pricingPageLineSets)
			{
				AddOtherCharges(row, pricingPage, pricingPageLineSet, ref setIndex);
			}
		}

		static void AddOtherCharges(PricingPageTableRowWrapper row, PricingPage pricingPage, PricingPageRateLineList set, ref int setIndex)
		{
			var quotationLineListHelper = new QuotationLineListHelper();
			var quotationLineList = quotationLineListHelper.GetLines(set, pricingPage.ViewAgentRates);
			int lineIndex = 0;

			if (quotationLineList.Count > 0)
			{
				setIndex++;

				foreach (var quotationLine in quotationLineList)
				{
					lineIndex++;
					row.OtherCharges.Add(new PricingPageLineWrapper(pricingPage, quotationLine, setIndex, lineIndex, row.Factory));
				}
			}
		}

		static void InsertValue(List<DocAmount> values, int index, DocAmount value)
		{
			if (values.Count > index)
			{
				values[index] = value;
			}
			else
			{
				while (values.Count < index)
				{
					values.Add(DocAmount.Empty);
				}

				values.Add(value);
			}
		}

		static void FillInMissingColumns(PricingPageTableRowWrapperCollection result, ColumnKeyList columns)
		{
			string[] columnLabels = new string[columns.Count];

			for (int i = 0; i < columns.Count; i++)
			{
				columnLabels[i] = columns[i].ToString();
			}

			foreach (PricingPageTableRowWrapper pricingPageTableRowWrapper in result)
			{
				foreach (PricingPageTableSubRowWrapper subrow in pricingPageTableRowWrapper.SubRows)
				{
					for (int i = subrow.Columns.Count; i < columnLabels.Length; i++)
					{
						subrow.Columns.Add(new PricingPageColumnWrapper(columnLabels[i], DocAmount.Empty, subrow.Factory));
					}
				}
			}
		}

		readonly bool isFreight;
		readonly CompactFormatter formatter;
	}
}
