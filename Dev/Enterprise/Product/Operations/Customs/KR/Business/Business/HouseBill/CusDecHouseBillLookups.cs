using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class CusDecHouseBillLookups : Customs.Business.CusDecHouseBillLookups
	{
		public CusDecHouseBillLookups(Bill houseBill)
			: base(houseBill)
		{
		}

		protected new Bill Parent => (Bill)base.Parent;

		public CodeDescriptionPairList HouseBillSplitDeclarationIndicatorCodeList => Factory.GetCachedValue<HouseBillSplitDeclarationIndicatorCodeList>();

		public CodeDescriptionPairList HouseBillSplitDeclarationReasonCodeList => Factory.GetCachedValue<HouseBillSplitDeclarationReasonCodeList>();
	}
}
