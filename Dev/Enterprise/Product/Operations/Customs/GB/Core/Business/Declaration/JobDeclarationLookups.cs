using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public partial class JobDeclarationLookups : AutoGBJobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent) : base(parent)
		{
		}

		protected override ZString CustomsOfficeDataGrouping => Parent.CountryCode;

		protected override CodeDescriptionPairList EntryStatusListForDefaultFallBack
		{
			get => Factory.GetCachedValue(" EntryStatusListForDefaultFallBack.GB", () =>
				{
					var list = new Customs.Common.EU.EntryStatusList();
					list.RemoveCode(Enterprise.Customs.Common.EU.EntryStatusList.Codes.NotSent);
					return list;
				});
		}

		public override CodeDescriptionPairList TransportTypeList => Factory.GetCachedValue<CodeDescriptionPairList>("GBTransportTypeList",
			() => new GBCombinedTransportTypeList());

		protected override CodeDescriptionPairList ModeOfTransportListCore()
		{
			var list = new ModeOfTransportList();
			list.AddPairIfNotExist(CodeDescriptionPairLists.GBModeOfTransportList.Codes._6_RoRoFreight, CodeDescriptionPairLists.GBModeOfTransportList.Descriptions._6_RoRoFreight);
			list.Sort();
			return list;
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public override CodeDescriptionPairList ProfileList => BadgeCodeGetter.InstanceCachedFor(Parent).GetBadgeList(Parent.IsImport ? BadgeDirectionList.Codes.IMP : BadgeDirectionList.Codes.EXP, Parent.BarrierPort);

		public override CodeDescriptionPairList IncoTermList
		{
			get
			{
				var transportMode = Parent.JE_TransportMode;
				return Factory.GetCachedValue("Declaration|IncoTermList|" + transportMode, () => new IncoTermsCodeDescriptionPairList(transportMode));
			}
		}

		protected ZZRefCusCodeListCombinedCollection FacilityCollection
		{
			get
			{
				var collection = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today);
				collection.Load();
				collection.Sort(ZZRefCusCodeListCombinedSchema.ZZD_Code.Name);
				return collection;
			}
		}

		protected override ICollection LocationsCore
		{
			get
			{
				var list = new CodeDescriptionPairList();
				if (!string.IsNullOrEmpty(Parent.TransportMode))
				{
					return Factory.GetCachedValue("GBPortsAndShedsList_" + Parent.TransportMode, () =>
					{
						var portList = new CodeDescriptionPairList();
						portList.AddRange(new PortCollection(Factory, Parent.CountryCode, Parent.TransportMode));
						portList.Sort();
						list.AddRange(portList);

						if (Parent.TransportMode == Customs.Business.TransportTypeList.Codes.Air)
						{
							foreach (ZZRefCusCodeListCombined item in FacilityCollection)
							{
								var airportNameAttribute = item.GetAttribute(EU.Business.UniversalReferenceConstants.ShedAttributes.AirportName);
								if (!string.IsNullOrEmpty(airportNameAttribute))
								{
									list.AddPairIfNotExist(item.ZZD_Code.Left(3), airportNameAttribute);
								}
							}
						}
						list.Sort();
						return list;
					});
				}
				return list;
			}
		}

		public CodeDescriptionPairList ShedsList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				if (!string.IsNullOrEmpty(Parent.TransportMode) && Parent.TransportMode == Customs.Business.TransportTypeList.Codes.Air)
				{
					return Factory.GetCachedValue("GBShedsList_" + Parent.TransportMode + Parent.JE_LocationOfGoods, () =>
					{
						foreach (Shed shed in new ShedCollection(Factory, Parent.CountryCode, Parent.JE_LocationOfGoods, ZString.Empty, findOnlyShedsWithAnAirportNameAttribute: true))
						{
							list.AddPair(shed.ShedCode, shed.Name);
						}
						list.Sort();

						return list;
					});
				}
				else
				{
					return Factory.GetCachedValue("GBShedsList_" + Parent.TransportMode + Parent.JE_LocationOfGoods, () =>
					{
						foreach (Shed shed in new ShedCollection(Factory, Parent.CountryCode, Parent.JE_LocationOfGoods, ZString.Empty, false, Parent.TransportMode))
						{
							list.AddPair(shed.ShedCode, shed.Name);
						}
						list.Sort();

						return list;
					});
				}
			}
		}

		public override CodeDescriptionPairList ApplicationCodeList
		{
			get
			{
				var interfaceSubmissionType = Parent.GetInterfaceSubmissionType();
				var isInterfaceSubmissionTypeSetToNotBuiltin = !interfaceSubmissionType.IsEmpty && interfaceSubmissionType != DeclarationApplicationCodeList.Codes.Builtin;
				var supportCHIEF = Parent.WasSavedAsCHIEF;
				return Factory.GetCachedValue("GBApplicationCodeList_" + "_" + isInterfaceSubmissionTypeSetToNotBuiltin + supportCHIEF, () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, Registry.Business.DeclarationApplicationCodeList.Descriptions.Customs_Declaration_Services);
					if (supportCHIEF)
					{
						result.AddPair(Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF, Registry.Business.DeclarationApplicationCodeList.Descriptions.CHIEF);
					}
					if (isInterfaceSubmissionTypeSetToNotBuiltin)
					{
						result.AddPair(DeclarationApplicationCodeList.Codes.Interfaced, DeclarationApplicationCodeList.Descriptions.Interfaced);
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList LocationQualifiers
		{
			get { return Factory.GetCachedValue<CodeDescriptionPairLists.LocationQualifierList>(); }
		}

		public RefCountryCollection LocationOfGoodsCountries
		{
			get { return new RefCountryCollection(Factory); }
		}

		public CodeDescriptionPairList LocationOfGoodsTypes
		{
			get { return RefCusCodeListTypes.GetCachedList(Factory, Parent.CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType, ZDateTime.Today, null, string.Empty, false); }
		}
		public const string BY_AuthorisedPlaceAuthorisationNumber = "BY";

		public CodeDescriptionPairList LocationOfGoods
		{
			get
			{
				return RefCusCodeListTypes.GetCachedListMatchAllAttributes(Factory,
											Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService,
											Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port,
											ZDateTime.Today,
											new[] { new KeyValuePair<ZString, ZString>(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Facility, Parent.JE_Calc_LocationOtherInformationType) },
											Parent.JE_TransportMode);
			}
		}

		protected override ZQuery OriginPortFilter()
		{
			return PortFilter(null, null);
		}

		protected override ZQuery FinalDestinationPortFilter()
		{
			return PortFilter(null, null);
		}

		protected override ZQuery DischargePortFilter()
		{
			return PortFilter(null, null);
		}

		public CodeDescriptionPairList GbNIMode
		{
			get => Factory.GetCachedValue<NIModeList>();
		}

		#region UKSupervisingOffice_List

		public ZZRefCusCodeListCombinedCollection CustomsSupervisingOfficeList
		{
			get
			{
				return Factory.GetCachedValue(
					"JobDeclaration.CustomsSupervisingOfficeList", () =>
					{
						var collection = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupervisingOffice, ZDateTime.Today);
						collection.Load();

						return collection;
					});
			}
		}

		#endregion

		#region Customs Office List

		public override CustomsOfficeCodeCollection CustomsOffices
		{
			get
			{
				var roles = Parent.CustomsOfficeRequirementHelper.MainOffice?.OfficeRolesForLookup.ToArray() ?? Array.Empty<ZString>();
				CustomsOfficeCodeCollection result;
				if (Parent.IsExport && Parent.ZG_NorthernIrelandMode == NIModeList.Codes.NotToOrFromNi)
				{
					result = GbCustomsOfficeCodeCollection.AllUKCustomsOfficesWithRequiredRoles(Factory, roles);
				}
				else if (Parent.IsExport && Parent.ZG_NorthernIrelandMode == NIModeList.Codes.ExportFromNiToRestOfWorld)
				{
					result = EUCustomsOfficeCodeCollection.AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRoles(Factory, roles);
				}
				else
				{
					var isLocalCountryOnly = Parent.CustomsOfficeRequirementHelper.MainOffice?.IsLocalCountryOnly ?? false;
					result = isLocalCountryOnly && CustomsOfficeDataGrouping == CountryCodes.UnitedKingdom
						? CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, new ZString[] { CountryCodes.UnitedKingdom, CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes }, roles)
						: base.CustomsOffices;
				}
				return result;
			}
		}

		public new BusinessObjectCollection CustomsOfficeList => EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, CustomsOfficeListPurpose());

		ZString[] CustomsOfficeListPurpose() => CustomsOfficeListPurposeCore();

		protected virtual ZString[] CustomsOfficeListPurposeCore() => new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit };

		#endregion

		public CodeDescriptionPairList CourierConsignmentTypes => Factory.GetCachedValue<CourierConsignmentTypes>();

		public CodeDescriptionPairList EidrTypes => Factory.GetCachedValue<EidrTypeList>();

		public CodeDescriptionPairList ImportClearanceStatusICSList => new DeclarationStatusICSList();
	}
}
