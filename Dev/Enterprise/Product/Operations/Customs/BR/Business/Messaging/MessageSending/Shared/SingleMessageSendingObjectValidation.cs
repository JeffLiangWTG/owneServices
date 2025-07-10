using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class SingleMessageSendingObjectValidation : ZValidation
	{
		public SingleMessageSendingObjectValidation(BaseSingleMessageSendingObject parent) : base(parent)
		{
			Parent = parent;
		}

		public BaseSingleMessageSendingObject Parent;

		public override Type AutoValidationType => typeof(SingleMessageSendingObjectValidation);

		public override void ValidateAll()
		{
			ValidateBrokerCode();
		}

		public void ValidateBrokerCode()
		{
			ValidateCalculatedProperty(Parent.BrokerCodeInfo);
		}

		protected void CheckBrokerCode()
		{
			MandatoryValidation.CheckEntered(Parent.BrokerCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.BrokerCodeInfo);
			if (Parent.Broker != null)
			{
				GlbExternalPasswordValidation_CCT.CheckValidCertificate(Parent.BrokerCertificate, Parent.BrokerCodeInfo);
			}
		}
	}
}
