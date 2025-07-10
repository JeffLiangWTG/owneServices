using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.Module
{
	class APAccQueryClaimController : AccQueryClaimController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.APAccQueryClaim; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.APAccQueryClaim; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APAccQueryClaim); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new APAccQueryClaimPlugIn(businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.PayablesClaimsAndQueriesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.PayablesClaimsAndQueriesEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.PayablesClaimsAndQueriesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.PayablesClaimsAndQueriesView; }
		}

		#endregion
	}
}
