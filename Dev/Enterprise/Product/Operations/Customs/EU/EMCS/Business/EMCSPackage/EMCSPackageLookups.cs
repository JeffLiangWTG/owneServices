using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSPackageLookups : CusInvPackLookups
	{
		public EMCSPackageLookups(EMCSPackage parent) : base(parent)
		{
		}

		public CodeDescriptionPairList PackTypeList => Factory.GetEMCSPackTypeList(Parent.Parent?.GetDefaultDataGroupingCode() ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		protected new EMCSPackage Parent => (EMCSPackage)base.Parent;
	}
}
