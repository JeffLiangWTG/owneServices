using Enterprise.Core.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportLicenseSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
	{
		public ImportLicenseSupplierHeaderUserControl()
		{
			InitializeComponent();
			InvoiceHeadersBoundGrid.ColumnLayoutContext = BRJobMessageTypeList.Codes.ImportLicense;
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			JZ_InvoiceCurrExRateCalcEdit.Visible = false;
			JZ_IncoTermPlaceTextBox.Visible = false;
			JZ_InvoiceCurrLandedCostExRateCalcEdit.Visible = false;
			NoOfPacksCalcDropEdit.Visible = false;
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			AddColumns();
			InvoiceHeadersBoundGrid.RemoveFromAvailableColumns(JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate, JobComInvoiceHeader.Schema.JZ_IncoTermPlace, JobComInvoiceHeader.Schema.JZ_InvoiceCurrLandedCostExRate, JobComInvoiceHeader.Schema.JZ_NoOfPacks,
				JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill, JobComInvoiceHeader.Schema.JZ_PaymentAmount, JobComInvoiceHeader.Schema.JZ_PaymentExRate);
		}

		protected override ZGridColumnInfo[] GetSupplierAddressColumns()
		{
			var supplierOrgPKOrganisationFindBox = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			supplierOrgPKOrganisationFindBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			supplierOrgPKOrganisationFindBox.ColumnName = JobComInvoiceHeader.Schema.SupplierDocOrgPK;
			supplierOrgPKOrganisationFindBox.GroupName = JobComInvoiceHeader.SupplierCaption;
			supplierOrgPKOrganisationFindBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			var supplierAddressGuidDropEdit = new ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			supplierAddressGuidDropEdit.BindToList = "SupplierDocumentaryAddress+Organisation+Addresses";
			supplierAddressGuidDropEdit.GroupName = JobComInvoiceHeader.SupplierCaption;
			supplierAddressGuidDropEdit.ColumnName = JobComInvoiceHeader.Schema.SupplierDocAddressPK;
			supplierAddressGuidDropEdit.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);

			return new ZGridColumnInfo[] { supplierOrgPKOrganisationFindBox, supplierAddressGuidDropEdit };
		}

		void AddColumns()
		{
			CreateNewDropEditColumn(JobComInvoiceHeader.Schema.ExchangeHedgeType, 100);
			CreateNewDropEditColumn(JobComInvoiceHeader.Schema.ExchangeHedgePaymentMethod, 100);
			CreateNewCalcEditColumn(JobComInvoiceHeader.Schema.ExchangeHedgePaymentDeadline, 90);
			CreateNewDropEditColumn(JobComInvoiceHeader.Schema.ExchangeHedgeReason, 100);
			CreateNewDropEditColumn(JobComInvoiceHeader.Schema.ExchangeHedgeFinancialInstitution, 100);
		}
	}
}
