using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(ADUserForLogin))]
	class ADUserForLogin_NonPersistentBizoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ADUserForLogin(new StaffForLoginForTest(Factory.NewWithValidTestData<GlbStaff>()));
		}
	}

	class StaffForLoginForTest : StaffForLogin
	{
		public StaffForLoginForTest(GlbStaff staff) : base()
		{
			LoginName = staff.GS_LoginName;
			DomainName = staff.GS_DomainName;
			IsActive = staff.GS_IsActive;
			ActiveDirectoryObjectGuid = staff.GS_ActiveDirectoryObjectGuid;
		}
	}
}
