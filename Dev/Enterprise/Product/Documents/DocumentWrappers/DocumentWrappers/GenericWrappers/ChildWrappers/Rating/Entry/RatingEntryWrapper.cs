using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("Rate Entry")]
	public class RatingEntryWrapper : GenericWrapper
	{
		public RatingEntryWrapper(RateEntry entry, BusinessObjectFactory factory)
			: base(entry, factory) { }

		public ZString PageHeader
		{
			get { return Entry.PageHeader; }
		}

		public ZString Mode
		{
			get { return Entry.FreightType; }
		}

		public ZString DiscountDescription
		{
			get { return new CompanyTariffCodes().GetCompanyTariffDiscountDescription(Entry.TI_RateCategory); }
		}

		public ZString OverseasCountries
		{
			get
			{
				if (Entry.IsCrossTrade())
				{
					if (Entry.Origin() != null && Entry.Destination() != null && Entry.Origin().Country != null && Entry.Destination().Country != null)
					{
						return Entry.Origin().Country.Description + " - " + Entry.Destination().Country.Description;
					}
					else
					{
						return ZString.Empty;
					}
				}
				else if (Entry.IsDomestic())
				{
					return ZString.Empty;
				}
				else
				{
					return Entry.OverseasPort == null || Entry.OverseasPort.Country == null ? ZString.Empty : Entry.OverseasPort.Country.Description;
				}
			}
		}

		public ZString ContractNumber
		{
			get { return DocumentsDataRegistry.Instance.ShowContractNumbersOnShippingQuotationDocuments.Value ? Entry.TI_ContractNumber : ZString.Empty; }
		}

		public ZString DeliveryAddressPostCode
		{
			get { return Entry.TI_CartageDeliveryAddressPostCode; }
		}

		public ZString PickUpAddressPostCode
		{
			get { return Entry.TI_CartagePickupAddressPostCode; }
		}

		public ZDateTime ValidFrom
		{
			get { return Entry.IsQuote() ? Entry.Parent.TH_QuoteDate : Entry.TI_RateStartDate; }
		}

		public ZDateTime ValidUntil
		{
			get { return Entry.IsQuote() ? Entry.Parent.TH_QuoteEndDate : Entry.TI_RateEndDate; }
		}

		public ZBool IsSupplementary
		{
			get { return Entry.IsSupplementaryEntry(); }
		}

		public RatingDirectionWrapper Direction
		{
			get
			{
				if (direction == null)
				{
					ILocation local = Entry.Parent.Company.Country;
					direction = new RatingDirectionWrapper(local.CompletelyCovers(Entry.Origin()), local.CompletelyCovers(Entry.Destination()), Factory);
				}
				return direction;
			}
		}
		RatingDirectionWrapper direction;

		public RatingAreaWrapper Origin
		{
			get { return origin ?? (origin = new RatingAreaWrapper(Entry.TI_OriginLRC, Factory)); }
		}
		RatingAreaWrapper origin;

		public RatingAreaWrapper Destination
		{
			get { return destination ?? (destination = new RatingAreaWrapper(Entry.TI_DestinationLRC, Factory)); }
		}
		RatingAreaWrapper destination;

		public RatingAreaWrapper Via
		{
			get { return via ?? (via = new RatingAreaWrapper(Entry.TI_ViaLRC, Factory)); }
		}
		RatingAreaWrapper via;

		public OrganisationWrapper Provider
		{
			get
			{
				if (provider == null && IfDisplayProvider)
				{
					if (Entry.IsAirFreight())
					{
						provider = new OrganisationWrapper(OrganisationUsageType.Airline, Entry.TransportProvider, ContactType.ShippingLine, Factory);
					}
					else if (Entry.IsSeaFreight())
					{
						provider = new OrganisationWrapper(OrganisationUsageType.ShippingLine, Entry.TransportProvider, ContactType.ShippingLine, Factory);
					}
					else
					{
						provider = new OrganisationWrapper(OrganisationUsageType.Carrier, Entry.TransportProvider, ContactType.ShippingLine, Factory);
					}
				}
				return provider;
			}
		}
		OrganisationWrapper provider;

		public bool IfDisplayProvider
		{
			get
			{
				return (!Entry.IsAirFreight() || DocumentsDataRegistry.Instance.AirFreightIncludeTransportProviderOnQuotation.Value)
					&& (!Entry.IsSeaFreight() || DocumentsDataRegistry.Instance.SeaFreightIncludeTransportProviderOnQuotation.Value);
			}
		}

		public OrganisationWrapper Consignor
		{
			get { return consignor ?? (consignor = new OrganisationWrapper(OrganisationUsageType.Consignor, Entry.Consignor, ContactType.Consignor, Factory)); }
		}
		OrganisationWrapper consignor;

		public OrganisationWrapper Consignee
		{
			get { return consignee ?? (consignee = new OrganisationWrapper(OrganisationUsageType.Consignee, Entry.Consignee, ContactType.Consignee, Factory)); }
		}
		OrganisationWrapper consignee;

		public CodeAndDescriptionWrapper TransportMode
		{
			get
			{
				if (transportMode == null)
				{
					string mode;

					if (Entry.IsSea())
					{
						mode = Constants.TransportModes.Sea;
					}
					else if (Entry.IsAir())
					{
						mode = Constants.TransportModes.Air;
					}
					else if (Entry.IsRoad())
					{
						mode = Constants.TransportModes.Road;
					}
					else if (Entry.IsRail())
					{
						mode = Constants.TransportModes.Rail;
					}
					else
					{
						mode = string.Empty;
					}

					transportMode = new CodeAndDescriptionWrapper(mode, Factory.GetCachedValue<RatingEntryTransportMode>(), Factory);
				}

				return transportMode;
			}
		}
		CodeAndDescriptionWrapper transportMode;

		public CodeAndDescriptionWrapper ServiceLevel
		{
			get
			{
				if (serviceLevel == null)
				{
					if (Entry.IsCosting())
					{
						serviceLevel = new CodeAndDescriptionWrapper(Entry.TI_PL_NKCarrierServiceLevel, Entry.Lookups.CarrierServiceLevels, Factory);
					}
					else
					{
						serviceLevel = new CodeAndDescriptionWrapper(Entry.TI_RS_NKServiceLevel_NI, Entry.Lookups.ServiceLevel_NIs, Factory);
					}
				}

				return serviceLevel;
			}
		}
		CodeAndDescriptionWrapper serviceLevel;

		public CodeAndDescriptionWrapper CommodityCode
		{
			get
			{
				if (commodityCode == null)
				{
					commodityCode = new CodeAndDescriptionWrapper(Entry.TI_RH_NKCommodityCode, Entry.Lookups.CommodityCodes, Factory);
				}

				return commodityCode;
			}
		}
		CodeAndDescriptionWrapper commodityCode;

		public RatingFrequencyWrapper Frequency
		{
			get { return frequency ?? (frequency = new RatingFrequencyWrapper(Entry.TI_Frequency, Entry.TI_FrequencyUnit, Factory)); }
		}
		RatingFrequencyWrapper frequency;

		public RatingTransitTimeWrapper TransitTime
		{
			get { return transitTime ?? (transitTime = new RatingTransitTimeWrapper(Entry.TI_TransitTime, Factory)); }
		}
		RatingTransitTimeWrapper transitTime;

		internal bool MayGSTBeApplicable
		{
			get { return Entry.RateLines.Cast<RateLine>().Any(line => line.MayGSTBeApplicable(IncoTerm)); }
		}

		string IncoTerm
		{
			get { return Entry.TI_QuotePageIncoTerm.IsEmpty ? (ZString)"ALL" : Entry.TI_QuotePageIncoTerm; }
		}

		#region Implementation

		RateEntry Entry
		{
			get { return (RateEntry)WrappedBO; }
		}

		#endregion
	}
}
