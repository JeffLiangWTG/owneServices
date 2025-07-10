using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.Quotation.RollUpSort;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.Rating.Business.RatingEnums;

namespace Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Rating.TableRow.TableStrategies
{
	sealed class RollUpSortStrategy
	{
		public RollUpSortStrategy(BusinessObjectFactory factory, PricingPageRateLineFactory pricingPageLineSetFactory)
		{
			Factory = factory;
			PricingPageRateLineFactory = pricingPageLineSetFactory;
		}

		public BusinessObjectFactory Factory { get; }

		public PricingPageRateLineFactory PricingPageRateLineFactory { get; }

		public PricingPageLineWrapperCollection Extract(PricingPage pricingPage)
		{
			var freightPricingPageRateLineFactory = new PricingPageRateLineFactory(EntryTypes.Freight);
			var originPricingPageRateLineFactory = new PricingPageRateLineFactory(EntryTypes.Origin);
			var destinationPricingPageRateLineFactory = new PricingPageRateLineFactory(EntryTypes.Destination);

			var freightPricingPageRateLineListList = freightPricingPageRateLineFactory.LoadLineSets(pricingPage, pricingPage.RateEntries);
			var originPricingPageRateLineListList = originPricingPageRateLineFactory.LoadLineSets(pricingPage, pricingPage.RateEntries);
			var destinationPricingPageRateLineListList = destinationPricingPageRateLineFactory.LoadLineSets(pricingPage, pricingPage.RateEntries);
			var clientForSorting = pricingPage.FirstRateEntry.ParentRatingHeader.Header;

			var docLineList = new DocLineList();
			docLineList.AddRange(freightPricingPageRateLineListList.SelectMany(pricingPageRateLineList => pricingPageRateLineList.Select(rateLine => InitialiseDocRateLine(rateLine, clientForSorting))));
			docLineList.AddRange(originPricingPageRateLineListList.SelectMany(pricingPageRateLineList => pricingPageRateLineList.Select(rateLine => InitialiseDocRateLine(rateLine, clientForSorting))));
			docLineList.AddRange(destinationPricingPageRateLineListList.SelectMany(pricingPageRateLineList => pricingPageRateLineList.Select(rateLine => InitialiseDocRateLine(rateLine, clientForSorting))));
			var docLineGrouperAndSorter = new RatingDocRollUpSorter(pricingPage, pricingPage.Factory);

			var groupAndSortedLines = docLineGrouperAndSorter.GroupAndSortLines
			(
				pricingPage.RollUpDisplay,
				pricingPage.RollUpStyle,
				docLineList,
				new[] { docLineList }
			);

			var result = new PricingPageLineWrapperCollection(Factory);

			foreach (var groupAndSortedLine in groupAndSortedLines.Cast<ISortableDocLine>())
			{
				var quotationLineList = GetQuotationLines(groupAndSortedLine);
				foreach (var quotationLine in quotationLineList)
				{
					result.Add(new PricingPageLineWrapper(pricingPage, quotationLine, 0, 0, Factory));
				}
			}

			return result;
		}

		DocRateLine InitialiseDocRateLine(RateLine rateLine, OrgHeader clientForSorting)
		{
			var rateLineDocRateLine = new DocRateLine(rateLine)
			{
				ClientForSorting = clientForSorting
			};
			return rateLineDocRateLine;
		}

		static QuotationLineList GetQuotationLines(ISortableDocLine docLine)
		{
			var result = new QuotationLineList();

			var parameters = Calculator.GetQuotationLinesParam.None;

			if (docLine is DocRateLine docRateLine)
			{
				var rateLine = docRateLine.RateLine;
				result = rateLine.Calculator.GetQuotationLines(parameters, rateLine.ParentRateEntry);
			}
			else if (docLine is RolledUpDocRateLine rolledUpDocRateLine)
			{
				result = GetQuotationLines(rolledUpDocRateLine);
			}

			return result;
		}

		static QuotationLineList GetQuotationLines(RolledUpDocRateLine rolledUpDocRateLine)
		{
			var docLineAmount = new DocLineAmount();

			RateLine rateLine = null;

			foreach (var docRateLine in rolledUpDocRateLine.DocLineList.Cast<DocRateLine>())
			{
				docLineAmount += docRateLine.RateLine.Calculator.GetDocLineAmount();

				if (rateLine == null)
				{
					rateLine = docRateLine.RateLine;
				}
			}

			var result = new QuotationLineList();

			if (rateLine != null)
			{
				var quotationLineList = docLineAmount.GetQuotationLineList(rateLine);
				var firstQuotationLine = quotationLineList.FirstOrDefault();
				quotationLineList.Remove(firstQuotationLine);

				result.Add(QuotationLine.HeaderWithDescriptionCurrencyAmountUnits(rateLine, QuotationLineType.UseOverrideDescription, rolledUpDocRateLine.Description, firstQuotationLine.Currency, firstQuotationLine.Amount, firstQuotationLine.Unit));
				result.AddRange(quotationLineList);
			}

			return result;
		}
	}
}
