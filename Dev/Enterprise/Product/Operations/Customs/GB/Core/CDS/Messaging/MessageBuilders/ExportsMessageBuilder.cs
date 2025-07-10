using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging.Wrappers;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public abstract class ExportsMessageBuilder : MessageBuilder
	{
		protected ExportsMessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCodeNewAmendDelete)
			: base(cusEntryHeader, errorCollector, functionCodeNewAmendDelete)
		{
		}

		protected override IDeclaration GetCdsDeclarationFromEntry()
		{
			return new GbCDSExportDeclarationWrapper(cusEntryHeader);
		}

		protected override void PopulateConsignment()
		{
			var consignmentProvider = Wrapper.Consignment;
			if (consignmentProvider != null)
			{
				var consignment = new DeclarationConsignment();

				PopulateCarrier(consignment, consignmentProvider.Carrier);
				PopulateFreight(consignment, consignmentProvider.FreightPaymentMethodCode);
				PopulateItinerary(consignment, consignmentProvider.ItineraryRoutingCountryCodes);
				PopulateConsignor(consignment, consignmentProvider.Consignor);

				decMessage.Consignment = consignment;
			}
		}

		void PopulateCarrier(DeclarationConsignment consignment, IOrganisation carrierProvider)
		{
			if (carrierProvider != null)
			{
				if (!carrierProvider.ID.IsEmpty)
				{
					consignment.Carrier = new DeclarationConsignmentCarrier
					{
						ID = new CarrierIdentificationIDType { Value = carrierProvider.ID }
					};
				}
				else
				{
					consignment.Carrier = new DeclarationConsignmentCarrier
					{
						Name = new CarrierNameTextType { Value = carrierProvider.Name },
						Address = GetIOrgAddress<DeclarationConsignmentCarrierAddress>(carrierProvider)
					};
				}
			}
		}

		void PopulateFreight(DeclarationConsignment consignment, ZString freightPaymentMethodCode)
		{
			if (!freightPaymentMethodCode.IsEmpty)
			{
				consignment.Freight = new DeclarationConsignmentFreight
				{
					PaymentMethodCode = new FreightPaymentMethodCodeType
					{
						Value = freightPaymentMethodCode
					}
				};
			}
			else
			{
				PopulateConsignmentConsignmentItems(consignment);
			}
		}

		void PopulateConsignmentConsignmentItems(DeclarationConsignment consignment)
		{
			var consinmentItems = new List<DeclarationConsignmentConsignmentItem>();

			int sequence = 1;

			foreach (var goodsItem in GoodsItems)
			{
				if (!goodsItem.TransportChargesMethodOfPayment.IsEmpty)
				{
					var consignItem = new DeclarationConsignmentConsignmentItem();
					consignItem.SequenceNumeric = sequence;

					PopulateConsignmentItemFreight(consignItem, goodsItem.TransportChargesMethodOfPayment);

					consinmentItems.Add(consignItem);
					sequence++;
				}
			}

			if (consinmentItems.Any())
			{
				consignment.ConsignmentItem = consinmentItems.ToArray();
			}
		}

		void PopulateConsignmentItemFreight(DeclarationConsignmentConsignmentItem consignmentItem, ZString freightPaymentMethodCode)
		{
			if (!freightPaymentMethodCode.IsEmpty)
			{
				consignmentItem.Freight = new DeclarationConsignmentConsignmentItemFreight()
				{
					PaymentMethodCode = new FreightPaymentMethodCodeType
					{
						Value = freightPaymentMethodCode
					}
				};
			}
		}

		void PopulateItinerary(DeclarationConsignment consignment, IEnumerable<ZString> itineraryRoutingCountryCodes)
		{
			var codes = itineraryRoutingCountryCodes?.ToArray() ?? Array.Empty<ZString>();
			if (codes.Any())
			{
				consignment.Itinerary = codes.Select((x, i) => new DeclarationConsignmentItinerary
				{
					SequenceNumeric = i + 1,
					RoutingCountryCode = new ItineraryRoutingCountryCodeType
					{
						Value = x
					}
				}).ToArray();
			}
		}

		protected virtual void PopulateConsignor(DeclarationConsignment consignment, IOrganisation consignorProvider)
		{
		}

		protected override void PopulateExitOffice()
		{
			decMessage.ExitOffice = new DeclarationExitOffice
			{
				ID = new ExitOfficeIdentificationIDType { Value = Wrapper.ExitOfficeID }
			};
		}

		//not needed except for SASP or centralised clearance
		protected override void PopulatePresentationOffice()
		{
		}

		protected override void PopulateAcceptanceDateTime()
		{
		}

		protected override void PopulateBorderTransportMeans()
		{
			if (BorderTransportMeans != null)
			{
				var borderTransportMeans = BorderTransportMeans == null ? new DeclarationBorderTransportMeans() : new DeclarationBorderTransportMeans
				{
					IdentificationTypeCode = new BorderTransportMeansIdentificationTypeCodeType { Value = BorderTransportMeans.IdentificationTypeCode },
					ID = new BorderTransportMeansIdentificationIDType { Value = BorderTransportMeans.ID },
					RegistrationNationalityCode = new BorderTransportMeansRegistrationNationalityCodeType { Value = BorderTransportMeans.RegistrationNationalityCode }
				};
				PopulateBorderTransportMeansModeCode(borderTransportMeans);
				decMessage.BorderTransportMeans = borderTransportMeans;
			}
		}

		void PopulateBorderTransportMeansModeCode(DeclarationBorderTransportMeans borderTransportMeans)
		{
			var borderTransportMeansModeCode = BorderTransportMeans?.ModeCode ?? ZString.Empty;
			borderTransportMeans.ModeCode = new BorderTransportMeansModeCodeType { Value = borderTransportMeansModeCode };
		}

		protected override void PopulateObligationGuarantees()
		{
			var obligationGuarantees = new List<DeclarationObligationGuarantee>();
			if (ObligationGuarantees != null)
			{
				foreach (var obligationGuarantee in ObligationGuarantees)
				{
					obligationGuarantees.Add(new DeclarationObligationGuarantee
					{
						SecurityDetailsCode = new ObligationGuaranteeSecurityDetailsCodeType { Value = obligationGuarantee.SecurityDetailsCode },
						ID = new ObligationGuaranteeIdentificationIDType { Value = obligationGuarantee.ID },
						ReferenceID = new ObligationGuaranteeReferenceIDType { Value = obligationGuarantee.ReferenceID },
						AccessCode = new ObligationGuaranteeAccessCodeType { Value = obligationGuarantee.AccessCode },
						AmountAmount = new ObligationGuaranteeAmountAmountType
						{
							currencyID = obligationGuarantee.AmountAmount?.Currency ?? ZString.Empty,
							Value = obligationGuarantee.AmountAmount?.Amount ?? ZDecimal.Zero
						},
						GuaranteeOffice = new DeclarationObligationGuaranteeGuaranteeOffice
						{
							ID = new GuaranteeOfficeIdentificationIDType
							{
								Value = obligationGuarantee.GuaranteeOfficeId
							}
						}
					});
				}
			}

			decMessage.ObligationGuarantee = obligationGuarantees.ToArray();
		}

		protected override void PopulateDecAdditionalDocuments()
		{
		}

		protected override void PopulateImporter()
		{
		}

		protected override void PopulateConsignee()
		{
			var consignee = Importer == null ? new DeclarationGoodsShipmentConsignee() : new DeclarationGoodsShipmentConsignee
			{
				ID = new ConsigneeIdentificationIDType { Value = Importer.ID },
				Name = new ConsigneeNameTextType { Value = Importer.Name.SubstringSafe(0, 35) },
				Address = GetIOrgAddress<DeclarationGoodsShipmentConsigneeAddress>(Importer)
			};
			decShipment.Consignee = consignee;
		}

		protected override void PopulateTradeTerms()
		{
		}

		protected override void PopulateCustomsValuations()
		{
		}

		protected override void PopulateExportCountryID()
		{
			var exportCountryID = ExportCountryID.IsEmpty ? new DeclarationGoodsShipmentExportCountry() : new DeclarationGoodsShipmentExportCountry
			{
				ID = new ExportCountryCountryCodeType { Value = ExportCountryID }
			};
			decShipment.ExportCountry = exportCountryID;
		}

		protected override void PopulateExportCountryID(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateValuationAdjustmentAdditionCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateItemConsignor(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateCustomsValuations(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateDutyTaxFee(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
		}

		protected override void PopulateDangerousGoods(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
			var undgid = giCommodity.UNDGID;
			if (!undgid.IsEmpty)
			{
				commodity.DangerousGoods = new[]
				{
					new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDangerousGoods
					{
						UNDGID = new DangerousGoodsUNDGIDType
						{
							Value = undgid
						}
					}
				};
			}
		}

		protected override void PopulateArrivalTransportMeans(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
		}

		protected override void PopulateDepartureTransportMeans(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
			if (IsDepartureTransportMeansDetailsRequired)
			{
				var departureTransportMeans = consignmentData.DepartureTransportMeans;
				consignment.DepartureTransportMeans = new DeclarationGoodsShipmentConsignmentDepartureTransportMeans
				{
					ID = new DepartureTransportMeansIdentificationIDType { Value = departureTransportMeans?.ID },
					IdentificationTypeCode = new DepartureTransportMeansIdentificationTypeCodeType { Value = departureTransportMeans?.IdentificationTypeCode },
					ModeCode = new DepartureTransportMeansModeCodeType { Value = departureTransportMeans?.ModeCode }
				};
			}
		}

		protected override DeclarationGoodsShipmentConsignmentTransportEquipmentSeal[] PopulateSeals(ITransportEquipment transportEquipment)
		{
			return GetSeals(transportEquipment).Select(x => new DeclarationGoodsShipmentConsignmentTransportEquipmentSeal { ID = new SealIdentificationIDType { Value = x } }).ToArray();
		}

		protected bool IsDepartureTransportMeansDetailsRequired => IsArrivalTransportMeansDetailsRequired;

		protected override void SetItemGrossMassUnit(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure goodsMeasure)
		{
			base.SetItemGrossMassUnit(goodsMeasure);

			if (goodsMeasure.GrossMassMeasure != null)
			{
				goodsMeasure.GrossMassMeasure.unitCode = "KGM";
			}
		}

		protected override void PopulateTransactionNatureCode()
		{
		}

		protected override DeclarationGoodsShipmentConsignmentTransportEquipment[] CreateTransportsForHeaderContainers()
		{
			int sequence = 1;
			var seals = Consignment.TransportEquipments?.SelectMany(x => PopulateSeals(x)).ToList();

			if (!seals.Any())
			{
				seals.Add(new DeclarationGoodsShipmentConsignmentTransportEquipmentSeal { ID = new SealIdentificationIDType { Value = Constants.MessagingCodes.NoSeals } });
			}

			seals.ForEach(x => x.SequenceNumeric = sequence++);

			return new[]
			{
				new DeclarationGoodsShipmentConsignmentTransportEquipment
				{
					Seal = seals.ToArray(),
					SequenceNumeric = 1
				}
			};
		}

		protected override void PopulateMarksNumbersID(DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging goodsItemPackaging, IPackaging packaging)
		{
			goodsItemPackaging.MarksNumbersID = new PackagingMarksNumbersIDType { Value = packaging.MarksNumbersID.StripNewlineCharacters(42) };
		}

		protected override ZInt AddressLineLength => 35;
	}
}
