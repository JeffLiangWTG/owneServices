using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class EmailToContactWithDelay : CustomerServiceEmail
	{
		public EmailToContactWithDelay(BusinessObject businessObjectSendingEmail, TimeSpan sendingDelay)
			: base(businessObjectSendingEmail)
		{
			this.sendingDelay = sendingDelay;
		}

		protected override void SendEmailCore(bool systemCommunication)
		{
			MailItem mailItem = OutgoingMailCreator.Instance.NewMailItem(Factory, GetEmail());
			mailItem.MI_SendDateTime = ZDateTime.UtcNow.Add(sendingDelay);
			AddNoteAndEvent(mailItem.MI_Body);
		}

		protected override bool ShouldSaveToEDocs
		{
			get { return false; }
		}

		readonly TimeSpan sendingDelay;
	}
}

