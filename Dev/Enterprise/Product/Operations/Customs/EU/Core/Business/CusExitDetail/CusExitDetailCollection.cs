using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitDetailCollection : DependentBusinessObjectCollection<CusExitDetail, CusExitControlHeader>
	{
		public CusExitDetailCollection(CusExitControlHeader parent) : base(parent)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var cusExitDetail = (CusExitDetail)child;
			var header = Master;
			cusExitDetail.CED_CustomsOffice = header.CEH_CustomsOffice;
			cusExitDetail.CED_ArrivalNotificationDate = header.CEH_ArrivalNotificationDate;
			cusExitDetail.CED_ArrivalNotificationPlace = header.CEH_ArrivalNotificationPlace;
			cusExitDetail.CED_ExitDate = header.CEH_ExitDate;
			cusExitDetail.CED_TransportID = header.CEH_TransportID;
			cusExitDetail.CED_OA_Carrier = header.CEH_OA_Carrier;
		}
	}
}
