using CargoWise.EntityFramework;
using CargoWise.Types;
using Res = MailManager.Res;

namespace Enterprise.MailManager.Business
{
	public class MailItemCopySenderValidation : AutoMailItemCopySenderValidation
	{
		public MailItemCopySenderValidation(AutoMailItemCopySender parent)
			: base(parent) { }

		#region Implementation

		public new MailItemCopySender Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (MailItemCopySender)base.Parent; }
		}

		#endregion

		protected override void CheckMailAddressToSendCopyTo()
		{
			base.CheckMailAddressToSendCopyTo();
			MandatoryValidation.CheckEntered(Parent.MailAddressToSendCopyToInfo);
			ZString[] recipients = Parent.MailAddressToSendCopyTo.Split(';');
			foreach (ZString recipient in recipients)
			{
				if (!EmailAddressValidation.IsEmailAddressValid(recipient.Trim()))
				{
					Parent.MailAddressToSendCopyToInfo.AddError(Res.GetString("b8553148-4dc8-4a14-adde-ab43711a3603", "Email Address is not valid"));
				}
			}
		}
	}
}
