using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class PaymentReceiptUniversalBatchProcessor : TxnHeaderProcessorBase
	{
		public PaymentReceiptUniversalBatchProcessor(BusinessObjectFactory factory, INotificationManager notification) : base(factory, notification)
		{
		}

		AccTransactionHeaderReference HeaderReference;

		protected override void SetupContext()
		{
			if (CurrentFactory == null)
			{
				CurrentFactory = new BusinessObjectFactory();
				CurrentFactory.SetContext(BusinessContext.UniversalTransactionBatchImport);
				CurrentFactory.Saved += new BusinessObjectFactory.SavedEventHandler(RemoveContext);
				FactoryToCollectChildFactories.SaveInTransactionActions.Add(SaveInTransactionAction);
			}

			HeaderReference = null;
		}

		void RemoveContext(BusinessObjectFactory factory, bool isSavedSuccessful)
		{
			if (isSavedSuccessful)
			{
				factory.RemoveContext(BusinessContext.UniversalTransactionBatchImport);
				factory.Saved -= new BusinessObjectFactory.SavedEventHandler(RemoveContext);
			}
		}

		protected override void RemoveCurrentFactory()
		{
			FactoryToCollectChildFactories.SaveInTransactionActions.Remove(SaveInTransactionAction);
		}

		SaveInTransactionActionImportPaymentReceipt SaveInTransactionAction => saveInTransactionAction ?? (saveInTransactionAction = new SaveInTransactionActionImportPaymentReceipt(CurrentFactory, () => { CurrentFactory.Save(); return ChangedTableNames.All; }, null));
		SaveInTransactionActionImportPaymentReceipt saveInTransactionAction;

		protected override bool CanContinueProcess => !(NotificationManager.ErrorsHaveBeenReported || NotificationBuffer.HasErrors);

		protected override void SetUpTransactionAndValidate()
		{
			using (((IBusinessObjectInternals)TransactionHeader).ResumeValidationForAllDescendantsTemporarily())
			{
				base.SetUpTransactionAndValidate();
			}
		}

		protected override void MatchAndClearTransactions(MatchingBase matching)
		{
			matching.DoNotSaveFactoryOnMatching = true;
			base.MatchAndClearTransactions(matching);
		}

		protected override bool IsNeedSaveWithCurrentFactory => false;

		protected override void PaymentApprovalAllocateCheckNumberAndSave(PaymentApprovalBase paymentApproval, bool isAutoAllocationEnabled)
		{
			if (isAutoAllocationEnabled)
			{
				new PaymentChequeNumberAllocatorBase(paymentApproval, paymentApproval.Factory);
			}

			if (paymentApproval.PaymentMatchingBaseObject != null)
			{
				paymentApproval.PaymentMatchingBaseObject.DeleteTemporaryTransactions(!paymentApproval.HasChanges);
			}
			paymentApproval.CreateNewPayment();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected override void ValidateAndSetUpTransactionHeaderReference()
		{
			if (!TxnHeader.ThirdPartyReference.IsEmpty && TransactionHeader.AH_OH.IsValid)
			{
				var sqlText = @"SELECT count(0)
								FROM dbo.AccTransactionHeader
								INNER JOIN dbo.AccTransactionHeaderReference ON AH_PK = AH1_AH
								WHERE AH_OH = @OrgHeader
									AND AH_Ledger = @Ledger
									AND AH_TransactionType = @TransactionType
									AND AH_GC = @Company
									AND AH1_Type = @ReferenceType
									AND AH1_Reference = @Reference"; // Direct access to database required

				using (var command = Db.Connection.Command(sqlText)) // Not using the BusinessObjectFactory since hitting the DB directly.
				{
					command.AddParameter("@OrgHeader", System.Data.SqlDbType.UniqueIdentifier, TransactionHeader.AH_OH.ToGuid());
					command.AddParameter("@Ledger", System.Data.SqlDbType.Char, TransactionHeader.AH_Ledger.ToString());
					command.AddParameter("@TransactionType", System.Data.SqlDbType.Char, TransactionHeader.AH_TransactionType.ToString());
					command.AddParameter("@Company", System.Data.SqlDbType.UniqueIdentifier, TransactionHeader.AH_GC.ToGuid());
					command.AddParameter("@ReferenceType", System.Data.SqlDbType.Char, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.OTI);
					command.AddParameter("@Reference", System.Data.SqlDbType.NVarChar, TxnHeader.ThirdPartyReference.ToString());

					if (Convert.ToInt32(command.ExecuteScalar()) > 0)
					{
						NotificationManager.AddErrorToNotifications(Res.GetString("fc3d5c83-f976-4a1a-9853-56da29c7e486", "Transaction ID {0} has already been posted in the system with the same organization, ledger and transaction type.", TxnHeader.ThirdPartyReference));
					}
					else
					{
						HeaderReference = CurrentFactory.New<AccTransactionHeaderReference>();
						HeaderReference.AH1_AH = TransactionHeader.PK;
						HeaderReference.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.OTI;
						HeaderReference.AH1_Reference = TxnHeader.ThirdPartyReference;
					}
				}
			}
		}

		protected override void SetNewPaymentToTransactionHeaderAndUpdateHeaderReference(PaymentApprovalBase paymentApproval)
		{
			base.SetNewPaymentToTransactionHeaderAndUpdateHeaderReference(paymentApproval);

			if (HeaderReference != null)
			{
				HeaderReference.AH1_AH = TransactionHeader.PK;
			}
		}
	}

	class SaveInTransactionActionImportPaymentReceipt : SaveInTransactionWithRollBackAction
	{
		public SaveInTransactionActionImportPaymentReceipt(IDbConnected connection, Func<ChangedTableNames> methodOnSaving, Action methodOnRollback) : base(connection, methodOnSaving, methodOnRollback)
		{
		}

		protected override bool AllowTransactionWithOtherParticipant => true;
	}
}
