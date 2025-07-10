using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.CommissionManagement.Business;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class CommissionApprovalRequestForm : ZChildForm
	{
		#region Constructors

		[Obsolete("Use the constructor that takes a filter biz obj, this constructor is just for the designer", true)]
		public CommissionApprovalRequestForm()
		{
		}

		public CommissionApprovalRequestForm(AccCommissionApprovalRequest approvalRequest)
			: base(approvalRequest)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
			}

			AddItemGrids();
		}

		#endregion

		#region Initialize

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			SetupApproveButtons();
			SetupProcessPaymentButton();

			ARInvoicePaymentValidation();
		}

		#endregion

		#region ARInvoiceValidation

		void ARInvoicePaymentValidation()
		{
			foreach (var commissionLine in ((ICommissionPayable)BusinessEntity).CommissionLinesForPayment)
			{
				commissionLine.Validation.ValidateFullyPaymentOfARInvoices();
			}
			BusinessEntity.CommissionApprovalRequestItemGroupingCollection.AddFullPaymentNotifications(NotificationType.Warning, true);
		}

		#endregion

		#region BusinessEntity

		new AccCommissionApprovalRequest BusinessEntity
		{
			get { return (AccCommissionApprovalRequest)base.BusinessEntity; }
		}

		#endregion

		#region Form Caption

		public override string FormCaption
		{
			get
			{
				var caption = base.FormCaption;
				if (BusinessEntity != null && !BusinessEntity.IsDeleted)
				{
					caption += " " + BusinessEntity.CRQ_BatchNumber;
				}

				return caption;
			}
		}

		#endregion

		#region Shown

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (BusinessEntity.Items.Count == 0)
			{
				Globals.Message.ShowError(
					Res.GetString("59520c8e-b534-496b-910b-5be6f25ad219", "This Commission Approval Request has been canceled. You will NOT be able to approve nor process this payment."),
					Res.GetString("03ddf7b9-8413-4df4-9685-c41143086c75", "Canceled Approval Request"));

				ApproveButton1.ReadOnly = true;
				ApproveButton2.ReadOnly = true;
				ProcessPaymentButton.ReadOnly = true;
			}
			else if (BusinessEntity.AddObsoleteErrors())
			{
				Globals.Message.ShowError(
					Res.GetString("805b5292-4816-4228-986a-3f8701d790af", "This Commission Approval Request contains entity commissions that have already been paid or canceled. You will NOT be able to approve nor process this payment."),
					Res.GetString("aba07ad1-98f5-40b3-a531-474a7407dcda", "Obsolete Approval Request"));

				ApproveButton1.ReadOnly = true;
				ApproveButton2.ReadOnly = true;
				ProcessPaymentButton.ReadOnly = true;
			}
			else if (BusinessEntity.AmendmentMadeToTransactionAfterApprovalRequest())
			{
				Globals.Message.ShowError(
					Res.GetString("474cc56f-b7c5-4a98-9915-29fd66897cab", "An amendment to a transaction was made after this Commission Approval Request. You will NOT be able to approve nor process this payment."),
					Res.GetString("aba07ad1-98f5-40b3-a531-474a7407dcda", "Obsolete Approval Request"));

				ApproveButton1.ReadOnly = true;
				ApproveButton2.ReadOnly = true;
				ProcessPaymentButton.ReadOnly = true;
			}
		}

		#endregion

		#region ItemGrids

		void AddItemGrids()
		{
			ZUserControl itemGridsControl = (OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value) ? new CommissionApprovalRequestItemGridsControl() :
				new GlobalCommissionApprovalRequestItemGridsControl();

			this.BindingSource.SetBindingMember(itemGridsControl, ".");
			itemGridsControl.Dock = DockStyle.Fill;
			ItemGridsPanel.Controls.Add(itemGridsControl);
		}

		#endregion

		#region Approve Buttons

		void SetupApproveButtons()
		{
			RefreshApproveButton1ReadOnlyValue();
			RefreshApproveButton2ReadOnlyValue();

			if (DisplayMode != ODisplayMode.ReadOnly)
			{
				BusinessEntity.CRQ_Staff1HasApprovedInfo.ValueChanged += CRQ_Staff1HasApprovedInfo_ValueChanged;
				BusinessEntity.CRQ_Staff2HasApprovedInfo.ValueChanged += CRQ_Staff2HasApprovedInfo_ValueChanged;
			}
		}

		void CRQ_Staff1HasApprovedInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshApproveButton1ReadOnlyValue();
		}

		void RefreshApproveButton1ReadOnlyValue()
		{
			ApproveButton1.ReadOnly =
				DisplayMode == ODisplayMode.ReadOnly ||
				BusinessEntity.CRQ_GS_NKApprovingStaff1.IsEmpty ||
				BusinessEntity.CRQ_Staff1HasApproved ||
				!Env.Security.CommissionAuthorizationLevel1.IsAllowed;
		}

		void CRQ_Staff2HasApprovedInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshApproveButton2ReadOnlyValue();
		}

		void RefreshApproveButton2ReadOnlyValue()
		{
			ApproveButton2.ReadOnly =
				DisplayMode == ODisplayMode.ReadOnly ||
				BusinessEntity.CRQ_GS_NKApprovingStaff2.IsEmpty ||
				BusinessEntity.CRQ_Staff2HasApproved ||
				!Env.Security.CommissionAuthorizationLevel2.IsAllowed;
		}

		void ApproveButton1_Click(object sender, EventArgs e)
		{
			if (!Env.Security.CommissionAuthorizationLevel1.IsAllowed)
			{
				Env.Security.CommissionAuthorizationLevel1.ShowError();
			}
			else if (!BusinessEntity.SelectedItems.Any())
			{
				Globals.Message.ShowError(NoItemsSelectedErrorMessage, CannotApproveCaption);
			}
			else
			{
				BusinessEntity.ApproveStaff1();
				try
				{
					BusinessEntity.Factory.Save();
					ARInvoicePaymentValidation();
				}
				catch (ZSaveException ex)
				{
					HandleSaveException(ex);
				}
			}
		}

		void ApproveButton2_Click(object sender, EventArgs e)
		{
			if (!Env.Security.CommissionAuthorizationLevel2.IsAllowed)
			{
				Env.Security.CommissionAuthorizationLevel2.ShowError();
			}
			else if (!BusinessEntity.SelectedItems.Any())
			{
				Globals.Message.ShowError(NoItemsSelectedErrorMessage, CannotApproveCaption);
			}
			else
			{
				BusinessEntity.ApproveStaff2();
				try
				{
					BusinessEntity.Factory.Save();
					ARInvoicePaymentValidation();
				}
				catch (ZSaveException ex)
				{
					HandleSaveException(ex);
				}
			}
		}

		#endregion

		#region Process Payment Button

		void SetupProcessPaymentButton()
		{
			if (DisplayMode == ODisplayMode.ReadOnly)
			{
				ProcessPaymentButton.ReadOnly = true;
			}
		}

		void ProcessPaymentButton_Click(object sender, EventArgs e)
		{
			var paymentController = new CommissionApprovalRequestPaymentController(this, BusinessEntity);
			var processed = paymentController.PromptUserForProcessPayment();
			if (!processed)
			{
				DialogResult = DialogResult.None;
			}
		}

		#endregion

		#region Close Button

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.CRQ_Staff1HasApprovedInfo.ValueChanged -= CRQ_Staff1HasApprovedInfo_ValueChanged;
				BusinessEntity.CRQ_Staff2HasApprovedInfo.ValueChanged -= CRQ_Staff2HasApprovedInfo_ValueChanged;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Messages

		static string NoItemsSelectedErrorMessage
		{
			get { return Res.GetString("e64f85e6-ee28-43ab-b477-852087558247", "Please select at least one entity commission to approve."); }
		}

		static string CannotApproveCaption
		{
			get { return Res.GetString("2a2e78af-9dc2-445e-8384-256d2c881185", "Cannot Approve"); }
		}

		#endregion
	}
}
