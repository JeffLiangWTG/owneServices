using System;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.GUI.PlugIn;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ExportSupplierHeaderUserControl : EUExportSupplierHeaderUserControl
	{
		public ExportSupplierHeaderUserControl()
		{
			InitializeComponent();
		}

		protected override Type GetAdditionalInfosUserControlType()
		{
			return typeof(AdditionalInfosUserControlWithGrid);
		}

		protected override Type GetPreviousDocumentsUserControlType()
		{
			return typeof(ExportSupplierHeaderPreviousDocumentsUserControl);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			AddColumns();
			RemoveColumns();
		}

		void RemoveColumns()
		{
			InvoiceChargesGrid.SetAvailability(false, InvoiceCharge.Schema.J7_IsDutiable);
			BaseGroupChargesGrid.SetAvailability(false, InvoiceCharge.Schema.J7_IsDutiable);
		}

		protected override Type GetSupportingDocumentsUserControlType() => typeof(ExportSupplierHeaderSupportingDocumentsUserControl);

		void AddColumns()
		{
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceHeader.Schema.JZ_UCR,
				CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("97E21B44-5559-4A51-8184-E930154059B5", "Commercial Ref"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(new ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceHeader.Schema.ZG_AgreedPlaceCode,
				CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("4209FEA8-7383-48BD-B977-498756BD6D53", "Place", "Inc. Place", "Incoterm Place"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});
		}

		protected override CargoWiseOne.ResourceStrings.ResourceStringData GetAdditionalInfoTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) => Enterprise.Customs.DE.GUI.Res.GetData("93f24a1b-37e0-43e0-8ae4-b6270bffebc3", "[44] Additional Documents");
	}
}
