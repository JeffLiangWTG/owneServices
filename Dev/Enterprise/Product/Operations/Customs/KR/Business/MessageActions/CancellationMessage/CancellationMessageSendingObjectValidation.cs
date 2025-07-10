using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class CancellationMessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
	{
		public CancellationMessageSendingObjectValidation(CancellationMessageSendingObject parent) : base(parent)
		{
		}
		protected new CancellationMessageSendingObject Parent => base.Parent as CancellationMessageSendingObject;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateCancellationReason();
		}
		public void ValidateCancellationReason()
		{
			ValidateCalculatedProperty(Parent.CancellationReasonInfo);
		}

		protected void CheckCancellationReason()
		{
			if (Parent.ShouldSend)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CancellationReasonInfo);
			}
		}
	}
}
