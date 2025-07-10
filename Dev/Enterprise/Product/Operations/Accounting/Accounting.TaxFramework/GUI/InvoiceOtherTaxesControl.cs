using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Presentation.GUI;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.TaxFramework.GUI
{
	public partial class InvoiceOtherTaxesControl : ZUserControl
	{
		public InvoiceOtherTaxesControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (presentationProvider == null)
			{
				throw new InvalidOperationException("presentationProvider must be initialised by invoking Initialize() method before binding the control to datasource.");
			}
			else
			{
				if (!presentationProvider.IsSupplyTypeColumnVisible())
				{
					LinesGrid.RemoveFromAvailableColumns("SupplyType");
				}

				if (!presentationProvider.IsTaxBranchColumnVisible())
				{
					LinesGrid.RemoveFromAvailableColumns("TaxBranch");
				}
			}
		}

		public void Bind(object dataSource, string dataMember) => base.SetDataBinding(dataSource, dataMember);

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			//Automatic binding is not allowed as we use custom binding.
			if (dataSource == null) //Only automatic unbinding is allowed.
			{
				base.SetDataBinding(dataSource, dataMember);
			}
		}

		public void ProcessWhenOtherTaxesAddedOrRemoved()
		{
			PointingToTransactionLines.Checked = false;
			PointingToOtherTaxes.Checked = false;
			Rebind();
		}

		void PointingToTransactionLines_Click(object sender, EventArgs e)
		{
			UncheckOtherButtonIfRequired(PointingToTransactionLines, PointingToOtherTaxes);
			Rebind();
		}

		void PointingToOtherTaxes_Click(object sender, EventArgs e)
		{
			UncheckOtherButtonIfRequired(PointingToOtherTaxes, PointingToTransactionLines);
			Rebind();
		}

		void UncheckOtherButtonIfRequired(ZToolStripButton buttonClicked, ZToolStripButton otherButton)
		{
			if (buttonClicked.Checked && otherButton.Checked)
			{
				otherButton.Checked = false;
			}
		}

		public void Initialize(IInvoiceFormPresentationProvider invoiceFormPresentationProvider)
		{
			presentationProvider = invoiceFormPresentationProvider;
		}

		IInvoiceFormPresentationProvider presentationProvider;

		void Rebind()
		{
			if (PointingToTransactionLines.Checked)
			{
				ApplyPointingToTransactionLinesBinding();
			}
			else if (PointingToOtherTaxes.Checked)
			{
				ApplyPointingToOtherTaxesBinding();
			}
			else
			{
				ApplyDefaultBinding();
			}
		}

		void ApplyDefaultBinding()
		{
			this.BindingSource.SetBindingMember(this.LinesGrid, "TransactionLinesForOtherTaxesDisplay");
			this.BindingSource.SetBindingMember(this.TaxRecordsGrid, "TaxTransactionCollection");
		}

		void ApplyPointingToTransactionLinesBinding()
		{
			if (!TaxRecordTransactionLinePivotForDisplay.TaxTransactionCollection.IsNullOrEmpty())
			{
				this.BindingSource.SetBindingMember(this.TaxRecordsGrid, "TaxTransactionCollection");
				this.BindingSource.SetBindingMember(this.LinesGrid, "TaxTransactionCollection.TransactionLinesLinkedToOtherTaxesCollection");
			}
		}

		void ApplyPointingToOtherTaxesBinding()
		{
			if (!TaxRecordTransactionLinePivotForDisplay.TransactionLinesForOtherTaxesDisplay.IsNullOrEmpty())
			{
				this.BindingSource.SetBindingMember(this.LinesGrid, "TransactionLinesForOtherTaxesDisplay");
				this.BindingSource.SetBindingMember(this.TaxRecordsGrid, "TransactionLinesForOtherTaxesDisplay.OtherTaxesLinkedToTransactionLinesCollection");
			}
		}

		public IDisposable SuspendTaxRecordsGridListChanged()
		{
			if (this.TaxRecordsGrid.List is IBusinessObjectCollection collection)
			{
				return collection.SuspendListChanged();
			}
			return DisposableAction.NoAction;
		}

		AccTaxRecordTransactionLinePivotForDisplay TaxRecordTransactionLinePivotForDisplay => CurrentDataItem as AccTaxRecordTransactionLinePivotForDisplay;
	}
}
