using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class StaffContactImporter : ContactImporter
	{
		public StaffContactImporter(BusinessObjectFactory factory) : base(FindProductRegistrationDatabase(factory))
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		protected override BusinessObjectFactory ImportFactory => factory;

		public static LicenceDatabase FindProductRegistrationDatabase(BusinessObjectFactory factory)
		{
			LicenceDatabase result = null;
			int dbNum = ObjectFactory.Get<IProductRegistration>().Key.DatabaseNumber;
			if (dbNum != 0)
			{
				result = factory.LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, dbNum));
			}
			return result;
		}

		public void CreateOrUpdateContactFromStaff(EDIGlbStaff staff)
		{
			if (database != null && org != null && staff != null)
			{
				var contactValue = new ContactImportValue(staff);
				var contact = ImportSingleContact(contactValue).Item2;

				if (contact != null)
				{
					contact.OC_PER = staff.GS_PER;
					SetWebAccessEnabled(contact);
				}
				else
				{
					ErrorReporter.ReportOnce("StaffContactImporter.CreateOrUpdateContactFromStaff",
						$"{org.OH_Code}|{contactValue.UserId}|{contactValue.Email}|{contactValue.JobTitle}|{contactValue.Language}|{contactValue.BranchCode}|{contactValue.IsUserActive}");
				}
			}
		}

		void SetWebAccessEnabled(OrgContact contact)
		{
			var query = new ZQuery(OrgContactSchema.OC_Email, contact.OC_Email);
			query.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);
			query.AddToFilter(OrgContactSchema.PK, SQLComparisonOperator.NotEqual, contact.PK);
			query.AddToFilter(OrgContactSchema.OC_OH, org.PK);
			query.AddToFilter(OrgContactSchema.OC_IsActive, true);

			var exists = factory.ExistsInDatabase(OrgContactSchema.Constants.TableName, query);

			contact.OC_WebAccessEnabled = !exists && !contact.OC_Email.IsEmpty;
		}

		protected override OrgContact FindContact(ContactImportValue contactValue, EdiCustomerUserAccount userAccount)
		{
			return FindSingleBestMatchContact(factory, contactValue, userAccount);
		}

		protected override bool CanLinkAndUpdateOrCreateContact(EdiCustomerUserAccount userAccount, OrgContact contact) => true;

		protected override ZString GetUniqueContactName(ZString proposedName, OrgContact contact)
		{
			return OrgContactUniqueNameHelper.GenerateUniqueContactName(contact, proposedName);
		}

		protected override bool NeedSave => false;
	}
}
