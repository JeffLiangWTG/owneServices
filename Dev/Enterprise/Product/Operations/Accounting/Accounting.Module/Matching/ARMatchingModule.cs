using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARMatchingModule : MatchingModule
	{
		public ARMatchingModule() : base()
		{
		}

		#region Overrides

		protected override ControllerID GetControllerID
		{
			get { return ControllerIDs.ZARMatching; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ARMatchingFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ARMatchingFilterControl(GridCollection, FilterBusinessObject);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ZARMatching; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.MatchReceivablesTransactions; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.Core; }
		}

		#endregion
	}
}
