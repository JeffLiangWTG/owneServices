using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(UserAccountDeactivationWrapper))]
	public class UserAccountDeactivationWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var userAccount1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			userAccount1.Database.LD_LicenceType = DatabaseTypes.Codes.Production;
			userAccount1.ContactOrganisation.OH_Code = "WISBNEMEL";
			userAccount1.ContactOrganisation.OH_FullName = "Wise Melbourne";
			userAccount1.EUA_IsContactRelationshipActive = true;
			var userAccount2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;
			userAccount2.Database.LD_LicenceType = DatabaseTypes.Codes.Training;
			userAccount2.ContactOrganisation.OH_Code = "WISBNEADL";
			userAccount2.ContactOrganisation.OH_FullName = "Wise Adelaide";
			userAccount2.EUA_IsContactRelationshipActive = false;
			var wrapper1 = new UserAccountDeactivationWrapper(userAccount1, 0);
			var wrapper2 = new UserAccountDeactivationWrapper(userAccount2, 1);
			var databaseTypes = new DatabaseTypes();
			AssertEquals(databaseTypes.GetDescriptionFromCode(userAccount1.Database.LD_LicenceType), wrapper1.LicenceType);
			AssertEquals(true, wrapper1.OriginalIsContactRelationshipActive);
			AssertEquals(userAccount1.GetSystemText(), wrapper1.SystemInfo);
			AssertEquals(0, wrapper1.ReferenceNumber);
			AssertEquals(databaseTypes.GetDescriptionFromCode(userAccount2.Database.LD_LicenceType), wrapper2.LicenceType);
			AssertEquals(false, wrapper2.OriginalIsContactRelationshipActive);
			AssertEquals(userAccount2.GetSystemText(), wrapper2.SystemInfo);
			AssertEquals(1, wrapper2.ReferenceNumber);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UserAccountDeactivationWrapper(Factory.NewWithValidTestData<EdiCustomerUserAccount>(), 0);
		}
		#endregion
	}
}
