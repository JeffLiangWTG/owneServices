using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class ComplianceDocumentForm : ZForm, IDataGridLayoutIdentifierRoot
	{
		public ComplianceDocumentForm(AccComplianceDocumentHeader complianceDocumentHeader)
			: base(complianceDocumentHeader)
		{
			PlugIns plugIns = PlugIns;
			ZWorkflowTabPage workflowTabPage = WorkflowTabPage;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			SetDataBinding(complianceDocumentHeader, "");
			DisplayModeChanged += ComplianceDocumentForm_DisplayModeChanged;
			plugIns.Add(ControllerIDs.eDocsPlugIn);
			var provider = complianceDocumentHeader as IWorkflowProvider;

			if (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value
				&& complianceDocumentHeader.IsInDatabase
				&& complianceDocumentHeader.ADH_Ledger == LedgerTypes.AccountsReceivable
				&& complianceDocumentHeader.IsEligibleToCreateEInvoicingTransactionPivot)
			{
				ResetStatusToQueuedMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("f30e178c-f1dc-4dea-96f6-84422e6d35f0", "Reset Status to Queued"), ResetStatusToQueued_Click);
			}

			if (provider != null && !provider.WorkflowType.IsEmpty)
			{
				workflowTabPage.Initialize(complianceDocumentHeader);
			}
			else
			{
				workflowTabPage.TabVisible = false;
			}
		}

		protected override void HandleSaveException(Exception ex)
		{
			var saveException = ex as ZSaveException;
			if (saveException != null)
			{
				if (saveException.IsExceptionPresentIncludingInner<System.Data.Common.DbException>(matchExactType: false))
				{
					this.DisplayMode = ODisplayMode.ReadOnly;
					SetReadOnlyIncludingChildren();
				}
			}

			base.HandleSaveException(ex);
		}

		bool isRunWhenTabInitializedCalledInSetDataBinding;

		protected override bool AllowNew => false;

		AccComplianceDocumentHeader fComplianceDocumentHeader;
		protected AccComplianceDocumentHeader ComplianceDocumentHeader => fComplianceDocumentHeader ?? (fComplianceDocumentHeader = (AccComplianceDocumentHeader)DataSource);

		string IDataGridLayoutIdentifierRoot.ID
		{
			get
			{
				var form = FindForm() as ZForm;
				string result = string.Empty;

				if (form != null)
				{
					result = form.Name;

					if (form.BusinessEntity != null)
					{
						result += form.BusinessEntity.GetType().Name;
					}
				}

				return result;
			}
		}

		public override string FormVerb
		{
			get
			{
				string result;

				if (VoidInsteadOfDelete)
				{
					result = Res.GetString("ComplianceDocumentFormVerb|Void", "Void");
				}
				else if (ComplianceDocumentHeader.IsFinalised || ComplianceDocumentHeader.IsVoided)
				{
					result = FormVerbs.View;
				}
				else if (DisplayMode != ODisplayMode.Delete)
				{
					result = isFormOpenedAsReadOnly ? FormVerbs.View : FormVerbs.Edit;
				}
				else
				{
					result = base.FormVerb;
				}

				return result;
			}
		}

		bool isFormOpenedAsReadOnly;

		void ComplianceDocumentForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			if (e.ToMode == ODisplayMode.ReadOnly)
			{
				isFormOpenedAsReadOnly = true;
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				if (ComplianceDocumentUserControl != null)
				{
					SetControlDefaults();
				}
				else if (!isRunWhenTabInitializedCalledInSetDataBinding)
				{
					ComplianceDocumentTabPage.RunWhenTabInitialized((sender, args) =>
					{
						SetControlDefaults();
					});
					isRunWhenTabInitializedCalledInSetDataBinding = true;
				}
			}
		}

		void SetControlDefaults()
		{
			if (ComplianceDocumentHeader == null)
			{
				return;
			}

			if (!this.IsDesignMode() && (ComplianceDocumentHeader.ADH_Ledger == LedgerTypes.AccountsPayable))
			{
				ComplianceDocumentUserControl.AddressWithContactControl.CaptionResourceString = Res.GetData("44E3A978-3496-45BA-84E5-E1DCB2A2F8D3", "Creditor Information");
			}

			if (ComplianceDocumentHeader.ADH_Ledger == LedgerTypes.AccountsReceivable && ComplianceDocumentHeader.IsVoided)
			{
				ComplianceDocumentUserControl.SetSpecialVoidingPanel();
			}
		}

		string voidComplianceDocumentCaption => Res.GetString("e88cb8a2-afac-47a1-b7cb-47c306411459", "Void Compliance Document");

		public bool VoidInsteadOfDelete { get; set; }

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			ContinueWithDelete result = ContinueWithDelete.No;

			if (VoidInsteadOfDelete)
			{
				BusinessEntity.RunPreSaveValidation();

				var complianceDocumentHeader = BusinessEntity as AccComplianceDocumentHeader;

				if (BusinessEntity.HasErrors())
				{
					using (var form = new ZErrorMessageBox(BusinessEntity, Res.GetString("96EFBE16-C989-4F2B-97E8-F454CD95E0DD", "compliance document"), Res.GetString("647CAEC1-9C0E-4DD9-B802-F161B79590B8", "void"), Res.GetString("51AC6F62-031D-4727-941F-544349C9F494", "voided")))
					{
						ZFormModaliser.ShowMessageBoxWithoutDispose(form);
					}
				}
				else if ((complianceDocumentHeader?.ADH_DocumentStatus ?? ZString.Empty) == Core.Constants.ComplianceDocumentStatus.Finalised && AccountingMasterFilesRegistry.Instance.EnableFinalisedComplianceDocumentToBeSpecialVoided.Value && !complianceDocumentHeader.IsSpecialVoiding)
				{
					var errorMessage = Res.GetString("9DEFCA48-898A-49CA-B8E4-D37B2FB52CC1", "This compliance document record has been finalized, it can only be voided via 'Special Voiding'. Please take the 'Special Voiding' check box, state the voiding reason, and provide the approval number.");
					Globals.Message.ShowError(errorMessage, ResString.GetMultilingualString("72749E95-1596-4A4A-B583-12B3FAFFAAF4", "Unable to void this compliance document"));
				}
				else
				{
					var confirmationMessage = Res.GetString("5001f0b6-cd9b-4bb0-a53d-376dc23cf97a", "The compliance document will be voided. Are you sure to proceed?");
					if (Globals.Message.Show(confirmationMessage, voidComplianceDocumentCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
					{
						var message = complianceDocumentHeader == null ? ZString.Empty : complianceDocumentHeader.CheckCanVoid();
						if (!string.IsNullOrEmpty(message))
						{
							Globals.Message.Show(message);
							result = ContinueWithDelete.No;
						}
						else
						{
							result = ContinueWithDelete.Yes;
						}
					}
					else
					{
						result = ContinueWithDelete.No;
					}
				}
			}
			else
			{
				result = base.ShowPreDeleteDialogs();
			}

			return result;
		}

		protected override void Delete()
		{
			if (VoidInsteadOfDelete)
			{
				ComplianceDocumentHeader.Void();
				SaveInternal();
				var message = Res.GetString("daf7da10-86d4-4936-8a49-3b8938456df5", "Successfully void compliance document.");
				Globals.Message.Show(message, voidComplianceDocumentCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				base.Delete();
			}
		}

		#region Event Handlers

		#region E-Reporting Requeuing

		void ResetStatusToQueued_Click(object sender, EventArgs e)
		{
			var pivotStatusesEligibleForRequeuing = new List<ZString>() { EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Failed };
			var requeueFactory = new BusinessObjectFactory();
			var complianceDocumentPivots = requeueFactory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, ComplianceDocumentHeader.PK).AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccComplianceDocumentHeaderSchema.Constants.Prefix));
			if (complianceDocumentPivots != null
				&& (pivotStatusesEligibleForRequeuing.Contains(complianceDocumentPivots.AIP_Status)
					|| (complianceDocumentPivots.AIP_Status == EInvoicingPivotState.Batched
					&& complianceDocumentPivots.Batch?.AIB_Status.ToString() == EInvoicingBatchState.Discarded)))
			{
				ResetStatusToQueuedCore(complianceDocumentPivots);
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("BCFEC7A0-216C-4114-A37A-4A8073B429E6", "You can only reset compliance documents where E-Reporting pivot status is 'FAL' - Fail or 'BER' - Batched with errors or 'BCH' - Batched and batch status is 'DCD' - Discarded."));
			}
		}

		#endregion

		protected virtual void ResetStatusToQueuedCore(AccEInvoicingTransactionPivot complianceDocumentPivot)
		{
			try
			{
				complianceDocumentPivot.Requeue();
				complianceDocumentPivot.Factory.Save();
				Globals.Message.ShowInformation(Res.GetString("263A670A-9B33-4847-8ACA-E39558C018C4", "Compliance Document was successfully reset."));
			}
			catch (ZSaveConcurrencyException)
			{
				Globals.Message.ShowError(Res.GetString("23EF8423-413A-4C96-8CFD-A0E289625DB4", "While you were working, another user has modified this compliance document. Please try again."));
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region TabPage Initialized

		void RelatedInvoicesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new ZOrganisationFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZDateEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			this.RelatedInvoicesGrid = new ZGrid();
			this.RelatedInvoicesTabPage.SuspendLayout();
			((ISupportInitialize)(this.RelatedInvoicesGrid)).BeginInit();
			this.RelatedInvoicesGrid.SuspendLayout();
			this.RelatedInvoicesTabPage.Controls.Add(this.RelatedInvoicesGrid);
			// 
			// RelatedInvoicesGrid
			// 
			this.RelatedInvoicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RelatedInvoicesGrid, "TransactionHeaders");
			this.RelatedInvoicesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "AH_Ledger";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7BDBABAE-C70B-4DC8-96E1-63BE0FAFAD41", "Transaction Number");
			zTextBoxColumnStyleInfo3.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DD4374E1-37E9-4336-B047-E484CC182758", "Job");
			zTextBoxColumnStyleInfo4.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "AH_DueDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo4.ColumnName = "AH_FullyPaidDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AH_RX_NKTransactionCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4CEE08F1-9B3E-46CC-AF30-431D956740D3", "Trans. Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "AH_OSTotal";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3B012662-48D5-4476-9FE1-F26CB682A138", "Local Tax Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "AH_GSTAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RelatedInvoicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedInvoicesGrid.GridId = "6BF107F1-0E43-4DC7-832E-9BB4E34464D7";
			this.RelatedInvoicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedInvoicesGrid.LayoutKey = "RelatedInvoicesGrid";
			this.RelatedInvoicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RelatedInvoicesGrid.Name = "RelatedInvoicesGrid";
			this.RelatedInvoicesGrid.ReadOnly = true;
			this.RelatedInvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 482, true);
			this.RelatedInvoicesGrid.TabIndex = 0;
			this.RelatedInvoicesTabPage.PerformLayout();
			((ISupportInitialize)(this.RelatedInvoicesGrid)).EndInit();
			this.RelatedInvoicesGrid.ResumeLayout(false);
			this.RelatedInvoicesGrid.PerformLayout();
			this.RelatedInvoicesTabPage.ResumeLayout(true);
		}

		void ComplianceDocumentTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ComplianceDocumentUserControl = new ComplianceDocumentUserControl();
			this.useControlPanel = new ZPanel();
			this.ComplianceDocumentTabPage.SuspendLayout();
			this.ComplianceDocumentUserControl.SuspendLayout();
			this.useControlPanel.SuspendLayout();
			this.ComplianceDocumentTabPage.Controls.Add(this.useControlPanel);
			// 
			// ComplianceDocumentUserControl
			// 
			this.ComplianceDocumentUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComplianceDocumentUserControl, ".");
			this.ComplianceDocumentUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplianceDocumentUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComplianceDocumentUserControl.Name = "ComplianceDocumentUserControl";
			this.ComplianceDocumentUserControl.ReadOnly = false;
			this.ComplianceDocumentUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 488, true);
			this.ComplianceDocumentUserControl.TabIndex = 0;
			// 
			// useControlPanel
			// 
			this.useControlPanel.Controls.Add(this.ComplianceDocumentUserControl);
			this.useControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.useControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.useControlPanel.Name = "useControlPanel";
			this.useControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 488, true);
			this.useControlPanel.TabIndex = 1;
			this.ComplianceDocumentTabPage.PerformLayout();
			this.ComplianceDocumentUserControl.ResumeLayout(true);
			this.ComplianceDocumentUserControl.PerformLayout();
			this.useControlPanel.ResumeLayout(false);
			this.useControlPanel.PerformLayout();
			this.ComplianceDocumentTabPage.ResumeLayout(true);
		}

		void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(true);
		}

		#endregion
	}
}

