using Enterprise.Integration;

namespace Enterprise.Registry.Business.Customs
{
	[RegistryEditor("Enterprise.Registry.GUI.GroupNotificationRegistryItemEditor, Enterprise.Registry.GUI")]
	sealed class GroupNotificationRegistryDataType<T> : NonPersistentBusinessObjectRegistryDataType<T> where T : GroupNotification
	{
		public GroupNotificationRegistryDataType()
		{
		}

		public GroupNotificationRegistryDataType(bool doNotSendToStaffMembers)
		{
			this.doNotSendToStaffMembers = doNotSendToStaffMembers;
		}
		readonly bool doNotSendToStaffMembers;

		protected override void ValidateCore(IRegistryItem registryItem, T proposedValue, System.Guid companyPK, System.Guid branchPK, System.Guid departmentPK)
		{
			proposedValue.DoNotSendToStaffMembers = this.doNotSendToStaffMembers;
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}
	}
}
