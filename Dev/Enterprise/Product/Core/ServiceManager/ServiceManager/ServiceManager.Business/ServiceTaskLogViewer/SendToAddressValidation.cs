using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ServiceManager.Business
{
	public class SendToAddressValidation : AutoSendToAddressValidation
	{
		public SendToAddressValidation(AutoSendToAddress parent)
			: base(parent) { }

		#region Implementation

		public new SendToAddress Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (SendToAddress)base.Parent; }
		}

		#endregion

		protected override void CheckAddress()
		{
			base.CheckAddress();

			var recipients = Parent.Recipients;
			if (recipients.Count == 0)
			{
				Parent.AddressInfo.AddError("Please enter a value.");
			}
			else
			{
				foreach (ZString recipient in recipients)
				{
					if (!EmailAddressValidation.IsEmailAddressValid(recipient))
					{
						Parent.AddressInfo.AddError("Address '" + recipient + "'is not valid.");
					}
				}
			}
		}
	}
}

