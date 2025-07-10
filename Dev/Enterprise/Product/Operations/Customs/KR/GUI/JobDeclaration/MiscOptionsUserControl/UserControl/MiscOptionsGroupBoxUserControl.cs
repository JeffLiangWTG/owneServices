using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class MiscOptionsGroupBoxUserControl : ZUserControl
	{
		public MiscOptionsGroupBoxUserControl()
		{
			InitializeComponent();

			SetLayout();
		}

		void SetLayout()
		{
			MiscellaneousDynamicLayoutPanel.UpdateLayout(new MiscellaneousOptionsLayout());
			ReturnDynamicLayoutPanel.UpdateLayout(new ReturnOptionsLayout());
			SouthNorthTradeDynamicLayoutPanel.UpdateLayout(new SouthNorthTradeOptionsLayout());
			AdditionalCargoDynamicLayoutPanel.UpdateLayout(new AdditionalCargoOptionsLayout());
			PenaltyDeclarationDynamicLayoutPanel.UpdateLayout(new PenaltyDeclarationOptionsLayout());
			RefundRequestDynamicLayoutPanel.UpdateLayout(new RefundRequestOptionsLayout());
		}
	}
}
