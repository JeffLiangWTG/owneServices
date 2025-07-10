using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business
{
	public class CusExitItemCollection : EU.Business.CusExitItemCollection
	{
		public CusExitItemCollection(CusExitDetail parent) : base(parent)
		{
		}

		public new CusExitItem this[int index] => (CusExitItem)base[index];

		public new CusExitItem AddNew() => (CusExitItem)base.AddNew();

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var exitItem = (CusExitItem)child;
			exitItem.CXI_Status = ZString.Empty;
		}
	}
}
