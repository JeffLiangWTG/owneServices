using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ClientSharedComponents.Testing
{
	class NotificationGroupValidatorTest : TestCaseWithFactory
	{
		public void TestIsValid()
		{
			Assert(!NotificationGroupValidator.IsValid(Guid.Empty, Factory));
			Assert(!NotificationGroupValidator.IsValid(ZGuid.Invalid, Factory));

			GlbGroup group = Factory.New<GlbGroup>();
			Assert(!NotificationGroupValidator.IsValid(group.PK, Factory));

			group.Staff.AddNew();
			Assert(!NotificationGroupValidator.IsValid(group.PK, Factory));

			group.Staff[0].GS_EmailAddress = "invalid email address";
			Assert(!NotificationGroupValidator.IsValid(group.PK, Factory));

			group.Staff[0].GS_EmailAddress = "test@test.com";
			Assert(NotificationGroupValidator.IsValid(group.PK, Factory));
		}
	}
}
