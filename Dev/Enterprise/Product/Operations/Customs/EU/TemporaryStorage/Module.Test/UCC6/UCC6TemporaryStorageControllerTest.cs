using System;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageController))]
	class UCC6TemporaryStorageControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(UCC6TemporaryStorageController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.UCC6TemporaryStorage;

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = new UCC6TemporaryStorageController();
			AssertEquals(typeof(TemporaryStorageHeader), controller.TypeOfTopLevelBusinessObject);
		}
	}
}
