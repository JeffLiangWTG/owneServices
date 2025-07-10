using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class DeltaIEJobDeclarationMessageSendingObjectValidation : Customs.Business.JobDeclarationMessageSendingObjectValidation
	{
		public DeltaIEJobDeclarationMessageSendingObjectValidation(DeltaIEJobDeclarationMessageSendingObject parent) : base(parent)
		{
		}

		protected override void CheckChangeAcknowledgementIndicator()
		{
			base.CheckChangeAcknowledgementIndicator();
			if (!Parent.ChangeAcknowledgementIndicatorInfo.ReadOnly)
			{
				if (Parent.ShouldSend)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ChangeAcknowledgementIndicatorInfo);
					ListValidation.MessageErrorIfInvalidCode(Parent.ChangeAcknowledgementIndicatorInfo);
				}
			}
		}

		public new DeltaIEJobDeclarationMessageSendingObject Parent => (DeltaIEJobDeclarationMessageSendingObject)base.Parent;

		protected override void CheckMessageType()
		{
			base.CheckMessageType();
			MandatoryValidation.CheckEntered(Parent.MessageTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.MessageTypeInfo);
		}
	}
}
