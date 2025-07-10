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
	public class ComponentRelationshipModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.ComponentRelationship;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ComponentRelationship);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ComponentRelationshipFilterControl(GridCollection, (ComponentRelationshipFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ComponentRelationshipCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ComponentRelationshipFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.BufferManagement;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ComponentRelationship;

		public override bool AllowUniversalCopy => false;
	}
}
