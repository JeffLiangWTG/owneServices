using System;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.JXC.Export.Validations;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class PreShipmentWrapperValidation : ZValidation
	{
		public PreShipmentWrapperValidation(PreShipmentWrapper parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public override void ValidateAll()
		{
			ValidateSendingForwarderPK();
			ValidateReceivingForwarderPK();
		}

		public void ValidateSendingForwarderPK()
		{
			ValidateCalculatedProperty(Parent.SendingForwarderPKInfo);
		}

		public void ValidateReceivingForwarderPK()
		{
			ValidateCalculatedProperty(Parent.ReceivingForwarderPKInfo);
		}

		protected void CheckSendingForwarderPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.SendingForwarderPKInfo, Parent.SendingForwarders);
			CompareValidation.CheckNotEqual(Parent.SendingForwarderPKInfo, Parent.ReceivingForwarderPKInfo);
			new JXCExportHeaderValidationHelper().ValidateSendingForwarder(Parent.SendingForwarderPKInfo, Parent);
		}

		protected void CheckReceivingForwarderPK()
		{
			MandatoryValidation.CheckEntered(Parent.ReceivingForwarderPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.ReceivingForwarderPKInfo, Parent.ReceivingForwarders);
			CompareValidation.CheckNotEqual(Parent.ReceivingForwarderPKInfo, Parent.SendingForwarderPKInfo);
			new ValidationHelper().ValidateJXCForwarder(Parent.ReceivingForwarderPKInfo, Parent.ReceivingForwarder);
		}

		public override Type AutoValidationType
		{
			get { return typeof(PreShipmentWrapperValidation); }
		}

		public readonly PreShipmentWrapper Parent;
	}
}
