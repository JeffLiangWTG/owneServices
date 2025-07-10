using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Module
{
	public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<MessageStatusList>();

		public CodeDescriptionPairList RequestedProcedureList => RequestedProcedureHelper.GetCachedRequestedProcedureCodeList(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZString[] { IEJobMessageTypeList.Codes.Import, IEJobMessageTypeList.Codes.Export }, ZString.Empty);

		public CodeDescriptionPairList PreviousProcedureCodeList => RequestedProcedureHelper.GetCachedPreviousProcedureCodeList(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZString[] { IEJobMessageTypeList.Codes.Import, IEJobMessageTypeList.Codes.Export }, ZString.Empty, ZString.Empty);

		public CodeDescriptionPairList AdditionalProcedureCodeList => RequestedProcedureHelper.GetCachedAdditionalProcedureCodeList(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZString[] { IEJobMessageTypeList.Codes.Import, IEJobMessageTypeList.Codes.Export }, ZString.Empty, ZString.Empty, ZString.Empty);

		public virtual CodeDescriptionPairList DeclarationTypeList
		{
			get
			{
				return Factory.GetCachedValue("EUJobDeclarationFilterLookupsDeclarationTypeList", () =>
				{
					var list = new CodeDescriptionPairList();
					var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					var loader = new RefCusProcedure.Loader(Factory);
					foreach (var code in loader.LoadDistinctGroupCodes(JobMessageTypeList.Codes.Import, countryCode))
					{
						list.AddPair(code);
					}
					foreach (var code in loader.LoadDistinctGroupCodes(JobMessageTypeList.Codes.Export, countryCode))
					{
						list.AddPair(code);
					}
					list.Sort();
					return list;
				});
			}
		}

		public CodeDescriptionPairList ShipmentTypeList
		{
			get { return Factory.GetCachedValue<ShipmentTypeList>(); }
		}

		public CodeDescriptionPairList ExportExitStatusList
		{
			get
			{
				var list = new CodeDescriptionPairList(Factory.GetCachedValue<ExportExitStatus>());
				list.RemoveCode(ExportExitStatus.Codes.UnknownOrNotReported);
				return list;
			}
		}

		public CodeDescriptionPairList CommunityTransitStatusIDList
		{
			get
			{
				return Factory.GetCachedValue("EUJobDeclarationFilterLookupsCommunityTransitStatusIDList", () =>
				{
					var list = new CodeDescriptionPairList(Factory.GetCachedValue<ImportCommunityTransitStatusList>());
					list.AddRange(Factory.GetCachedValue<ExportCommunityTransitStatusList>());
					return list;
				});
			}
		}

		public CodeDescriptionPairList EntrySubTypes => EntrySubTypesCore;

		protected virtual CodeDescriptionPairList EntrySubTypesCore
		{
			get
			{
				return Factory.GetCachedValue(
					"EUJobDeclarationFilterLookupsEntrySubTypes",
					() => new CodeDescriptionPairList(Factory.GetCachedValue<EntrySubStyleList>())
				);
			}
		}

		public ZZRefCusCodeListCombinedCollection SupportingDocumentsType
		{
			get
			{
				return Factory.GetCachedValue("EUJobDeclarationFilterLookupsSupportingDocumentTypeList", () =>
				{
					return Factory.GetSupportingDocumentList(GetDataGroupingCodesForSupportingDocumentList);
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Underlying Reference Data Architecture requires an array.")]
		protected virtual ZString[] GetDataGroupingCodesForSupportingDocumentList => new ZString[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode };

		public CodeDescriptionPairList PackTypeList => Declaration.Lookups?.PackingUnitTypesList ?? new CodeDescriptionPairList();

		BaseJobDeclaration Declaration => declaration ?? (declaration = Factory.GetNull<BaseJobDeclaration>());
		BaseJobDeclaration declaration;

		public CodeDescriptionPairList CustomsOfficePurposeList => CustomsOfficePurposeListCore;

		protected virtual CodeDescriptionPairList CustomsOfficePurposeListCore => new CodeDescriptionPairList();

		public CodeDescriptionPairList CountryOfOrigins => CountryOfOriginsCore;
		protected CodeDescriptionPairList CountryOfOriginsCore => CusRefTradeGroupCountryView.Loader.GetCachedListTradeGroupCountries(Factory, ZString.Empty, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		public CodeDescriptionPairList ExitPresentationStatuses => ExitPresentationStatusesCore;

		protected virtual CodeDescriptionPairList ExitPresentationStatusesCore
		{
			get
			{
				return Factory.GetCachedValue("EUJobDeclarationFilterLookupsExitPresentationStatusesCore", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(AESEntryStatusList.Codes.ReleasedForExit, AESEntryStatusList.Descriptions.ReleasedForExit);
					result.AddPair(AESEntryStatusList.Codes.ControlledForExit, AESEntryStatusList.Descriptions.ControlledForExit);
					result.AddPair(AESEntryStatusList.Codes.MultipleStatus, AESEntryStatusList.Descriptions.MultipleStatus);
					result.AddPair(AESEntryStatusList.Codes.ExitReleaseRejected, AESEntryStatusList.Descriptions.ExitReleaseRejected);
					return result;
				});
			}
		}
	}
}
