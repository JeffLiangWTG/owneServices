using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APPaymentBatchPostingController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var poster = businessEntity as APPaymentBatchPoster;
			poster.LoadPayments();
			return new PaymentBatchForm(poster);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.APPaymentProcessingNew; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.APPaymentBatchPosting; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APPaymentBatchPoster); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewPaymentBatch; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.EditPaymentBatch; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return APPaymentBatchPoster.Create(Factory, groupByInvoicePaymentCriticality, groupByInvoiceRelatedDebtorOrganisation, groupByUser);
		}

		public override IZForm ShowEditForm(BusinessObject businessEntity)
		{
			if ((businessEntity as APPaymentBatchPoster).APB_Status != Core.Constants.AccPaymentBatchStatus.Working)
			{
				return base.ShowViewForm(businessEntity);
			}
			else
			{
				return base.ShowEditForm(businessEntity);
			}
		}

		public override IZForm ShowNewForm()
		{
			bool okToProceed = true;

			if (AccountingConfigurationRegistry.Instance.PayInvoicesAllowFurtherGrouping.Value)
			{
				using (PaymentBatchGroupingForm groupingForm = new PaymentBatchGroupingForm())
				{
					okToProceed = ZFormModaliser.ShowDialogWithoutDispose(groupingForm) == DialogResult.OK;
					groupByInvoicePaymentCriticality = groupingForm.GroupByInvoicePaymentCriticality;
					groupByInvoiceRelatedDebtorOrganisation = groupingForm.GroupByInvoiceRelatedDebtorOrganisation;
					groupByUser = groupingForm.GroupByUser;
				}
			}
			else
			{
				groupByInvoicePaymentCriticality = AccountingConfigurationRegistry.Instance.PayInvoicesDefaultGroupByInvoicePaymentPaymentRequisitionStatus.Value;
				groupByInvoiceRelatedDebtorOrganisation = AccountingConfigurationRegistry.Instance.PayInvoicesDefaultGroupByInvoiceRelatedDebtorOrganisation.Value;
				groupByUser = AccountingConfigurationRegistry.Instance.PayInvoicesDefaultGroupByInvoiceCreatingUser.Value;
			}

			return okToProceed ? base.ShowNewForm() : null;
		}

		bool groupByInvoicePaymentCriticality;
		bool groupByInvoiceRelatedDebtorOrganisation;
		bool groupByUser;
	}
}
