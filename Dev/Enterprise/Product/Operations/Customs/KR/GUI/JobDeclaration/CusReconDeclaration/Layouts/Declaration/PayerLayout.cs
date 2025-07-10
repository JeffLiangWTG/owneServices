using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class PayerLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public PayerLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var layout = new PanelLayout();
			var bag = RefundDeclarationControlBag.Instance;
			layout.RegisterControlBag(bag);

			var ruler1 = layout.CreateRuler(105);
			layout.Include(ruler1, bag.PayerAddressControl);
			layout.Include(ruler1, bag.RegistrationNumberOneTextBox);
			layout.Include(ruler1, bag.KoreanRegistrationNumberOfCEOTextBox);
			layout.Include(ruler1, bag.PayerBankDropEdit);
			layout.Include(ruler1, bag.BankAccountNumberTextBox);
			return layout;
		}
	}
}
