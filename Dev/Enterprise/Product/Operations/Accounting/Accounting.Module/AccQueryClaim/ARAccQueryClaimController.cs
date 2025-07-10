using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.Module
{
	class ARAccQueryClaimController : AccQueryClaimController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.ARAccQueryClaim; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARAccQueryClaim); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ARAccQueryClaim; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Accounting.Module.Res.GetData("PlugInTabPage|ARAccQueryClaim", "Receivables Claims and Queries", "The Receivables Claims and Queries tab."); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new ARAccQueryClaimPlugIn(businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReceivablesClaimsAndQueriesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ReceivablesClaimsAndQueriesEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ReceivablesClaimsAndQueriesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ReceivablesClaimsAndQueriesView; }
		}

		#endregion
	}
}
