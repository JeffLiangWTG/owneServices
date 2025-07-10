using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDIInterchangeController))]
	sealed class EDIInterchangeControllerBasherTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Messaging.EDIInterchange;
		}
	}
}
