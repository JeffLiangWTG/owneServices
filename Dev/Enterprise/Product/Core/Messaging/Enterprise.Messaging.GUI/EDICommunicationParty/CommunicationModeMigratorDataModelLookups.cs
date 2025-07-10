using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Messaging.GUI
{
	public class CommunicationModeMigratorDataModelLookups : ZLookups
	{
		public CommunicationModeMigratorDataModelLookups(CommunicationModeMigratorDataModel parent)
			: base(parent)
		{
		}

		public EDICommunicationPartyCollection Parties
		{
			get
			{
				if (fParties == null)
				{
					fParties = Factory.GetCachedValue("EDICommunicationModeLookups.Parties", () => new EDICommunicationPartyCollection(Factory));
				}
				return fParties;
			}
		}

		EDICommunicationPartyCollection fParties;
	}
}

