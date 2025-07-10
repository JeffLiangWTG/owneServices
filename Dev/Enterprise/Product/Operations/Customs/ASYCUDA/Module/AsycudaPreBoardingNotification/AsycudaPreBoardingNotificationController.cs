using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class AsycudaPreBoardingNotificationController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.ASYCUDA.ASYCUDAPreBoardingNotification;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.ASYCUDA.PreBoardingNotification;

		public override Type TypeOfTopLevelBusinessObject => typeof(AsycudaManifestHeader);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.AsycudaManifestReporting;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.AsycudaManifestReporting;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.AsycudaManifestReporting;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.AsycudaManifestReporting;

		protected override IZForm GetForm(IBusiness businessEntity) => new ManifestForm(businessEntity as AsycudaManifestHeader);
	}
}
