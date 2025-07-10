using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Module
{
	public class JobDeclarationFilterLookups : EU.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		public CodeDescriptionPairList DeltaModeList => Factory.GetCachedValue<OrgCusAccountDeltaGTypeList>();

		public override CodeDescriptionPairList ApplicationCodeList() => Factory.GetCachedValue<DeclarationApplicationCodeList>();

		protected override ZString[] GetDataGroupingCodesForSupportingDocumentList => FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.Value || FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.Value
			? base.GetDataGroupingCodesForSupportingDocumentList.Union(new ZString[] { Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE }).ToArray()
			: base.GetDataGroupingCodesForSupportingDocumentList;
	}
}
