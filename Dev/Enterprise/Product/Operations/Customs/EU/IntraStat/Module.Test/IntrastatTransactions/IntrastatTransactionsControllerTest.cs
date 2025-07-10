using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.Module.Testing
{
	[TestedType(typeof(IntrastatTransactionsController))]
	sealed class IntrastatTransactionsControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(IntrastatTransactionsController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.IntrastatTransactionsController;
	}
}
