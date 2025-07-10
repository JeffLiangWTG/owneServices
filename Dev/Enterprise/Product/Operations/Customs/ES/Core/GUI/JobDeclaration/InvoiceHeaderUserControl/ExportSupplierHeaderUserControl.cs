using System;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.GUI
{
	public partial class ExportSupplierHeaderUserControl : EU.GUI.EUNonLayoutExportSupplierHeaderUserControl
	{
		public ExportSupplierHeaderUserControl()
		{
			InitializeComponent();
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			SetupHeaderColumns();
		}

		void SetupHeaderColumns()
		{
			var supplierColumnStyle = JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == JobComInvoiceHeader.Schema.SupplierOrgPK);
			if (supplierColumnStyle != null)
			{
				JobComInvoiceHeadersBoundGrid.ColumnStyles.Remove(supplierColumnStyle);
			}
			var buyerOrgColumnStyle = JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == JobComInvoiceHeader.Schema.BuyerOrgPK);
			if (buyerOrgColumnStyle != null)
			{
				JobComInvoiceHeadersBoundGrid.ColumnStyles.Remove(buyerOrgColumnStyle);
			}

			var buyerGroup = Res.GetData("A6CEEA7E-07CB-4F61-86B6-C920A9D77D4B", "Buyer");
			var buyerColumnStyle = JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == JobComInvoiceHeader.Schema.JZ_OH_Buyer);
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(available: true, JobComInvoiceHeader.Schema.JZ_OH_Buyer);
			buyerColumnStyle.GroupName = buyerGroup;

			var buyerAddressColumnStyle = JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == JobComInvoiceHeader.Schema.JZ_OA_BuyerAddress);
			buyerAddressColumnStyle.GroupName = buyerGroup;
		}

		protected override Type GetAdditionalInfosUserControlType() => typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid);

		protected override Type GetPreviousDocumentsUserControlType() => typeof(PreviousDocumentsUserControl);

		protected override Type GetSupportingDocumentsUserControlType() => typeof(ExportSupportingDocumentsUserControl);
	}
}
