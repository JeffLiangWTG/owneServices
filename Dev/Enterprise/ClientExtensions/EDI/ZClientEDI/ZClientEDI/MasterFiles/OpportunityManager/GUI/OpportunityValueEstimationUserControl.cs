using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public partial class OpportunityValueEstimationUserControl : ZUserControl
	{
		public OpportunityValueEstimationUserControl()
		{
			InitializeComponent();

			this.ContractedValueCalcFindBox.CaptionResourceString = ZClientEDI.Res.GetData("a395a7df-221c-4ff2-8824-023fde862771", "Contract (p.a)");
			this.LifetimeValueCalcFindBox.CaptionResourceString = ZClientEDI.Res.GetData("2bdc735b-8d95-4af8-acf4-c92bbd530e7d", "Lifetime (3 CLV)");
			this.ContractedUserCountTextBox.CaptionResourceString = ZClientEDI.Res.GetData("7d4dc3ff-4908-4189-b873-24ac8e6fecf0", "User Count", "Contracted");
			this.GlobalPotentialLabel.CaptionResourceString = ZClientEDI.Res.GetData("b500ec71-0a4d-4114-860b-cd8de2ed619f", "Global Reach");
			this.ContractedLabel.Text = OrganisationsDataRegistry.Instance.CurrentLabel.Value;
			this.LocalPotentialLabel.Text = OrganisationsDataRegistry.Instance.PotentialLabel.Value;
		}
	}
}
