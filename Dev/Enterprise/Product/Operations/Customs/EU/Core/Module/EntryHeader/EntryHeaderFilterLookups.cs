using System;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Universal.RefCusCodeListTypes;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Module
{
	public class EntryHeaderFilterLookups : Customs.Module.EntryHeaderFilterLookups
	{
		public EntryHeaderFilterLookups(EntryHeaderFilterBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		public CodeDescriptionPairList RequestedProcedureList => RequestedProcedureHelper.GetCachedRequestedProcedureCodeList(Factory, Country, new ZString[] { IEJobMessageTypeList.Codes.Import, IEJobMessageTypeList.Codes.Export }, ZString.Empty);

		public CodeDescriptionPairList PreviousProcedureCodeList => RequestedProcedureHelper.GetCachedPreviousProcedureCodeList(Factory, Country, new ZString[] { IEJobMessageTypeList.Codes.Import, IEJobMessageTypeList.Codes.Export }, ZString.Empty, ZString.Empty);

		public CodeDescriptionPairList AdditionalProcedureCodeList => RequestedProcedureHelper.GetCachedAdditionalProcedureCodeList(Factory, Country, new ZString[] { IEJobMessageTypeList.Codes.Import, IEJobMessageTypeList.Codes.Export }, ZString.Empty, ZString.Empty, ZString.Empty);

		public FullCustomsOfficeCodeCollection CustomsOfficeList => new FullCustomsOfficeCodeCollection(Factory);

		public CodeDescriptionPairList EntryStyleList => Factory.GetCachedValue(GetEntryStyleListCacheKey(), GetEntryStyleList);

		public CodeDescriptionPairList GoodsOriginList => GetGoodsOriginDestinationList(GoodsOriginCodeTypeSuffix);

		public CodeDescriptionPairList GoodsDestinationList => GetGoodsOriginDestinationList(GoodsDestinationCodeTypeSuffix);

		public BondedWarehouseCollection Warehouses => new BondedWarehouseCollection(Factory);

		public DebtorCollection LocalClients => new DebtorCollection(Factory);

		public CodeDescriptionPairList ExportExitStatusList
		{
			get
			{
				var list = new CodeDescriptionPairList(Factory.GetCachedValue<ExportExitStatus>());
				list.RemoveCode(ExportExitStatus.Codes.UnknownOrNotReported);
				return list;
			}
		}

		#region Implementation

		string GetEntryStyleListCacheKey() => FormattableString.Invariant($"{Country}.EntryHeaderFilterLookups.EntryStyleList");

		CodeDescriptionPairList GetEntryStyleList()
		{
			var result = new CodeDescriptionPairList();

			result.AddRange(Factory.GetCachedValue<EntryStyleListImportUCC>());
			result.AddRangeOverwriteIfExists(Factory.GetCachedValue<EntryStyleListImport>());
			result.AddRangeOverwriteIfExists(Factory.GetCachedValue<EntryStyleListExportUCC>());
			result.AddRangeOverwriteIfExists(Factory.GetCachedValue<EntryStyleListExport>());
			result.AddRange(ZZRefCusCodeListCombined.Loader.Load(Factory, Country, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, ZDateTime.Now, includeParentDataGrouping: false));

			return result;
		}

		CodeDescriptionPairList GetGoodsOriginDestinationList(string codeTypeSuffix)
		{
			var entryStyleList = EntryStyleList;
			var result = new CodeDescriptionPairList();
			foreach (var entryStyle in entryStyleList)
			{
				var codeType = entryStyle + codeTypeSuffix;
				var goodsOriginList = EUUniversalLookupsHelper.GetCachedList(Factory, Country, codeType, null, UniversalReferenceConstants.RefCusCodeListDirectionType.Both, ignoreLevel: true, IncludeParentDataGroupingOptions.ChildFirstThenParent);
				result.AddRangeOverwriteIfExists(goodsOriginList);
			}

			return result;
		}

		ZString Country => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		const string GoodsDestinationCodeTypeSuffix = "17";
		const string GoodsOriginCodeTypeSuffix = "15";

		#endregion
	}
}
