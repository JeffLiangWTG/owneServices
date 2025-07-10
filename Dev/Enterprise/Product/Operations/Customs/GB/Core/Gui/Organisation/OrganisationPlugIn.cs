using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS.Organisation;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.GB.GUI.Organisation
{
	public class OrganisationPlugIn : ZPlugIn
	{
		public OrganisationPlugIn(OrgHeader organisation)
			: base(organisation)
		{
			this.organisation = organisation;
			orgHeaderWrapper = new OrgHeaderWrapper(organisation);
		}

		readonly OrgHeader organisation;
		readonly OrgHeaderWrapper orgHeaderWrapper;

		public override string Name => Res.GetString("Customs.GB.OrganisationPlugIn", "Customs Messaging");

		public override bool ShouldBeReadOnly => true;

		protected override ZBool HasUserControl => true;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override MenuItem GetNewTopLevelMenu() => new OrganisationPlugInMenu(organisation, this);

		protected override Control GetNewUserControl() => new Customs.GUI.MessageUserControl();

		protected override IBusiness GetBusinessEntityForPlugIn() => orgHeaderWrapper;

		internal void ReloadMessages() => orgHeaderWrapper.Messages.Reload(false);
	}
}
