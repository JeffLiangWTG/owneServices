using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3CommunicationWrapper : IG3Communication
	{
		public static G3CommunicationWrapper New(OrgContact contact) => contact == null ? null : new G3CommunicationWrapper(contact);

		G3CommunicationWrapper(OrgContact contact)
		{
			this.contact = contact;
		}

		readonly OrgContact contact;

		public ZString CommunicationType => contact.OC_Email.IsEmpty ? contact.OC_Mobile.IsEmpty ? string.Empty : TelephoneType : EmailType;

		public ZString CommunicationId => contact.OC_Email.IsEmpty ? contact.OC_Mobile : contact.OC_Email;

		const string EmailType = "EM";

		const string TelephoneType = "TE";
	}
}
