using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Module
{
	public class JobDeclarationFilterLookups : EU.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(EU.Module.JobDeclarationFilterBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		public CodeDescriptionPairList ParallelList => Factory.GetCachedValue<Customs.Business.YesNoList>();

		public override CodeDescriptionPairList DeclarationTypeList => Factory.GetNull<CusEntryInstruction>().Lookups.StyleList;

		protected override CodeDescriptionPairList EntrySubTypesCore
		{
			get
			{
				return Factory.GetCachedValue("EUJobDeclarationFilterLookupsEntrySubTypes", () =>
				{
					var list = new CodeDescriptionPairList(Factory.GetCachedValue<ExsEntrySubStyleList>());
					list.Sort();
					return list;
				});
			}
		}

		protected override CodeDescriptionPairList CustomsOfficePurposeListCore
		{
			get
			{
				return Factory.GetCachedValue("EUJobDeclarationFilterLookupsCustomsOfficePurposeList_ES", () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, EuOfficeCodesTypes.Descriptions.OfficeOfEntryFirstOrSubsequent);
					list.AddPair(EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Descriptions.OfficeOfExit);
					list.AddPair(EuOfficeCodesTypes.Codes.OfficeOfExport, EuOfficeCodesTypes.Descriptions.OfficeOfExport);
					return list;
				});
			}
		}

		public CodeDescriptionPairList StateIslandCodesList => CustomsFiscalTerritoriesList.GetSpainFullList(Factory);
	}
}
