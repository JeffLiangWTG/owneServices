using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	public class EDIOpportunityRelatedPSQsPlugIn : ZPlugIn
	{
		public EDIOpportunityRelatedPSQsPlugIn(EDIOrgOpportunity hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		protected override Control GetNewUserControl()
		{
			return new EDIOpportunityRelatedPSQsUserControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return HostBusinessEntity;
		}

		public override string Name
		{
			get { return "Related PSQs"; }
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
