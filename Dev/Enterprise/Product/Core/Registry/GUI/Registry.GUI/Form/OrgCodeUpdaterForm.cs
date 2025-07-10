using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class OrgCodeUpdaterForm : ZChildForm
	{
		readonly OrgCodeUpdater codeUpdater;

		public OrgCodeUpdaterForm()
		{
			InitializeComponent();
			codeUpdater = new OrgCodeUpdater();
			codeUpdater.OrgCodeBlockUpdated += new EventHandler<OrgCodeUpdater.OrgCodeUpdateEventArgs>(OrgCodeUpdater_OrgCodeBlockUpdated);
		}

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Legacy code, incident raised for removal")]
		void OrgCodeUpdater_OrgCodeBlockUpdated(object sender, OrgCodeUpdater.OrgCodeUpdateEventArgs e)
		{
			UpdateProgressBar.Maximum = e.MaxRecordsToProcess;
			UpdateProgressBar.Value = e.TotalProcessed;
			Application.DoEvents();
		}

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Legacy code, incident raised for removal")]
		public void UpdateOrgs(OrgCodeAlgorithmType algorithmType)
		{
			Application.DoEvents();
			bool errorOccurred = false;
			try
			{
				codeUpdater.UpdateOrgCodes(algorithmType);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowDeveloperException(ex);
				errorOccurred = true;
			}
			if (!errorOccurred)
			{
				Globals.Message.ShowInformation(Res.GetString("03f00d8b-7916-456b-ba8e-5ea59644f1c9", "Organization codes have been updated."));
			}
			Close();
		}
	}
}
