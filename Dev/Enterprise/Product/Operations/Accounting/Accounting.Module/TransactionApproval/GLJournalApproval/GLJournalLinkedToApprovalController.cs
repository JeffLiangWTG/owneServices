using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class GLJournalLinkedToApprovalController : GLJournalController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.GLJournalLinkedToApproval; }
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			return sourceEntity;
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.GLJournalApproval; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GLJournalApproval; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.GLJournalApprovalEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.GLJournalApproval; }
		}

		#endregion
	}
}
