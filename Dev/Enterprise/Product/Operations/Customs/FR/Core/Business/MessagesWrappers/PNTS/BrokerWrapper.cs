using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class BrokerWrapper : ICommunication
	{
		BrokerWrapper(GlbStaff contact)
		{
			this.contact = contact;
		}
		readonly GlbStaff contact;

		public static BrokerWrapper New(GlbStaff contact) => contact != null ? new BrokerWrapper(contact) : null;

		public string Identifier => identifier ?? (identifier = contact.GS_EmailAddress);
		string identifier;

		public string Type => type ?? (type = contact.GS_EmailAddress.IsEmpty ? string.Empty : "EM");
		string type;
	}
}
