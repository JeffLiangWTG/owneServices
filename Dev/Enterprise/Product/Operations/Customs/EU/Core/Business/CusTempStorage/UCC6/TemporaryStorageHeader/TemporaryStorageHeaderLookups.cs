using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageHeaderLookups : AsycudaManifestHeaderLookups
	{
		public TemporaryStorageHeaderLookups(TemporaryStorageHeader parent) : base(parent)
		{
		}

		protected new TemporaryStorageHeader Parent => (TemporaryStorageHeader)base.Parent;

		public CodeDescriptionPairList MessageTypeList => MessageTypeListCore;

		protected virtual CodeDescriptionPairList MessageTypeListCore
		{
			get
			{
				var parent = Parent;
				if (parent != null && (parent.AMA_MessageType == PNTSMessageTypeList.Codes.PreLodgedTempStorage || parent.AMA_MessageType == PNTSMessageTypeList.Codes.PresentationNotification) && parent.CustomsStatus == UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStoragePreLodged)
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(PNTSMessageTypeList.Codes.PresentationNotification, PNTSMessageTypeList.Descriptions.PresentationNotification);
					result.AddPair(PNTSMessageTypeList.Codes.PreLodgedTempStorage, PNTSMessageTypeList.Descriptions.PreLodgedTempStorage);
					return result;
				}
				else
				{
					return Factory.GetCachedValue<PNTSMessageTypeList>();
				}
			}
		}

		public ShippingProviderCollection CarrierOrganisations => new ShippingProviderCollection(Factory);

		protected AirShippingProviderCollection fAirShippingLineList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Carrier)]
		public AirShippingProviderCollection AirShippingLineList
		{
			get
			{
				if (fAirShippingLineList == null)
				{
					fAirShippingLineList = new AirShippingProviderCollection(Factory);
				}
				return fAirShippingLineList;
			}
		}

		protected SeaShippingProviderCollection fSeaShippingLineList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Carrier)]
		public SeaShippingProviderCollection SeaShippingLineList
		{
			get
			{
				if (fSeaShippingLineList == null)
				{
					fSeaShippingLineList = new SeaShippingProviderCollection(Factory);
				}
				return fSeaShippingLineList;
			}
		}

		public ShippingProviderCollection CarrierList
		{
			get
			{
				if (Parent.AMA_TransportMode == Customs.Business.TransportTypeList.Codes.Air)
				{
					return AirShippingLineList;
				}
				else if (Parent.AMA_TransportMode == Customs.Business.TransportTypeList.Codes.Sea)
				{
					return SeaShippingLineList;
				}
				else
				{
					return CarrierOrganisations;
				}
			}
		}

		public OrganisationsFindBoxCollection DeclarantList => GetOrganisationsFindBoxCollection();

		public OrganisationsFindBoxCollection PresenterList => GetOrganisationsFindBoxCollection();

		public OrganisationsFindBoxCollection OwnerList => GetOrganisationsFindBoxCollection();

		public OrganisationsFindBoxCollection RepresentativeList => GetOrganisationsFindBoxCollection();

		OrganisationsFindBoxCollection GetOrganisationsFindBoxCollection() => Factory.GetCachedValue("EU.UCC6TempStorageLookups.OrganisationsList", () => new OrganisationsFindBoxCollection(Factory));

		public CustomsOfficeCodeCollection CustomsOfficeCodeList => CustomsOfficeCodeListCore;

		protected virtual CustomsOfficeCodeCollection CustomsOfficeCodeListCore => Factory.GetCachedValue("EU.UCC6TempStorageLookups.CustomsOfficeCodeList", () =>
		{
			var result = Parent.PresentationCustomsOfficeCode.Lookups.OfficeCodeList;
			if (!result.IsLoaded && !result.IsLoading)
			{
				result.Load();
			}
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", (ZString)Universal.RefCusCodeListAttributeTypes.Codes.ROLE, false));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "ComparisonOperator", (ZString)ModuleTextFilter.ComparisonConstants.Exact, false));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", (ZString)EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, FilterOrCategory.Blue, 0, false));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", (ZString)EuOfficeCodesTypes.Codes.OfficeOfLodgementEntry, FilterOrCategory.Blue, 1, false));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", (ZString)EuOfficeCodesTypes.Codes.CustomsOfficeForTemporaryStorage, FilterOrCategory.Blue, 2, false));
			return result;
		});

		public CodeDescriptionPairList TransportTypeList => TransportTypeListCore;

		protected virtual CodeDescriptionPairList TransportTypeListCore => Factory.GetCachedValue("EU.TemporaryStorageHeaderLookups.TransportTypeList." + Parent.AMA_TransportMode, () =>
		{
			var transportTypeList = TransportMeansList.ToArray();
			IEnumerable<ICodeDescription> filteredList;
			switch (Parent.AMA_TransportMode)
			{
				case Customs.Business.TransportTypeList.Codes.Air:
					filteredList = transportTypeList.Where(x => x.Code.StartsWith("4"));
					break;
				case Customs.Business.TransportTypeList.Codes.Sea:
					filteredList = transportTypeList.Where(x => x.Code.StartsWith("1"));
					break;
				case Customs.Business.TransportTypeList.Codes.Rail:
					filteredList = transportTypeList.Where(x => x.Code.StartsWith("2"));
					break;
				case Customs.Business.TransportTypeList.Codes.Road:
					filteredList = transportTypeList.Where(x => x.Code.StartsWith("3"));
					break;
				case Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport:
					filteredList = transportTypeList.Where(x => x.Code.StartsWith("8"));
					break;
				default:
					filteredList = transportTypeList;
					break;
			}

			var resultList = new CodeDescriptionPairList();
			foreach (var transportType in filteredList)
			{
				resultList.Add(transportType);
			}
			return resultList;
		});

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				var resultList = new CodeDescriptionPairList();
				resultList.AddPair(Customs.Business.TransportTypeList.Codes.Air, Customs.Business.TransportTypeList.Descriptions.Air);
				resultList.AddPair(Customs.Business.TransportTypeList.Codes.Sea, Customs.Business.TransportTypeList.Descriptions.Sea);
				resultList.AddPair(Customs.Business.TransportTypeList.Codes.Rail, Customs.Business.TransportTypeList.Descriptions.Rail);
				resultList.AddPair(Customs.Business.TransportTypeList.Codes.Road, Customs.Business.TransportTypeList.Descriptions.Road);
				resultList.AddPair(Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport, Customs.Business.TransportTypeList.Descriptions.InlandWaterwayTransport);
				return resultList;
			}
		}

		[SuppressWeaklyTypedCollectionMessage]
		public System.Collections.IList CustomsStatusList => CustomsStatusListCore;

		protected virtual System.Collections.IList CustomsStatusListCore
		{
			get
			{
				var filterDate = Parent.AMA_DateAtCustomsOffice;
				filterDate = filterDate == ZDateTime.Empty ? ZDateTime.Today : filterDate;
				var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Parent.DataGrouping, UniversalReferenceConstants.RefCusCodeListType.Code.TemporaryStorageCustomsStatus, filterDate);
				if (!result.IsLoaded && !result.IsLoading)
				{
					result.Load();
				}

				return result;
			}
		}

		public CusAuthorisationHeaderCollectionFiltered AuthorizationNumberList
		{
			get
			{
				var parent = Parent;
				var cusAuthorisationHeaderCollectionFiltered = new CusAuthorisationHeaderCollectionFiltered(Factory, parent.AuthorizationType, parent.AuthorizationOwner);
				cusAuthorisationHeaderCollectionFiltered.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType, "Property", parent.AuthorizationType, isRemovable: false));
				cusAuthorisationHeaderCollectionFiltered.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.Country, "Property", parent.AMA_RN_NKCountry, isRemovable: false));
				cusAuthorisationHeaderCollectionFiltered.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder, "Property", parent.AuthorizationOwner, isRemovable: false));
				return cusAuthorisationHeaderCollectionFiltered;
			}
		}

		public CodeDescriptionPairList AuthorizationTypeList => Factory.GetCachedValue<AuthorizationTypeList>();

		public CodeDescriptionPairList PNTSMessageStatusList => GetPNTSMessageStatusListCore();

		protected virtual CodeDescriptionPairList GetPNTSMessageStatusListCore() => Factory.GetCachedValue<PNTSMessageStatusList>();

		public RefUNLOCOCollection RefUNLOCOCollection => new RefUNLOCOCollection(Factory);

		protected virtual CodeDescriptionPairList TransportMeansList => Factory.GetCachedValue<TemporaryStorageMeansOfTransportList>();
	}
}
