using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebPrintNudgeSuspendingUserControl))]
	sealed class WebPrintNudgeSuspendingUserControlTest : RegistryZUserControlTestCase
	{
		protected override RegistryZUserControl GetNewControl()
		{
			return new WebPrintNudgeSuspendingUserControl(new WebPrintNudgeSuspendingWrapper(new WebPrintNudgeSuspending()));
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new WebPrintNudgeSuspendingWrapper(new WebPrintNudgeSuspending());
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;
	}
}
