using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class CusExitItemLookups : EU.Business.CusExitItemLookups
	{
		public CusExitItemLookups(CusExitItem parent) : base(parent)
		{
		}

		public new CusExitItem Parent => (CusExitItem)base.Parent;

		public override CodeDescriptionPairList StatusList => Parent.CusExitDetail.Lookups.StatusList;
	}
}
