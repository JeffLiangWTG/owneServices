using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.ReleaseBuilds.Module
{
	public class ReleaseBuildModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.ReleaseBuild; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.ReleaseBuild; }
		}

		public override bool AllowNew
		{
			get { return true; }
		}

		#region Implementation

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(Modules.ClientControllerRegistration.ReleaseBuild);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ReleaseBuildFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ReleaseBuildCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ReleaseBuildFilterControl(GridCollection, FilterBusinessObject);
		}

		#endregion
	}
}
