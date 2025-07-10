using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.Customs.Universal.RefCusCodeListTypes;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public partial class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public virtual CodeDescriptionPairList ProfileList => new CodeDescriptionPairList();

		public override CodeDescriptionPairList TransportTypeList => Factory.GetCachedValue<TransportTypeList>();

		public ICollection TransportCountryList => TransportCountryListCore;

		protected virtual ICollection TransportCountryListCore
		{
			get
			{
				if (Parent.IsExport)
				{
					return EUUniversalLookupsHelper.GetEXNATCountryList(Factory);
				}
				else
				{
					return new RefCountryCollection(Factory);
				}
			}
		}

		public ICollection Locations => LocationsCore;

		protected virtual ICollection LocationsCore => new CodeDescriptionPairList();

		public override CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<DefermentMethodList>();

		public CodeDescriptionPairList EntryStyleList => EntryStyleListCore;

		protected virtual CodeDescriptionPairList EntryStyleListCore => GetEntryStyleList(Parent);

		internal CodeDescriptionPairList GetEntryStyleList(ICanBeImportOrExport parent)
		{
			var result = new CodeDescriptionPairList();
			if (parent.IsImport)
			{
				result = GetImportEntryStyleList(parent);
			}
			if (parent.IsExport)
			{
				result = GetExportEntryStyleList(parent);
			}
			return result;
		}

		CodeDescriptionPairList GetImportEntryStyleList(ICanBeImportOrExport parent)
		{
			CodeDescriptionPairList result;
			if (Parent.IsUCCCompliant)
			{
				result = Factory.GetCachedValue("JobDeclaration.Lookups.EntryStyleList.ImportUCC",
					() => ReConstructCodeDescriptionPairList(parent, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, Factory.GetCachedValue<EntryStyleListImportUCC>()));
			}
			else
			{
				result = Factory.GetCachedValue("JobDeclaration.Lookups.EntryStyleList.Import",
					() => ReConstructCodeDescriptionPairList(parent, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, Factory.GetCachedValue<EntryStyleListImport>()));
			}
			return result;
		}

		protected CodeDescriptionPairList GetExportEntryStyleList(ICanBeImportOrExport parent)
		{
			CodeDescriptionPairList result;
			if (Parent.IsUCCCompliant)
			{
				result = Factory.GetCachedValue("JobDeclaration.Lookups.EntryStyleList.ExportUCC",
				() => ReConstructCodeDescriptionPairList(parent, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, Factory.GetCachedValue<EntryStyleListExportUCC>()));
			}
			else
			{
				result = Factory.GetCachedValue("JobDeclaration.Lookups.EntryStyleList.Export",
				() => ReConstructCodeDescriptionPairList(parent, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, Factory.GetCachedValue<EntryStyleListExport>()));
			}
			return result;
		}

		CodeDescriptionPairList ReConstructCodeDescriptionPairList(ICanBeImportOrExport parent, string code, CodeDescriptionPairList codeDescriptionPairList)
		{
			var result = new CodeDescriptionPairList();
			var additionalFilter = new RefCusCodeListAttributeFilter(Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType, JoinCondition.And, parent.IsExport ? JobMessageTypeList.Codes.Export : JobMessageTypeList.Codes.Import).Filter;
			result.AddRange(ZZRefCusCodeListCombined.Loader.Load(Factory, parent.DataGroupingCode, code, ZDateTime.Now, additionalFilter, false, "", false));
			result.AddPairsIfNotExist(codeDescriptionPairList.ToArray());
			result.Sort();
			return result;
		}

		public override ICodeDescriptionPairList DeclarantTypeList => Factory.GetCachedValue<RepresentationTypeList>();

		public CodeDescriptionPairList ModeOfTransportList => ModeOfTransportListCore();
		protected virtual CodeDescriptionPairList ModeOfTransportListCore()
		{
			return Factory.GetCachedValue<ModeOfTransportList>();
		}
		public override CodeDescriptionPairList MessageStatusList => EntryStatusList;

		public WarehouseClientCollection LocationOfGoodsList => new WarehouseClientCollection(Factory);

		protected override CodeDescriptionPairList EntryStatusListForCustomsWare => Factory.GetCachedValue<Common.EU.CustomsWareEntryStatusList>();

		protected override CodeDescriptionPairList EntryStatusListForDefaultFallBack => Factory.GetCachedValue<Common.EU.EntryStatusList>();

		public OrganisationsFindBoxCollection RepresentativeList => Factory.GetCachedValue("EU.JobDeclarationLookups.BrokerCollection", () => new BrokerCollection(Factory));

		public OrganisationsFindBoxCollection SellerList => Factory.GetCachedValue("EU.JobDeclarationLookups.ConsignorCollection", () => new ConsignorCollection(Factory));

		public override OrgHeaderCollection Buyers => buyers ?? (buyers = new ConsigneeCollection(Factory));

		public SupervisingOfficeCollection SupervisingOfficeCollection => new SupervisingOfficeCollection(Factory);

		public OrganisationsFindBoxCollection DefermentPartyCollection => new OrganisationsFindBoxCollection(Factory);

		protected override CodeDescriptionPairList PackingUnitTypesListCore
			=> RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, UniversalReferenceConstants.UNPackTypeStartDate);

		public override CodeDescriptionPairList CargoIdTypeList
		{
			get
			{
				return Factory.GetCachedValue("JobDeclaration.Lookups.JE_ContainerMode." + Parent.JE_TransportMode, delegate
				{
					var list = FreightCodePairLists.JS_PackingModeList(Parent.JE_TransportMode);
					if (Parent.JE_TransportMode != Constants.TransportModes.Air)
					{
						list.AddPairIfNotExist(Constants.ContainerModes.Containerised, Constants.ContainerModeDescriptions.Containerised);
					}
					list.AddPairIfNotExist(Constants.ContainerModes.NonContainerised, Constants.ContainerModeDescriptions.NonContainerised);
					list.RemoveCode(Constants.ContainerModes.BuyersConsol);
					list.RemoveCode(Constants.ContainerModes.ShippersConsol);
					list.RemoveCode(Constants.ContainerModes.AgentConsol);  // these three are not container modes.... why are they in the list in the first place. These are methods of consolidating freight, not methods of containerisation. Well said, Daniel.
					return list;
				});
			}
		}

		protected override BusinessObjectCollection IATALoadPortsCore
		{
			get
			{
				if ((Parent.IsImport) && (Parent.JE_EntryStyle == EntryStyleListImport.Codes.ImportFromSpecialTerritory))
				{
					return new AirportCollection(Factory, Parent.JE_RL_NKPortOfLoading, isNonEuAirport: false);
				}
				else if (HasIATAForThisDataGrouping)
				{
					return new ZZRefCusCodeListCombinedCollection(Factory, ThisDataGrouping, new ZString[] { Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA }, ZDate.Today, null, includeParentDataGroupings: false);
				}
				else if (HasIATAForParentDataGrouping)
				{
					return new ZZRefCusCodeListCombinedCollection(Factory, ParentDataGrouping, Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, ZDate.Today);
				}
				else
				{
					if (!Parent.JE_RL_NKPortOfLoading.IsEmpty)
					{
						return new AirportCollection(Factory, Parent.JE_RL_NKPortOfLoading, isNonEuAirport: true);
					}
					return new AirportCollection(Factory, isNonEuAirport: true);
				}
			}
		}

		bool HasIATAForThisDataGrouping
		{
			get
			{
				var date = ZDate.Today;
				var groupingCode = ThisDataGrouping;
				return GetCachedEUIATADataGrouping(date, groupingCode);
			}
		}

		bool HasIATAForParentDataGrouping
		{
			get
			{
				var date = ZDate.Today;
				return GetCachedEUIATADataGrouping(date, ParentDataGrouping);
			}
		}

		string ParentDataGrouping
		{
			get
			{
				return Factory.GetCachedValue("JobDeclarationLookups.ParentDataGrouping_" + Parent.PK, () =>
				{
					return RefDataGrouping.GetParentDataGroupingCode(Factory, ThisDataGrouping);
				});
			}
		}

		string ThisDataGrouping
		{
			get
			{
				return Factory.GetCachedValue("JobDeclarationLookups.ThisDataGrouping_" + Parent.PK, () =>
				{
					return Parent.GetDefaultDataGroupingCode();
				});
			}
		}

		bool GetCachedEUIATADataGrouping(ZDate date, ZString groupingCode)
		{
			return Factory.GetCachedValue(Invariant($"HasIATAForThisCountry_{groupingCode}_{date.ToShortDateString()}"), () =>
			{
				var query = ZZRefCusCodeListCombined.Loader.GetFilter(Factory, groupingCode, Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, date, (ZQuery)null, false);
				return Factory.LoadTop1<ZZRefCusCodeListCombined>(query) != null;
			});
		}

		protected override ZQuery OriginPortFilter()
		{
			ZQuery result;
			var parent = Parent;
			if (parent.IsImport)
			{
				var entryStyle = parent.JE_EntryStyle;
				var extendedEUMembers = GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers();
				if (entryStyle == EntryStyleListImport.Codes.ImportFromSpecialTerritory)
				{
					result = PortFilter(extendedEUMembers, null);
				}
				else if (entryStyle == EntryStyleListImport.Codes.ImportFromEFTAMember)
				{
					result = PortFilter(((CodeDescriptionPairList)GoodsOrigin).GetAllCodes(), null);
				}
				else if (entryStyle == EntryStyleListImport.Codes.ImportNormal)
				{
					result = PortQuery(string.Empty, PortLocation.All);
				}
				else
				{
					result = PortFilter(null, extendedEUMembers);
				}
			}
			else
			{
				result = PortFilter(Core.Constants.CountryCodes.GetCountriesAndTerritoriesBelongingToCustomsJurisdiction(parent.BrokerageCountryCode).ToArray(), null);
			}
			return result;
		}

		protected override ZQuery FinalDestinationPortFilter()
		{
			ZQuery result;
			var parent = Parent;
			var extendedEUMembers = GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers();
			if (parent.IsExport)
			{
				var entryStyle = parent.JE_EntryStyle;
				if (entryStyle == EntryStyleListExport.Codes.ExportToSpecialTerritory)
				{
					result = PortFilter(extendedEUMembers, null);
				}
				else if (entryStyle == EntryStyleListExport.Codes.ExportToEFTAMember)
				{
					result = PortFilter(null, extendedEUMembers);
				}
				else if (entryStyle == EntryStyleListExport.Codes.ExportNormal)
				{
					result = PortQuery(string.Empty, PortLocation.All);
				}
				else
				{
					result = PortFilter(null, extendedEUMembers);
				}
			}
			else
			{
				result = PortFilter(extendedEUMembers, null);
			}
			return result;
		}

		protected override ZQuery DischargePortFilter() => FinalDestinationPortFilter();

		protected virtual ZQuery FirstArrivalPortFilter()
		{
			return PortFilter(GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers(), null);
		}

		protected override ZQuery LoadingPortFilter()
		{
			var result = base.LoadingPortFilter();
			var parent = Parent;
			if (parent.IsImport && parent.JE_EntryStyle == EntryStyleListImport.Codes.ImportFromEFTAMember)
			{
				result = PortFilter(null, GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers());
			}
			return result;
		}

		public override RefUNLOCOCollection PortOfLoadings
		{
			get
			{
				var result = new RefUNLOCOCollection(Factory, LoadingPortFilter(), LocoMapSystemUsage);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", Parent.JE_RL_NKPortOfLoading));//This is a default for filter business object for which schema does not exist
				return result;
			}
		}

		public override RefUNLOCOCollection PortOfFirstArrivals
		{
			get
			{
				var result = new RefUNLOCOCollection(Factory, FirstArrivalPortFilter(), LocoMapSystemUsage);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", Parent.JE_RL_NKPortOfFirstArrival));
				return result;
			}
		}

		protected ZQuery PortFilter(string[] positiveCountries, string[] negativeCountries)
		{
			var parent = Parent;
			var result = new ZQuery();

			if (parent.IsExport && !string.IsNullOrEmpty(parent.TransportMode))
			{
				CargoWise.Schema.SchemaBoolColumn filterColumn = null;
				switch (Parent.TransportMode)
				{
					case Constants.TransportModes.Air:
						filterColumn = RefUNLOCOSchema.RL_HasAirport;
						break;
					case Constants.TransportModes.Sea:
						filterColumn = RefUNLOCOSchema.RL_HasSeaport;
						break;
					case Constants.TransportModes.Rail:
						filterColumn = RefUNLOCOSchema.RL_HasRail;
						break;
					case Constants.TransportModes.Mail:
						filterColumn = RefUNLOCOSchema.RL_HasPost;
						break;
					default:
						break;
				}
				if (filterColumn != null)
				{
					result.AddToFilter(filterColumn, true);
				}
			}

			if (positiveCountries != null && positiveCountries.Length > 0)
			{
				result.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, positiveCountries);
			}

			if (negativeCountries != null && negativeCountries.Length > 0)
			{
				result.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, negativeCountries);
			}

			return result;
		}

		public virtual CustomsOfficeCodeCollection CustomsOffices
		{
			get
			{
				var roles = Parent.CustomsOfficeRequirementHelper.MainOffice?.OfficeRolesForLookup.ToArray() ?? Array.Empty<ZString>();
				var isLocalCountryOnly = Parent.CustomsOfficeRequirementHelper.MainOffice?.IsLocalCountryOnly ?? false;
				var isForeignCountryOnly = Parent.CustomsOfficeRequirementHelper.MainOffice?.IsForeignCountryOnly ?? false;
				return isLocalCountryOnly
					? CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, CustomsOfficeDataGrouping, roles)
					: (isForeignCountryOnly
						? EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRolesExceptLocal(Factory, CustomsOfficeDataGrouping, roles)
						: EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, roles));
			}
		}

		protected virtual ZString CustomsOfficeDataGrouping => Parent.GetDefaultDataGroupingCode();

		public override ICollection GoodsOrigin => EUUniversalLookupsHelper.GetCachedList(Parent.Factory, Parent.GetDefaultDataGroupingCode(),
			GoodsOriginCodeType, null, GoodsOriginDirection, true, includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildFirstThenParent);

		protected virtual ZString GoodsOriginDirection => GetGoodsDirection();

		protected virtual ZString GoodsOriginCodeType => Parent.JE_EntryStyle + "15";

		public override ICollection GoodsDestination => EUUniversalLookupsHelper.GetCachedList(Parent.Factory, Parent.GetDefaultDataGroupingCode(),
			GoodsDestinationCodeType, null, GoodsDestinationDirection, true, includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildFirstThenParent);

		protected virtual ZString GoodsDestinationDirection => GetGoodsDirection();

		protected virtual ZString GoodsDestinationCodeType => Parent.JE_EntryStyle + "17";

		public override CodeDescriptionPairList IncoTermList => Factory.GetCachedIncoTermListEU(Parent.IsUCC6);

		string[] GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers()
		{
			return Factory.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers().ToArray();
		}

		ZString GetGoodsDirection()
		{
			var entryStyle = Parent.JE_EntryStyle;
			if (Parent.IsImport)
			{
				return entryStyle == EntryStyleListImport.Codes.ImportFromSpecialTerritory || entryStyle == EntryStyleListImport.Codes.ImportFromEFTAMember
					? UniversalReferenceConstants.RefCusCodeListDirectionType.Import
					: UniversalReferenceConstants.RefCusCodeListDirectionType.Both;
			}

			return entryStyle == EntryStyleListExport.Codes.ExportToSpecialTerritory || entryStyle == EntryStyleListExport.Codes.ExportToEFTAMember
				? UniversalReferenceConstants.RefCusCodeListDirectionType.Export
				: UniversalReferenceConstants.RefCusCodeListDirectionType.Both;
		}

		protected override RefVesselCollection GetInlandVesselNamesOrLloydsCore() => new RefVesselCollection(Factory, Parent.IsSeaInland && Parent.IsTransportMeansImoShipIdentificationNumber);

		public override CodeDescriptionPairList TransportMeansList => Parent.TransportMeansDependency == EUCommonConstants.TransportModeSource.None ? base.TransportMeansList : Parent.AddInfoLookups.InlandTransportCodeList;
	}
}
