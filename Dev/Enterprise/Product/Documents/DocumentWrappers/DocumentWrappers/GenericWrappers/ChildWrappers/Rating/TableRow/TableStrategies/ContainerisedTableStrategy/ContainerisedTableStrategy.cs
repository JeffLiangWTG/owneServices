using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;
using Enterprise.Rating.Business.RatingEnums;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <summary>
	///	Only for Freight charges, not Origin and Destination i.e. OriginDestinationAndChargeableTableRows, OriginDestinationAndContainerTableRows
	/// Origin/Destination charges are using PricingPageLineWrapperCollection
	/// Only used in 'Forwarding Landscape Pricing Page - Non Loose' i.e. FCL, LSE, etc.
	/// </summary>
	partial class ContainerisedTableStrategy : BaseTableStrategy
	{
		public ContainerisedTableStrategy(BusinessObjectFactory factory)
			: base(factory, new PricingPageRateLineFactory(EntryTypes.Freight))
		{
			this.formatterFCL = new PricingPageTableCellFormatter(QuantityUnit.CN);
			this.formatterLCL = new PricingPageTableLclCellFormatter(QuantityUnit.M3);
		}

		public override PricingPageTableRowWrapperCollection Extract(PricingPage pricingPage)
		{
			var result = new PricingPageTableRowWrapperCollection(Factory);

			var groupedSimilarRateEntries = GroupSimilarRateEntries(pricingPage.RateEntries);
			foreach (var similarRateEntries in groupedSimilarRateEntries)
			{
				var row = ConstructRow(pricingPage, similarRateEntries);
				var hasLines = row.SubRows.Any() || row.OtherCharges.Any();
				if (hasLines)
				{
					row.Index = result.Count;
					result.Add(row);
				}
			}

			return result;
		}

		PricingPageTableRowWrapper ConstructRow(PricingPage pricingPage, List<RateEntry> similarRateEntries)
		{
			var freightCharge = Factory.Load<AccChargeCode>(GetFreightChargeCodePK(pricingPage.RateEntries));
			PricingPageTableRowWrapper row = null;

			var pricingPageLineSetList = PricingPageRateLineFactory.LoadLineSets(pricingPage, similarRateEntries, exactMatch: true, checkEmptyContainer: true);

			if (similarRateEntries.Count == 1)
			{
				var rateEntry = similarRateEntries.FirstOrDefault();
				if (rateEntry.TI_RateCategory == RatingConstants.RateCategory.LCL)
				{
					row = GetLCLRow(pricingPage, rateEntry, pricingPageLineSetList, freightCharge);
				}
			}
			else if (IsOverridingLCLRows(similarRateEntries))
			{
				row = GetOverridingLCLRow(pricingPage, similarRateEntries, pricingPageLineSetList);
			}

			if (row == null)
			{
				row = GetRow(pricingPage, similarRateEntries, pricingPageLineSetList, freightCharge);
			}

			return row;
		}

		static bool IsOverridingLCLRows(List<RateEntry> rateEntries)
		{
			var previousRateMode = string.Empty;

			foreach (var rateEntry in rateEntries)
			{
				if (rateEntry.TI_RateCategory == RatingConstants.RateCategory.LCL)
				{
					if (string.IsNullOrEmpty(previousRateMode))
					{
						previousRateMode = rateEntry.TI_Mode;
					}
					else if (previousRateMode != rateEntry.TI_Mode) // detecting different mode of same i.e. Road (FTL and LRO) or Rail (LRA and FWL)
					{
						return true;
					}
				}
			}

			return false;
		}

		PricingPageTableRowWrapper GetRow(PricingPage pricingPage, List<RateEntry> similarRateEntries, PricingPageRateLineListList pricingPageLineSetList, AccChargeCode freightCharge)
		{
			var row = new PricingPageTableRowWrapper(similarRateEntries.ToArray(), Factory);
			var subrows = new List<SubRow>();
			var containerList = GetContainers(pricingPage);

			int setIndex = 0;

			foreach (var pricingPageLineSet in pricingPageLineSetList)  // lines with different Rail/Road modes or different RateDesc 
			{
				int write = 0;

				for (int read = 0; read < pricingPageLineSet.Count; read++) // lines with same RateDesc
				{
					var rateLine = pricingPageLineSet[read];

					AddValueToCellOrSetPricingPageLineSet(pricingPage.ViewAgentRates, freightCharge, containerList, rateLine, subrows, pricingPageLineSet, ref write);
				}

				if (write < pricingPageLineSet.Count)
				{
					pricingPageLineSet.RemoveRange(write, pricingPageLineSet.Count - write);
				}

				AddOtherCharges(pricingPage, pricingPageLineSet, row, ref setIndex);
			}

			AddSubRows(pricingPage.ContainsLCL, freightCharge, containerList, subrows, row); // FRT Charges

			return row;
		}

		static List<IRefContainer> GetContainers(PricingPage pricingPage)
		{
			var result = new List<IRefContainer>();

			if (pricingPage.HasFCLWithEmptyContainer)
			{
				result.Add(new EmptyContainer());
			}

			result.AddRange(pricingPage.ContainerSet.Cast<IRefContainer>());

			return result;
		}

		void AddValueToCellOrSetPricingPageLineSet(bool pricingPageViewAgentRates, AccChargeCode freightCharge, List<IRefContainer> containerList, RateLine rateLine, List<SubRow> subrows, PricingPageRateLineList pricingPageLineSet, ref int write)
		{
			bool lineViewAgentRates = rateLine.ViewAgentRates;
			rateLine.ViewAgentRates = pricingPageViewAgentRates;

			if (freightCharge == null || rateLine.TL_AC != freightCharge.PK || !AddValueToCell(containerList, rateLine, subrows))
			{
				pricingPageLineSet[write] = rateLine; // For other charges (non FRT charges)
				write++;
			}

			rateLine.ViewAgentRates = lineViewAgentRates;
		}

		void AddOtherCharges(PricingPage pricingPage, PricingPageRateLineList pricingPageLineSet, PricingPageTableRowWrapper row, ref int setIndex)
		{
			var helper = new QuotationLineListHelper();
			var quotationLineList = helper.GetLines(pricingPageLineSet, pricingPage.ViewAgentRates);
			int lineIndex = 0;

			if (quotationLineList.Count > 0)
			{
				setIndex++;

				foreach (var quotationLine in quotationLineList)
				{
					lineIndex++;
					row.OtherCharges.Add(new PricingPageLineWrapper(pricingPage, quotationLine, setIndex, lineIndex, Factory));
				}
			}
		}

		PricingPageTableRowWrapper GetLCLRow(PricingPage pricingPage, RateEntry rateEntry, PricingPageRateLineListList pricingPageLineSetList, AccChargeCode freightCharge)
		{
			PricingPageTableRowWrapper row = null;

			var filteredPricingPageLineSet = pricingPageLineSetList.Where(x => x.Any(y => y.ParentRateEntry.PK == rateEntry.PK));
			if (filteredPricingPageLineSet.Any())
			{
				row = new PricingPageTableRowWrapper(new[] { rateEntry }, Factory);
				var subrows = new List<SubRow>();
				var containerList = GetContainers(pricingPage);

				int setIndex = 0;

				foreach (var pricingPageLineSet in filteredPricingPageLineSet)
				{
					var rateLine = pricingPageLineSet.FirstOrDefault(x => x.ParentRateEntry.PK == rateEntry.PK);
					if (pricingPageLineSet != null && rateLine != null)
					{
						int write = 0;
						AddValueToCellOrSetPricingPageLineSet(pricingPage.ViewAgentRates, freightCharge, containerList, rateLine, subrows, pricingPageLineSet, ref write);

						if (write < pricingPageLineSet.Count)
						{
							pricingPageLineSet.RemoveRange(write, pricingPageLineSet.Count - write);
						}

						AddOtherCharges(pricingPage, pricingPageLineSet, row, ref setIndex);
					}
				}

				AddSubRows(pricingPage.ContainsLCL, freightCharge, containerList, subrows, row);
			}

			return row;
		}

		PricingPageTableRowWrapper GetOverridingLCLRow(PricingPage pricingPage, List<RateEntry> rowEntries, PricingPageRateLineListList pricingPageLineSetList)
		{
			var row = new PricingPageTableRowWrapper(rowEntries.ToArray(), Factory);

			int setIndex = 0;

			foreach (var pricingPageLineSet in pricingPageLineSetList)  // lines with different Rail/Road modes or different RateDesc 
			{
				int write = 0;

				for (int read = 0; read < pricingPageLineSet.Count; read++) // lines with same RateDesc
				{
					var rateLine = pricingPageLineSet[read];
					var lineViewAgentRates = rateLine.ViewAgentRates;
					rateLine.ViewAgentRates = pricingPage.ViewAgentRates;

					pricingPageLineSet[write] = rateLine; // For other charges (non FRT charges)
					write++;

					rateLine.ViewAgentRates = lineViewAgentRates;
				}

				AddOtherCharges(pricingPage, pricingPageLineSet, row, ref setIndex);
			}

			return row;
		}

		void AddSubRows(bool containsLCL, AccChargeCode freightCharge, List<IRefContainer> containerList, List<SubRow> subrows, PricingPageTableRowWrapper row)
		{
			if (subrows.Count > 0)
			{
				CodeAndDescriptionWrapper freightChargeWrapper = new CodeAndDescriptionWrapper(freightCharge.AC_Code, new AccChargeCodeCollection(Factory, new ZQuery(), freightCharge.AC_GC.ToGuid()), Factory);

				int index = 0;

				foreach (var subRow in subrows)
				{
					var wrapper = new PricingPageTableSubRowWrapper(Factory);
					wrapper.Index = index++;
					wrapper.Charge = freightChargeWrapper;
					wrapper.ConversionFactor = new RatingConversionFactorWrapper(subRow.ConversionFactor, Factory);
					wrapper.Currency = new CurrencyWrapper(Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, subRow.Currency), Factory);

					AddValueToSubrowColumn(containsLCL, containerList, wrapper, subRow.Values);
					row.SubRows.Add(wrapper);
				}
			}
			else if (row.OtherCharges.Any())
			{
				PricingPageTableSubRowWrapper dummySubrow = new PricingPageTableSubRowWrapper(Factory) { Index = 0 };
				AddValueToSubrowColumn(containsLCL, containerList, dummySubrow, DocAmount.Create(count: containerList.Count + 2));
				row.SubRows.Add(dummySubrow);
			}
		}

		/// <summary>
		/// Subrows is only for FRT charges, non-FRT charges would go to OtherCharges
		/// </summary>
		void AddValueToSubrowColumn(bool containsLCL, List<IRefContainer> containerList, PricingPageTableSubRowWrapper subrow, DocAmount[] values)
		{
			for (int containerIndex = 0; containerIndex < containerList.Count; containerIndex++)
			{
				subrow.Columns.Add(new PricingPageColumnWrapper(containerList[containerIndex].RC_Code, values[containerIndex], Factory));
			}

			if (containsLCL)
			{
				subrow.Columns.Add(new PricingPageColumnWrapper(Res.GetString("52e041aa-aca9-481b-a44c-9a27a86453aa", "LCL Min."), values[containerList.Count], Factory));
				subrow.Columns.Add(new PricingPageColumnWrapper(Res.GetString("afd9def0-062d-410f-a043-5ab1fc38d85b", "LCL per M3"), values[containerList.Count + 1], Factory));
			}
		}

		/// <summary>
		/// Subrows is only for FRT charges, non-FRT charges would go to OtherCharges
		/// </summary>
		bool AddValueToCell(List<IRefContainer> containerList, RateLine rateLine, List<SubRow> subRows)
		{
			var currency = rateLine.TL_RX_NKCurrency;
			DocAmount[] subRowValues;

			var subRow = subRows.FirstOrDefault(s => s.Currency.EqualsIgnoringCase(currency));
			if (subRow == null)
			{
				subRowValues = DocAmount.Create(count: containerList.Count + genericContainersCount);
				subRows.Add(new SubRow(currency, subRowValues, rateLine.ConversionFactorForDocumentPrintingOnly));
			}
			else
			{
				subRowValues = subRow.Values;
			}

			bool isValid;
			var valueList = DocAmount.Create(count: genericContainersCount);
			var entry = rateLine.Parent;
			var container = entry.Container;

			if (container == null && (rateLine.ParentRateEntry.IsLCL() || rateLine.ParentRateEntry.IsLooseFreight()))
			{
				isValid = formatterLCL.TryFormat(rateLine, valueList);
				subRowValues[containerList.Count] = valueList[PricingPageTableLclCellFormatter.MinimumColumn];
				subRowValues[containerList.Count + 1] = valueList[PricingPageTableLclCellFormatter.UnitColumn];
			}
			else
			{
				isValid = formatterFCL.TryFormat(rateLine, valueList);

				for (var containerIndex = 0; containerIndex < containerList.Count; containerIndex++)
				{
					var colContainer = containerList[containerIndex];
					if (entry.TI_MatchContainerRateClass)
					{
						if (colContainer is RefContainer refColContainer
							&& container.RC_FreightRateClass == refColContainer.RC_FreightRateClass
							&& DocAmount.IsEmpty(subRowValues[containerIndex]))
						{
							subRowValues[containerIndex] = valueList[PricingPageTableCellFormatter.UnitColumn];
						}
					}
					else if ((container == null && colContainer is EmptyContainer)
						|| (container != null && container.PK == colContainer.PK))
					{
						subRowValues[containerIndex] = valueList[PricingPageTableCellFormatter.UnitColumn];
						break;
					}
				}
			}

			return isValid;
		}

		const int genericContainersCount = 2; // 20GP and 40GP

		static List<List<RateEntry>> GroupSimilarRateEntries(IEnumerable<RateEntry> entryCollection)
		{
			var results = new List<List<RateEntry>>();
			var rateEntries = new List<RateEntry>();

			foreach (var entry in entryCollection)
			{
				if (!entry.IsFreightEntry())
				{
					continue;
				}

				bool isDuplicate = false;

				for (var resultIndex = 0; resultIndex < results.Count; resultIndex++)
				{
					if (entry.IsDuplicateForPricingPageGrouping(rateEntries[resultIndex], ignoreCategoryAndMode: true, checkMatchContainerRateClass: false))
					{
						isDuplicate = true;
						results[resultIndex].Add(entry);
					}
				}

				if (!isDuplicate)
				{
					rateEntries.Add(entry);
					results.Add(new List<RateEntry>() { entry });
				}
			}

			return results;
		}

		static ZGuid GetFreightChargeCodePK(IEnumerable<RateEntry> rateEntries)
		{
			using (var e = rateEntries.GetEnumerator())
			{
				if (e.MoveNext())
				{
					var company = e.Current.Company() ?? Env.CurrentCompany;

					return Env.Registry.GetFreightChargeCode(company.PK);
				}
			}

			return ZGuid.Empty;
		}

		readonly PricingPageTableCellFormatter formatterFCL;
		readonly PricingPageTableCellFormatter formatterLCL;
	}
}
