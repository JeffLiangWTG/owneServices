using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APMatchingController : MatchingController
	{
		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new APMatchingBase(Factory);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ZAPMatching; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ZAPMatching; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(UnmatchingRow); }
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.MatchPayablesTransactions; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.PayablesUnMatchTransactions; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.PayablesUnMatchTransactions; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.PayablesNewMatchTransactions; }
		}

		#endregion
	}
}
