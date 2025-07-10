using System;
using Enterprise.Customs.ES.TemporaryStorage.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(G5V1TemporaryStorageController))]
	public class G5V1TemporaryStorageControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		[RequiresSTA]
		public void TestGetForm()
		{
			var controller = new G5V1TemporaryStorageController();
			using (var form = controller.ShowNewForm())
			{
				AssertType<G5V1TemporaryStorageForm>("GetForm", form);
			}
		}

		public override Type ControllerToBashType => typeof(G5V1TemporaryStorageController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.UCC6TemporaryStorage;
	}
}
