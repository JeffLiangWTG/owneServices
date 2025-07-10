using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class BaseCustomsSupplierHeaderUserControl : Customs.GUI.DeclarationInvoiceHeaderUserControl
	{
		public BaseCustomsSupplierHeaderUserControl()
		{
			InitializeComponent();
			AddColumns();
			RemoveColumns();
			ResetColumnsInChargesGrid();
			JZ_Calc_TNIBoundInvoiceCurrencyControl.Visible = false;
		}

		protected virtual void RemoveColumns()
		{
			var columnInfosToRemove = new List<ZGridColumnInfo>() { };
			foreach (ZGridColumnInfo column in JobComInvoiceHeadersBoundGrid.ColumnStyles)
			{
				if (columnsNameToRemove.Contains(column.ColumnName))
				{
					columnInfosToRemove.Add(column);
				}
			}
			foreach (var item in columnInfosToRemove)
			{
				JobComInvoiceHeadersBoundGrid.ColumnStyles.Remove(item);
			}
		}

		string[] columnsNameToRemove => new string[]
		{
			JobComInvoiceHeaderSchema.JZ_PaymentNo.Name,
			JobComInvoiceHeaderSchema.JZ_PaymentAmount.Name,
			JobComInvoiceHeaderSchema.JZ_PaymentExRate.Name,
			JobComInvoiceHeaderSchema.JZ_CU_RelatedHouseBill.Name,
			JobComInvoiceHeaderSchema.JZ_PaymentDate.Name
		};

		void ResetColumnsInChargesGrid()
		{
			InvoiceChargesGrid.RemoveFromAvailableColumns(Customs.Business.BaseJobComInvHeaderCharge.Schema.ChargeCodeDescription);
			InvoiceChargesGrid.ColumnStyles.Insert(1, GetNewDescriptionColumnstyleInfo());

			ApportionedChargesGrid.RemoveFromAvailableColumns(Customs.Business.BaseJobComInvHeaderCharge.Schema.ChargeCodeDescription);
			ApportionedChargesGrid.ColumnStyles.Insert(1, GetNewDescriptionColumnstyleInfo());

			BaseGroupChargesGrid.RemoveFromAvailableColumns(Customs.Business.BaseJobComInvHeaderCharge.Schema.ChargeCodeDescription);
			BaseGroupChargesGrid.ColumnStyles.Insert(1, GetNewDescriptionColumnstyleInfo());
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

		void AddColumns()
		{
			var valuationDateOverrideDateEditColumnStyleInfo = new ZDateEditColumnStyleInfo();
			valuationDateOverrideDateEditColumnStyleInfo.CaptionResourceString = Res.GetData("c85db14d-62f5-43be-a880-d307baf82fad", "Valuation Date Override");
			valuationDateOverrideDateEditColumnStyleInfo.ColumnName = "JZ_ValuationDateOverride";
			valuationDateOverrideDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			valuationDateOverrideDateEditColumnStyleInfo.IsVisible = false;
			valuationDateOverrideDateEditColumnStyleInfo.ToolTip = "Valuation Date Override"; // tool tip message
			valuationDateOverrideDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(valuationDateOverrideDateEditColumnStyleInfo);
		}

		protected override string ColumnTitleForGSTApplies => Res.GetString("c83d8969-8577-4fd7-991e-6d2a5a55d0a0", "{0} Apply", Core.Constants.Customs.CusEntryFeeTypes.VAT);
	}
}
