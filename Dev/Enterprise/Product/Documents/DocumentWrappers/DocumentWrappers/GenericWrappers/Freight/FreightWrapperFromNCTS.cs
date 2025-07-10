using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromNCTS<T1, T2> : FreightWrapperFromNCTS
		where T1 : NctsHeader
		where T2 : NctsHeaderDocumentWrapper
	{
		protected FreightWrapperFromNCTS(NctsHeader nctsHeaderBO, Transport transportBO, BusinessObjectFactory factory)
			: base(nctsHeaderBO, transportBO, factory)
		{
		}

		public new T1 NctsHeader
		{
			get { return (T1)base.NctsHeader; }
		}

		public new T2 DocNCTS
		{
			get { return (T2)base.DocNCTS; }
		}
	}

	public abstract class FreightWrapperFromNCTS : FreightWrapper, IDocTypeCode
	{
		public static FreightWrapperFromNCTS New(NctsHeader nctsHeaderBO, Transport transportBO, BusinessObjectFactory factory)
		{
			var nctsType = nctsHeaderBO.GetType();
			var wrapperType = typeof(FreightWrapperFromNCTS<,>).MakeGenericType(new[] { nctsType, NctsHeaderDocumentWrapper.GetDocNctsType(nctsType) });
			var constructorInfo = wrapperType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(NctsHeader), typeof(Transport), typeof(BusinessObjectFactory) }, null);
			return (FreightWrapperFromNCTS)constructorInfo.Invoke(new object[] { nctsHeaderBO, transportBO, factory });
		}

		public static FreightWrapperFromNCTS New(NctsHeader nctsHeaderBO, BusinessObjectFactory factory)
		{
			return New(nctsHeaderBO, null, factory);
		}

		protected FreightWrapperFromNCTS(NctsHeader nctsHeaderBO, Transport transportBO, BusinessObjectFactory factory)
			: base(nctsHeaderBO, factory)
		{
			NctsHeaderBO = nctsHeaderBO;
			TransportBO = transportBO;
			if (TransportBO == null)
			{
				TransportBO = Factory.GetNull<Transport>();
			}
		}
		readonly NctsHeader NctsHeaderBO;
		readonly Transport TransportBO;

		public new NctsHeaderDocumentWrapper DocNCTS
		{
			get { return docNCTS ?? (docNCTS = NctsHeaderDocumentWrapper.New(NctsHeader, Factory)); }
		}
		NctsHeaderDocumentWrapper docNCTS;

		public override BusinessObject BusinessObjectForPrintJob => (BusinessObject)ARInvoice?.WrappedObject ?? base.BusinessObjectForPrintJob;

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get { return NctsHeaderBO.Shipment ?? base.BusinessObjectToLogAgainst; }
		}

		protected override BusinessObject BusinessObjectForCustomFields
		{
			get { return NctsHeaderBO; }
		}

		#region Freight Business Objects used for fallbacks

		ForwardingShipment ShipmentBO
		{
			get
			{
				if (object.ReferenceEquals(fShipmentBO, null))
				{
					fShipmentBO = NctsHeaderBO.Shipment;
					if (fShipmentBO == null)
					{
						fShipmentBO = Factory.GetNull<ForwardingShipment>();
					}
				}
				return fShipmentBO;
			}
		}
		ForwardingShipment fShipmentBO;

		ForwardingConsol ConsolBO
		{
			get
			{
				if (object.ReferenceEquals(fConsolBO, null))
				{
					fConsolBO = GetConsolFromTransport(ShipmentBO, TransportBO);

					if (fConsolBO == null)
					{
						fConsolBO = Factory.GetNull<ForwardingConsol>();
					}
				}
				return fConsolBO;
			}
		}
		ForwardingConsol fConsolBO;

		#endregion

		#region Related Business Objects

		protected override NctsHeader GetNctsHeader()
		{
			return NctsHeaderBO;
		}

		protected override ForwardingShipment GetShipment()
		{
			return ShipmentBO;
		}

		protected override CommonShipment GetBaseShipment()
		{
			return ShipmentBO;
		}

		protected override ForwardingConsol GetConsol()
		{
			return ConsolBO;
		}

		protected override ZString GetCustomsEntryNumber()
		{
			return NctsHeaderBO == null ? ZString.Empty : NctsHeaderBO.MovementReferenceNumber;
		}

		protected override ZString GetJobNumber()
		{
			return NctsHeaderBO?.JobNumber ?? ZString.Empty;
		}

		#endregion

		protected override OrganisationWrapper GetConsignor()
		{
			return GetWrappedJobDocAddressOrFallbackIfEmpty(OrganisationUsageType.Consignor, NctsHeaderBO.Consignor?.Organisation, ContactType.Consignor);
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return GetWrappedJobDocAddressOrFallbackIfEmpty(OrganisationUsageType.Consignee, NctsHeaderBO.Consignee?.Organisation, ContactType.Consignee);
		}

		OrganisationWrapper GetWrappedJobDocAddressOrFallbackIfEmpty(OrganisationUsageType usageType, OrgHeader organisation, ContactType contactType)
		{
			return new OrganisationWrapper(usageType, organisation?.MainAddress, contactType, Factory);
		}

		protected internal NctsCommonMovementHeader MovementHeader => (NctsCommonMovementHeader)NctsHeaderBO.MovementHeader ?? NctsHeaderBO.ArrivalMovementHeader;

		protected override CodeAndDescriptionWrapper GetNCTSDeclarationType() => new CodeAndDescriptionWrapper(MovementHeader?.BM_InBondEntryType ?? ZString.Empty, MovementHeader.Lookups?.DeclarationTypeList ?? new CodeDescriptionPairList(), Factory);

		protected override ZString GetNCTSDepartureTransportID() => MovementHeader?.BM_TransportAtDeparture ?? ZString.Empty;

		protected override CodeAndDescriptionWrapper GetNCTSDepartureTransportCountry() => new CodeAndDescriptionWrapper(MovementHeader?.BM_RN_NKTransportAtDepartureCountry ?? ZString.Empty, MovementHeader.Lookups?.TransportAtDepartureCountries ?? new RefCountryCollection(Factory), Factory);

		protected override CodeAndDescriptionWrapper GetNCTSDepartureTransportMode() => new CodeAndDescriptionWrapper(MovementHeader?.BM_InlandTransportMode ?? ZString.Empty, NctsHeaderBO.MovementHeader?.Lookups.ModeOfTransportList ?? new CodeDescriptionPairList(), Factory);

		protected override ZString GetNCTSFrontierTransportID() => MovementHeader?.BM_TOLCarrierID ?? ZString.Empty;

		protected override CodeAndDescriptionWrapper GetNCTSFrontierTransportCountry() => new CodeAndDescriptionWrapper(MovementHeader?.BM_TOLCarrierCode ?? ZString.Empty, MovementHeader.Lookups?.TransportAtDepartureCountries ?? new RefCountryCollection(Factory), Factory);

		protected override CodeAndDescriptionWrapper GetNCTSFrontierTransportMode() => new CodeAndDescriptionWrapper(MovementHeader?.BM_ExportTransportMode ?? ZString.Empty, NctsHeaderBO.MovementHeader?.Lookups.ModeOfTransportList ?? new CodeDescriptionPairList(), Factory);

		protected override ZString GetNCTSGoodsLocationCode() => NctsHeaderBO.IsArrivalMovement ? ZString.Empty : MovementHeader.BM_LocationOfGoodsCode;

		protected override ZString GetNCTSGoodsLocation() => NctsHeaderBO.IsArrivalMovement ? MovementHeader.BM_PlaceOfUnloading : MovementHeader.BM_LocationOfGoods;

		protected override CodeAndDescriptionWrapper GetNCTSDepartureOffice()
		{
			var euOfficeCode = GetEuOfficeCode(false);
			return new CodeAndDescriptionWrapper(euOfficeCode, GetCustomsOffices().Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Data == euOfficeCode)?.CY_OfficeDescription ?? ZString.Empty, Factory);
		}

		protected override CodeAndDescriptionWrapper GetNCTSDestinationOffice()
		{
			var euOfficeCode = GetEuOfficeCode(true);

			return new CodeAndDescriptionWrapper(euOfficeCode, GetCustomsOffices().Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Data == euOfficeCode)?.CY_OfficeDescription ?? ZString.Empty, Factory);
		}

		NctsEuOfficeCodeCollection GetCustomsOffices() => NctsHeaderBO.IsPhase5
															? NctsHeaderBO.MovementHeader is NctsDepartureMovementHeader departureMovementHeader
																? departureMovementHeader.CustomsOffices
																: NctsHeaderBO.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader
																	? arrivalMovementHeader.CustomsOffices
																	: null
															: NctsHeaderBO.CustomsOffices;

		ZString GetEuOfficeCode(bool isDestinationCustomsOffice) => NctsHeaderBO.IsPhase5
																	? NctsHeaderBO.MovementHeader is NctsDepartureMovementHeader departureMovementHeader
																		? isDestinationCustomsOffice ? departureMovementHeader.DestinationCustomsOfficeCode : departureMovementHeader.DepartureCustomsOfficeCode
																		: NctsHeaderBO.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader
																			? isDestinationCustomsOffice ? arrivalMovementHeader.DestinationCustomsOfficeCode : arrivalMovementHeader.DepartureCustomsOfficeCode
																			: ZString.Empty
																	: isDestinationCustomsOffice ? NctsHeaderBO.DestinationCustomsOfficeCode : NctsHeaderBO.DepartureCustomsOfficeCode;

		#region IDocTypeCode

		ZString DocTypeCode;

		ZString IDocTypeCode.DocTypeCode
		{
			get { return DocTypeCode; }
			set { DocTypeCode = value; }
		}

		#endregion
	}
}
