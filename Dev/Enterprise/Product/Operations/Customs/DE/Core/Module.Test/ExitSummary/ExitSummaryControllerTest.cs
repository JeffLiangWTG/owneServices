using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(ExitSummaryController))]
	class ExitSummaryControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(ExitSummaryController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.ExitSummaryController;
	}
}
