using System;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobCostingJournalForm : ZForm
	{
		public JobCostingJournalForm(JCJournalHeader journalHeader) : base(journalHeader)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			AH_NumberOfSupportingDocumentsCalcEdit.Visible = journalHeader.AH_NumberOfSupportingDocumentsVisible_ReadOnly;

			var dataExportBatchSource = BusinessEntity as IDataExportBatchSource;
			if (dataExportBatchSource != null && dataExportBatchSource.IsDataExportBatchSupported)
			{
				PlugIns.Add(ControllerIDs.DataExportBatchPlugin);
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void HandleSaveException(Exception ex)
		{
			if (BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation))
			{
				this.DisplayMode = ODisplayMode.ReadOnly;
				SetReadOnlyIncludingChildren();
			}

			base.HandleSaveException(ex);
		}

		#region IDisposable Members 

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

