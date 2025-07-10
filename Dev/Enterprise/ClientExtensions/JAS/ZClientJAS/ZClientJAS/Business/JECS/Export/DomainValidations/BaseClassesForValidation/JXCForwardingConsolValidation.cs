using Enterprise.Freight.Common.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCForwardingConsolValidation : AutoJobConsolValidation
	{
		public JXCForwardingConsolValidation(JASForwardingConsol parent)
			: base(parent)
		{
		}

		protected override void CheckJK_OA_SendingForwarderAddress()
		{
			base.CheckJK_OA_SendingForwarderAddress();
			new JXCExportHeaderValidationHelper().ValidateSendingForwarder(Parent.JK_OA_SendingForwarderAddressInfo, Parent);
		}

		protected override void CheckJK_OA_ReceivingForwarderAddress()
		{
			base.CheckJK_OA_ReceivingForwarderAddress();

			ValidationHelper.AddJXCWarningIfNotEntered(Parent.JK_OA_ReceivingForwarderAddressInfo);
			ValidationHelper.ValidateJXCForwarder(Parent.JK_OA_ReceivingForwarderAddressInfo, Parent.ReceivingForwarder);
		}

		public new JASForwardingConsol Parent
		{
			get { return (JASForwardingConsol)base.Parent; }
		}

		protected ValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new ValidationHelper();
				}
				return fValidationHelper;
			}
		}

		ValidationHelper fValidationHelper;
	}
}

#region Implementation
#endregion
