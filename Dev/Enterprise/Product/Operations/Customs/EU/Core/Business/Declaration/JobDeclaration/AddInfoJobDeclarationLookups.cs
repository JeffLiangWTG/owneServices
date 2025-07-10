using System.Collections;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using TransportTypeListCodes = Enterprise.Customs.Business.TransportTypeList.Codes;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public partial class JobDeclarationLookups
	{
		protected JobDeclaration Declaration => Parent;

		public virtual EUNctsGuaranteeTypeList NctsGuaranteeList => new EUNctsGuaranteeTypeList();

		public virtual CodeDescriptionPairList CustomsCopyTemplateList => new CodeDescriptionPairList();

		public CodeDescriptionPairList RouteOfEntryList => RouteOfEntryListCore;
		protected virtual CodeDescriptionPairList RouteOfEntryListCore => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList GatewayList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList AuthorisationNumberList => new CodeDescriptionPairList();

		public CodeDescriptionPairList RegionOfDestinationList => RegionOfDestinationListCore;
		protected virtual CodeDescriptionPairList RegionOfDestinationListCore => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList SpecificCircumstanceIndicatorList => Factory.GetCachedValue<SpecificCircumstanceIndicator>();

		public CodeDescriptionPairList InlandTransportCodeList => GetInlandTransportCodeDescriptionPairList();

		public RefCountryCollection Box18TransportCountryList => new RefCountryCollection(Factory);

		public CodeDescriptionPairList CommunityTransitStatusIDList
		{
			get
			{
				CodeDescriptionPairList result;
				if (Declaration.IsExport)
				{
					result = GetExportCommunityTransitStatusIDList();
				}
				else if (Declaration.IsImport)
				{
					result = GetImportCommunityTransitStatusIDList();
				}
				else
				{
					result = GetOthersCommunityTransitStatusIDList();
				}
				return result;
			}
		}

		protected virtual CodeDescriptionPairList GetExportCommonTransitStatusIDList()
		{
			return Factory.GetCachedValue("GetExportCommonTransitStatusIDList", () =>
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddRange(GetExportCommunityTransitStatusIDList());
				result.RemoveCode("C");
				return result;
			});
		}

		protected virtual CodeDescriptionPairList GetExportCommunityTransitStatusIDList() => Factory.GetCachedValue<ExportCommunityTransitStatusList>();

		protected virtual CodeDescriptionPairList GetImportCommunityTransitStatusIDList() => Factory.GetCachedValue<ImportCommunityTransitStatusList>();

		protected virtual CodeDescriptionPairList GetOthersCommunityTransitStatusIDList() => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList DeferTypeList => Factory.GetCachedValue<DefermentMethodList>();

		public virtual CodeDescriptionPairList ShipmentTypeList => Factory.GetCachedValue<ShipmentTypeList>();

		public CodeDescriptionPairList SecurityTypeList => Factory.GetCachedValue<ExportSecurityTypeList>();

		public CodeDescriptionPairList StyleOfEntrySOEList => StyleOfEntrySOEListCore;
		protected virtual CodeDescriptionPairList StyleOfEntrySOEListCore => new CodeDescriptionPairList();

		public virtual ICollection AgreedPlaceCodeList => Declaration.AgreedPlaceCodeSupport ? (Declaration.ZG_AgreedPlaceCode.Length == 2 ? new RefCountryCollection(Factory) : new RefUNLOCOCollection(Factory)) : Factory.GetAgreedPlaceCodeList(Declaration.CountryCode);

		public virtual CodeDescriptionPairList MethodOfPaymentList
		{
			get
			{
				var dataGroupingCode = Declaration.GetDefaultDataGroupingCode();
				return Factory.GetCachedValue("MethodOfPaymentList_" + dataGroupingCode, () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, ZDateTime.Now).OrderBy(x => x.ZZD_Code).ToArray());
					return result;
				});
			}
		}

		public virtual CodeDescriptionPairList BorderTransportMeansList
		{
			get
			{
				var isImport = Declaration.IsImport;
				var transportMode = Declaration.JE_TransportMode;
				return Factory.GetCachedValue(string.Join("|", "EU.JobDeclarationLookups.BorderTransportMeansList", isImport, transportMode), () =>
				{
					var result = new CodeDescriptionPairList();
					if (isImport)
					{
						result = GetBorderTransportMeansListForImport(transportMode, result);
					}
					else
					{
						result = GetBorderTransportMeansListForExport(transportMode, result);
					}
					return result;
				});
			}
		}

		CodeDescriptionPairList GetBorderTransportMeansListForImport(ZString transportMode, CodeDescriptionPairList result)
		{
			switch (transportMode)
			{
				case TransportTypeListCodes.Road:
					result.AddPair(ImportEUBorderTransportMeansList.Codes.Truck, ImportEUBorderTransportMeansList.Descriptions.Truck);
					result.AddPair(ImportEUBorderTransportMeansList.Codes.Car, ImportEUBorderTransportMeansList.Descriptions.Car);
					result.AddPair(ImportEUBorderTransportMeansList.Codes.Excluded, ImportEUBorderTransportMeansList.Descriptions.Excluded);
					break;
				case TransportTypeListCodes.InlandWaterwayTransport:
				case TransportTypeListCodes.Sea:
					result.AddPair(ImportEUBorderTransportMeansList.Codes.Ship, ImportEUBorderTransportMeansList.Descriptions.Ship);
					result.AddPair(ImportEUBorderTransportMeansList.Codes.Excluded, ImportEUBorderTransportMeansList.Descriptions.Excluded);
					break;
				case TransportTypeListCodes.Rail:
					result.AddPair(ImportEUBorderTransportMeansList.Codes.Train, ImportEUBorderTransportMeansList.Descriptions.Train);
					result.AddPair(ImportEUBorderTransportMeansList.Codes.Excluded, ImportEUBorderTransportMeansList.Descriptions.Excluded);
					break;
				case TransportTypeListCodes.Air:
					result.AddPair(ImportEUBorderTransportMeansList.Codes.Plane, ImportEUBorderTransportMeansList.Descriptions.Plane);
					result.AddPair(ImportEUBorderTransportMeansList.Codes.Excluded, ImportEUBorderTransportMeansList.Descriptions.Excluded);
					break;
				case TransportTypeListCodes.FixedTransportInstallations:
				case TransportTypeListCodes.OwnPropulsion:
				case TransportTypeListCodes.Mail:
					result.AddPair(ImportEUBorderTransportMeansList.Codes.Excluded, ImportEUBorderTransportMeansList.Descriptions.Excluded);
					result.AddPair(ImportEUBorderTransportMeansList.Codes.Other, ImportEUBorderTransportMeansList.Descriptions.Other);
					break;
				default:
					break;
			}
			return result;
		}

		protected CodeDescriptionPairList GetBorderTransportMeansListForExport(ZString transportMode, CodeDescriptionPairList result)
		{
			switch (transportMode)
			{
				case TransportTypeListCodes.Sea:
					result.AddPair(ExportBorderTransportMeansList.Codes._10, ExportBorderTransportMeansList.Descriptions._10);
					result.AddPair(ExportBorderTransportMeansList.Codes._11, ExportBorderTransportMeansList.Descriptions._11);
					result.DefaultCode = ExportBorderTransportMeansList.Codes._10;
					break;
				case TransportTypeListCodes.Rail:
					result.AddPair(ExportBorderTransportMeansList.Codes._21, ExportBorderTransportMeansList.Descriptions._21);
					result.DefaultCode = ExportBorderTransportMeansList.Codes._21;
					break;
				case TransportTypeListCodes.Road:
					result.AddPair(ExportBorderTransportMeansList.Codes._30, ExportBorderTransportMeansList.Descriptions._30);
					result.DefaultCode = ExportBorderTransportMeansList.Codes._30;
					break;
				case TransportTypeListCodes.Air:
					result.AddPair(ExportBorderTransportMeansList.Codes._40, ExportBorderTransportMeansList.Descriptions._40);
					result.AddPair(ExportBorderTransportMeansList.Codes._41, ExportBorderTransportMeansList.Descriptions._41);
					result.DefaultCode = ExportBorderTransportMeansList.Codes._40;
					break;
				case TransportTypeListCodes.Mail:
				case TransportTypeListCodes.FixedTransportInstallations:
				case TransportTypeListCodes.OwnPropulsion:
					result.AddPair(ExportBorderTransportMeansList.Codes._10, ExportBorderTransportMeansList.Descriptions._10);
					result.AddPair(ExportBorderTransportMeansList.Codes._11, ExportBorderTransportMeansList.Descriptions._11);
					result.AddPair(ExportBorderTransportMeansList.Codes._21, ExportBorderTransportMeansList.Descriptions._21);
					result.AddPair(ExportBorderTransportMeansList.Codes._30, ExportBorderTransportMeansList.Descriptions._30);
					result.AddPair(ExportBorderTransportMeansList.Codes._40, ExportBorderTransportMeansList.Descriptions._40);
					result.AddPair(ExportBorderTransportMeansList.Codes._41, ExportBorderTransportMeansList.Descriptions._41);
					result.AddPair(ExportBorderTransportMeansList.Codes._80, ExportBorderTransportMeansList.Descriptions._80);
					result.AddPair(ExportBorderTransportMeansList.Codes._81, ExportBorderTransportMeansList.Descriptions._81);
					result.DefaultCode = null;
					break;
				case TransportTypeListCodes.InlandWaterwayTransport:
					result.AddPair(ExportBorderTransportMeansList.Codes._80, ExportBorderTransportMeansList.Descriptions._80);
					result.AddPair(ExportBorderTransportMeansList.Codes._81, ExportBorderTransportMeansList.Descriptions._81);
					result.DefaultCode = ExportBorderTransportMeansList.Codes._80;
					break;
				default:
					break;
			}
			return result;
		}

		CodeDescriptionPairList GetInlandTransportCodeDescriptionPairList()
		{
			var declaration = Declaration;
			var factoryCacheKey = string.Join("|", "EU.JobDeclarationLookups.InlandTransportCodeList", declaration.JE_MessageType, declaration.IsUCC6, declaration.TransportModeValueForTransportMeans);

			return Factory.GetCachedValue(factoryCacheKey, () =>
			{
				var codeDescriptionPairBuilder = GetInlandTransportCodeDescriptionPairListBuilder();
				return codeDescriptionPairBuilder?.GetList() ?? new MeansOfTransportList();
			});
		}

		protected InlandTransportCodeDescriptionPairListBuilder GetInlandTransportCodeDescriptionPairListBuilder()
		{
			InlandTransportCodeDescriptionPairListBuilder result = null;
			var declaration = Declaration;
			if (declaration.IsUCC6AndIsImport)
			{
				result = GetImportInlandTransportCodeDescriptionPairListBuilder(declaration);
			}
			else if (declaration.IsUCC6AndIsExport)
			{
				result = GetExportInlandTransportCodeDescriptionPairListBuilder(declaration);
			}

			return result;
		}

		protected virtual InlandTransportCodeDescriptionPairListBuilder GetImportInlandTransportCodeDescriptionPairListBuilder(JobDeclaration declaration) => new InlandTransportCodeDescriptionPairListBuilder(declaration);

		protected virtual InlandTransportCodeDescriptionPairListBuilder GetExportInlandTransportCodeDescriptionPairListBuilder(JobDeclaration declaration) => new InlandTransportCodeDescriptionPairListBuilder(declaration);
	}
}
