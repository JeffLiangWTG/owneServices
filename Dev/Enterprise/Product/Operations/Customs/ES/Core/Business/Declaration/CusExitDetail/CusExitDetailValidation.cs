using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusExitDetailValidation : EU.Business.CusExitDetailValidation
	{
		public CusExitDetailValidation(CusExitDetail parent) : base(parent)
		{
		}

		protected new CusExitDetail Parent => base.Parent as CusExitDetail;

		protected override void CheckCED_Status() { }

		protected override void CheckCED_ArrivalNotificationDate() { }

		protected override void CheckCED_ArrivalNotificationPlace()
		{
			base.CheckCED_ArrivalNotificationPlace();
			if (Parent.Lookups.ArrivalNotificationCodeList.Count > 0)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CED_ArrivalNotificationPlaceInfo);
			}
		}
	}
}
