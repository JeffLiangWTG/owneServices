using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(GroupNotificationControl))]
	class GroupNotificationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new GroupNotification();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((GroupNotificationControl)control).ReadOnly;
		}
	}
}
