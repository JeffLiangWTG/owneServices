using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class OnlineOrderValidation : CusCodeDataValidation
	{
		public OnlineOrderValidation(OnlineOrder parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);
		}
	}
}
