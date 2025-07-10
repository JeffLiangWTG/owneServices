using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("Pricing Page Line")]
	public class PricingPageLineWrapper : GenericWrapper
	{
		public PricingPageLineWrapper(PricingPage pricingPage, QuotationLine quotationLine, int setIndex, int lineIndex, BusinessObjectFactory factory)
			: base(quotationLine, factory)
		{
			if (pricingPage == null)
			{
				throw new ArgumentNullException(nameof(pricingPage));
			}

			if (quotationLine == null)
			{
				throw new ArgumentNullException(nameof(quotationLine));
			}

			this.pricingPage = pricingPage;
			this.quotationLine = quotationLine;
			this.SetIndexNum = setIndex;
			this.LineIndexNum = lineIndex;
		}

		public ZInt SetIndexNum { get; private set; }
		public ZInt LineIndexNum { get; private set; }

		public ZString SetIndex => SetIndexNum.ToString("00000", CultureInfo.InvariantCulture);

		public ZString LineIndex => LineIndexNum.ToString("00000", CultureInfo.InvariantCulture);

		[CodeStringFinderHint(typeof(QuotationLineListHelper), "GetLines")]
		public ZString Description
		{
			get
			{
				var incoTerm = IncoTerm.Code.IsEmpty ? (ZString)"ALL" : IncoTerm.Code;

				bool? hasLocalClientForRelatedRates = null;

				if (quotationLine?.Master?.Parent?.Parent != null
					&& pricingPage?.FirstRateEntry?.Parent != null
					&& quotationLine.Master.Parent.Parent.PK != pricingPage.FirstRateEntry.Parent.PK
					&& pricingPage.FirstRateEntry.Parent.Header != null)
				{
					hasLocalClientForRelatedRates = pricingPage.FirstRateEntry.Parent.Header.IsLocalCountry;
				}

				return quotationLine.GetDescription(incoTerm, hasLocalClientForRelatedRates);
			}
		}

		public ZInt NumOfIndents => quotationLine.NumberOfTabs;

		public ZString Currency => quotationLine.Currency;

		public ZInt DecimalPlaces => quotationLine.DecimalPlaces;

		public MultilingualString Amount => quotationLine.Amount;

		public MultilingualString Units => quotationLine.Unit;

		public ZString Validity => quotationLine.Validity;

		public ZBool IsGSTApplicable => quotationLine.Master.MayGSTBeApplicable(IncoTerm.Code.IsEmpty ? (ZString)"ALL" : IncoTerm.Code);

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

		public RatingEntryWrapper ParentEntry => parentEntry ??= new RatingEntryWrapper(quotationLine.Master.Parent, Factory);
		RatingEntryWrapper parentEntry;

		public CodeAndDescriptionWrapper IncoTerm => incoTerm ??= new CodeAndDescriptionWrapper(GetIncoTerm(), Factory.GetCachedValue<IncoTermsCodeDescriptionPairList>(), Factory);
		CodeAndDescriptionWrapper incoTerm;

		public RatingAreaWrapper Origin => origin ??= new RatingAreaWrapper(GetDisplayLocation((e) => e.Origin()), Factory);
		RatingAreaWrapper origin;

		public RatingAreaWrapper Destination => destination ??= new RatingAreaWrapper(GetDisplayLocation(e => e.Destination()), Factory);
		RatingAreaWrapper destination;

		public RatingAreaWrapper Via => via ??= new RatingAreaWrapper(GetDisplayLocation(e => e.Via), Factory);
		RatingAreaWrapper via;

		#region Implementation

		ILocation GetDisplayLocation(Converter<RateEntry, ILocation> locator)
		{
			var pageLocation = GetMostSpecificLocationContainingAllEntryLocations(pricingPage, locator);
			var lineLocation = locator(quotationLine.Master.Parent);

			if (pageLocation == null)
			{
				return lineLocation;
			}
			else if (lineLocation == null || lineLocation.CompletelyCovers(pageLocation))
			{
				return pageLocation;
			}
			else
			{
				return lineLocation;
			}
		}

		ZString GetIncoTerm()
		{
			ZString result = "";

			foreach (var rateEntry in pricingPage.RateEntries)
			{
				var inco = GetIncoTermInIndustryTerm(rateEntry.TI_QuotePageIncoTerm);

				if (!inco.IsEmpty)
				{
					if (result.IsEmpty)
					{
						result = inco;
					}
					else if (result != inco)
					{
						return "";
					}
				}
			}

			return result.IsEmpty ? (ZString)"" : result;
		}

		static ZString GetIncoTermInIndustryTerm(ZString incoTerm)
		{
			switch (incoTerm)
			{
				case IncoTerms.FreeCarrier:
				case IncoTerms.FreeCarrierSeller:
				case IncoTerms.FreeCarrierBuyer:
					return Res.GetString("E740CE12-1934-48B6-B91F-945F2D67D512", "Free Carrier");
				default:
					return incoTerm;
			}
		}

		static ILocation GetMostSpecificLocationContainingAllEntryLocations(PricingPage pricingPage, Converter<RateEntry, ILocation> locator)
		{
			ILocation location = null;

			if (pricingPage != null)
			{
				foreach (var rateEntry in pricingPage.RateEntries)
				{
					var current = locator(rateEntry);

					if (current != null && location != current)
					{
						if (location == null || current.CompletelyCovers(location))
						{
							location = current;
						}
						else if (!location.CompletelyCovers(current))
						{
							location = location.Country;
							current = current.Country;

							if (location == null || current == null || location != current)
							{
								return null;
							}
						}
					}
				}
			}

			return location;
		}

		readonly PricingPage pricingPage;
		readonly QuotationLine quotationLine;

		#endregion
	}
}
