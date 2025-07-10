using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitItemCollection : DependentBusinessObjectCollection<CusExitItem, CusExitDetail>
	{
		const string defaultStatus = ExitItemStatusList.Codes.UNK;

		public CusExitItemCollection(CusExitDetail parent) : base(parent)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var exitItem = (CusExitItem)child;
			exitItem.CXI_LineNumber = (ZShort)exitItem.CusExitDetail.CusExitItems.Count + 1;
			exitItem.CXI_Status = defaultStatus;
		}
	}
}
