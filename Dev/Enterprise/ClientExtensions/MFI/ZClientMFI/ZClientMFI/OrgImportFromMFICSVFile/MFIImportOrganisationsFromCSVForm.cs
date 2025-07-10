using System.Windows.Forms;

using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.MFI.OrgImportFromMFICSVFile
{
	class MFIImportOrganisationsFromCSVForm : ImportOrganisationsFromCSVForm
	{
		public override string FormHeading
		{
			get { return formHeading; }
		}

		protected override OrgDataLoad GetOrgDataLoader()
		{
			return new MFIOrgDataLoad();
		}

		public override bool ConfirmLoadData()
		{
			string loadingData = "Please Note: Existing Organisations will be updated.";
			DialogResult result = Globals.Message.Show(loadingData, "Confirm Organisation Load", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			return (result == DialogResult.OK);
		}

		internal const string formHeading = "Import MFI Organisation CSV Files";
	}
}
