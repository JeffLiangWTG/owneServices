using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.H7.Business
{
	public class AsycudaBillValidationForMasterChild : EU.H7.Business.AsycudaBillValidationForMasterChild
	{
		public AsycudaBillValidationForMasterChild(AsycudaBill parent) : base(parent)
		{
		}

		protected override void CheckABL_RL_NKPortOfDischarge()
		{
			base.CheckABL_RL_NKPortOfDischarge();

			if (Parent.ABL_RL_NKPortOfDischarge.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RL_NKPortOfDischargeInfo);
			}
			else if (!Parent.ABL_RL_NKPortOfDischarge.StartsWith(Core.Constants.CountryCodes.UnitedKingdom))
			{
				Parent.ABL_RL_NKPortOfDischargeInfo.AddMessageError(Res.GetString("05c22ae0-057b-4cd5-8bb3-b238fc9b36d3", "The port code entered is a non GB port code. A GB port code is required to send a valid declaration."));
			}
		}
	}
}
