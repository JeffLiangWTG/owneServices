using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(GroupNotificationRegistryDataType<GroupNotification>))]
	sealed class GroupNotificationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<GroupNotificationRegistryDataType<GroupNotification>>
	{
		public void TestValidation()
		{
			const string staffMemberNotificationIsInvalid = "Staff Member Notification is not valid for this registry.";
			var dummyRegItem = new GroupNotificationRegistryItem<GroupNotification>("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default, GroupNotification.Default, true);
			var proposedValue = new GroupNotification(Core.Constants.EmailTo.StaffMember, Core.Constants.Groups.PostMastersGroupPK);
			AssertExceptionThrown<RegistryValidationException>(() => dummyRegItem.DataType.Validate(dummyRegItem, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(true, proposedValue.SendModeInfo.HasError(staffMemberNotificationIsInvalid));
			proposedValue = new GroupNotification(Core.Constants.EmailTo.NominatedGroup, Core.Constants.Groups.PostMastersGroupPK);
			dummyRegItem.DataType.Validate(dummyRegItem, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(false, proposedValue.SendModeInfo.HasError(staffMemberNotificationIsInvalid));
		}

		protected override GroupNotificationRegistryDataType<GroupNotification> GetNewDataType()
		{
			return new GroupNotificationRegistryDataType<GroupNotification>();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var groupNotification1 = GroupNotification.Default;
			var groupNotification2 = new GroupNotification(Core.Constants.EmailTo.NoEmails, ZGuid.Empty);

			return
			[
				new ValidSampleAndBinaryValueInDB(groupNotification1, new GroupNotificationRegistryDataType<GroupNotification>().Serialise(groupNotification1)),
				new ValidSampleAndBinaryValueInDB(groupNotification2, new GroupNotificationRegistryDataType<GroupNotification>().Serialise(groupNotification2))
			];
		}

		protected override string ExpectedEditorName => "GroupNotificationRegistryItemEditor";
	}
}
