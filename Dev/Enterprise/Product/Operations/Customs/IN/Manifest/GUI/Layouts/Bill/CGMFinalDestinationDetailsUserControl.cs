using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.Manifest.GUI;

public sealed partial class CGMFinalDestinationDetailsUserControl : ZUserControl
{
	public CGMFinalDestinationDetailsUserControl()
	{
		InitializeComponent();
	}

	public void ChangeVisibility(CGMAsycudaBill bill)
	{
		if (bill != null)
		{
			CustomsFinalDestinationPortTextBox.Visible = bill.FinalDestinationIsCFS;
			CustomsFinalDestinationPortCodeFindBox.Visible = bill.FinalDestinationIsCustomsHouse;
		}
	}
}
