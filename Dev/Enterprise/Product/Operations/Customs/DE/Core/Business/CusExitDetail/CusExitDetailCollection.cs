using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class CusExitDetailCollection : EU.Business.CusExitDetailCollection
	{
		public CusExitDetailCollection(CusExitControlHeader parent) : base(parent)
		{
		}

		public new CusExitDetail this[int index] => (CusExitDetail)base[index];

		public new CusExitDetail AddNew() => (CusExitDetail)base.AddNew();

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var cusExitDetail = (CusExitDetail)child;
			cusExitDetail.CED_LocationOfGoods = Master.CEH_LocationOfGoods;
		}
	}
}
