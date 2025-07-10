using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class TableStrategyTestHelper
	{
		public TableStrategyTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		BusinessObjectFactory Factory { get; }

		public static RateEntry AddRateEntryWithFlatRateLine<T>(RatingHeader ratingHeader, ZString category, string mode, string origin, string destination, short lineOrder, ZString chargeCode, ZDecimal amount, Action<RateEntry, T> setRateEntry, T value, string container = "", string rateLineDescription = "")
		{
			var rateEntry = ratingHeader.AddRateEntry(category, mode, origin, destination, container: container, removeLines: true);
			rateEntry.TI_LineOrder = lineOrder;
			setRateEntry(rateEntry, value);
			rateEntry.AddFlatRateLine(chargeCode, amount, container: container, description: rateLineDescription);
			return rateEntry;
		}

		public static string ExtractAsString(PricingPageCollection pages, BaseTableStrategy strategy)
		{
			var actual = new StringBuilder();

			var pageNo = 0;
			foreach (PricingPage page in pages)
			{
				var extractedPage = strategy.Extract(page);
				var pageAsString = Render(extractedPage, tabs: "  ");

				actual.AppendLine();
				actual.AppendLine($"[Page {pageNo}]");
				actual.Append(pageAsString);
				pageNo++;
			}

			return actual.ToString().Trim();
		}

		public static PricingPage NewPricingPage(params RateEntry[] entries)
		{
			var mainEntry = entries[0];
			if (mainEntry != null)
			{
				var page = new PricingPage(mainEntry, mainEntry.Factory, PricingPageStyle.Landscape);
				foreach (var otherEntry in entries.Skip(1))
				{
					page.AddRateEntry(otherEntry);
				}

				return page;
			}

			return null;
		}

		public static IEnumerable<PricingPage> GetPricingPages(BusinessObjectFactory factory, RatingHeader ratingHeader, string pageSetIndex)
		{
			var pricingPageSetWrappers = new PricingPageSetWrapperCollection(ratingHeader, "Pricing Page", factory);
			var pricingPageSetWrapper = pricingPageSetWrappers[pageSetIndex];
			return pricingPageSetWrapper.PricingPages.Cast<PricingPageWrapper>().Select(pricingPage => (PricingPage)pricingPage.WrappedObject);
		}

		public static string Render(PricingPageTableRowWrapperCollection collection, string tabs = "")
		{
			var builder = new StringBuilder();

			foreach (PricingPageTableRowWrapper row in collection)
			{
				builder.AppendLine();
				builder.AppendLine($"{tabs}[Row {row.Index}]");

				foreach (RatingEntryWrapper entry in row.Entries)
				{
					builder.Append($"{tabs}  [Entry {GetText(entry.ServiceLevel.Code, "[Empty Service Level]")}-{GetText(entry.CommodityCode.Code, "[Empty Commodity]")}-{entry.Mode} {entry.Origin.Code} => {entry.Destination.Code}");
					var customColumnValues = GetCustomColumnValues(entry);
					if (!string.IsNullOrEmpty(customColumnValues))
					{
						builder.AppendLine($"{customColumnValues}]");
					}
					else
					{
						builder.AppendLine("]");
					}
				}

				foreach (PricingPageTableSubRowWrapper subrow in row.SubRows)
				{
					var currency = subrow.Currency == null ? "[Empty Currency]" : subrow.Currency.ToString();
					var charge = subrow.Charge == null ? "[Empty Charge]" : subrow.Charge.ToString();

					builder.AppendLine($"{tabs}  [SubRow {subrow.Index}, '{currency}', '{charge}']");

					if (subrow.CaptionGroup > 0)
					{
						builder.AppendLine($"{tabs}    [Caption Group {subrow.CaptionGroup}]");
					}

					if (!subrow.Split.IsEmpty)
					{
						builder.AppendLine($"{tabs}    [Split {subrow.Split}]");
					}

					foreach (PricingPageColumnWrapper column in subrow.Columns)
					{
						builder.AppendLine($"{tabs}    [Column '{column.Heading.Replace("\n", "")}' = '{column.Value}']");
					}
				}

				foreach (PricingPageLineWrapper line in row.OtherCharges)
				{
					builder.AppendLine($"{tabs}  [Line {line.SetIndexNum},{line.LineIndexNum}, '{line.Description}', '{line.Currency}', '{line.Amount}', '{line.Units}']");
				}
			}

			return builder.ToString();
		}

		static string GetText(string value, string emptyValue)
			=> !string.IsNullOrEmpty(value)
				? value
				: emptyValue;

		static string GetCustomColumnValues(RatingEntryWrapper entry)
		{
			var stringBuilder = new StringBuilder();

			var frequency = entry.Frequency.FriendlyText;
			if (!string.IsNullOrEmpty(frequency))
			{
				stringBuilder.Append($", Frequency: {frequency}");
			}
			if (!string.IsNullOrEmpty(entry.ContractNumber))
			{
				stringBuilder.Append($", Contract Number: {entry.ContractNumber}");
			}

			return stringBuilder.ToString();
		}

		public OrgHeader TransportProvider1 => transportProvider1 ?? (transportProvider1 = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader transportProvider1;

		public OrgHeader TransportProvider2 => transportProvider2 ?? (transportProvider2 = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader transportProvider2;

		public OrgHeader NewClient1 => newClient1 ?? (newClient1 = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader newClient1;

		public OrgHeader NewClient2 => newClient2 ?? (newClient2 = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader newClient2;

		public OrgHeader OrgHeader1 => orgHeader1 ?? (orgHeader1 = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader orgHeader1;

		public OrgHeader OrgHeader2 => orgHeader2 ?? (orgHeader2 = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader orgHeader2;

		public OrgHeader OrgHeader3 => orgHeader3 ?? (orgHeader3 = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader orgHeader3;
	}
}
