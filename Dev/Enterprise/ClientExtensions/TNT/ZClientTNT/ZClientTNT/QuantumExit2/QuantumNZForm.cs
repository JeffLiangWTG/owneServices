using System;
using Enterprise.Freight.Forwarding.Business;

using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TNT.NZ
{
	public partial class QuantumNZForm : Exit2ImportForm
	{
		public QuantumNZForm(NZImportManager businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return "Quantum Interface Import"; }
		}

		protected override Exit2ImportManager Manager
		{
			get { return (NZImportManager)BusinessEntity; }
		}

		protected override void ProcessAllMatches()
		{
			Manager.ProcessAllMatches(true);
		}

		protected override void MatchConsolCore()
		{
			if (QuantumMawbsGrid.SelectedElements.Length == 1 && EnterpriseConsolsGrid.SelectedElements.Length == 1)
			{
				NZQuantumMawb selectedMawb = (NZQuantumMawb)QuantumMawbsGrid.SelectedElements[0];
				selectedMawb.MissingShippingLineOnConsolEvent += new EventHandler(MissingShippingLineOnConsol);
				try
				{
					selectedMawb.LinkedConsol = (ForwardingConsol)EnterpriseConsolsGrid.SelectedElements[0];
				}
				finally
				{
					selectedMawb.MissingShippingLineOnConsolEvent -= new EventHandler(MissingShippingLineOnConsol);
				}
				ProcessButton.Enabled = Manager.IsAnyMawbLinkedToAnEnterpriseConsol;
			}
		}

		protected virtual
 void MissingShippingLineOnConsol(object sender, EventArgs e)
		{
			string message = string.Format("Cannot Match to Consol {0}, the Carrier is not specified.{1}If a carrier is not specified, then ECI Manifests cannot be created.{1}Please enter a carrier on Consol {0}, then re-match Quantum Mawbs to Consol {0}.",
				sender.ToString(), System.Environment.NewLine);
			Globals.Message.ShowWarning(message, "No Carrier on Consol");
		}
	}
}
