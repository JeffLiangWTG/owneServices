using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class ChinaJournalListingModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ChinaJournalListing; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ChinaJournalListing; }
		}

		protected override ZPopupController GetNewController()
		{
			return new ChinaJournalListingController();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}
	}
}
