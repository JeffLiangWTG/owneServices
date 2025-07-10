using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GroupNotificationControl))]
	sealed class GroupNotificationControlTest : RegistryZUserControlTestCase
	{
		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return new Business.Customs.GroupNotification();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((GroupNotificationControl)control).GroupToSendGuidFindBox.ReadOnly;
		}
	}
}
