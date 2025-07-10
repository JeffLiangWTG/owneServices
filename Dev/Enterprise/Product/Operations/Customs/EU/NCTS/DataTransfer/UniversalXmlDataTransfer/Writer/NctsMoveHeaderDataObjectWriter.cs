using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4
{
	public class NctsMoveHeaderDataObjectWriter : DataObjectWriter<NctsCommonMovementHeader, Shipment>
	{
		public NctsMoveHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, Shipment headerData)
			: base(manager)
		{
			this.helper = Argument.NotNull(helper, nameof(helper));
			this.headerData = headerData;
		}

		readonly UniversalDataObjectWriterHelper helper;
		readonly Shipment headerData;

		protected override Shipment PopulateDataObject(NctsCommonMovementHeader moveHeaderBO)
		{
			var headerBO = moveHeaderBO.Header;
			var commercialInvoiceHeaderData = new UniversalCustoms.CommercialInvoiceHeader(writeManager.WriterStrategy)
			{
				RelatedIndicator = ListHelper.GetWithDescription<CodeDescriptionPair>(moveHeaderBO.BM_SubApplicationCode, moveHeaderBO.Lookups.NctsMoveHeaderTypeList)
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>()));

			var refUNLOCOList = headerBO.Factory.GetRefUNLOCOList();
			var refCountryList = headerBO.Factory.GetRefCountryList();

			if (headerBO.IsArrivalMovement)
			{
				var arrivalMoveHeader = headerBO.ArrivalMovementHeader;
				headerData.CustomsOffice = new CodeDescriptionPair10Char { Code = headerBO.DeclarationPlace.Left(10), Description = headerBO.DeclarationPlace };
				headerData.SubLocationAtClearance = GetLocationOfGoods<CodeDescriptionPair35Char>(arrivalMoveHeader.IsSimplifiedNctsProcedure, arrivalMoveHeader.BM_LocationOfGoodsCode, arrivalMoveHeader.BM_LocationOfGoodsCode, arrivalMoveHeader.BM_LocationOfGoods, arrivalMoveHeader.BM_CustomsSubPlace, true);
				PopulateArrivalAddInfosData(arrivalMoveHeader);
			}

			if (moveHeaderBO is NctsDepartureMovementHeader departureMovementHeader)
			{
				headerData.CustomsOffice = new CodeDescriptionPair10Char { Code = headerBO.DeclarationPlace.Left(10), Description = headerBO.DeclarationPlace };
				headerData.OwnerRef = headerBO.LocalReferenceNumber;
				headerData.MessageType = ListHelper.GetWithDescription<CodeDescriptionPair>(departureMovementHeader.BM_InBondEntryType, departureMovementHeader.Lookups.DeclarationTypeList);
				headerData.CustomsBroker = new Staff { Code = departureMovementHeader.BM_GS_NKCusAgent, Name = departureMovementHeader.RepresentativeName };
				headerData.PortOfLoading = ListHelper.GetWithName(GetPlaceOfLoading(departureMovementHeader), refUNLOCOList);
				headerData.PortOfOrigin = ListHelper.GetWithName(headerBO.BH_RL_NKImportLoadPort, refCountryList);
				headerData.PortOfDestination = ListHelper.GetWithName(departureMovementHeader.BM_RL_NKDestinationPort, refCountryList);
				headerData.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(departureMovementHeader.BM_InlandTransportMode, departureMovementHeader.Lookups.ModeOfTransportList);
				headerData.VesselCountryOfRegistration = ListHelper.GetWithName<Country>(departureMovementHeader.BM_TOLCarrierCode, refCountryList);
				headerData.VesselName = departureMovementHeader.BM_TOLCarrierID;
				headerData.DeliveryMode = ListHelper.GetWithDescription<CodeDescriptionPair>(departureMovementHeader.BM_BTAIndicator, departureMovementHeader.Lookups.SpecificCircumstanceIndicatorList);
				headerData.PaymentMethod = ListHelper.GetWithDescription<CodeDescriptionPair>(departureMovementHeader.BM_MethodOfPayment, departureMovementHeader.Lookups.TransportChargesModeOfPaymentList);
				headerData.PortOfDischarge = ListHelper.GetWithName(departureMovementHeader.BM_PlaceOfUnloading, refUNLOCOList);
				headerData.VoyageFlightNo = departureMovementHeader.BM_ConveyanceNumber;
				headerData.AdditionalTerms = headerBO.ExplanationToCustomsForWhyCancelling;
				headerData.LocationAtClearance = GetLocationOfGoods<CodeDescriptionPair35Char>(departureMovementHeader.IsSimplifiedNctsProcedure, departureMovementHeader.BM_LocationOfGoodsCode, departureMovementHeader.BM_LocationOfGoodsCode, departureMovementHeader.BM_LocationOfGoods, departureMovementHeader.BM_CustomsSubPlace, false);
				headerData.SealInfo = GetSealInfo(departureMovementHeader);

				PopulateDepartureAddInfosData(headerBO, departureMovementHeader);
				PopulateContainersAndSeals(headerBO, headerData);
				PopulateItinerary(headerBO, headerData);
				PopulateAddresses(departureMovementHeader, headerData);
				ProcessCollection(helper.Load<NctsDepartureCargoDesc>(departureMovementHeader.GoodsItems.CompleteFilter).OrderBy(x => x.BY_LineNo), GetNewDepartureGoodsItemDataObjectWriter(writeManager, helper, headerData, commercialInvoiceHeaderData));
			}
			else
			{
				ProcessCollection(helper.Load<NctsCommonCargoDesc>(moveHeaderBO.GoodsItems.CompleteFilter).OrderBy(x => x.BY_LineNo).ThenBy(x => x.BY_SystemCreateTimeUtc), new ArrivalAndUnloadingGoodsItemDataObjectWriter(writeManager, helper, headerData, commercialInvoiceHeaderData));
			}

			headerData.CommercialInfo.CommercialInvoiceCollection.Add(commercialInvoiceHeaderData);

			return headerData;
		}

		protected virtual ZString GetPlaceOfLoading(NctsDepartureMovementHeader departureMovementHeader) => departureMovementHeader.BM_RL_NKForeignDestPort; // IT should return BM_PlaceOfLoading

		protected virtual DepartureGoodsItemDataObjectWriter GetNewDepartureGoodsItemDataObjectWriter(IDataWritingManager writeManager, UniversalDataObjectWriterHelper helper, Shipment headerData, UniversalCustoms.CommercialInvoiceHeader commercialInvoiceHeaderData)
		{
			return new DepartureGoodsItemDataObjectWriter(writeManager, helper, headerData, commercialInvoiceHeaderData);
		}

		static SealInfo GetSealInfo(NctsDepartureMovementHeader movementHeader)
		{
			return new SealInfo()
			{
				Type = ListHelper.GetWithDescription<CodeDescriptionPair>(movementHeader.BM_SealType, movementHeader.Lookups.SealTypeList),
				Quantity = movementHeader.BM_SealQty
			};
		}

		static T GetLocationOfGoods<T>(bool isSimplified, ZString authorisedLocationOfGoodsCode, ZString agreedLocationOfGoodsCode, ZString agreedLocationOfGoods, ZString customsSubPlace, bool isSubLocation) where T : ICodeDescriptionDataObject, new()
		{
			var locationOfGoods = ZString.Empty;
			var locationOfGoodsDescription = ZString.Empty;

			if (isSimplified)
			{
				locationOfGoods = authorisedLocationOfGoodsCode;
				locationOfGoodsDescription = DataObjectWriterConstants.MovementHeader.LocationDescriptions.AuthorisedLocationOfGoodsCode;
			}
			else
			{
				if (locationOfGoods.IsEmpty)
				{
					locationOfGoods = agreedLocationOfGoodsCode;
					locationOfGoodsDescription = DataObjectWriterConstants.MovementHeader.LocationDescriptions.AgreedLocationOfGoodsCode;
				}
				if (locationOfGoods.IsEmpty)
				{
					locationOfGoods = agreedLocationOfGoods;
					locationOfGoodsDescription = DataObjectWriterConstants.MovementHeader.LocationDescriptions.AgreedLocationOfGoods;
				}
				if (locationOfGoods.IsEmpty)
				{
					locationOfGoods = customsSubPlace;
					locationOfGoodsDescription = DataObjectWriterConstants.MovementHeader.LocationDescriptions.CustomsSubPlace;
				}
			}

			int maxLen = isSubLocation ? 10 : 35;
			locationOfGoodsDescription = locationOfGoods.Length > maxLen
				? (ZString)$"{locationOfGoods} : {locationOfGoodsDescription}"
				: locationOfGoodsDescription;

			return new T
			{
				Code = locationOfGoods.Left(maxLen),
				Description = locationOfGoodsDescription.Left(80)
			};
		}

		void PopulateItinerary(NctsHeader headerBO, Shipment shipmentData)
		{
			if (headerBO.Itinerary.Count > 0)
			{
				var itineraryLeg = ZByte.Zero;
				shipmentData.SetTransportLegCollection(() =>
				{
					var legCollection = new DataObjectList<TransportLeg> { Content = CollectionContent.Complete };
					foreach (NonPersistentItineraryCountry country in headerBO.Itinerary)
					{
						AddItineraryLegToList(country.CountryCode, ++itineraryLeg, legCollection);
					}
					return legCollection;
				});
			}
		}

		void AddItineraryLegToList(ZString countryCode, ZByte itineraryLeg, DataObjectList<TransportLeg> list)
		{
			list.Add(new TransportLeg(writeManager.WriterStrategy)
			{
				DepartureReference = countryCode,
				LegOrder = itineraryLeg
			});
		}

		void PopulateContainersAndSeals(NctsHeader headerBO, Shipment shipmentData)
		{
			shipmentData.SetContainerCollection(() =>
			{
				var collection = new DataObjectList<Container>();
				foreach (NctsDepartureHeaderContainer item in headerBO.DepartureHeaderContainers)
				{
					AddContainerToList(item, collection);
				}
				return collection;
			});
		}

		void AddContainerToList(NctsDepartureHeaderContainer containerBO, DataObjectList<Container> containerList)
		{
			containerList.Add(new Container(writeManager.WriterStrategy)
			{
				ContainerNumber = containerBO.BC_ContainerNum,
				Seal = containerBO.Seal1,
				SecondSeal = containerBO.Seal2
			});
		}

		void PopulateAddresses(NctsDepartureMovementHeader departureMovementHeader, IOrganizationAddressCollectionParent organizationAddressCollectionParent)
		{
			organizationAddressCollectionParent.AddOrgAddresses(writeManager, departureMovementHeader);
			organizationAddressCollectionParent.AddOrgAddress(writeManager, departureMovementHeader.WarehouseAddress, AddressTypes.Warehouse2);
		}
		
		void PopulateDepartureAddInfosData(NctsHeader headerBO, NctsDepartureMovementHeader departureMovementHeader)
		{
			headerData.AddAddInfo(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.ControlResultCode, departureMovementHeader.IsSimplifiedNctsProcedure ? new ZString(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.IsSimplifiedNctsProcedureA3) : ZString.Empty);
			var addInfoCollection = headerData.AddInfoCollection;
			var dateLimit = departureMovementHeader.BM_ExportDate;
			if (!dateLimit.IsDefault)
			{
				addInfoCollection.AddAddInfo(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.ControlResultDateLimit, new ZString(headerBO.MovementHeader.BM_ExportDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)));
			}
			addInfoCollection.AddAddInfo(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.SecurityIndicator, headerBO.BH_FTZMove);
			addInfoCollection.AddAddInfo(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.TransportModeAtBorder, departureMovementHeader.BM_ExportTransportMode);
			addInfoCollection.AddAddInfo(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.Box18TransportID, departureMovementHeader.BM_TransportAtDeparture);
			addInfoCollection.AddAddInfo(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.Box18TransportNationality, departureMovementHeader.BM_RN_NKTransportAtDepartureCountry);
		}

		void PopulateArrivalAddInfosData(NctsArrivalMovementHeader arrivalMovementHeader)
		{
			headerData.AddAddInfo(DataObjectWriterConstants.ArrivalMovementHeader.AddInfo.SimplifiedArrivalProcedureFlag, new ZString(arrivalMovementHeader.IsSimplifiedNctsProcedure ? DataObjectWriterConstants.ArrivalMovementHeader.AddInfo.IsSimplifiedArrivalProcedure : "0"));
		}
	}
}
