using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Organisation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Organisation
{
	public class OrganisationPlugInMenu : KMenuItem
	{
		public OrganisationPlugInMenu(OrgHeader organisation, OrganisationPlugIn plugIn)
		{
			this.organisation = organisation;
			this.plugIn = plugIn;
			Text = Res.GetString("Customs.GB.OrganisationPlugIn", "Customs Messaging");
			InitialiseMenu();
		}

		readonly OrgHeader organisation;
		readonly OrganisationPlugIn plugIn;

		void InitialiseMenu()
		{
			MenuItems.Add(new ZMenuItem(Res.GetString("Customs.GB.OrganisationPlugIn.ShowHmrcQueryForm", "Verify GB EORI, UK VAT number or XI NOP Waiver"), ShowHmrcQueryForm_Click));
		}

		void ShowHmrcQueryForm_Click(object sender, EventArgs e)
		{
			var form = new HmrcQueryForm(new NonPersistentOrgHeaderChecker(organisation.Factory, organisation));
			if (ZFormModaliser.ShowDialogAndDispose(form) != DialogResult.Cancel)
			{
				plugIn.ReloadMessages();
			}
		}
	}
}
