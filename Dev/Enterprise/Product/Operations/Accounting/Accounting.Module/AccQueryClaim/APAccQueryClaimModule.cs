using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
#if DEBUG
	internal
#endif
	class APAccQueryClaimModule : AccQueryClaimModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.APAccQueryClaim; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new APAccQueryClaimFilterControl(GridCollection, (APAccQueryClaimFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new APAccQueryClaimCollection(Factory);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.APAccQueryClaim);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new APAccQueryClaimFilterBusinessObject();
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.PayablesClaimsAndQueries; }
		}

		#endregion
	}
}
