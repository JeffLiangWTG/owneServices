using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Quotation;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RatingEnums;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocEntryQuotation : DocQuotation
	{
		protected DocEntryQuotation(PricingPage page, BusinessObjectFactory factoryToWrap)
			: base(page, factoryToWrap)
		{ }

		public new static DocEntryQuotation New(PricingPage page, BusinessObjectFactory factoryToWrap)
		{
			return page != null ? new DocEntryQuotation(page, factoryToWrap) : null;
		}

		public ZString Origin
		{
			get { return FirstEntry.Origin() == null ? ZString.Empty : FirstEntry.Origin().Description; }
		}

		public ZString OriginCode
		{
			get { return FirstEntry.TI_OriginLRC; }
		}

		public ZString Destination
		{
			get { return FirstEntry.Destination() == null ? ZString.Empty : FirstEntry.Destination().Description; }
		}

		public ZString DestinationCode
		{
			get { return FirstEntry.TI_DestinationLRC; }
		}

		public ZString OriginDestination
		{
			get
			{
				ZString orig = Origin;
				ZString dest = Destination;

				if (orig.IsEmpty && dest.IsEmpty)
				{
					return ZString.Empty;
				}
				else if (orig.IsEmpty)
				{
					return (NoResString)"to " + dest;
				}
				else if (dest.IsEmpty)
				{
					return (NoResString)"from " + orig;
				}
				else
				{
					return orig + (NoResString)" to " + dest;
				}
			}
		}

		public ZString TranshipmentPort => FirstEntry.Via?.Description ?? ZString.Empty;

		public ZString TranshipmentPortCode => FirstEntry.TI_ViaLRC;

		public ZString Provider
		{
			get
			{
				ZString result = ZString.Empty;
				RateEntry entry = FirstEntry;
				OrgHeader carrier = OneOffQuote == null ? entry.TransportProvider : OneOffQuote.Carrier;

				if (carrier != null)
				{
					if ((entry.IsAir() && DocumentsDataRegistry.Instance.AirFreightIncludeTransportProviderOnQuotation.Value) ||
						(entry.IsSea() && DocumentsDataRegistry.Instance.SeaFreightIncludeTransportProviderOnQuotation.Value))
					{
						result = carrier.OH_FullName;

						if (OneOffQuote == null && !entry.TransportProviderCarrierCode.IsEmpty)
						{
							result += " (" + entry.TransportProviderCarrierCode.Trim() + ")";
						}
					}
				}

				return result;
			}
		}

		public ZGuid TransportProviderPK
		{
			get { return FirstEntry.TI_OH_TransportProvider; }
		}

		public ZString ProviderHeading
		{
			get
			{
				ZString result = ZString.Empty;
				RateEntry entry = FirstEntry;

				if (!Provider.IsEmpty)
				{
					if ((entry.IsAir() && DocumentsDataRegistry.Instance.AirFreightIncludeTransportProviderOnQuotation.Value) ||
						(entry.IsSea() && DocumentsDataRegistry.Instance.SeaFreightIncludeTransportProviderOnQuotation.Value))
					{
						result = entry.IsAirFreight() ? Res.GetString("45a8c370-8e7f-47a3-9a78-9e564f909922", "Airline") : Res.GetString("8af40662-fc9b-4bcd-bea6-89194b6170d6", "Shipping Line");
						result += ":";
					}
				}

				return result;
			}
		}

		public ZString TransitTime
		{
			get
			{
				RateEntry entry = FirstEntry;
				ZString transit = entry.TI_TransitTime;

				if (transit.IsEmpty)
				{
					Quote quote = Factory.Load<RatingHeader>(entry.TI_TH) as Quote;

					if (quote != null && quote.CurrentOneOffQuote != null)
					{
						transit = quote.CurrentOneOffQuote.TT_TransitTime;
					}
				}

				switch (transit)
				{
					case RatingConstants.TransitTimes.Overnight:
						return Res.GetString("1e8cfad5-8a28-4d9b-adc4-432a3f105080", "Overnight");

					case RatingConstants.TransitTimes.SameDay:
						return Res.GetString("7495c9cb-37e0-4ad9-a086-023a18379c14", "Same Day");

					case "1":
						return Res.GetString("768d8778-45cd-4841-8d5d-785b0a9b9077", "1 Day");

					case "":
						return "";

					default:
						return Res.GetString("41afcb06-1159-402a-bf7d-3612a76c1540", "{0} Days", transit);
				}
			}
		}

		public ZString Frequency
		{
			get
			{
				RateEntry entry = FirstEntry;
				Quote quote = Factory.Load<RatingHeader>(entry.TI_TH) as Quote;

				if (entry.TI_Frequency != 0 || (quote != null && quote.CurrentOneOffQuote != null && quote.CurrentOneOffQuote.TT_Frequency != 0))
				{
					ZString freqUnit = entry.TI_FrequencyUnit;
					ZInt freq = entry.TI_Frequency;

					if (freqUnit.IsEmpty)
					{
						if (quote != null)
						{
							freqUnit = quote.CurrentOneOffQuote.TT_FrequencyUnit;
							freq = quote.CurrentOneOffQuote.TT_Frequency;
						}
					}

					switch (freqUnit.ToUpper())
					{
						case RatingConstants.FrequencyUnits.Daily:
							return Res.GetString("e33222ed-3a67-4947-beaa-3c5a26a57e2d", "{0} per Day", freq);

						case RatingConstants.FrequencyUnits.Days:
							return Res.GetString("67bfa938-b1b6-411a-a853-603f592df26a", "Every {0}", freq) + " " + ((freq <= 1) ? Res.GetString("5089da46-3e52-4d4a-bb8e-aebc335ef6fc", "Day") : Res.GetString("9e39eef6-753d-4d29-90da-ab76db4b2725", "Days"));

						case RatingConstants.FrequencyUnits.Week:
							return Res.GetString("f6aa8945-8375-42c4-84de-3752cd766f8a", "{0} per Week", freq);

						case RatingConstants.FrequencyUnits.Fortnight:
							return Res.GetString("8a6cfa0f-a455-4cad-aca3-bafce87db0b3", "{0} per Fortnight", freq);

						case RatingConstants.FrequencyUnits.Monthly:
							return Res.GetString("856d6574-28fc-461d-aa6b-9faf9c138a88", "{0} per Month", freq);
					}
				}
				return ZString.Empty;
			}
		}

		public ZString PageHeading
		{
			get
			{
				QuoteEntry quoteEntry = (QuoteEntry)FirstEntry;
				ZString result = quoteEntry.PageHeader.IsEmpty ? quoteEntry.QuotationHeader : quoteEntry.PageHeader;

				if (!IncoTerm.IsEmpty)
				{
					result += " (" + IncoTerm + ")";
				}

				return result;
			}
		}

		public ZDecimal ValueOfGoods
		{
			get { return OneOffQuote != null ? OneOffQuote.TT_ValueOfGoods : ZDecimal.Zero; }
		}

		public ZString ValueOfGoodsCurrency
		{
			get { return OneOffQuote != null && OneOffQuote.GoodsCurrency != null ? OneOffQuote.GoodsCurrency.RX_Code : ZString.Empty; }
		}

		public ZString InsuranceValue
		{
			get
			{
				ZDecimal result = OneOffQuote != null ? OneOffQuote.TT_InsureVal : ZDecimal.Zero;
				return result.IsEmpty ? string.Empty : result.ToString("n2");
			}
		}

		public ZString InsuranceValueCurrency
		{
			get { return OneOffQuote != null && OneOffQuote.InsureValCurr != null ? OneOffQuote.InsureValCurr.RX_Code : ZString.Empty; }
		}

		public ZInt CustomsEntryCount
		{
			get { return OneOffQuote != null ? OneOffQuote.TT_NumberOfEntries : (ZShort)0; }
		}

		public ZInt CustomsLinesCount
		{
			get { return OneOffQuote != null ? OneOffQuote.TT_NumberOfEntryLines : (ZShort)0; }
		}

		public ZString Weight
		{
			get { return OneOffQuote != null ? (ZString)(OneOffQuote.TT_ActualWeight.ToString(3) + " " + OneOffQuote.TT_UnitOfWeight) : ZString.Empty; }
		}

		public ZString Volume
		{
			get { return OneOffQuote != null ? (ZString)(OneOffQuote.TT_ActualVolume.ToString(3) + " " + OneOffQuote.TT_UnitOfVolume) : ZString.Empty; }
		}

		public ZString Chargeable
		{
			get { return OneOffQuote != null ? (ZString)(OneOffQuote.TT_Chargeable.ToString(3) + " " + OneOffQuote.TT_ChargeableUnit) : ZString.Empty; }
		}

		public DocOneOffContainerCollection OneOffContainers
		{
			get
			{
				if (OneOffQuote != null)
				{
					return new DocOneOffContainerCollection(OneOffQuote.Containers, Factory);
				}
				else
				{
					return new DocOneOffContainerCollection(Factory);
				}
			}
		}

		public DocOneOffPackLineCollection OneOffLooseCargo
		{
			get
			{
				if (OneOffQuote != null)
				{
					return new DocOneOffPackLineCollection(OneOffQuote.LooseCargo, Factory);
				}
				else
				{
					return new DocOneOffPackLineCollection(Factory);
				}
			}
		}

		public DocJobInvoicingJobChargeCollection OneOffQuoteCharges
		{
			get
			{
				DocJobInvoicingJobChargeCollection result = new DocJobInvoicingJobChargeCollection(Factory);

				if (Header != null)
				{
					QuotedBooking quotedBooking;
					DocJobInvoicingJob docJob;
					DocJobInvoicingJobChargeCollection charges;

					var viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, Header.PK));

					if (viewQuotedBooking != null &&
						(quotedBooking = QuotedBooking.New(viewQuotedBooking, Factory)) != null &&
						(docJob = DocJobInvoicingJob.New((Job)quotedBooking.Job, Factory)) != null &&
						(charges = docJob.Charges) != null)
					{
						foreach (DocJobInvoicingJobCharge charge in charges)
						{
							if (charge.ShowChargeOnQuotation)
							{
								result.Add(charge);
							}
						}
					}
				}

				return result;
			}
		}

		public ZDecimal TotalChargesAmount
		{
			get
			{
				ZDecimal result = 0;

				foreach (DocJobInvoicingJobCharge charge in OneOffQuoteCharges)
				{
					result += charge.LocalSellAmt;
				}

				return result;
			}
		}

		public ZBool ShowLocalCurrencyonSpotQuotePricingPage
		{
			get { return DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage.Value; }
		}

		public DocJobExchangeRateCollection OneOffQuoteExchangeRates
		{
			get
			{
				if (Header != null)
				{
					ViewQuotedBooking viewQuotedBooking;
					QuotedBooking quotedBooking;
					DocJobInvoicingJob docJob;

					if ((viewQuotedBooking = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_TH, Header.PK))) != null &&
						(quotedBooking = QuotedBooking.New(viewQuotedBooking, Factory)) != null &&
						(docJob = DocJobInvoicingJob.New((Job)quotedBooking.Job, Factory)) != null)
					{
						return docJob.ExchangeRates;
					}
				}

				return new DocJobExchangeRateCollection(Factory);
			}
		}

		public ZString PickUpAddress
		{
			get { return OneOffQuote != null && OneOffQuote.PickUpDocAddress != null ? OneOffQuote.PickUpDocAddress.AddressAsASingleLine : ZString.Empty; }
		}

		public ZString DeliveryAddress
		{
			get { return OneOffQuote != null && OneOffQuote.DeliveryDocAddress != null ? OneOffQuote.DeliveryDocAddress.AddressAsASingleLine : ZString.Empty; }
		}

		public ZBool IsAirFreight
		{
			get { return FirstEntry.IsAirFreight(); }
		}

		public ZBool IsLCLFreight
		{
			get { return FirstEntry.IsLCLFreight(); }
		}

		public ZBool IsFCLFreight
		{
			get { return FirstEntry.TI_RateCategory == RatingConstants.RateCategory.FCL; }
		}

		public ZBool IsSeaFreight
		{
			get { return FirstEntry.IsSeaFreight(); }
		}

		public ZBool IsRoadFreight
		{
			get { return FirstEntry.IsRoadFreight(); }
		}

		public ZBool IsRailFreight
		{
			get { return FirstEntry.IsRailFreight(); }
		}

		public ZBool IsFreightEntry
		{
			get { return FirstEntry.IsFreightEntry(); }
		}

		public ZBool IsFCL
		{
			get { return FirstEntry.IsFCL(); }
		}

		public ZString IncoTerm
		{
			get { return OneOffQuote != null ? OneOffQuote.TT_IncoTerm : FirstEntry.TI_QuotePageIncoTerm; }
		}

		public ZString IncoTermDescription
		{
			get { return IncoTerms.GetDescriptionFromCode(IncoTerm); }
		}

		public DocQuotationLineCollection FreightDocRateLineItems
		{
			get
			{
				if (freightDocRateLineItems == null)
				{
					freightDocRateLineItems = new DocQuotationLineCollection(this, new PricingPageRateLineFactory(EntryTypes.Freight), Page.RateEntries, Factory);

					if (Header != null)
					{
						freightDocRateLineItems.Load();
					}
				}

				return freightDocRateLineItems;
			}
		}
		DocQuotationLineCollection freightDocRateLineItems;

		#region Implementation

		internal ILocation OriginObj
		{
			get { return FirstEntry.Origin(); }
		}

		internal ILocation DestinationObj
		{
			get { return FirstEntry.Destination(); }
		}

		internal ILocation ViaObj
		{
			get { return FirstEntry.Via; }
		}

		protected override bool IsGSTApplicable
		{
			get
			{
				foreach (DocRateLineItem item in OriginDocRateLineItems)
				{
					if (item.ChargeIsGSTApplicable)
					{
						return true;
					}
				}

				foreach (DocRateLineItem item in FreightDocRateLineItems)
				{
					if (item.ChargeIsGSTApplicable)
					{
						return true;
					}
				}

				foreach (DocRateLineItem item in DestinationDocRateLineItems)
				{
					if (item.ChargeIsGSTApplicable)
					{
						return true;
					}
				}

				foreach (DocJobInvoicingJobCharge item in OneOffQuoteCharges)
				{
					if (item.ChargeIsGSTApplicable)
					{
						return true;
					}
				}

				return false;
			}
		}

		RateOneOffShipment OneOffQuote
		{
			get
			{
				Quote quote = Header as Quote;
				return quote != null && quote.TH_OneTimeQuote ? quote.CurrentOneOffQuote : null;
			}
		}

		CodeDescriptionPairList IncoTerms
		{
			get { return incoTerms ?? (incoTerms = new IncoTermsCodeDescriptionPairList()); }
		}
		CodeDescriptionPairList incoTerms;

		#endregion
	}
}
