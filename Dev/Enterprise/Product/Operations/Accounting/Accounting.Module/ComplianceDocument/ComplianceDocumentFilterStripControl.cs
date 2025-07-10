using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class ComplianceDocumentFilterStripControl : ZFilterStripControl
	{
		public ComplianceDocumentFilterStripControl(IBusinessObjectCollection gridCollection, ComplianceDocumentFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitialiseForm();
			ManageColumns();
		}

		protected void InitialiseForm()
		{
			SuspendLayout();
			try
			{
				InitializeComponent();
				BindingSource.DataSource = FilterBusinessObject;
			}
			finally
			{
				ResumeLayout();
			}
		}

		void ManageColumns()
		{
			ZGridColumnInfo eInvoicingStatusInfo = null;
			ZGridColumnInfo eInvoicingErrorInfo = null;
			ZGridColumnInfo eInvoicingLastResponseReceivedUtcInfo = null;
			ZGridColumnInfo eInvoicingLastSentTimeUtcInfo = null;
			ZGridColumnInfo eInvoicingBatchNumberInfo = null;
			ZGridColumnInfo eInvoicingBatchStatusInfo = null;
			ZGridColumnInfo isSpecialVoidingInfo = null;
			ZGridColumnInfo voidingReasonInfo = null;
			ZGridColumnInfo approvalNumberInfo = null;

			IEnumerator columnEnum = FilteredGrid.ColumnStyles.GetEnumerator();
			while (columnEnum.MoveNext())
			{
				ZGridColumnInfo column = (ZGridColumnInfo)columnEnum.Current;
				if (column.ColumnName == "EInvoicingStatus")
				{
					eInvoicingStatusInfo = column;
				}
				else if (column.ColumnName == "EInvoicingError")
				{
					eInvoicingErrorInfo = column;
				}
				else if (column.ColumnName == "EInvoicingLastResponseReceivedUtc")
				{
					eInvoicingLastResponseReceivedUtcInfo = column;
				}
				else if (column.ColumnName == "EInvoicingLastSentTimeUtc")
				{
					eInvoicingLastSentTimeUtcInfo = column;
				}
				else if (column.ColumnName == "EInvoicingBatchNumber")
				{
					eInvoicingBatchNumberInfo = column;
				}
				else if (column.ColumnName == "EInvoicingBatchStatus")
				{
					eInvoicingBatchStatusInfo = column;
				}
				else if (column.ColumnName == "IsSpecialVoiding")
				{
					isSpecialVoidingInfo = column;
				}
				else if (column.ColumnName == "ADH_VoidingReason")
				{
					voidingReasonInfo = column;
				}
				else if (column.ColumnName == "ADH_ApprovalNumber")
				{
					approvalNumberInfo = column;
				}
			}

			var isAPComplianceDocument = (FilterBusinessObject as ComplianceDocumentFilterStripBusinessObject).LedgerType == LedgerTypes.AccountsPayable;

			if (isAPComplianceDocument || !AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value)
			{
				RemoveColumnFromGrid(FilteredGrid, eInvoicingStatusInfo);
				RemoveColumnFromGrid(FilteredGrid, eInvoicingErrorInfo);
				RemoveColumnFromGrid(FilteredGrid, eInvoicingLastResponseReceivedUtcInfo);
				RemoveColumnFromGrid(FilteredGrid, eInvoicingLastSentTimeUtcInfo);
				RemoveColumnFromGrid(FilteredGrid, eInvoicingBatchNumberInfo);
				RemoveColumnFromGrid(FilteredGrid, eInvoicingBatchStatusInfo);
			}

			if (isAPComplianceDocument)
			{
				RemoveColumnFromGrid(FilteredGrid, isSpecialVoidingInfo);
				RemoveColumnFromGrid(FilteredGrid, voidingReasonInfo);
				RemoveColumnFromGrid(FilteredGrid, approvalNumberInfo);
			}
		}

		void RemoveColumnFromGrid(ZDisplayGrid grid, ZGridColumnInfo columnInfo)
		{
			if (columnInfo != null)
			{
				grid.ColumnStyles.Remove(columnInfo);
			}
		}

		protected override void HandleGridSizing()
		{
			if (FilteredGrid != null)
			{
				FilteredGrid.Location = ControlDpiScalingHelper.NewScaledPoint(0, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(FilteredGrid.Top));

				if (GridSplitter != null)
				{
					ControlDpiScalingHelper.SetHeight(FilteredGrid, ClientSize.Height - FilteredGrid.Top - GridSplitter.SplitPosition - ControlDpiScalingHelper.ScaleToCurrentDpiY(10), false);
				}
				else
				{
					ControlDpiScalingHelper.SetHeight(FilteredGrid, ClientSize.Height - FilteredGrid.Top, false);
				}

				ControlDpiScalingHelper.SetWidth(FilteredGrid, ClientSize.Width, false);
			}
		}

		void GridSplitter_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (FilteredGrid != null)
			{
				var innerSender = ((KSplitter)sender);
				int maxSplitterPosition = Math.Max(innerSender.MinSize, ClientSize.Height - FilteredGrid.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(100));

				if (innerSender.SplitPosition > maxSplitterPosition)
				{
					innerSender.SplitPosition = maxSplitterPosition;
				}
			}
		}

		void FilteredGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			ResetComplianceDocumentLinesForSelectedHeader();
		}

		AccComplianceDocumentHeader fPreviousHeader;
		void ResetComplianceDocumentLinesForSelectedHeader()
		{
			if (FilteredGrid.CurrentRowIndex >= 0)
			{
				fPreviousHeader = ((AccComplianceDocumentHeaderCollection)FilteredGrid.DataSource)[FilteredGrid.CurrentRowIndex];
				((ComplianceDocumentFilterStripBusinessObject)FilterBusinessObject).SetComplianceDocumentLinesCurrentLinesForHeader(fPreviousHeader);
			}
		}
	}
}
