using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceDatabaseRegistrationWizard : NonPersistentBusinessObject
	{
		public LicenceDatabaseRegistrationWizard(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Related Business Objects

		#region LicenceDatabaseCollection

		public LicenceDatabaseNonDependentCollection LicenceDatabaseCollection
		{
			get
			{
				if (licenceDatabaseCollection == null)
				{
					var localLicenceDatabaseCollection = new LicenceDatabaseNonDependentCollection(Factory);
					localLicenceDatabaseCollection.Load();
					licenceDatabaseCollection = localLicenceDatabaseCollection;
					RegisterEditableChildObject(licenceDatabaseCollection);
				}

				return licenceDatabaseCollection;
			}
		}
		LicenceDatabaseNonDependentCollection licenceDatabaseCollection;

		#endregion

		#region OrgHeaderCollection

		public EDIOrgHeaderCollection OrgHeaderCollection
		{
			get
			{
				if (orgHeaderCollection == null)
				{
					orgHeaderCollection = new EDIOrgHeaderCollection(Factory);
					RegisterEditableChildObject(orgHeaderCollection);
				}

				return orgHeaderCollection;
			}
		}
		EDIOrgHeaderCollection orgHeaderCollection;

		#endregion

		#endregion

		public void ClearNotifications()
		{
			if (Notifications.Any())
			{
#if DEBUG
				using (SuspendValidationTesting())
#endif
				{
					ClearAllNotifications();
				}
			}
		}
	}
}
