using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.BR.Business;

namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
	{
		public ImportSupplierHeaderUserControl()
		{
			InitializeComponent();
			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
		}

		protected JobDeclaration Declaration => JobDeclaration as JobDeclaration;

		protected override ZGridColumnInfo[] GetSupplierAddressColumns()
		{
			var columns = new List<ZGridColumnInfo>();

			var supplierOrgPKOrganisationFindBox = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			supplierOrgPKOrganisationFindBox.CaptionResourceString = JobComInvoiceHeader.SupplierCaption;
			supplierOrgPKOrganisationFindBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			supplierOrgPKOrganisationFindBox.ColumnName = JobComInvoiceHeader.Schema.SupplierOrgPK;
			supplierOrgPKOrganisationFindBox.GroupName = JobComInvoiceHeader.SupplierCaption;
			supplierOrgPKOrganisationFindBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			columns.Add(supplierOrgPKOrganisationFindBox);

			var supplierAddressGuidDropEdit = new ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			supplierAddressGuidDropEdit.CaptionResourceString = Res.GetData("32deb4e7-1b19-4420-ace5-57792c7310ac", "Supplier Address");
			supplierAddressGuidDropEdit.GroupName = JobComInvoiceHeader.SupplierCaption;
			supplierAddressGuidDropEdit.ColumnName = JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress;
			supplierAddressGuidDropEdit.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);
			columns.Add(supplierAddressGuidDropEdit);

			if (Declaration.IsImportOnly)
			{
				CreateSupplierColumns();
			}

			return columns.ToArray();
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			AddColumns();
		}

		void AddColumns()
		{
			CreateNewDropEditColumn(JobComInvoiceHeader.Schema.JZ_RelatedIndicator, 100);
			CreateNewDropEditColumn(JobComInvoiceHeader.Schema.JZ_ValuationCode, 100);
			CreateNewDropEditColumn(JobComInvoiceHeader.Schema.ExchangeHedgeType, 100);
			CreateNewDropEditColumn(JobComInvoiceHeader.Schema.ExchangeHedgeFinancialInstitution, 140);
			CreateNewDropEditColumn(JobComInvoiceHeader.Schema.ExchangeHedgeReason, 100);
			CreateNewCalcEditColumn(JobComInvoiceHeader.Schema.ExchangeHedgeValue, 90);
			CreateNewTextBoxColumn(JobComInvoiceHeader.Schema.ExchangeHedgeROFBACENNumber, 90);
		}

		void CreateSupplierColumns()
		{
			CreateNewTextBoxColumn(JobComInvoiceHeader.Schema.JZ_SupplierAuthorityIdentifier, 120, defaultColumn: true, groupName: JobComInvoiceHeader.SupplierCaption);
			CreateNewTextBoxColumn(JobComInvoiceHeader.Schema.JZ_SupplierAuthorityVersion, 120, defaultColumn: true, groupName: JobComInvoiceHeader.SupplierCaption);
		}

		protected override IEnumerable<string> GetDefaultColumnsInOrderCore()
		{
			var columns = new List<string>
			{
				JobComInvoiceHeader.Schema.JZ_InvoiceNumber,
				JobComInvoiceHeader.Schema.SupplierOrgPK,
				JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress,
			};
			if (Declaration.IsImportOnly)
			{
				columns.AddRange(new List<string>
				{
					JobComInvoiceHeader.Schema.JZ_SupplierAuthorityVersion,
					JobComInvoiceHeader.Schema.JZ_SupplierAuthorityIdentifier,
				});
			}
			columns.AddRange(new List<string>
			{
				JobComInvoiceHeader.Schema.JZ_IncoTermPlace,
				JobComInvoiceHeader.Schema.JZ_InvoiceAmount,
				JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency,
				JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate,
				JobComInvoiceHeader.Schema.JZ_Calc_BalanceString,
				JobComInvoiceHeader.Schema.JZ_PaymentDate,
				JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill,
				JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice,
				JobComInvoiceHeader.Schema.JZ_Remarks,
			});

			return columns;
		}
	}
}
