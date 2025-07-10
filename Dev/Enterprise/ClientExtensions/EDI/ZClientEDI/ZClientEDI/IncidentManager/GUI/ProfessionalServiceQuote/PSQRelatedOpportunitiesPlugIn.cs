using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	public class PSQRelatedOpportunitiesPlugIn : ZPlugIn
	{
		public PSQRelatedOpportunitiesPlugIn(ProfessionalServicesQuote hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		protected override Control GetNewUserControl()
		{
			return new PSQRelatedOpportunitiesUserControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return HostBusinessEntity;
		}

		public override string Name
		{
			get { return "Related Opportunities"; }
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
