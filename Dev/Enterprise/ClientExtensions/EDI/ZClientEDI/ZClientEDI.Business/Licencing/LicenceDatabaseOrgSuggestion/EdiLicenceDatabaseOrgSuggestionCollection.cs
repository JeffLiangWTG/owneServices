using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class EdiLicenceDatabaseOrgSuggestionCollection : ActiveBusinessObjectCollection<EdiLicenceDatabaseOrgSuggestion>
	{
		public EdiLicenceDatabaseOrgSuggestionCollection(LicenceDatabase master) : base(master)
		{
		}

		public EdiLicenceDatabaseOrgSuggestionCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
