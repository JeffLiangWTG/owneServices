using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class CommissionFinalizerForm : ZChildForm
	{
		#region Constructors

		[Obsolete("Use the constructor that takes a filter biz obj, this constructor is just for the designer", true)]
		public CommissionFinalizerForm()
		{
		}

		public CommissionFinalizerForm(CommissionFinalizer finalizer)
			: base(finalizer)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
			}

			AddFilterControl();
			SetupButtons();
		}

		#endregion

		#region Initialize

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#endregion

		#region BusinessEntity

		new CommissionFinalizer BusinessEntity
		{
			get { return (CommissionFinalizer)base.BusinessEntity; }
		}

		#endregion

		#region Form Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Filter

		protected CommissionFinalizerFilterControl FilterControl;
		CommissionFinalizerResultCountMessage resultCountMessage;

		void AddFilterControl()
		{
			FilterControl = new CommissionFinalizerFilterControl(BusinessEntity, new CommissionFinalizerFilterBusinessObject()) { Dock = DockStyle.Fill };
			filtersGroupBox.Controls.Add(FilterControl);

			FilterControl.PerformSearch += FilterControl_PerformSearch;
			resultCountMessage = new CommissionFinalizerResultCountMessage(FilterControl);
		}

		void FilterControl_PerformSearch(object sender, EventArgs e)
		{
			BusinessEntity.Find(FilterControl.FilterBusinessObject);
			resultCountMessage.UpdateResultCountMessage(BusinessEntity.CommissionFinalizerLineItemCollection.Count);

			if (BusinessEntity.CommissionFinalizerLineItemCollection.Count > OrganisationsDataRegistry.Instance.CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids.Value)
			{
				BusinessEntity.CommissionFinalizerLineItemCollection.RemoveAndDeleteAll();
				return;
			}

			BusinessEntity.CommissionFinalizerLineItemGroupingCollection.AddAmountNotifications(NotificationType.Error, true);
			BusinessEntity.CommissionFinalizerLineItemGroupingCollection.AddFullPaymentNotifications(NotificationType.Warning, true);
		}

		#endregion

		#region Buttons

		void SetupButtons()
		{
			if (OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.Value > 0)
			{
				RequestApprovalButton.Visible = true;
			}
			else
			{
				RequestApprovalButton.Visible = false;
			}
		}

		void RequestApprovalButton_Click(object sender, EventArgs e)
		{
			if (!BusinessEntity.CommissionFinalizerLineItems.Any(x => x.IsSelected))
			{
				Globals.Message.ShowError(NoSelectedCommissionLinesErrorMessage, CannotRequestApprovalCaption);
				DialogResult = DialogResult.None;
				return;
			}

			if (BusinessEntity.CommissionFinalizerLineItems.Where(x => x.IsSelected).Any(p => p.Validation.ShouldStopApprovalRequest))
			{
				Globals.Message.ShowError(SelectedCommissionLinesHaveProblemsErrorMessage, CannotRequestApprovalCaption);
				DialogResult = DialogResult.None;
				return;
			}

			var approvalRequest = BusinessEntity.GetNewApprovalRequest(new BusinessObjectFactory());
			var requestApprovalForm = new RequestApprovalForm(approvalRequest);
			var dialogResult = ZFormModaliser.ShowDialogAndDispose(requestApprovalForm);

			DialogResult = dialogResult;
			if (dialogResult == DialogResult.OK)
			{
				Close();
			}
		}

		void ProcessPaymentButton_Click(object sender, EventArgs e)
		{
			var paymentController = new CommissionFinalizerPaymentController(this, BusinessEntity);
			var processed = paymentController.PromptUserForProcessPayment();
			if (!processed)
			{
				DialogResult = DialogResult.None;
			}
		}

		void FormCancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Messages

		static string NoSelectedCommissionLinesErrorMessage
		{
			get { return Res.GetString("ae15c922-c429-4a61-847b-04f793f30927", "No entity commissions were selected for approval."); }
		}

		static string SelectedCommissionLinesHaveProblemsErrorMessage
		{
			get { return Res.GetString("9a83eb1f-4820-4112-bfdd-563be3133e47", "Selected entity commission(s) have issues that must be fixed before they can be approved. Please reference the warning(s) for additional information."); }
		}

		static string CannotRequestApprovalCaption
		{
			get { return Res.GetString("b25c3517-7ade-47bc-acf9-c4783daaea38", "Cannot Request Approval"); }
		}

		#endregion
	}
}
