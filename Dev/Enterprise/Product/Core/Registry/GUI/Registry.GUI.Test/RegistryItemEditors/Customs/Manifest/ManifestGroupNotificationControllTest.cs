using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ManifestGroupNotificationControl))]
	sealed class ManifestGroupNotificationControllTest : RegistryZUserControlTestCase
	{
		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return new Enterprise.Registry.Business.Customs.ManifestGroupNotification();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((ManifestGroupNotificationControl)control).SendErrorOnlyCheckBox.ReadOnly;
		}
	}
}
