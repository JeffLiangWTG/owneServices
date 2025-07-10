using CargoWise.EntityFramework;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MailManager
{
	/*
	 * THIS IS FOR CUSTOMS TEAM USE ONLY.
	 * IT WILL ADD ALL RECIPIENTS FOR SYSTEM COMMUNICATION, THAT IS IT WILL IGNORE THE EMAIL DESTINATION OVERRIDE FROM THE REGISTRY.
	 */
	public class OutgoingCustomsMailCreator : OutgoingBaseMailCreator<OutgoingCustomsMailCreator>, IOutgoingCustomsMailManager
	{
		protected OutgoingCustomsMailCreator()
		{
		}

		protected override void AddRecipientCore(EmailDef emailDef, string[] emails, RecipientDef.RecipientTypes type)
		{
			emailDef.AddRecipientForSystemCommunication(emails, type);
		}

		protected override MailItem GetNewMailItem(BusinessObjectFactory factory)
		{
			return factory.New<CustomsMailItem>();
		}
	}
}
