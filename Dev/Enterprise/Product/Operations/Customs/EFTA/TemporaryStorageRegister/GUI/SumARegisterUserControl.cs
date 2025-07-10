using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

public partial class SumARegisterUserControl : ZUserControl
{
	public SumARegisterUserControl()
	{
		InitializeComponent();

		if (SumARegisterFormLayoutProvider != null)
		{
			SetDetailsHeaderLayout();
			SetLinesDetailsLayout();
		}
	}

	void SetDetailsHeaderLayout()
	{
		if (DetailsHeaderLayout != null)
		{
			DetailsHeaderDynamicLayoutPanel.UpdateLayout(DetailsHeaderLayout);
		}
	}

	void SetLinesDetailsLayout()
	{
		if (LinesDetailsLayout != null)
		{
			LinesDetailsDynamicLayoutPanel.UpdateLayout(LinesDetailsLayout);
		}
	}

	internal IPanelLayoutProvider DetailsHeaderLayout => SumARegisterFormLayoutProvider.GetDetailsHeaderLayout();

	internal IPanelLayoutProvider LinesDetailsLayout => SumARegisterFormLayoutProvider.GetLinesDetailsLayout();

	ISumARegisterFormLayoutProvider SumARegisterFormLayoutProvider =>
		sumARegisterFormLayoutProvider ??= GUI.SumARegisterFormLayoutProvider.GetLayoutProvider();
	ISumARegisterFormLayoutProvider sumARegisterFormLayoutProvider;
}
