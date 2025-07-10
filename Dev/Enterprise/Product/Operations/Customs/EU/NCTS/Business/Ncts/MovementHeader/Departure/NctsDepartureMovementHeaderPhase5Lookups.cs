using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureMovementHeaderPhase5Lookups : NctsCommonMovementHeaderLookups, INctsDepartureMovementHeaderLookups
	{
		public NctsDepartureMovementHeaderPhase5Lookups(NctsDepartureMovementHeader parent)
			: base(parent)
		{
		}

		protected new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

		public CodeDescriptionPairList NctsMessageStatusList => Parent.Header.Lookups.NctsMessageStatusList;

		public BondedWarehouseCollection BondedWarehouseCollection => new BondedWarehouseCollection(Parent.Factory);

		public OrgHeaderCollection Organisations => new OrgHeaderCollection(Factory);

		public virtual CodeDescriptionPairList SpecificCircumstanceIndicatorList => new CodeDescriptionPairList();

		public CodeDescriptionPairList NctsSpecificCircumstanceIndicatorList => NctsSpecificCircumstanceIndicatorListCore;

		protected virtual CodeDescriptionPairList NctsSpecificCircumstanceIndicatorListCore => Factory.GetCachedValue<NctsSpecificCircumstanceIndicatorList>();

		public CodeDescriptionPairList TransportChargesModeOfPaymentList => Factory.GetCachedValue<TransportChargesModeOfPayment>();

		public CodeDescriptionPairList NctsControlResultList => Factory.GetCachedValue<NctsControlResult>();

		public CodeDescriptionPairList SealTypeList => Factory.GetCachedValue<SealTypeList>();

		public CodeDescriptionPairList ModeOfTransportList => ModeOfTransportListCore;

		protected virtual CodeDescriptionPairList ModeOfTransportListCore => Factory.GetCachedValue<ModeOfTransportList>();

		public virtual CodeDescriptionPairList AdditionalDeclarationTypeList => Factory.GetCachedValue<NctsTypeOfAdditionalDeclarationList>();

		public CodeDescriptionPairList BorderModeOfTransportList => BorderModeOfTransportListCore;

		protected virtual CodeDescriptionPairList BorderModeOfTransportListCore => Factory.GetCachedValue<ModeOfTransportList>();

		public CodeDescriptionPairList TransportAtBorderTypeOfIdList => TransportAtBorderTypeOfIdListCore;

		protected virtual CodeDescriptionPairList TransportAtBorderTypeOfIdListCore
		{
			get
			{
				var transportMode = Parent.BM_ExportTransportMode;

				return Factory.GetCachedValue($"EUTransportAtBorderTypeOfIdList_{transportMode}", () =>
				{
					var list = new CodeDescriptionPairList();
					switch (transportMode)
					{
						case EU.Business.ModeOfTransportList.Codes._1_SeaTransport:
							list.AddPair(NctsTransportTypeOfIdList.Codes._10, NctsTransportTypeOfIdList.Descriptions._10);
							list.AddPair(NctsTransportTypeOfIdList.Codes._11, NctsTransportTypeOfIdList.Descriptions._11);
							list.DefaultCode = NctsTransportTypeOfIdList.Codes._10;
							break;
						case EU.Business.ModeOfTransportList.Codes._2_RailTransport:
							list.AddPair(NctsTransportTypeOfIdList.Codes._21, NctsTransportTypeOfIdList.Descriptions._21);
							list.DefaultCode = NctsTransportTypeOfIdList.Codes._21;
							break;
						case EU.Business.ModeOfTransportList.Codes._3_RoadTransport:
							list.AddPair(NctsTransportTypeOfIdList.Codes._30, NctsTransportTypeOfIdList.Descriptions._30);
							list.DefaultCode = NctsTransportTypeOfIdList.Codes._30;
							break;
						case EU.Business.ModeOfTransportList.Codes._4_AirTransport:
							list.AddPair(NctsTransportTypeOfIdList.Codes._40, NctsTransportTypeOfIdList.Descriptions._40);
							list.AddPair(NctsTransportTypeOfIdList.Codes._41, NctsTransportTypeOfIdList.Descriptions._41);
							list.DefaultCode = NctsTransportTypeOfIdList.Codes._40;
							break;
						case EU.Business.ModeOfTransportList.Codes._8_InlandWaterwayTransport:
							list.AddPair(NctsTransportTypeOfIdList.Codes._80, NctsTransportTypeOfIdList.Descriptions._80);
							list.AddPair(NctsTransportTypeOfIdList.Codes._81, NctsTransportTypeOfIdList.Descriptions._81);
							list.DefaultCode = NctsTransportTypeOfIdList.Codes._80;
							break;
						default:
							list = Factory.GetCachedValue("EU.NCTS.DefaultTransportAtBorderTypeOfIdList", GetTransportAtBorderTypeOfIdList);
							break;
					}
					return list;
				});
			}
		}

		protected CodeDescriptionPairList GetTransportAtBorderTypeOfIdList()
		{
			var list = new NctsTransportTypeOfIdList();
			list.RemoveCode(NctsTransportTypeOfIdList.Codes._20);
			list.RemoveCode(NctsTransportTypeOfIdList.Codes._31);
			return list;
		}

		public CodeDescriptionPairList TransportAtDepartureTypeOfIdList => TransportAtDepartureTypeOfIdListCore;

		protected virtual CodeDescriptionPairList TransportAtDepartureTypeOfIdListCore
		{
			get
			{
				CodeDescriptionPairList result;
				if (Parent is NctsDepartureMovementHeader departureMovementHeader)
				{
					var inlandTransportMode = departureMovementHeader.InlandTransportModeAtDeparture;
					result = Factory.GetCachedValue($"EUNCTSTransportAtDepartureTypeOfIdList_{inlandTransportMode}", () =>
					{
						var list = new CodeDescriptionPairList();
						switch (inlandTransportMode)
						{
							case EU.Business.ModeOfTransportList.Codes._1_SeaTransport:
								list.AddPair(NctsTransportTypeOfIdList.Codes._10, NctsTransportTypeOfIdList.Descriptions._10);
								list.AddPair(NctsTransportTypeOfIdList.Codes._11, NctsTransportTypeOfIdList.Descriptions._11);
								list.DefaultCode = NctsTransportTypeOfIdList.Codes._10;
								break;
							case EU.Business.ModeOfTransportList.Codes._2_RailTransport:
								list.AddPair(NctsTransportTypeOfIdList.Codes._20, NctsTransportTypeOfIdList.Descriptions._20);
								list.AddPair(NctsTransportTypeOfIdList.Codes._21, NctsTransportTypeOfIdList.Descriptions._21);
								list.DefaultCode = NctsTransportTypeOfIdList.Codes._20;
								break;
							case EU.Business.ModeOfTransportList.Codes._3_RoadTransport:
								list.AddPair(NctsTransportTypeOfIdList.Codes._30, NctsTransportTypeOfIdList.Descriptions._30);
								list.DefaultCode = NctsTransportTypeOfIdList.Codes._30;
								break;
							case EU.Business.ModeOfTransportList.Codes._4_AirTransport:
								list.AddPair(NctsTransportTypeOfIdList.Codes._40, NctsTransportTypeOfIdList.Descriptions._40);
								list.AddPair(NctsTransportTypeOfIdList.Codes._41, NctsTransportTypeOfIdList.Descriptions._41);
								list.DefaultCode = NctsTransportTypeOfIdList.Codes._40;
								break;
							case EU.Business.ModeOfTransportList.Codes._8_InlandWaterwayTransport:
								list.AddPair(NctsTransportTypeOfIdList.Codes._80, NctsTransportTypeOfIdList.Descriptions._80);
								list.AddPair(NctsTransportTypeOfIdList.Codes._81, NctsTransportTypeOfIdList.Descriptions._81);
								list.DefaultCode = NctsTransportTypeOfIdList.Codes._80;
								break;
							case EU.Business.ModeOfTransportList.Codes._7_FixedTransportInstallations:
							case EU.Business.ModeOfTransportList.Codes._9_OwnPropulsion:
								list = new NctsTransportTypeOfIdList();
								list.RemoveCode(NctsTransportTypeOfIdList.Codes._99);
								break;
							default:
								list = new NctsTransportTypeOfIdList();
								break;
						}

						return list;
					});
				}
				else
				{
					result = Factory.GetCachedValue<NctsTransportTypeOfIdList>();
				}
				return result;
			}
		}

		public override CodeDescriptionPairList DeclarationTypeList => Factory.GetCachedValue<NctsPhase5DeclarationTypeList>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public OrganisationsFindBoxCollection Representatives
		{
			get
			{
				const string categoryFilterCode = "Category";
				var representatives = new OrganisationsFindBoxCollection(Factory);
				representatives.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(categoryFilterCode, "Property", (ZString)OrgConstants.Category.NaturalPersonIndividual, true));
				return representatives;
			}
		}

		public CodeDescriptionPairList TypeOfSecurityList => TypeOfSecurityListCore;
		protected virtual CodeDescriptionPairList TypeOfSecurityListCore => Factory.GetCachedValue<NctsTypeOfSecurityList>();

		public virtual CodeDescriptionPairList LocationOfGoodsCodeList => new CodeDescriptionPairList();

		public RefVesselCollection Vessels
		{
			get
			{
				if (Parent is NctsDepartureMovementHeader departureMovementHeader && departureMovementHeader.IsSeaInlandTransport
					&& departureMovementHeader.TransportTypeAtDeparture == NctsTransportTypeOfIdList.Codes._10)
				{
					return new RefVesselCollection(Factory, true);
				}
				else
				{
					return new RefVesselCollection(Factory);
				}
			}
		}

		public CodeDescriptionPairList OfficeCodeList => Factory.GetCachedValue(OfficeCodeListCacheKey, () =>
		{
			var list = new CodeDescriptionPairList();
			var moveHeader = Parent;
			if (moveHeader != null)
			{
				foreach (var office in moveHeader.CustomsOfficesForDeparture.Cast<NctsEuOfficeCode>())
				{
					if (IsOfficeValidForOfficeCodeList(office))
					{
						list.AddPairIfNotExist(office.CY_Data, office.CY_OfficeDescription);
					}
				}
			}
			return list;
		});

		protected virtual bool IsOfficeValidForOfficeCodeList(NctsEuOfficeCode office) => validOfficeCodesForOfficeCodeList.Contains(office.CY_Code);

		internal const string OfficeCodeListCacheKey = "EU.NCTS.NctsDepartureMovementHeaderPhase5Lookups.OfficeCodeList";

		public ICollection TOLCarrierIDList
		{
			get
			{
				if (Parent.IsSeaExportTransportMode)
				{
					return new RefVesselCollection(Factory, Parent.BM_ActiveBorderIdentificationType == NctsTransportTypeOfIdList.Codes._10);
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		public ICollection TOLCarrierNationalityList => Factory.GetNCNATCountryList();

		public ICollection ForeignDestPortCodes => Parent.BM_ForeignDestPortKCode.Length == 2
			? Factory.GetCountryNC008Collection(Parent.DefaultDataGroupingCode)
			: new RefUNLOCOCollection(Factory);

		public ICollection PortOfPresentationCodes => Parent.BM_PortOfPresentationCode.Length == 2
			? Factory.GetCountryNC008Collection(Parent.DefaultDataGroupingCode)
			: new RefUNLOCOCollection(Factory);

		protected override CodeDescriptionPairList NctsTransitStatusListCore => Factory.GetCachedValue<NCTS5DepartureCustomsStatusList>();

		static readonly ImmutableHashSet<string> validOfficeCodesForOfficeCodeList = new HashSet<string>
		{
			OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture,
			OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination,
			OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit,
			OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit
		}.ToImmutableHashSet();
	}
}
