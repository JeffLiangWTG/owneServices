using System;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class TransactionPendingAllocationApprovalBulkForm : TransactionPendingAllocationApprovalBulkFormForDesigner
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public TransactionPendingAllocationApprovalBulkForm()
		{
		}

		public TransactionPendingAllocationApprovalBulkForm(TransactionPendingAllocationApprovalBulk bo, TransactionApprovalFormModes actionMode)
			: base(bo, actionMode)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				if (!TransactionHeader.IsNumberOfSupportingDocumentsVisible)
				{
					AH_NumberOfSupportingDocumentsCalcEdit.Visible = false;
					importedXMLTabPage.RunWhenTabInitialized((sender, args) => importedInvoiceXMLControl.HideNumberOfDocuments());
				}

				if (this.TopGrid.ListManager != null)
				{
					this.TopGrid.ListManager.CurrentChanged += TopGridListManager_CurrentChanged;
				}

				SetImportedXMLTabPageVisibility();
				placeOfSupplyTextBox.Visible = PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany);
				detailsTabControl.SelectedIndexChanged += detailsTabControl_SelectedIndexChanged;
			}
		}

		void detailsTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!LastActiveDetailsTabIndexUpdateSuspender.IsSuspended)
			{
				lastActiveDetailsTabIndex = detailsTabControl.SelectedIndex;
			}
		}

		FunctionalitySuspender LastActiveDetailsTabIndexUpdateSuspender
		{
			get { return lastActiveDetailsTabIndexUpdateSuspender ?? (lastActiveDetailsTabIndexUpdateSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender lastActiveDetailsTabIndexUpdateSuspender;

		void TopGridListManager_CurrentChanged(object sender, EventArgs e)
		{
			SetImportedXMLTabPageVisibility();
		}

		void SetImportedXMLTabPageVisibility()
		{
			using (LastActiveDetailsTabIndexUpdateSuspender.GetSuspender())
			{
				var currentRequest = TopGrid.ListManager != null ? (TopGrid.ListManager.GetCurrent() as TransactionPendingAllocationApprovalRequest) : ((TransactionPendingAllocationApprovalBulk)BusinessEntity).Approvals[0];
				var prevImportedXMLTabPage_TabVisibleValue = importedXMLTabPage.TabVisible;
				var importedXMLTabPage_TabVisible = currentRequest != null && !currentRequest.PostingDetails.SourceXML.IsEmpty;
				if (!importedXMLTabPage_TabVisible)
				{
					lastActiveDetailsTabIndex = detailsTabControl.SelectedIndex;
				}
				importedXMLTabPage.TabVisible = importedXMLTabPage_TabVisible;
				if (prevImportedXMLTabPage_TabVisibleValue != importedXMLTabPage.TabVisible && importedXMLTabPage.TabVisible)
				{
					detailsTabControl.SelectedIndex = lastActiveDetailsTabIndex;
				}
			}
		}

		int lastActiveDetailsTabIndex;

		public override bool IsReasonDescriptionReadOnly
		{
			get { return base.IsReasonDescriptionReadOnly && ActionMode != TransactionApprovalFormModes.Reject; }
		}
	}

#if DEBUG
	// This ZForm serves as base class only. It's not abstract so that it can be open in designer tool. Hence it has TestExcludeZWinFormsAllHaveFormBashers attribute applied.
	[TestExcludeZWinFormsAllHaveFormBashers]
#endif
	public class TransactionPendingAllocationApprovalBulkFormForDesigner : TransactionApprovalBulkForm<TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails>
	{
		public TransactionPendingAllocationApprovalBulkFormForDesigner()
		{
		}

		public TransactionPendingAllocationApprovalBulkFormForDesigner(TransactionPendingAllocationApprovalBulk bo, TransactionApprovalFormModes actionMode)
			: base(bo, actionMode)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.DetailsGroupBox.Controls.Remove(this.DetailsGrid);
			this.DetailsGroupBox.Controls.Remove(this.DetailsTopPanel);
		}
	}
}
