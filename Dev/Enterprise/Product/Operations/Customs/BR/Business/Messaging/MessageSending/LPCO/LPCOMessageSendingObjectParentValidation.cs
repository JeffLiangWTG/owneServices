using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class LPCOMessageSendingObjectParentValidation : ZValidation
	{
		public LPCOMessageSendingObjectParentValidation(LPCOMessageSendingObjectParent parent) : base(parent)
		{
			Parent = parent;
		}

		public LPCOMessageSendingObjectParent Parent;

		public override Type AutoValidationType => typeof(LPCOMessageSendingObjectParentValidation);

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
