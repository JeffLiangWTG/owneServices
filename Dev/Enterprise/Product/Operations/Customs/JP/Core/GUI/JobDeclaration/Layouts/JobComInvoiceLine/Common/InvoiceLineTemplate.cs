using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI;
public partial class InvoiceLineTemplate : ZUserControl
{
	public InvoiceLineTemplate()
	{
		InitializeComponent();
		SetupComponents();
	}

	void SetupComponents()
	{
		CustomsQuantityCalcDropEdit.AllowNegative = false;
		VolumeCalcDropEdit.AllowNegative = false;
		WeightCalcDropEdit.AllowNegative = false;
		UnitPriceCalcEdit.AllowNegative = false;
		InvoiceQuantityCalcDropEdit.AllowNegative = false;
		LinePriceCurrencyCalcFindBox.AllowNegative = false;
	}
}
