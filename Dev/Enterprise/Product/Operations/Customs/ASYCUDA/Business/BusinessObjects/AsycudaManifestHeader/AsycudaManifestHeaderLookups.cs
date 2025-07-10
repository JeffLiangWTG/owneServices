using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public partial class AsycudaManifestHeaderLookups : ManifestBase.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent) : base(parent)
		{
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public override RefCountryCollection Countries
		{
			get
			{
				return Factory.GetCachedValue("ASYCUDA.All.Supported.Countries", delegate
				{
					var baseList = base.Countries;
					var f = new ZQuery();
					f.AddToFilter(RefCountrySchema.RN_Code, CountryHelper.SupportedCountries(Factory));
					baseList.AdditionalFilter = f;
					baseList.ApplySort(RefCountrySchema.RN_Desc.Name, System.ComponentModel.ListSortDirection.Ascending);
					return baseList;
				});
			}
		}

		public virtual CodeDescriptionPairList Natures => Factory.GetCachedValue(Parent.AMA_ManifestType + "AsycudaManifestNatures", () => Parent.ManifestType?.ManifestNatures ?? GetNatureList(Factory));

		public static ShipmentTypeList GetNatureList(BusinessObjectFactory factory) => factory.GetCachedValue<ShipmentTypeList>();

		public ICollection CustomsOffices => CustomsOfficesCore;

		protected virtual ICollection CustomsOfficesCore
		{
			get
			{
				var ports = GetEffectivePortForCustomsOffices();
				return GetCustomsOfficesListForCountry(Factory, Parent.DataGrouping, ports.ToArray(), Parent.AMA_TransportMode);
			}
		}

		public static CodeDescriptionPairList GetCustomsOfficesListForCountry(BusinessObjectFactory factory, string countryCode, ZString[] ports = null, string transportMode = null)
		{
			return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(factory, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				ZDateTime.Today, true, RefCusCodeListAttributeTypes.Codes.Port, ports ?? System.Array.Empty<ZString>(), transportMode ?? string.Empty);
		}

		List<ZString> GetEffectivePortForCustomsOffices()
		{
			var ports = new List<ZString>();

			ports.Add(Parent.AMA_RL_NKPortOfLoading);
			ports.Add(Parent.AMA_RL_NKPortOfDischarge);
			if (!Parent.IsStandAlone && Parent.Consol.MostInterestingTransportForBinding.Any())
			{
				if (Parent.Consol is IRoutingSupport routingSupport)
				{
					var transports = routingSupport.TransportsIncludingRelated.OfType<Transport>();
					ports.AddRange(transports.Select(x => x.JW_RL_NKDiscPort));
					ports.AddRange(transports.Select(x => x.JW_RL_NKLoadPort));
				}
			}
			ports.RemoveAll(x => x.IsEmpty);
			return ports;
		}

		public static CodeDescriptionPairList GetAllApplicationManifestTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ManifestTypesList", () =>
			{
				var manifestTypes = ApplicationBusinessProvider.GetAllApplicationManifestTypes(factory);
				var result = new CodeDescriptionPairList();
				result.AddPairsIfNotExist(manifestTypes);
				result.Sort();
				return result;
			});
		}

		public CodeDescriptionPairList ManifestTypes
		{
			get
			{
				var header = Parent;
				if (header != null)
				{
					var manifestStyle = header.AMA_ApplicationCode;
					var countryCode = header.AMA_RN_NKCountry;
					var transportMode = header.AMA_TransportMode;
					var manifestType = header.AMA_ManifestType;

					if (!manifestStyle.IsEmpty && !countryCode.IsEmpty && !manifestType.IsEmpty)
					{
						var cacheKey = string.Format(CultureInfo.InvariantCulture, "ManifestTypes_{0}_{1}_{2}_{3}", countryCode, manifestStyle, transportMode, manifestType); // CachedValueKey
						return Factory.GetCachedValue(cacheKey, () =>
						{
							var manifestTypes = Parent.ApplicationBusinessProvider.ManifestTypes
								.Where(t => t.ApplicableManifestStyles.Contains(manifestStyle.ToString()) && (transportMode.IsEmpty || t.ApplicableTransportModes.Contains(transportMode.ToString())));
							var result = new CodeDescriptionPairList();
							result.AddPairsIfNotExist(manifestTypes.Select(t => new CodeDescriptionPair(t.Code, t.Description)));
							return result;
						});
					}
				}
				return new CodeDescriptionPairList();
			}
		}

		public virtual CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<MessageStatusCodeList>();

		public virtual CodeDescriptionPairList AgentTypeList
		{
			get
			{
				var key = "AsycudaManifestHeader.Lookups.AgentTypeList";
				var isAir = Parent.IsAir;
				if (isAir)
				{
					key += "_AIR";
				}

				return Factory.GetCachedValue(key, () =>
				{
					var list = new CodeDescriptionPairList(OLookUpEditType.AgentType);
					if (isAir)
					{
						list.AddPair(Core.Constants.AgentType.AWBCoload, Core.Constants.AgentTypeDescriptions.AWBCoload);
						list.AddPair(Core.Constants.AgentType.AWBMaster, Core.Constants.AgentTypeDescriptions.AWBMaster);
					}
					return list;
				});
			}
		}

		public virtual CodeDescriptionPairList TransportModeList
		{
			get
			{
				var relevantCountry = Parent.AMA_RN_NKCountry;
				var manifestStyle = Parent.AMA_ApplicationCode;
				var direction = Parent.Consol?.JobDirection ?? ImportExportHelper.GetJobDirection(Parent.AMA_RL_NKPortOfLoading, Parent.AMA_RL_NKPortOfDischarge);

				return Factory.GetCachedValue($"AsycudaManifestHeader.Lookups.TransportModes|{relevantCountry}|{manifestStyle}|{direction}", delegate
				{
					var allPossibleModes = Factory.GetCachedValue<TransportTypeList>();
					var result = new CodeDescriptionPairList();

					GetAcceptableTransportModesFromApplicationBusinessProvider(Factory, relevantCountry, manifestStyle, direction).ForEach(x =>
					{
						result.AddPair(x, allPossibleModes.GetDescriptionFromCode(x) ?? x);
					});
					result.Sort();
					return result;
				});
			}
		}

		public static ISet<string> GetAcceptableTransportModesFromApplicationBusinessProvider(BusinessObjectFactory factory, ZString relevantCountry, ZString manifestStyle, Directions direction)
		{
			return factory.GetCachedValue<ISet<string>>($"GetAcceptableTransportModesFromApplicationBusinessProvider|{relevantCountry}|{manifestStyle}|{direction}", () =>
			{
				var result = new HashSet<string>();
				foreach (var provider in ApplicationBusinessProvider.GetApplicationBusinessProviders(factory, relevantCountry))
				{
					if (provider.CountryCodes.Contains(relevantCountry))
					{
						var transportModes = provider.GetAcceptableTransportModesFromAsycudaManifestHeader(factory, relevantCountry, manifestStyle, direction);
						result.UnionWith(transportModes);
					}
				}
				return result;
			});
		}

		public ShippingProviderCollection CarrierList
		{
			get
			{
				if (Parent.IsAir)
				{
					return AirShippingLineList;
				}
				else if (Parent.IsSea)
				{
					return SeaShippingLineList;
				}
				else
				{
					return AirOrSeaShippingLineList;
				}
			}
		}

		protected ShippingProviderCollection fAirOrSeaShippingLineList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Carrier)]
		public ShippingProviderCollection AirOrSeaShippingLineList
		{
			get
			{
				if (fAirOrSeaShippingLineList == null)
				{
					fAirOrSeaShippingLineList = new ShippingProviderCollection(Factory);
				}
				return fAirOrSeaShippingLineList;
			}
		}

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

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public virtual CodeDescriptionPairList ContainerModes
		{
			get
			{
				var result = ContainerModeList;
				if (Parent.IsStandAlone
					&& !Parent.HasContainers)
				{
					result.Remove(new CodeDescriptionPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised));
				}
				return result;
			}
		}

		public static CodeDescriptionPairList ContainerModeList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPairIfNotExist(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
				list.AddPairIfNotExist(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
				list.AddPairIfNotExist(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised); // helpful
				list.AddPairIfNotExist(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
				list.AddPairIfNotExist(Core.Constants.ContainerModes.Other, Core.Constants.ContainerModeDescriptions.Other);
				return list;
			}
		}

		public static CodeDescriptionPairList TransportModeForWorkFlow
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("FF8DCC9F-59E5-4823-87B0-DC844C55357F", "All"));
				result.AddRange(new TransportTypeList());
				result.AddPairIfNotExist(Core.Constants.TransportModes.Road, "Road");
				return result;
			}
		}

		public ICollection CustomsLoadingPortList => GetCustomsLoadingPortListCore();

		protected virtual ICollection GetCustomsLoadingPortListCore()
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Parent.AMA_RN_NKCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
		}

		public ICollection CustomsDischargePortList => GetCustomsDischargePortListCore();

		protected virtual ICollection GetCustomsDischargePortListCore()
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Parent.AMA_RN_NKCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
		}

		public RefUNLOCOCollection DischargePortList => GetDischargePortListCore();

		protected virtual RefUNLOCOCollection GetDischargePortListCore() => new RefUNLOCOCollection(Factory);

		public RefUNLOCOCollection LoadingPortList => GetLoadingPortListCore();

		protected virtual RefUNLOCOCollection GetLoadingPortListCore() => new RefUNLOCOCollection(Factory);

		public RefUNLOCOCollection OriginPortList => GetOriginPortListCore();

		protected virtual RefUNLOCOCollection GetOriginPortListCore() => new RefUNLOCOCollection(Factory);

		public RefUNLOCOCollection DestinationPortList => GetDestinationPortListCore();

		protected virtual RefUNLOCOCollection GetDestinationPortListCore() => new RefUNLOCOCollection(Factory);
	}
}
