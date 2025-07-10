
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(MexicoNotificationRemainingFolioConfigurationControl))]
	class MexicoNotificationRemainingFolioConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new MexicoNotificationRemainingFolioConfiguration();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var mexicoNotificationRemainingFolioConfigurationControl = control as MexicoNotificationRemainingFolioConfigurationControl;
			AssertNotNull(mexicoNotificationRemainingFolioConfigurationControl);

			var foliosQuantity = mexicoNotificationRemainingFolioConfigurationControl.FindSingleOrDefault<ZCalcEdit>("FoliosQuantityCalcEdit");
			AssertNotNull(foliosQuantity);

			var interval = mexicoNotificationRemainingFolioConfigurationControl.FindSingleOrDefault<ZCalcEdit>("IntervalCalcEdit");
			AssertNotNull(interval);

			return foliosQuantity.ReadOnly && interval.ReadOnly;
		}
	}
}
