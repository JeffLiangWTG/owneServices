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
	public class AcceptabilityBandModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AcceptabilityBand; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AcceptabilityBand);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AcceptabilityBandFilterControl(GridCollection, (AcceptabilityBandFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new BMComponentAcceptabilityBandCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AcceptabilityBandFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.BufferManagement; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AcceptabilityBand; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}
	}
}
