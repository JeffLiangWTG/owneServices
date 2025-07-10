using System.Windows.Forms;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class BaseInvoiceLineChargesUserControl : Customs.GUI.InvoiceLineChargesUserControl
	{
		public BaseInvoiceLineChargesUserControl()
		{
			InitializeComponent();
			ResetColumnsInChargesGrid();
		}

		void ResetColumnsInChargesGrid()
		{
			ChargesGrid.SetColumnVisible(false, Customs.Business.BaseJobComInvHeaderCharge.Schema.ChargeCodeDescription);
			ChargesGrid.RemoveFromAvailableColumns(Customs.Business.BaseJobComInvHeaderCharge.Schema.ChargeCodeDescription);
			ChargesGrid.ColumnStyles.Insert(1, GetNewDescriptionColumnstyleInfo());

			ApportionedChargesGrid.SetColumnVisible(false, Customs.Business.BaseJobComInvHeaderCharge.Schema.ChargeCodeDescription);
			ApportionedChargesGrid.RemoveFromAvailableColumns(Customs.Business.BaseJobComInvHeaderCharge.Schema.ChargeCodeDescription);
			ApportionedChargesGrid.ColumnStyles.Insert(1, GetNewDescriptionColumnstyleInfo());
		}

		ZTextBoxColumnStyleInfo GetNewDescriptionColumnstyleInfo()
		{
			return new ZTextBoxColumnStyleInfo()
			{
				CharacterCasing = CharacterCasing.Upper,
				ColumnName = AutoJobComInvHeaderCharge.Schema.J7_ChargeDescription,
				CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("EC98073A-1C05-4A58-B145-8351F5BB1B2F", "Description"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			};
		}

		protected override string ColumnTitleForGSTApplies => Res.GetString("2bea7d06-7f4b-4b78-8f5b-1cc3061c9efb", "VAT Apply");
	}
}
