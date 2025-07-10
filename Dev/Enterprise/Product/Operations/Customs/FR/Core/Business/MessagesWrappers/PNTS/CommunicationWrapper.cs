using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class CommunicationWrapper : CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces.ICommunication
	{
		CommunicationWrapper(OrgContact contact)
		{
			this.contact = contact;
		}
		readonly OrgContact contact;

		public string Identifier => identifier ?? (identifier = contact.OC_Email.IsEmpty ? contact.OC_Phone : contact.OC_Email);
		string identifier;

		public string Type => type ?? (type = contact.OC_Email.IsEmpty ? (contact.OC_Phone.IsEmpty ? string.Empty : "TE") : "EM");
		string type;

		public static CommunicationWrapper New(OrgContact contact) => contact == null ? null : new CommunicationWrapper(contact);
	}
}
