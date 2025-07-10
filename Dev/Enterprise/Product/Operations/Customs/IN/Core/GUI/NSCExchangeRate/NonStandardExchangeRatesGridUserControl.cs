using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class NonStandardExchangeRatesGridUserControl : ZUserControl
{
	public NonStandardExchangeRatesGridUserControl()
	{
		InitializeComponent();
		UpdateGridColumnLayout();
	}

	void UpdateGridColumnLayout()
	{
		NonStandardExchangeRatesGrid.ApplyGridColumnLayout(GetGridColumnLayoutProvider());
	}

	protected IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new NonStandardExchangeRatesGridColumnLayout();
}

