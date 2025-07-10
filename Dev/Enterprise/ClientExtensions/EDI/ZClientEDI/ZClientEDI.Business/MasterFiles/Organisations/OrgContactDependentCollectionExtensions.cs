using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public static class OrgContactDependentCollectionExtensions
	{
		public static OrgContact FindOrCreateFromStaff(this OrgContactDependentCollection orgContacts, GlbStaff staff)
		{
			OrgContact result = orgContacts.Cast<OrgContact>().FirstOrDefault(c => c.OC_Email == staff.GS_EmailAddress && c.OC_IsActive);
			if (result == null)
			{
				result = orgContacts.AddNew();
				result.OC_ContactName = GetUniqueContactName(orgContacts, staff.GS_FullName);
				result.OC_Email = staff.GS_EmailAddress;
				result.OC_Phone = staff.GS_WorkPhone;
			}
			return result;
		}

		static string GetUniqueContactName(OrgContactDependentCollection orgContacts, string staffName)
		{
			string result = staffName;

			IEnumerable<OrgContact> contacts = orgContacts.Cast<OrgContact>();
			int counter = 1;
			while (contacts.Any(c => c.OC_ContactName == result))
			{
				result = string.Format(CultureInfo.CurrentCulture, "{0} ({1})", staffName, counter++);
			}

			return result;
		}
	}
}
