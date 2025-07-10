using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class PaymentMethodUserControl : ZUserControl
{
	public PaymentMethodUserControl()
	{
		InitializeComponent();
	}

	internal void SetBindingMembers(string paymentMethodDataMember, string accountNoDataMember)
	{
		BindingSource.SetBindingMember(PaymentMethodDropEdit, paymentMethodDataMember);
		BindingSource.SetBindingMember(AccountNoTextBox, accountNoDataMember);
	}
}
