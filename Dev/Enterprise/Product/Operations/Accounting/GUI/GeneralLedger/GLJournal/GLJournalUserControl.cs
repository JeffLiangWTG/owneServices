using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	public partial class GLJournalUserControl : ZUserControl
	{
		public GLJournalUserControl()
		{
			InitializeComponent();

			ShowApprovalRequestControls = true;

			if (!AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.Value)
			{
				DissectionAttributesTabPage.TabVisible = false;
			}
		}

		void ShowGLAccountsForImportAction(AccGLHeaderCollection collection, List<AccGLHeader> glHeaderList)
		{
			ZFormModaliser.ShowDialogAndDispose(new GLAccountSelectionForm(collection, glHeaderList));
		}

		[DefaultValue(true)]
		public bool ShowApprovalRequestControls
		{
			get { return showApprovalRequestControls; }
			set
			{
				showApprovalRequestControls = value;

				ApprovalRequestStatusDropEdit.Visible = showApprovalRequestControls;
			}
		}
		bool showApprovalRequestControls;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.China)
				{
					JournalLinesGrid.RemoveFromAvailableColumns("GLLocalCNAccountDescription");
					JournalLinesGrid.RemoveFromAvailableColumns("GLLocalCNAccountCode");
				}

				if (!AccountingMasterFilesUtils.HasGLAccountSelectionAndEntry)
				{
					JournalLinesGrid.RemoveFromAvailableColumns("AlternateGLAccountNumber");
					JournalLinesGrid.RemoveFromAvailableColumns("AlternateGLAccountDescription");
				}

				if (Journal != null)
				{
					AH_NumberOfSupportingDocumentsCalcEdit.Visible = Journal.AH_NumberOfSupportingDocumentsVisible_ReadOnly;
					ZGridColumnInfo columnInfo = JournalLinesGrid.GetColumnStyle("UnsignedLineAmount");
					if (columnInfo != null)
					{
						((ZCalcEditColumnStyleInfo)columnInfo).Decimals = Journal.CurrencySubUnitRatio;
					}

					Journal.Lines.ShowGLAccountsForImportAction = ShowGLAccountsForImportAction;
				}

				JournalLinesGrid.OnRemovingBizOFromList += new ZGrid.RemoveBizOFromListHandler(RemoveBizOFromListHandler);

				PostPeriodPostDateEdit.Visible = IsPostDateEnabled;
				ReversePeriodDueDateEdit.Visible = IsDueDateEnabled;
			}
		}

		GLJournal Journal => (GLJournal)BindingSource.Current;

		bool IsPostDateEnabled => Journal?.IsPostDateEnabled ?? false;

		bool IsDueDateEnabled => Journal?.IsDueDateEnabled ?? false;

		ZGrid.ContinueWithRemove RemoveBizOFromListHandler(BusinessObject bizOToRemoveOrDelete)
		{
			ZGrid.ContinueWithRemove result = ZGrid.ContinueWithRemove.Remove;
			GLJournalLine line = bizOToRemoveOrDelete as GLJournalLine;
			if (line != null && line.HasAssignedExportBatchNumber)
			{
				result = ZGrid.ContinueWithRemove.CancelRemoval;
				Globals.Message.ShowWarning(Res.GetString("F5F1DCB9-7F0E-466d-957A-3B0CA9AFAE99", "This line has been assigned a 'GL Export Batch Number' and can't be deleted."));
			}
			return result;
		}

		void AH_TransactionTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			bool isNoteJournal = Journal?.IsNoteJournal ?? false;

			JournalLinesGrid.SetAvailability(isNoteJournal, [GLJournalLine.Schema.Units, GLJournalLine.Schema.UnitQuantity]);
			JournalLinesGrid.SetAvailability(!isNoteJournal, [GLJournalLine.Schema.UnsignedOSLineAmount, GLJournalLine.Schema.UnsignedLocalLineAmount]);
			InvoiceAmountCalcEdit.Visible = !isNoteJournal;

			PostPeriodPostDateEdit.Visible = IsPostDateEnabled;
			ReversePeriodDueDateEdit.Visible = IsDueDateEnabled;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (Journal != null)
			{
				Journal.AH_TransactionTypeInfo.ValueChanged -= AH_TransactionTypeInfo_ValueChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (Journal != null)
			{
				Journal.AH_TransactionTypeInfo.ValueChanged -= AH_TransactionTypeInfo_ValueChanged;
				Journal.AH_TransactionTypeInfo.ValueChanged += AH_TransactionTypeInfo_ValueChanged;
				AH_TransactionTypeInfo_ValueChanged(null, null);
			}
		}
	}
}
