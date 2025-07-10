using CargoWise.EntityFramework;
using Enterprise.CRM.Common;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CRM.Module
{
	public class CrmOpportunityModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CrmOpportunity;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.CrmOpportunity);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CrmOpportunityFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CrmOpportunityFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CrmOpportunityCollection(Factory);
		}
	}
}
