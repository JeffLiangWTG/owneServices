using System;
using Enterprise.Customs.IE.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageController))]
	sealed class UCC6TemporaryStorageControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public void TestGetForm()
		{
			var controller = new UCC6TemporaryStorageController();
			using (var form = controller.ShowNewForm())
			{
				AssertType<TemporaryStorageForm>("GetForm", form);
			}
		}

		public override Type ControllerToBashType => typeof(UCC6TemporaryStorageController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.UCC6TemporaryStorage;
	}
}
