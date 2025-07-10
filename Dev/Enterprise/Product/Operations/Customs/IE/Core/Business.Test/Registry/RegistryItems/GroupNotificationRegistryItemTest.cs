using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Customs.IE.Business.GroupNotificationRegistryItem;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(GroupNotificationRegistryItem))]
	class GroupNotificationRegistryItemTest : StronglyTypedRegistryItemTestCase<GroupNotification>
	{
		protected override StronglyTypedRegistryItem<GroupNotification, GroupNotification> GetNewRegistryItem()
		{
			var result = new GroupNotificationRegistryItem(
				name: "AESResponseNotificationGroup",
				category: IECustomsDataRegistry.Categories.Customs_Ireland_Notifications_Export,
				caption: (NoResString)"AES Responses",
				hint: (NoResString)"The notification settings for emailing when AES messages are received from customs.",
				storage: RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				options: RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory,
				defaultValue: GroupNotification.Default
			);
			result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Ireland;
			return result;
		}
	}

	[TestedType(typeof(GroupNotificationRegistryDataType))]
	class GroupNotificationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<GroupNotificationRegistryDataType>
	{
		protected override GroupNotificationRegistryDataType GetNewDataType() => new GroupNotificationRegistryDataType();

		protected override string ExpectedEditorName => "GroupNotificationRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample1 = new GroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, Core.Constants.Groups.PostMastersGroupPK);
			var sample2 = new GroupNotification(Core.Constants.EmailTo.StaffMember, Core.Constants.Groups.PostMastersGroupPK);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample1, new GroupNotificationRegistryDataType().Serialise(sample1)),
				new ValidSampleAndBinaryValueInDB(sample2, new GroupNotificationRegistryDataType().Serialise(sample2)),
			};
		}
	}
}
