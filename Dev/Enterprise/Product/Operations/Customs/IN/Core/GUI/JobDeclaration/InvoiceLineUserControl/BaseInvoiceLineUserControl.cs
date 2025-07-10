using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class BaseInvoiceLineUserControl : DeclarationInvoiceLineUserControl
{
	public BaseInvoiceLineUserControl()
	{
		InitializeComponent();
		SupportingDocumentTabPage.RunWhenBindingOrFirstShown((_, _) => SupportingDocumentUserControl.UserControlType = typeof(LayoutSupportingDocumentsUserControl));
	}

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		AddColumnsToGrid();
	}

	void AddColumnsToGrid()
	{
		if (base.JobDeclaration is JobDeclaration { IsPersistent: true })
		{
			var zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_CEI,
				ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(104),
			};

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Insert(2, zGuidDropEditColumnStyleInfo1);
		}
	}

	protected override bool UseUniversalTariff => false;
}
