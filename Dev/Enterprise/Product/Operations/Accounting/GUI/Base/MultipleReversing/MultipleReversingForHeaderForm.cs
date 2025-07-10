using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI.Base
{
	public partial class MultipleReversingForHeaderForm : MultipleReversingBaseForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public MultipleReversingForHeaderForm()
		{
			InitializeComponent();
		}

		public MultipleReversingForHeaderForm(MultipleReversingProviderForHeader multipleReversingProvider)
			: base(multipleReversingProvider)
		{
			InitializeComponent();
			SetupGridForAmendStatusCode();
		}

		protected override ResourceStringData RemoveErrorTransactionsButtonCaption => Res.GetData("873458b8-b993-4ed1-a86d-68bbe603f2b4", "Remove Original Transactions That Can\'t Be Reversed");

		protected override void OnClosing(CancelEventArgs e)
		{
			if (MultipleReversingProvider.ShouldSaveApprovalFactory && MultipleReversingProvider.ApprovalFactory != null)
			{
				bool isThereNothingToPost = MultipleReversingProvider.TransactionsAlreadyReversed.Count == 0;
				bool allTransactionsHasRowErrors = !isThereNothingToPost && MultipleReversingProvider.TransactionsAlreadyReversed.All(x => ((BusinessObject)((IReversingImplicitlyImplementedWrapperForBinding)x).WrappedBusinessEntity).HasRowErrors);
				if (isThereNothingToPost || allTransactionsHasRowErrors)
				{
					var messageText = Res.GetString("7d3a7904-97a8-42d3-bdfa-1e30c45d283d", @"You have pending credit note approval requests that will be lost if you cancel. 

Click OK to queue the approval requests.
Click CANCEL to cancel this form.");
					var messageCaption = Res.GetString("709ee08c-a377-4a24-b6bb-0ce33febb3bf", "Pending Credit Note Approval Requests");
					var result = Globals.Message.Show(messageText, messageCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
					if (result == DialogResult.OK)
					{
						try
						{
							MultipleReversingProvider.ApprovalFactory.Save();
						}
						catch (ZSaveConcurrencyException)
						{
							Globals.Message.ShowError(Res.GetString("b05fc3ba-198f-4df5-a7e8-0e871b8bd11f", "While you were working, another user has modified these transactions. Please try again."));
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
				}
			}
		}

		MultipleReversingProviderForHeader MultipleReversingProvider => (MultipleReversingProviderForHeader)BusinessEntity;

		public override string FormCaption => Res.GetString("E8749A3A-0C6A-4188-B843-E3C5598BDA67", "Multiple Transactions");

		protected override void DeleteCore()
		{
			foreach (IReversingImplicitlyImplementedWrapperForBinding transactionWrapper in MultipleReversingProvider.TransactionsAlreadyReversed)
			{
				var wrappedObject = (BusinessObject)transactionWrapper.WrappedBusinessEntity;
				if (GetIsUATransaction(wrappedObject))
				{
					wrappedObject.Delete();
				}
			}
		}

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			ContinueWithDelete result = ContinueWithDelete.Yes;

			if (ReverseTransaction != null)
			{
				if (HasClosedJob && !AllowedReOpenClosedJob)
				{
					result = ContinueWithDelete.No;
				}

				var hasComplianceErrors = MultipleReversingProvider?.CheckHasComplianceSubTypeAndNumberingErrors();
				if (hasComplianceErrors.HasValue && hasComplianceErrors.Value)
				{
					result = ContinueWithDelete.No;
				}
			}

			if (result == ContinueWithDelete.Yes)
			{
				result = base.ShowPreDeleteDialogs();
				if (result == ContinueWithDelete.Yes && HasClosedJob)
				{
					result = ReOpenClosedJob() ? ContinueWithDelete.Yes : ContinueWithDelete.No;
				}
			}

#if DEBUG
			if (OnShowPreDeleteDialogs_ForTestOnly != null && Globals.IsTest)
			{
				var args = new DeleteDialogEventArgs() { Result = result };
				OnShowPreDeleteDialogs_ForTestOnly(this, args);
				result = args.Result;
				if (args.ReturnResult)
				{
					return result;
				}
			}
#endif

			if (result == ContinueWithDelete.Yes && MultipleReversingProvider != null)
			{
				MultipleReversingProvider.RunPreSaveValidation();
				result = MultipleReversingProvider.HasErrors ? ContinueWithDelete.No : ContinueWithDelete.Yes;
			}

			return result;
		}

		void SetupGridForAmendStatusCode()
		{
			var amendStatusCodeInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.Country.Code) as IInstanceProvider<IAmendStatusCodeProvider>;
			var amendStatusCodeProvider = amendStatusCodeInstanceProvider?.Get();
			if (amendStatusCodeProvider == null || !amendStatusCodeProvider.ShouldShowAmendStatusCode())
			{
				Grid.RemoveFromAvailableColumns("AmendStatusCode");
			}
		}

#if DEBUG
		internal class DeleteDialogEventArgs : EventArgs
		{
			public ContinueWithDelete Result { get; set; }
			public bool ReturnResult { get; set; }
		}

		internal event EventHandler<DeleteDialogEventArgs> OnShowPreDeleteDialogs_ForTestOnly;
#endif

		#region ReOpen Closed Job

		bool HasClosedJob => MultipleReversingProvider?.HasClosedJob ?? false;

		bool AllowedReOpenClosedJob => JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(MultipleReversingProvider, MultipleReversingProvider.GetAllJobs());

		bool ReOpenClosedJob() => MultipleReversingProvider?.ReOpenClosedJob() ?? false;

		#endregion
	}
}
