using System;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Module.Testing
{
	[TestedType(typeof(Controller))]
	public class ControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(Controller);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.EMCS;
	}
}
