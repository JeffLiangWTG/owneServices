using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

public sealed class StaffDataSetupTestHelper
{
	public static GlbStaff CreateStaffWithMainEmail(BusinessObjectFactory factory, string emailId)
	{
		var staff = factory.NewWithValidTestData<GlbStaff>();
		var copyToEmailAddress = staff.EmailAddresses.AddNew();
		copyToEmailAddress.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;
		copyToEmailAddress.GSE_EmailAddress = emailId;
		return staff;
	}
}
