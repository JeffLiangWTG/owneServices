using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportMethodFourUserControl : ZUserControl
	{
		public ImportMethodFourUserControl()
		{
			InitializeComponent();
		}

		public void BindToMessageSendingObject()
		{
			BindingSource.DataSourceType = typeof(Business.ValuationDeclarationMessageSendingObjectParent);
			Controls.ChangeBindingPaths(BindingSource, ControlExtensionMethods.BindingPathForMessageSending);

			DeductionCostCustomsReferenceNumberTextBox.ReadOnly = true;
			DeductionCostCostRateCalcEdit.ReadOnly = true;
			DeductionCostCostRateCodeDropEdit.ReadOnly = true;
		}
	}
}
