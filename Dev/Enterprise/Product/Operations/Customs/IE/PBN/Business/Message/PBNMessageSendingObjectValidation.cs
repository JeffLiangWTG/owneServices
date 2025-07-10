using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.PBN.Business
{
	public sealed class PBNMessageSendingObjectValidation : AutoPBNMessageSendingObjectValidation
	{
		public PBNMessageSendingObjectValidation(AutoPBNMessageSendingObject parent) : base(parent)
		{
		}

		protected override void CheckMessageType()
		{
			base.CheckMessageType();
			ListValidation.MessageErrorIfInvalidCode(Parent.MessageTypeInfo);
		}
	}
}
