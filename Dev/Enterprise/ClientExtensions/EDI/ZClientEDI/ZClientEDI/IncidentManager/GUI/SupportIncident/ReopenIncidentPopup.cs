using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class ReopenIncidentPopup : ZChildForm
	{
		public enum ReopenIncidentAction
		{
			AssignToSelf,
			AssignToCapability,
			Cancel
		}

		public ReopenIncidentPopup()
		{
			DialogResult = ReopenIncidentAction.Cancel;
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public new ReopenIncidentAction DialogResult
		{
			get; set;
		}

		void AssignToSelfButton_Click(object sender, EventArgs e)
		{
			DialogResult = ReopenIncidentAction.AssignToSelf;
			this.Close();
		}

		void AssignToCapabilityButton_Click(object sender, EventArgs e)
		{
			DialogResult = ReopenIncidentAction.AssignToCapability;
			this.Close();
		}

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			DialogResult = ReopenIncidentAction.Cancel;
			this.Close();
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
		}
	}
}
