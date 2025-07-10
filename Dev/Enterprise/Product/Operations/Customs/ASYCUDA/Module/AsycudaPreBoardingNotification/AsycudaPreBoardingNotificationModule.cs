using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class AsycudaPreBoardingNotificationModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.ASYCUDA.PreBoardingNotification;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.AsycudaManifestReporting;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.ASYCUDA.ASYCUDAPreBoardingNotification);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new AsycudaPreBoardingNotificationFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new AsycudaPreBoardingNotificationFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new AsycudaPreBoardingNotificationCollection(Factory, GlbCompany.CurrentCompany.PK);
	}
}
