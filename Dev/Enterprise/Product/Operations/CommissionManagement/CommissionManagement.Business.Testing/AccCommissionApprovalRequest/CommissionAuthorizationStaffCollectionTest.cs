using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionAuthorizationStaffCollection))]
	public class CommissionAuthorizationStaffCollectionTest : ActiveBusinessObjectCollectionTestCase<CommissionAuthorizationStaffCollection>
	{
		#region IFilterModuleExtraNotificationProvider Members

		public void TestGetExtraNotification()
		{
			var staffWithSecurity = GlbStaff.CurrentUser;
			var staffWithoutSecurity = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var collection = new CommissionAuthorizationStaffCollection(Factory, x => x.CommissionAuthorizationLevel1);

			var staffWithoutSecurityExtraNotification = collection.GetExtraNotification(staffWithoutSecurity);
			AssertNotNull(staffWithoutSecurityExtraNotification);
			AssertEquals(NotificationType.Error, staffWithoutSecurityExtraNotification.Type);
			AssertEquals("Staff does not have the appropriate security rights to approve this.", staffWithoutSecurityExtraNotification.Message);

			var staffWithSecurityExtraNotification = collection.GetExtraNotification(staffWithSecurity);
			AssertNull(staffWithSecurityExtraNotification);
		}

		#endregion

		#region Overrides

		protected override CommissionAuthorizationStaffCollection GetCollectionToTest()
		{
			return new CommissionAuthorizationStaffCollection(Factory, x => x.CommissionAuthorizationLevel1);
		}

		#endregion
	}
}
