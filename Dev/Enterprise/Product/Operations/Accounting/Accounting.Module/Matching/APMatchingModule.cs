using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APMatchingModule : MatchingModule
	{
		public APMatchingModule() : base()
		{
		}

		#region Overrides

		protected override ControllerID GetControllerID
		{
			get { return ControllerIDs.ZAPMatching; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new APMatchingFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new APMatchingFilterControl(GridCollection, FilterBusinessObject);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ZAPMatching; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.MatchPayablesTransactions; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.Core; }
		}

		#endregion
	}
}
