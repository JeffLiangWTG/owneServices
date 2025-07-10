using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Organisation
{
	public class OrgHeaderWrapper : NonPersistentBusinessObject
	{
		public OrgHeaderWrapper(OrgHeader organisation)
			: base(organisation.Factory)
		{
			this.organisation = organisation;
		}

		readonly OrgHeader organisation;

		public OrgHeaderWrapperMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new OrgHeaderWrapperMessageCollection(organisation);
					fMessages.Load();
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}

		OrgHeaderWrapperMessageCollection fMessages;
	}
}
