using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class AccountReconfigurationRoutingDescriptor : ILoginRoutingDescriptor
	{
		public AccountReconfigurationRoutingDescriptor(OrgContact contact, LicenceDatabase database, MyAccountLoginRouterIdentityManager identityManager)
		{
			this.contact = contact;
			this.database = database;
			this.identityManager = identityManager;
		}
		readonly OrgContact contact;
		readonly LicenceDatabase database;
		readonly MyAccountLoginRouterIdentityManager identityManager;

		public bool IsRoutingRequired => contact != null && database != null && identityManager.IsValidID() && !contact.OC_OH.Equals(database.LD_OH_WebAccessOrg);

		public Uri RoutingUrl => null;

		public void RoutingAction()
		{
			var factory = new BusinessObjectFactory();
			var contactInNewFactory = factory.Load<EDIOrgContact>(contact.PK);
			var databaseInNewFactory = factory.Load<LicenceDatabase>(database.PK);

			var loginContact = ContactCloner.MoveContactToMasterOrg(contactInNewFactory, databaseInNewFactory);

			if (loginContact.PK != contact.PK)
			{
				var regeneratedToken = MyAccountLoginRouterIdentityManager.GenerateToken(identityManager.UserAccount);
				identityManager.PopulatePropertiesFromToken(regeneratedToken);
			}
		}
	}
}
