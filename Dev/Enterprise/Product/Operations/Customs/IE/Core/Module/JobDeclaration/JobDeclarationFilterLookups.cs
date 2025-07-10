using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Module
{
	public class JobDeclarationFilterLookups : EU.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList MessageStatusList() => Factory.GetCachedValue<IELogicalStatusList>();

		public override CodeDescriptionPairList DeclarationTypeList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("IE|JobDeclarationFilterLookups|DeclarationTypeList", () =>
				{
					var list = new ExportDeclarationTypeList();
					list.AddRange(new ImportDeclarationTypeList());
					list.AddRange(new ExitSummaryDeclarationTypeList());
					list.AddRange(new ReExportDeclarationTypeList());
					list.Sort();
					return list;
				});
			}
		}

		public override CodeDescriptionPairList ApplicationCodeList() => Factory.GetCachedValue<ImportDeclarationApplicationCodeList>();

		public CodeDescriptionPairList ApplicationCodeSearchFilterList() => Factory.GetCachedValue<DeclarationApplicationSearchFilterCodeList>();

		protected override ZString[] GetDataGroupingCodesForSupportingDocumentList => base.GetDataGroupingCodesForSupportingDocumentList.Union(new ZString[] { Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUcc5 }).ToArray();
	}
}
