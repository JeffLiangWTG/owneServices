using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.EDI.ServiceTasks.Testing
{
	static class FactoryExtensions
	{
		public static IGlbGroup CreateNotificationGroup(this BusinessObjectFactory factory)
		{
			var notificationGroup = factory.New<IGlbGroup>();
			notificationGroup.GG_Code = "FOO";
			notificationGroup.GG_Desc = "[_MOCK_NOTIFICATION_GROUP_]";
			return notificationGroup;
		}

		public static IGlbGroup AddStaff(this IGlbGroup notificationGroup, string code, string email)
		{
			var staff = notificationGroup.Factory.New<IGlbStaff>();
			staff.GS_Code = code;
			staff.GS_EmailAddress = email;
			staff.GS_LoginName = email;
			notificationGroup.Staff.Add(staff);
			return notificationGroup;
		}
	}
}
