using System;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(AsycudaPreBoardingNotificationController))]
	sealed class AsycudaPreBoardingNotificationControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(AsycudaManifestHeader), new ASYCUDAManifestController().TypeOfTopLevelBusinessObject);
		}

		public override void TestNewForm()
		{
			base.TestNewForm();
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.ASYCUDA.ASYCUDAPreBoardingNotification);
			using (var form = controller.ShowNewForm())
			{
				AssertType<ManifestForm>("Should return ManifestForm", form);
			}
		}

		public override Type ControllerToBashType => typeof(ASYCUDAManifestController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.ASYCUDA.ASYCUDAPreBoardingNotification;
	}
}
