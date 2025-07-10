using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("Pricing Page Set")]
	public class PricingPageSetWrapper : GenericWrapper
	{
		public PricingPageSetWrapper(RatingHeader header, PricingPaginationStrategy paginationStrategy, BusinessObjectFactory factory)
			: base(header, factory)
		{
			strategy = paginationStrategy;
		}

		const string Label_TableRows = "TableRows";
		const string Label_FreightRates = "FreightRates";
		const string Label_OriginRates = "OriginRates";
		const string Label_DestinationRates = "DestinationRates";

		const string SubLabel_SubRow = "SubRow";
		const string SubLabel_RowBody = "RowBody";
		const string SubLabel_OtherCharges = "OtherCharges";

		public PricingPageWrapperCollection PricingPages
		{
			get { return pricingPages ?? (pricingPages = CreatePricingPageWrapperCollection()); }
		}

		PricingPageWrapperCollection pricingPages;

		/// <summary>
		/// Dirty hack to workaround doc-engine short commings.
		/// Used in 'Forwarding Standard Pricing Page'
		/// </summary>
		public PricingPageCompoundLineWrapperCollection OriginDestinationAndFreightRates
		{
			get
			{
				if (originDestinationAndFreightRates == null)
				{
					originDestinationAndFreightRates = new PricingPageCompoundLineWrapperCollection(Factory);

					foreach (PricingPageWrapper page in PricingPages)
					{
						page.IsGSTApplicable = false;

						if (page.CanRollUpAndSort
							&& RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.Value
							&& (page.RollUpDisplay != DocRollupOrSortDisplayList.Codes.RollUpCharges
								|| page.RollUpStyle != DocRollupOrSortStyleList.Codes.NoGrouping))
						{
							Add(page, originDestinationAndFreightRates, page.RollUpSortRates, Label_FreightRates, 1);
						}
						else
						{
							Add(page, originDestinationAndFreightRates, page.OriginRates, Label_OriginRates, 1);
							Add(page, originDestinationAndFreightRates, page.FreightRates, Label_FreightRates, 2);
							Add(page, originDestinationAndFreightRates, page.DestinationRates, Label_DestinationRates, 3);
						}
					}
				}

				return originDestinationAndFreightRates;
			}
		}
		PricingPageCompoundLineWrapperCollection originDestinationAndFreightRates;

		/// <summary>
		/// Dirty hack to workaround doc-engine short commings.
		/// Cannot find in 'System Document Elements.xls', may be obsolete?
		/// </summary>
		public PricingPageCompoundLineWrapperCollection OriginAndDestinationRates
		{
			get
			{
				if (originAndDestinationRates == null)
				{
					originAndDestinationRates = new PricingPageCompoundLineWrapperCollection(Factory);

					foreach (PricingPageWrapper page in PricingPages)
					{
						page.IsGSTApplicable = false;
						Add(page, originAndDestinationRates, page.OriginRates, Label_OriginRates, 1);
						Add(page, originAndDestinationRates, page.DestinationRates, Label_DestinationRates, 2);
					}
				}

				return originAndDestinationRates;
			}
		}
		PricingPageCompoundLineWrapperCollection originAndDestinationRates;

		/// <summary>
		/// Dirty hack to workaround doc-engine short commings.
		/// Used in 'Forwarding Landscape Pricing Page - Non Loose'
		/// </summary>
		public PricingPageCompoundLineWrapperCollection OriginDestinationAndContainerTableRows
		{
			get
			{
				if (originDestinationAndContainerTableRows == null)
				{
					originDestinationAndContainerTableRows = new PricingPageCompoundLineWrapperCollection(Factory);

					foreach (PricingPageWrapper page in PricingPages)
					{
						page.IsGSTApplicable = false;
						Add(page, originDestinationAndContainerTableRows, page.ContainerTableRows, Label_TableRows, 1);
						Add(page, originDestinationAndContainerTableRows, page.OriginRates, Label_OriginRates, 2);
						Add(page, originDestinationAndContainerTableRows, page.DestinationRates, Label_DestinationRates, 3);
					}
				}

				return originDestinationAndContainerTableRows;
			}
		}
		PricingPageCompoundLineWrapperCollection originDestinationAndContainerTableRows;

		/// <summary>
		/// Dirty hack to workaround doc-engine short commings.
		/// Used in 'Forwarding Landscape Pricing Page - Loose'
		/// </summary>
		public PricingPageCompoundLineWrapperCollection OriginDestinationAndChargeableTableRows
		{
			get
			{
				if (originDestinationAndChargeableTableRows == null)
				{
					originDestinationAndChargeableTableRows = new PricingPageCompoundLineWrapperCollection(Factory);

					foreach (PricingPageWrapper page in PricingPages)
					{
						page.IsGSTApplicable = false;
						Add(page, originDestinationAndChargeableTableRows, page.ChargeableTableRows, Label_TableRows, 1);
						Add(page, originDestinationAndChargeableTableRows, page.OriginRates, Label_OriginRates, 2);
						Add(page, originDestinationAndChargeableTableRows, page.DestinationRates, Label_DestinationRates, 3);
					}
				}

				return originDestinationAndChargeableTableRows;
			}
		}
		PricingPageCompoundLineWrapperCollection originDestinationAndChargeableTableRows;

		/// <summary>
		/// Dirty hack to workaround doc-engine short commings.
		/// Used in 'Forwarding Compact Pricing Page'
		/// </summary>
		public PricingPageCompoundLineWrapperCollection OriginDestinationAndFreightTableRows
		{
			get
			{
				if (originDestinationAndFreightTableRows == null)
				{
					originDestinationAndFreightTableRows = new PricingPageCompoundLineWrapperCollection(Factory);

					foreach (PricingPageWrapper page in PricingPages)
					{
						page.IsGSTApplicable = false;
						Add(page, originDestinationAndFreightTableRows, page.CompactFreightTableRows, Label_TableRows, 1);
						Add(page, originDestinationAndFreightTableRows, page.CompactOriginTableRows, Label_OriginRates, 2);
						Add(page, originDestinationAndFreightTableRows, page.CompactDestinationTableRows, Label_DestinationRates, 3);
					}
				}

				return originDestinationAndFreightTableRows;
			}
		}
		PricingPageCompoundLineWrapperCollection originDestinationAndFreightTableRows;

		#region Implementation

		void Add(PricingPageWrapper page, PricingPageCompoundLineWrapperCollection collection, PricingPageTableRowWrapperCollection src, string label, int labelOrdinal)
		{
			bool isGSTApplicable = page.IsGSTApplicable;

			foreach (PricingPageTableRowWrapper row in src)
			{
				if (!isGSTApplicable)
				{
					isGSTApplicable = row.Entries.Cast<RatingEntryWrapper>().Any(rowEntry => rowEntry.MayGSTBeApplicable);
				}

				foreach (PricingPageTableSubRowWrapper subrow in row.SubRows)
				{
					collection.Add(new PricingPageCompoundLineWrapper(Factory)
					{
						Label = label,
						LabelOrdinal = labelOrdinal,
						SubLabel = SubLabel_SubRow,
						SubLabelOrdinal = 1,
						Page = page,
						Row = row,
						SubRow = subrow,
					});
				}

				collection.Add(new PricingPageCompoundLineWrapper(Factory)
				{
					Label = label,
					LabelOrdinal = labelOrdinal,
					SubLabel = SubLabel_RowBody,
					SubLabelOrdinal = 2,
					Page = page,
					Row = row,
				});

				foreach (PricingPageLineWrapper line in row.OtherCharges)
				{
					collection.Add(new PricingPageCompoundLineWrapper(Factory)
					{
						Label = label,
						LabelOrdinal = labelOrdinal,
						SubLabel = SubLabel_OtherCharges,
						SubLabelOrdinal = 3,
						Page = page,
						Row = row,
						RateLine = line,
					});
				}
			}

			page.IsGSTApplicable |= isGSTApplicable;
		}

		void Add(PricingPageWrapper page, PricingPageCompoundLineWrapperCollection collection, PricingPageLineWrapperCollection src, string label, int labelOrdinal)
		{
			bool isGSTApplicable = false;

			foreach (PricingPageLineWrapper line in src)
			{
				collection.Add(new PricingPageCompoundLineWrapper(Factory)
				{
					Label = label,
					LabelOrdinal = labelOrdinal,
					Page = page,
					RateLine = line,
				});

				isGSTApplicable |= line.IsGSTApplicable;
			}

			page.IsGSTApplicable |= isGSTApplicable;
		}

		PricingPageWrapperCollection CreatePricingPageWrapperCollection()
		{
			Func<PricingPage, RateEntry, bool> looseFilter;
			var collection = new PricingPageWrapperCollection(Factory);

			bool shouldLoadPricingPages;
			switch (strategy & PricingPaginationStrategy.CategoryFilterMask)
			{
				case PricingPaginationStrategy.ForwardingCategoryFilter:
				case PricingPaginationStrategy.ShippingCategoryFilter:
				case PricingPaginationStrategy.ShippingDetentionCategoryFilter:
				case PricingPaginationStrategy.CFSCategoryFilter:
					shouldLoadPricingPages = true;
					break;

				default:
					shouldLoadPricingPages = false;
					break;
			}

			switch (strategy & PricingPaginationStrategy.ModeContainerizedMask)
			{
				case PricingPaginationStrategy.AirModeAndNonContainerizedFilter:
					looseFilter = LooseFilter;
					break;

				case PricingPaginationStrategy.NonAirModeOrContainerizedFilter:
					looseFilter = NonLooseFilter;
					break;

				default:
					looseFilter = null;
					break;
			}

			if (shouldLoadPricingPages)
			{
				var pages = new PricingPageCollection(Header);
				pages.Load(strategy);

				foreach (PricingPage page in pages)
				{
					if (page.FirstRateEntry != null && (looseFilter == null || looseFilter(page, page.FirstRateEntry)))
					{
						collection.Add(new PricingPageWrapper(page, collection.Count, Factory));
					}
				}
			}

			return collection;
		}

		static bool LooseFilter(PricingPage page, RateEntry entry) => entry.IsAirFreight() && page.ContainerSet.Count == 0;

		static bool NonLooseFilter(PricingPage page, RateEntry entry) => !entry.IsAirFreight() || page.ContainerSet.Count != 0;

		RatingHeader Header => (RatingHeader)WrappedBO;

		readonly PricingPaginationStrategy strategy;

		#endregion
	}
}
