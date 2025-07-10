using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3MessageSendingObjectValidation : MessageSendingObjectValidation
	{
		public G3MessageSendingObjectValidation(AutoMessageSendingObject parent) : base(parent)
		{
		}

		protected override void CheckRevokeReason()
		{
			base.CheckRevokeReason();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.RevokeReasonInfo);
		}
	}
}
