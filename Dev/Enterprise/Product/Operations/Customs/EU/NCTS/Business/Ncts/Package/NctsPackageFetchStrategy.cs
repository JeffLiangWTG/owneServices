using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	class NctsPackageFetchStrategy : Customs.Business.CusInvPackFetchStrategy<NctsPackage>
	{
		public NctsPackageFetchStrategy(CusInvPack pivot)
		: base(pivot)
		{
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
		}
	}
}
