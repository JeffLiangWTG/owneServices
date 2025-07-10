using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitItemLookups : AutoCusExitItemLookups
	{
		public CusExitItemLookups(AutoCusExitItem parent) : base(parent)
		{
		}

		public new CusExitItem Parent => (CusExitItem)base.Parent;

		public virtual CodeDescriptionPairList StatusList => Factory.GetCachedValue<ExitItemStatusList>();

		public CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);
	}
}
