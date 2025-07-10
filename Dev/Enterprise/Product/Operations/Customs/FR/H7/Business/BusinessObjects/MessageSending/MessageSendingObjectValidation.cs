using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.FR.H7.Business
{
	public class MessageSendingObjectValidation : EU.H7.Business.MessageSendingObjectValidation
	{
		public MessageSendingObjectValidation(AutoMessageSendingObject parent)
			: base(parent)
		{
		}

		public new MessageSendingObject Parent => (MessageSendingObject)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateMotivation();
		}

		public void ValidateMotivation()
		{
			ValidateCalculatedProperty(Parent.MotivationInfo);
		}

		protected void CheckMotivation()
		{
			if (Parent.MotivationInfo.HasNotifications())
			{
				Parent.MotivationInfo.ClearAllNotifications();
			}

			if (!Parent.MotivationInfo.ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.MotivationInfo);
			}
		}
	}
}
