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
	class ARAccQueryClaimModule : AccQueryClaimModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ARAccQueryClaim; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ARAccQueryClaimFilterControl(GridCollection, (ARAccQueryClaimFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ARAccQueryClaimCollection(Factory);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ARAccQueryClaim);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ARAccQueryClaimFilterBusinessObject();
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ReceivablesClaimsAndQueries; }
		}

		#endregion
	}
}
