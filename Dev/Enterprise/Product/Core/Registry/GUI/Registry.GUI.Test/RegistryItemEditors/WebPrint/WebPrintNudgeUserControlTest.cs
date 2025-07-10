using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebPrintNudgeUserControl))]
	sealed class WebPrintNudgeUserControlTest : RegistryZUserControlTestCase
	{
		protected override RegistryZUserControl GetNewControl()
		{
			return new WebPrintNudgeUserControl(new WebPrintNudgeWrapper(new WebPrintNudge { EnableIPAddress = true }));
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new WebPrintNudgeWrapper(new WebPrintNudge());
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;
	}
}
