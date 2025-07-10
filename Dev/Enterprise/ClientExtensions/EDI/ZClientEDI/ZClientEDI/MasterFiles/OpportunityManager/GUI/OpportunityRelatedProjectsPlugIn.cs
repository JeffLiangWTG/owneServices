using System.Windows.Forms;

using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class OpportunityRelatedProjectsPlugIn : ZPlugIn
	{
		public OpportunityRelatedProjectsPlugIn(EDIOrgOpportunity hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		protected override Control GetNewUserControl()
		{
			return new OpportunityRelatedProjectsUserControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return HostBusinessEntity;
		}

		public override string Name
		{
			get { return "Related Projects"; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.AlwaysAllow; }
		}
	}
}
