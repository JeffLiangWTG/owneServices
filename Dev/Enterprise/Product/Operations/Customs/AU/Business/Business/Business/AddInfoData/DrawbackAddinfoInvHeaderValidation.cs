using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DrawbackAddinfoInvHeaderValidation : AUAddInfoHeaderValidation
	{
		public DrawbackAddinfoInvHeaderValidation(AUAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckZA_DAM_Hidden()
		{
			base.CheckZA_DAM_Hidden();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZA_DAM_HiddenInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_DAM_HiddenInfo, Parent.Lookups.ZA_DAM_List);
		}

		protected override void CheckZA_EDN_Hidden()
		{
			base.CheckZA_EDN_Hidden();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZA_EDN_HiddenInfo);
			new Common.AU.CMR.CANValidation().ValidateCANField(Parent.ZA_EDN_HiddenInfo);
		}
	}
}
