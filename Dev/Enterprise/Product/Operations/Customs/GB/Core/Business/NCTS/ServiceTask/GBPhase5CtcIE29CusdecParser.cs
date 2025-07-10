using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc029c;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.ctypes;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.custom_ctypes;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;

namespace Enterprise.Customs.GB.Business
{
	public class GBPhase5CtcIE29CusdecParser : NctsIE29CusdecParser
	{
		public override NctsIE29CusdecResponseData Parse()
		{
			if (!(EdiMessage?.EM_MessageText ?? ZString.Empty).IsEmpty)
			{
				var message = EdiMessage.EM_MessageText;
				xmlMessage = CTCExtensions.Deserialize<Cc029CType>(message);
				var responseData = new NctsIE29CusdecResponseData(Factory);
				ie29ResponseData = responseData;

				if (xmlMessage.TransitOperation != null)
				{
					responseData.LocalReferenceNumber = xmlMessage.TransitOperation.Lrn;
					responseData.MovementReferenceNumber = xmlMessage.TransitOperation.Mrn;
					responseData.DeclarationType = xmlMessage.TransitOperation.DeclarationType;
					responseData.AcceptanceDate = xmlMessage.TransitOperation.DeclarationAcceptanceDate.ToString("yyyy-MM-dd");
					responseData.IssuingDate = xmlMessage.TransitOperation.ReleaseDate.ToString("yyyy-MM-dd");
					responseData.DialogLanguageIndicatorAtDeparture = xmlMessage.TransitOperation.CommunicationLanguageAtDeparture;
					responseData.NctsAccompanyingDocumentLanguageCode = xmlMessage.TransitOperation.CommunicationLanguageAtDeparture;
					responseData.BindingItinerary = xmlMessage.TransitOperation.BindingItinerary == CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.tcl.Flag.Item1 ? "1" : "0";
					responseData.IsSecurity = !string.IsNullOrEmpty(xmlMessage.TransitOperation.Security) && xmlMessage.TransitOperation.Security != "0";
				}
				if (xmlMessage.Consignment != null)
				{
					responseData.CountryOfDestination = xmlMessage.Consignment.CountryOfDestination;
					if (xmlMessage.Consignment.LocationOfGoods != null)
					{
						responseData.AgreedLocationOfGoods = xmlMessage.Consignment.LocationOfGoods.TypeOfLocation +
							xmlMessage.Consignment.LocationOfGoods.QualifierOfIdentification +
							xmlMessage.Consignment.LocationOfGoods.AuthorisationNumber;
					}
					responseData.PlaceOfLoadingCode = xmlMessage.Consignment.PlaceOfLoading?.Location;
					responseData.CountryOfDispatch = xmlMessage.Consignment.PlaceOfLoading?.Country;
					responseData.InlandTransportMode = xmlMessage.Consignment.InlandModeOfTransport;
					responseData.TransportModeAtBorder = xmlMessage.Consignment.ModeOfTransportAtTheBorder;
					if (xmlMessage.Consignment.ActiveBorderTransportMeans != null && xmlMessage.Consignment.ActiveBorderTransportMeans.Count > 0)
					{
						var means = xmlMessage.Consignment.ActiveBorderTransportMeans[0];
						if (means != null)
						{
							responseData.MeansOfTransportCrossingBorderIdentity = means.IdentificationNumber;
							responseData.MeansOfTransportCrossingBorderNationality = means.Nationality;
							responseData.MeansOfTransportCrossingBorderType = means.TypeOfIdentification;
							responseData.ConveyanceReferenceNumber = means.ConveyanceReferenceNumber;
						}
					}
					if (xmlMessage.Consignment.DepartureTransportMeans != null && xmlMessage.Consignment.DepartureTransportMeans.Count > 0)
					{
						var means = xmlMessage.Consignment.DepartureTransportMeans[0];
						if (means != null)
						{
							responseData.MeansOfTransportAtDepartureIdentity = means.IdentificationNumber;
							responseData.MeansOfTransportAtDepartureNationality = means.Nationality;
						}
					}
					responseData.IsContainerised = xmlMessage.Consignment.ContainerIndicator == CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.tcl.Flag.Item1;

					if (xmlMessage.Consignment.HouseConsignment != null)
					{
						var allGoodsItems = xmlMessage.Consignment.HouseConsignment.Where(hc => hc != null && hc.ConsignmentItem != null)
							.SelectMany(hc => hc.ConsignmentItem.Where(ci => ci != null));
						responseData.TotalNumberOfItems = allGoodsItems.Count().ToString(NumberFormatInfo.InvariantInfo);
						responseData.TotalNumberOfPackages = allGoodsItems.Sum(
							x => x.Packaging.Sum(y => SafeNumber(y.NumberOfPackages))).ToString(NumberFormatInfo.InvariantInfo);
					}

					responseData.TotalGrossMassInKilograms = xmlMessage.Consignment.GrossMass.ToString(NumberFormatInfo.InvariantInfo);
					responseData.CommercialReferenceNumber = xmlMessage.Consignment.ReferenceNumberUcr;
					responseData.PlaceOfUnloadingCode = xmlMessage.Consignment.PlaceOfUnloading?.Location;
					if (xmlMessage.Consignment.Carrier != null)
					{
						responseData.Carrier = new AddressResponseData { GovRegNum = xmlMessage.Consignment.Carrier.IdentificationNumber };
					}
					responseData.ReferenceNumberUCR = xmlMessage.Consignment.ReferenceNumberUcr;
				}
				if (xmlMessage.Authorisation != null && xmlMessage.Authorisation.Count > 0)
				{
					responseData.AuthorisationId = xmlMessage.Authorisation[0].ReferenceNumber;
				}
				responseData.DeclarationDate = xmlMessage.PreparationDateAndTime.ToString("yyyy-MM-dd");

				responseData.Principal = CreateAddressResponseData(xmlMessage.HolderOfTheTransitProcedure?.IdentificationNumber,
					xmlMessage.HolderOfTheTransitProcedure?.Name, xmlMessage.HolderOfTheTransitProcedure?.Address);
				responseData.Consignor = CreateAddressResponseData(xmlMessage.Consignment?.Consignor?.IdentificationNumber,
					xmlMessage.Consignment?.Consignor?.Name, xmlMessage.Consignment?.Consignor?.Address);
				responseData.Consignee = CreateAddressResponseData(xmlMessage.Consignment?.Consignee?.IdentificationNumber,
					xmlMessage.Consignment?.Consignee?.Name, xmlMessage.Consignment?.Consignee?.Address);

				responseData.DepartureCustomsOfficeCode = xmlMessage.CustomsOfficeOfDeparture?.ReferenceNumber;

				responseData.TransitCustomsOffices.AddRange(GetTransitOffices());

				responseData.DestinationCustomsOfficeCode = xmlMessage.CustomsOfficeOfDestinationDeclared?.ReferenceNumber;

				if (xmlMessage.ControlResult != null)
				{
					responseData.ControlResultControlDate = xmlMessage.ControlResult.Date.ToString("yyyy-MM-dd");
					responseData.ControlResultControlResultCode = xmlMessage.ControlResult.Code;
					responseData.ControlResultControlledBy = xmlMessage.ControlResult.ControlledBy;
					responseData.ControlResultTimeLimit = xmlMessage.ControlResult.Date.ToString("yyyy-MM-dd");
				}

				responseData.RepresentativeName = xmlMessage.Representative?.ContactPerson?.Name;
				responseData.RepresentativeCapacity = xmlMessage.Representative?.Status;
				responseData.Representative = new RepresentativeResponseData
				{
					IdentificationNumber = xmlMessage.Representative?.IdentificationNumber,
					Status = xmlMessage.Representative?.Status,
				};

				responseData.Guarantees.AddRange(GetGuarantees());
				responseData.GoodsItems.AddRange(GetGoodsItemsResponseData());
				responseData.Seals.AddRange(GetSeals());
				responseData.Itinerary = string.Join(" ", GetItinerary());
				responseData.Authorisations.AddRange(GetAuthorisations());

				if (responseData.IsSecurity)
				{
					responseData.SecurityConsignor = CreateAddressResponseData(xmlMessage.Consignment?.Consignor?.IdentificationNumber,
						xmlMessage.Consignment?.Consignor?.Name, xmlMessage.Consignment?.Consignor?.Address);
					responseData.SecurityConsignee = CreateAddressResponseData(xmlMessage.Consignment?.Consignee?.IdentificationNumber,
						xmlMessage.Consignment?.Consignee?.Name, xmlMessage.Consignment?.Consignee?.Address);
				}
			}
			return ie29ResponseData;
		}

		AddressResponseData CreateAddressResponseData(string id, string name, IXMLAddress address)
		{
			return new AddressResponseData
			{
				GovRegNum = id,
				CompanyName = name,
				Address1 = address?.StreetAndNumber,
				Postcode = address?.Postcode,
				City = address?.City,
				CountryCode = address?.Country,
			};
		}

		IEnumerable<CustomsOfficeResponseData> GetTransitOffices()
		{
			return xmlMessage.CustomsOfficeOfTransitDeclared?
				.Where(x => x != null)
				.OrderBy(x => SafeNumber(x.SequenceNumber))
				.Select(locSegment => new CustomsOfficeResponseData
				{
					OfficeCode = locSegment.ReferenceNumber,
				})
				?? Enumerable.Empty<CustomsOfficeResponseData>();
		}

		IEnumerable<GuaranteeResponseData> GetGuarantees()
		{
			GuaranteeResponseData CreateGuaranteeResponse(GuaranteeType03 guaranteeHeader)
			{
				var guarantee = new GuaranteeResponseData
				{
					GuaranteeType = guaranteeHeader.GuaranteeType,
					OtherGuaranteeReference = guaranteeHeader.OtherGuaranteeReference,
				};
				var guarefref = guaranteeHeader.GuaranteeReference;
				if (guarefref != null && guarefref.Count > 0)
				{
					var lastGuaRef = guarefref[guarefref.Count - 1];
					guarantee.GuaranteeReferenceNumber = lastGuaRef.Grn;
					guarantee.AccessCode = lastGuaRef.AccessCode;
				}
				return guarantee;
			}

			return xmlMessage.Guarantee?
				.Where(x => x != null)
				.OrderBy(x => SafeNumber(x.SequenceNumber))
				.Select(CreateGuaranteeResponse)
				?? Enumerable.Empty<GuaranteeResponseData>();
		}

		IEnumerable<NctsGoodsItemResponseData> GetGoodsItemsResponseData()
		{
			if (xmlMessage.Consignment?.HouseConsignment != null)
			{
				var orderedHouseConsignments = xmlMessage.Consignment.HouseConsignment.Where(hc => hc != null && hc.ConsignmentItem != null)
					.OrderBy(hc => SafeNumber(hc.SequenceNumber));
				foreach (var houseConsignment in orderedHouseConsignments)
				{
					var orderedGoodsItems = houseConsignment.ConsignmentItem.Where(ci => ci != null).OrderBy(ci => SafeNumber(ci.GoodsItemNumber));
					foreach (var goods in orderedGoodsItems)
					{
						var goodsItem = new NctsGoodsItemResponseData(ie29ResponseData);
						goodsItem.ItemNumber = goods.DeclarationGoodsItemNumber;
						goodsItem.CommodityCode = goods.Commodity?.CommodityCode?.HarmonizedSystemSubHeadingCode;
						goodsItem.DeclarationType = goods.DeclarationType;
						goodsItem.DescriptionOfGoods = goods.Commodity?.DescriptionOfGoods;
						goodsItem.GrossMassInKilograms = (goods.Commodity?.GoodsMeasure?.GrossMass)?.ToString(NumberFormatInfo.InvariantInfo);
						goodsItem.NetMassInKilograms = goods.Commodity?.GoodsMeasure?.NetMass?.ToString(NumberFormatInfo.InvariantInfo);
						goodsItem.CountryOfDispatch = goods.CountryOfDispatch;
						goodsItem.CountryOfDestination = goods.CountryOfDestination;
						goodsItem.TransportChargesMoP = goods.TransportCharges?.MethodOfPayment;
						goodsItem.CommercialReferenceNumber = goods.ReferenceNumberUcr;
						goodsItem.PreviousDocuments.AddRange(GetPreviousDocuments(goods));
						goodsItem.SupportingDocuments.AddRange(GetSupportingDocuments(goods));

						if (houseConsignment.Consignor != null)
						{
							goodsItem.Consignor = CreateAddressResponseData(houseConsignment.Consignor.IdentificationNumber, houseConsignment.Consignor.Name, houseConsignment.Consignor.Address);
						}
						if (goods.Consignee != null)
						{
							goodsItem.Consignee = CreateAddressResponseData(goods.Consignee.IdentificationNumber, goods.Consignee.Name, goods.Consignee.Address);
						}

						goodsItem.ContainerNumbers.AddRange(GetContainers(goods));

						goodsItem.Packages.AddRange(GetPackages(goods));

						if (ie29ResponseData.IsSecurity)
						{
							if (houseConsignment.Consignor != null)
							{
								goodsItem.SecurityConsignor = CreateAddressResponseData(houseConsignment.Consignor.IdentificationNumber, houseConsignment.Consignor.Name, houseConsignment.Consignor.Address);
							}
							if (goods.Consignee != null)
							{
								goodsItem.SecurityConsignee = CreateAddressResponseData(goods.Consignee.IdentificationNumber, goods.Consignee.Name, goods.Consignee.Address);
							}
						}

						yield return goodsItem;
					}
				}
			}
		}

		IEnumerable<PreviousDocumentResponseData> GetPreviousDocuments(CustomConsignmentItemType03 goods)
		{
			return goods.PreviousDocument?
				.Where(x => x != null)
				.OrderBy(x => SafeNumber(x.SequenceNumber))
				.Select(doc => new PreviousDocumentResponseData
				{
					TypeCode = doc.Type,
					Reference = doc.ReferenceNumber,
					MoreInfo = doc.ComplementOfInformation,
				})
				?? Enumerable.Empty<PreviousDocumentResponseData>();
		}

		IEnumerable<SupportingDocumentResponseData> GetSupportingDocuments(CustomConsignmentItemType03 goods)
		{
			return goods.SupportingDocument?
				.Where(x => x != null)
				.OrderBy(x => SafeNumber(x.SequenceNumber))
				.Select(doc => new SupportingDocumentResponseData
				{
					TypeCode = doc.Type,
					Reference = doc.ReferenceNumber,
					Reason = doc.ComplementOfInformation,
				})
				?? Enumerable.Empty<SupportingDocumentResponseData>();
		}

		IEnumerable<ZString> GetContainers(CustomConsignmentItemType03 goods)
		{
			return xmlMessage.Consignment?.TransportEquipment?
				.Where(x => x != null && x.GoodsReference.Any(y => y.DeclarationGoodsItemNumber == goods.DeclarationGoodsItemNumber))
				.OrderBy(x => SafeNumber(x.SequenceNumber))
				.Select(x => new ZString(x.ContainerIdentificationNumber))
				?? Enumerable.Empty<ZString>();
		}

		IEnumerable<PackageResponseData> GetPackages(CustomConsignmentItemType03 goodsItem)
		{
			return goodsItem.Packaging?
				.OrderBy(x => SafeNumber(x.SequenceNumber))
				.Select(y => new PackageResponseData
				{
					MarksAndNumbers = y.ShippingMarks,
					PackageType = y.TypeOfPackages,
					NumberOfPackages = y.NumberOfPackages,
				})
				?? Enumerable.Empty<PackageResponseData>();
		}

		IEnumerable<ZString> GetSeals()
		{
			return xmlMessage.Consignment?.TransportEquipment?
				.Where(x => x != null)
				.OrderBy(x => SafeNumber(x.SequenceNumber))
				.SelectMany(x => x.Seal.Where(y => y != null)
					.OrderBy(y => SafeNumber(y.SequenceNumber))
					.Select(y => new ZString(y.Identifier)))
				?? Enumerable.Empty<ZString>();
		}

		IEnumerable<ZString> GetItinerary()
		{
			return xmlMessage.Consignment?.CountryOfRoutingOfConsignment?
				.Where(x => x != null)
				.OrderBy(x => SafeNumber(x.SequenceNumber))
				.Select(x => new ZString(x.Country))
				?? Enumerable.Empty<ZString>();
		}

		IEnumerable<AuthorisationResponseData> GetAuthorisations()
		{
			return xmlMessage.Authorisation?
				.Where(x => x != null)
				.OrderBy(x => SafeNumber(x.SequenceNumber))
				.Select(x => new AuthorisationResponseData
				{
					SequenceNumber = x.SequenceNumber,
					Type = x.Type,
					ReferenceNumber = x.ReferenceNumber,
				})
				?? Enumerable.Empty<AuthorisationResponseData>();
		}

		int SafeNumber(string number) => int.TryParse(number, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result)
			? result
			: 0;

		Cc029CType xmlMessage;
	}
}
