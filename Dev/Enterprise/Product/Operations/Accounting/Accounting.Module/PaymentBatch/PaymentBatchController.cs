using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class PaymentBatchController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ModuleIDs.PaymentBatch;

		public override Type TypeOfTopLevelBusinessObject => typeof(APPaymentBatchPoster);

		public override ControllerID ID => ControllerIDs.PaymentBatch;

		protected override bool DisallowMultiDeleteBecauseDeleteIsNotWhatIsReallyHappeningInAccounting => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var poster = businessEntity as APPaymentBatchPoster;
			poster.LoadPayments();
			return new PaymentBatchForm(poster);
		}

		public override IZForm ShowEditForm(BusinessObject businessEntity)
		{
			if ((businessEntity as AccPaymentBatch).APB_Status != Core.Constants.AccPaymentBatchStatus.Working)
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
			return null;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			return null;
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			return null;
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewPaymentBatch; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.EditPaymentBatch; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}
	}
}
