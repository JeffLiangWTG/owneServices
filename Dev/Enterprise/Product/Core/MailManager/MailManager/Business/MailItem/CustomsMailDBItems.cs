using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MailManager.Business
{
	/*
	 * THIS IS FOR CUSTOMS TEAM USE ONLY.
	 * IT WILL ADD ALL RECIPIENTS FOR SYSTEM COMMUNICATION, THAT IS IT WILL IGNORE THE EMAIL DESTINATION OVERRIDE FROM THE REGISTRY.
	 */
	class CustomsMailItem : MailItem
	{
		public CustomsMailItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override IMailRecipient AddRecipientCore(string email, MailRecipient.RecipientTypes type)
		{
			return AddRecipientForSystemCommunication(email, type);
		}
	}
}
