using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class H7MessageSendingObjectValidation : MessageSendingObjectValidation
	{
		public H7MessageSendingObjectValidation(H7MessageSendingObject parent)
			: base(parent)
		{
		}

		new AutoMessageSendingObject Parent => base.Parent;

		protected override void CheckOperationCode()
		{
			base.CheckOperationCode();
			if (Parent.Action == DeclarationMessageTypeList.Codes.H7ReExport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OperationCodeInfo);
			}
		}
	}
}
