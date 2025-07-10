using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(CalloutController))]
	internal class CalloutControllerBasherTest : ZControllerBasherTest
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.Callout;
		}
	}
}
