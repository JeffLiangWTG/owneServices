using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class BMFilterRuleController : ProcessHeaderController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.BMFilterRule; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.BMFilterRule; }
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.BMFilterRule; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.BMFilterRule; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.BMFilterRule; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.BMFilterRule; }
		}

		#endregion
	}
}
