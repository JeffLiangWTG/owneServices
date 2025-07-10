using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Rating.TableRow.TableStrategies;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RatingEnums;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("Pricing Page")]
	public class PricingPageWrapper : GenericWrapper
	{
		public PricingPageWrapper(PricingPage page, int index, BusinessObjectFactory factory)
			: base(page, factory)
		{
			this.IndexNum = index;
		}

		public ZInt IndexNum { get; private set; }

		public ZString Index => IndexNum.ToString("00000");

		public ZString OpeningText
		{
			get
			{
				if (!openingText.HasValue)
				{
					openingText = CalculateOpeningClosingText(e => e.TI_PageOpeningText) ?? DocumentsDataRegistry.Instance.QuoteOpeningText.Value;
				}

				return openingText ?? ZString.Empty;
			}
		}

		ZString? openingText;

		public ZString ClosingText
		{
			get
			{
				if (!closingText.HasValue)
				{
					closingText = CalculateOpeningClosingText(e => e.TI_PageClosingText) ?? DocumentsDataRegistry.Instance.QuoteClosingText.Value;
				}

				return closingText ?? ZString.Empty;
			}
		}

		ZString? closingText;

		public ZString ClosingTaxText
		{
			get
			{
				if (IsGSTApplicable)
				{
					var taxCode = GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription;
					return Res.GetString("d84035fb-16d9-4403-a76c-716f82c7aaad", "A local Value Added Tax charge (equivalent to {0}) may apply to all items marked with an asterisk (*).", taxCode);
				}

				return ZString.Empty;
			}
		}

		public ZBool IsGSTApplicable { get; set; }

		public RatingEntryWrapperCollection Entries => entries ?? (entries = new RatingEntryWrapperCollection(Page.RateEntries, Factory));
		RatingEntryWrapperCollection entries;

		public PricingPageTableRowWrapperCollection ContainerTableRows => containerTableRows ?? (containerTableRows = new ContainerisedTableStrategy(Factory).Extract(Page));
		PricingPageTableRowWrapperCollection containerTableRows;

		public PricingPageTableRowWrapperCollection ChargeableTableRows => chargeableTableRows ?? (chargeableTableRows = new ChargeableTableStrategy(Factory).Extract(Page));
		PricingPageTableRowWrapperCollection chargeableTableRows;

		#region Compact

		public PricingPageTableRowWrapperCollection CompactFreightTableRows
			=> compactFreightTableRows ?? (compactFreightTableRows = GetPricingPageTableRowWrapperCollectionForCompact(EntryTypes.Freight));
		PricingPageTableRowWrapperCollection compactFreightTableRows;

		public PricingPageTableRowWrapperCollection CompactOriginTableRows
			=> compactOriginTableRows ?? (compactOriginTableRows = GetPricingPageTableRowWrapperCollectionForCompact(EntryTypes.Origin));
		PricingPageTableRowWrapperCollection compactOriginTableRows;

		public PricingPageTableRowWrapperCollection CompactDestinationTableRows
			=> compactDestinationTableRows ?? (compactDestinationTableRows = GetPricingPageTableRowWrapperCollectionForCompact(EntryTypes.Destination));
		PricingPageTableRowWrapperCollection compactDestinationTableRows;

		PricingPageTableRowWrapperCollection GetPricingPageTableRowWrapperCollectionForCompact(EntryTypes entryType)
		{
			var pricingPageLineSetFactory = new PricingPageRateLineFactory(entryType);
			var compactTableStrategy = new CompactTableStrategy(Factory, pricingPageLineSetFactory);
			return compactTableStrategy.Extract(Page);
		}

		#endregion

		public PricingPageCFXWrapperCollection CFX => cfx ?? (cfx = new PricingPageCFXWrapperCollection(Page, Factory));
		PricingPageCFXWrapperCollection cfx;

		public PricingPageLineWrapperCollection OriginRates => originRates ?? (originRates = new PricingPageLineWrapperCollection(Page, new PricingPageRateLineFactory(EntryTypes.Origin), Factory));
		PricingPageLineWrapperCollection originRates;

		public PricingPageLineWrapperCollection DestinationRates => destinationRates ?? (destinationRates = new PricingPageLineWrapperCollection(Page, new PricingPageRateLineFactory(EntryTypes.Destination), Factory));
		PricingPageLineWrapperCollection destinationRates;

		public PricingPageLineWrapperCollection FreightRates => freightRates ?? (freightRates = new PricingPageLineWrapperCollection(Page, new PricingPageRateLineFactory(EntryTypes.Freight), Factory));
		PricingPageLineWrapperCollection freightRates;

		public PricingPageLineWrapperCollection RollUpSortRates => rollUpGroupRates ??= new RollUpSortStrategy(Factory, new PricingPageRateLineFactory(EntryTypes.Freight)).Extract(Page);
		PricingPageLineWrapperCollection rollUpGroupRates;

		#region Implementation

		PricingPage Page => (PricingPage)WrappedBO;

		string CalculateOpeningClosingText(Converter<RateEntry, ZString> accessor)
		{
			var builder = new StringBuilder();
			var seen = new Dictionary<string, bool>();

			foreach (var entry in Page.RateEntries)
			{
				var text = accessor(entry);

				if (!text.IsEmpty && !seen.ContainsKey(text))
				{
					if (builder.Length > 0)
					{
						builder.AppendLine();
						builder.AppendLine();
					}

					builder.Append(text);
					seen.Add(text, true);
				}
			}

			return builder.Length == 0 ? null : builder.ToString();
		}

		#endregion

		#region RollUpAndSort

		public bool CanRollUpAndSort => Page.FirstRateEntry.Parent?.CanRollUpAndSort() ?? false;

		public ZString RollUpDisplay => Page.RollUpDisplay;

		public ZString RollUpStyle => Page.RollUpStyle;

		#endregion
	}
}
