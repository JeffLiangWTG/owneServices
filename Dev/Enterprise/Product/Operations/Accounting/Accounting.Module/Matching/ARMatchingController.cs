using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARMatchingController : MatchingController
	{
		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new ARMatchingBase(Factory);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ZARMatching; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ZARMatching; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(UnmatchingRow); }
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.MatchReceivablesTransactions; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReceivablesUnMatchTransaction; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ReceivablesUnMatchTransaction; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ReceivablesNewMatchTransactions; }
		}

		#endregion
	}
}
