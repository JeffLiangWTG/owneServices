using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class BMBufferTimespanModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.BMBufferTimespan;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.BMBufferTimespan);
		protected override IFilterControl GetNewFilterControl() => new BMBufferTimespanFilterControl(GridCollection, FilterBusinessObject);
		protected override IBusinessObjectCollection GetNewGridCollection() => new BMBufferTimespanCollection(Factory);
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new BMBufferTimespanFilterBusinessObject();

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.BufferManagement;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.BMSystems;
	}
}
