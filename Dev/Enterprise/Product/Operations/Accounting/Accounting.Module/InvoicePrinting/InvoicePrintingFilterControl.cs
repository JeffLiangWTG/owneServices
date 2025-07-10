using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class InvoicePrintingFilterControl : ZFilterStripControl
	{
		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ZFilterStrip();
		}

		#region Constructor(s) & Dispose

		public InvoicePrintingFilterControl()
		{
			InitializeComponent();
			ManageColumnOrdering();
			RemoveTaxInvoiceColumn();
			RemoveTaxBranchColumn();
		}

		public InvoicePrintingFilterControl(IBusinessObjectCollection gridCollection, InvoicePrintingFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			ManageColumnOrdering();
			RemoveTaxInvoiceColumn();
			RemoveTaxBranchColumn();
		}

		#endregion
		IInvoicePrintingControlPresentationProvider InvoicePrintingControlPresentationProvider => invoicePrintingControlPresentationProvider ?? (invoicePrintingControlPresentationProvider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetInvoicePrintingControlPresentationProvider());
		IInvoicePrintingControlPresentationProvider invoicePrintingControlPresentationProvider;

		#region Implementation

		void ManageColumnOrdering()
		{
			ZGridColumnInfo complianceSubTypeInfo = null;
			IEnumerator columnEnum = FilteredGrid.ColumnStyles.GetEnumerator();
			while (columnEnum.MoveNext())
			{
				ZGridColumnInfo column = (ZGridColumnInfo)columnEnum.Current;
				if (column.ColumnName == "AH_ComplianceSubType")
				{
					complianceSubTypeInfo = column;
				}
			}
			if (complianceSubTypeInfo != null & !GlbCompany.CurrentCompany.Country.SupportComplianceSubType)
			{
				FilteredGrid.ColumnStyles.Remove(complianceSubTypeInfo);
			}
		}

		void RemoveTaxInvoiceColumn()
		{
			if (!GlbCompany.CurrentCompany.Country.HasGovtTaxInvoice)
			{
				ZGridColumnInfo taxInvoiceNum = null;

				foreach (ZGridColumnInfo column in FilteredGrid.ColumnStyles)
				{
					if (column.ColumnName == AccTransactionHeaderSchema.AH_TransactionReference.Name)
					{
						taxInvoiceNum = column;
						break;
					}
				}

				if (taxInvoiceNum != null)
				{
					FilteredGrid.ColumnStyles.Remove(taxInvoiceNum);
				}
			}

			if (!GlbCompany.CurrentCompany.GC_IsGSTRegistered)
			{
				FilteredGrid.RemoveFromAvailableColumns(TransactionHeader.Schema.AH_OSTaxAmount);
				FilteredGrid.RemoveFromAvailableColumns(TransactionHeader.Schema.AH_LocalTaxAmount);
			}
		}

		void RemoveTaxBranchColumn()
		{
			if (!InvoicePrintingControlPresentationProvider.IsTaxBranchColumnAvailable())
			{
				FilteredGrid.RemoveFromAvailableColumns(TransactionHeader.Schema.AH_GB_TaxBranch);
			}
		}
		#endregion
	}
}

