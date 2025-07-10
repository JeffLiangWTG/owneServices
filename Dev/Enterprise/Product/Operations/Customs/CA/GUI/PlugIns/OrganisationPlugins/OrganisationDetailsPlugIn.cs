using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CA.GUI
{
	public class OrganisationDetailsPlugIn : ZPlugIn
	{
		public OrganisationDetailsPlugIn(OrgHeader organisation)
			: base(organisation)
		{
			this.organisation = organisation;
		}
		readonly OrgHeader organisation;

		protected override CargoWise.Types.ZBool HasUserControl
		{
			get { return true; }
		}

		protected override System.Windows.Forms.Control GetNewUserControl()
		{
			return new OrganisationDetailPlugInUserControl();
		}

		protected override CargoWise.EntityFramework.IBusiness GetBusinessEntityForPlugIn()
		{
			return AddInfo;
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		public override string Name
		{
			get { return "Canada"; }
		}

		OrgImpAddInfo AddInfo
		{
			get { return addInfo ?? (addInfo = OrgImpAddInfo.Get(organisation)); }
		}
		OrgImpAddInfo addInfo;
	}
}
