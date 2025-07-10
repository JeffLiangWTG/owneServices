namespace Enterprise.Client.SWL
{
	using System;
	using System.Windows.Forms;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.GUI;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class SWLOrganisationForm : ZOrganisationsForm
	{
		protected SWLOrganisationForm()
		{
		}

		public SWLOrganisationForm(OrgHeader organisation)
			: base(organisation)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			if (TransportTabPage != null)
			{
				TransportTabPage.ControlAdded += TransportTab_ControlAdded;
			}
		}

		void TransportTab_ControlAdded(object o, EventArgs e)
		{
			var eventArgs = (ControlEventArgs)e;

			if (!DesignModeFinder.IsDesigning
				&& eventArgs != null
				&& eventArgs.Control is CarrierUserControl)
			{
				((CarrierUserControl)eventArgs.Control).Load += TransportTab_ControlLoaded;
			}
		}

		void TransportTab_ControlLoaded(object carrierUserControl, EventArgs e)
		{
			TransportControl.TransportTabControl.PlugIns.Add(ControllerIDs.ShipnetSetupPlugIn);
		}
	}
}
