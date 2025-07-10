using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class ShipmentDetailsUserControl : ZUserControl
{
	public ShipmentDetailsUserControl()
	{
		InitializeComponent();
		VatPaidByUserControl.SetBindingMembers(nameof(JobDeclaration.JE_VATPaidBy), nameof(JobDeclaration.VATPaidByAccountNo));
	}
}
