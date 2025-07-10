using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CreditControlledDocumentsCheckConfigurationControl : PaymentAuthorisationSettingsControl
	{
		public CreditControlledDocumentsCheckConfigurationControl()
		{
			InitializeComponent();
			InititalizeAdditionalColumns();
		}

		void InititalizeAdditionalColumns()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CreditControlledDocumentsCheckConfiguration|88136a9b-62a3-4a15-8bcc-bf53b6654451", "Invoice Type");
			zDropEditColumnStyleInfo1.ColumnName = "InvoiceType";
			PaymentAuthorisationSettingsGrid.ColumnStyles.Insert(0, zDropEditColumnStyleInfo1);

			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CreditControlledDocumentsCheckConfiguration|a5d1b3bb-e9e7-4e9a-b97b-862e9c5e92ae", "No. Of Days Overdue");
			zCalcEditColumnStyleInfo1.ColumnName = "NumberOfDaysOverdue";
			PaymentAuthorisationSettingsGrid.ColumnStyles.Insert(3, zCalcEditColumnStyleInfo1);
		}

#if DEBUG
		public ZGrid PaymentAuthorisationSettingsGrid_ForTest { get { return PaymentAuthorisationSettingsGrid; } }
#endif

	}
}

