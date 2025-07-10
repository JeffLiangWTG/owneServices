using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public class DirectDebitBatchHeaderValidation : TransactionHeaderValidation
	{
		public static string GetInvalidBSBNumberNonASBErrorMsg(ZString companyCode)
		{
			return Res.GetString("80115e84-a9ae-47c8-ac8f-1b9ba562041c", "The Direct Debit Bank Account BSB Number (in Organizations Master Files) for {0} must have the pattern XXX-XXX. To correct the error, select the transaction, right click your mouse and select “Edit Payment Organization Detail” to correct the BSB Number.", companyCode);
		}

		public static string GetInvalidBSBNumberNonASBDirectPaymentErrorMsg(ZString paymentNumber)
		{
			return Res.GetString("f84cf1be-c090-4466-ac4c-538b27234b50", "The Direct Debit Bank Account BSB Number setup in Direct Payment Number {0} must have the pattern XXX-XXX. \r\nReverse the transaction and re-enter it with a BSB with the following pattern: XXX-XXX", paymentNumber);
		}

		public static string GetInvalidBSBNumberASBErrorMsg(ZString companyCode)
		{
			return Res.GetString("28b0d078-e0b2-494d-88c3-dfe9b40d3a38", "The Direct Debit Bank Account BSB Number (in Organizations Master Files) for {0} must have the pattern XXXXXX. To correct the error, select the transaction, right click your mouse and select “Edit Payment Organization Detail” to correct the BSB Number.", companyCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used in exception")]
		public const string InvalidBSBNumberASBDirectPaymentErrorMsg = "The Direct Debit Bank Account BSB Number setup in Direct Payment"
			+ " Number {0} must have the pattern XXXXXX. \r\nReverse the transaction and re-enter it with a BSB with the following"
			+ " pattern: XXXXXX";

		protected readonly new DirectDebitBatchHeader Parent;

		public DirectDebitBatchHeaderValidation(DirectDebitBatchHeader parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		#region AH_AB

		protected override void CheckAH_AB()
		{
			base.CheckAH_AB();
			MandatoryValidation.CheckEntered(Parent.AH_ABInfo);
		}

		#endregion

		#region AH_OSExTaxAmount

		protected override void CheckAH_OSExTaxAmount()
		{
			base.CheckAH_OSExTaxAmount();
			if (Parent.AH_OSExTaxAmount == 0m && !Parent.AH_IsCancelled)
			{
				Parent.AH_OSExTaxAmountInfo.AddError(Res.GetString("27fcb655-66ee-40fe-b1df-1a6170c636c3", "DDR Batch amount cannot be 0. Please select payments in order to generate DDR Batch"));
			}
		}

		#endregion

		protected override void CheckAH_PostDate()
		{
			base.CheckAH_PostDate();

			ZDateTime latestTransactionPostDate = DateTime.MinValue;
			foreach (IDirectDebitBatchTransaction line in Parent.Lines)
			{
				if (line.IncludeInTheBatch && line.AH_PostDate.CompareTo(latestTransactionPostDate) > 0)
				{
					latestTransactionPostDate = line.AH_PostDate;
				}
			}

			if (Parent.AH_PostDate.Date.CompareTo(latestTransactionPostDate.Date) < 0)
			{
				Parent.AH_PostDateInfo.AddError(Res.GetString("65e4125c-3bbf-44bf-af2b-70d9beb5610b", "Post Date must be equal to or later than each transaction's Post Date."));
			}
		}

		protected override void CheckAH_PostDateNotInFuture()
		{
			if (!Parent.AH_PostDateInfo.HasErrors())
			{
				if (Parent.AH_PostDate.Date > ZDateTime.Today)
				{
					if (!AccountingUtils.IsAllowFuturePostingRegistryEnabled)
					{
						Parent.AH_PostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
					}
					else if (!AccountingUtils.DoesUserHaveFuturePostingSecurity)
					{
						Parent.AH_PostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);
					}
				}
			}
		}

		protected override void CheckAH_PostDateNotInPast()
		{
			if (!Parent.AH_PostDateInfo.HasErrors())
			{
				if (!Parent.IsReverseTransaction && Parent.AH_PostDate.Date < ZDateTime.Today)
				{
					if (!Parent.AllowBackPosting)
					{
						Parent.AH_PostDateInfo.AddError(PreviousPostDateError);
					}
					else
					{
						if (!Parent.AH_PostDateInfo.HasErrors())
						{
							Parent.AH_PostDateInfo.AddWarning(PreviousPostDateWarning);
						}
					}
				}
			}
		}

		#region Other Helper Methods

		public void ValidateBeforePosting()
		{
			ValidateAH_OSExTaxAmount();
			ValidateAH_PostDate();
			ValidateAH_InvoiceDate();
			ValidateAH_ReceiptBatchNoForLines();

			if (Parent.Lines.Count > 0 && Parent.BankAccount != null)
			{
				if (Parent.BankAccount.AB_AllowAutoDDR)
				{
					ValidateBatchLines();
				}
			}
		}

		void ValidateAH_ReceiptBatchNoForLines()
		{
			foreach (IDirectDebitBatchTransaction transaction in Parent.Lines)
			{
				var transactionBO = transaction as AccTransactionHeader;
				transactionBO.RemoveRowError(TransactionAlreadyIncludedInAnotherBatch);

				if (transaction.IncludeInTheBatch && !transaction.AH_ReceiptBatchNo.IsEmpty)
				{
					transactionBO.AddRowError(TransactionAlreadyIncludedInAnotherBatch);
				}
			}
		}

		static string TransactionAlreadyIncludedInAnotherBatch
		{
			get { return Res.GetString("2C764255-FDF9-44DF-BFC3-2F3F5B3F0C67", "The transaction is already included in another Direct Debit Batch."); }
		}

		public void ValidateBatchLines()
		{
			foreach (IDirectDebitBatchTransaction line in Parent.Lines)
			{
				if (line is Payment)
				{
					Payment linePayment = (Payment)line;
					linePayment.ResetAccountDetails();
					if (!linePayment.AccountDetailsFound)
					{
						linePayment.Validation.ValidateAH_OH();
					}
					else
					{
						ValidateBankDetails(line);
					}
				}
				else
				{
					ValidateBankDetails(line);
				}
			}
		}

		void ValidateBankDetails(IDirectDebitBatchTransaction line)
		{
			line.ValidateBankAccountNumber();
			line.ValidateBankBSB();
			ValidateAutoDDRLine(line);
		}

		void ValidateAutoDDRLine(IDirectDebitBatchTransaction line)
		{
			if (!line.AllowAutoDDR)
			{
				line.ValidateAutoDDR();
			}
		}

		#region Implementation

		internal static ZString GetBankAccountNumberError(IDirectDebitBatchTransaction directTransaction)
		{
			ZString error = "";

			if (!directTransaction.BankAccount.IsValidAccountNumber(directTransaction.PayeeBankAccountNumber))
			{
				if (directTransaction.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.WNZ)
				{
					if (directTransaction.AH_Ledger != LedgerTypes.CashBook && directTransaction.Header != null)
					{
						error = Res.GetString("90b5b189-f48a-4c49-951a-e0857c6bc65f", @"The Direct Debit Bank Account Number (in Organizations Master Files) for {0} must be equal to or less than 12 characters in length.
						To correct the error, select the transaction, right click your mouse and select “Edit Payment Organization Detail” to correct the BSB Number.", directTransaction.Header.OH_Code.Trim());
					}
					else
					{
						error = Res.GetString("f82b581b-a983-4f49-bfbf-d295d170dbd9", "The Direct Debit Bank Account Number setup in Direct Payment Number {0} must be equal to or less than 12 characters in length.\r\nReverse the transaction and re-enter it with a Bank Account Number 12 characters long", directTransaction.AH_TransactionNum);
					}
				}
				else if (directTransaction.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.ASB || directTransaction.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.BNZ)
				{
					if (directTransaction.AH_Ledger != LedgerTypes.CashBook && directTransaction.Header != null)
					{
						error = Res.GetString("c496f9b0-baad-4ecb-a3a9-af3f706f53f9", @"The Direct Debit Bank Account Number (in Organizations Master Files) for {0} must be 9 or 10 characters in length.
						To correct the error, select the transaction, right click your mouse and select “Edit Payment Organization Detail” to correct the BSB Number.", directTransaction.Header.OH_Code.Trim());
					}
					else
					{
						error = Res.GetString("47e8fd28-30c2-40df-9db9-daf0cba86797", "The Direct Debit Bank Account Number setup in Direct Payment Number {0} must be 9 or 10 characters in length.\r\nReverse the transaction and re-enter it with a Bank Account Number 9 or 10 characters long", directTransaction.AH_TransactionNum);
					}
				}
				else if (directTransaction.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.BCS)
				{
					error = Res.GetString("2cb96c04-8564-48b1-9320-9828f614e023", "The account number format required for BACS DDR Files is 'XXXXXXXX'");
				}
				else
				{
					if (directTransaction.AH_Ledger != LedgerTypes.CashBook && directTransaction.Header != null)
					{
						if (directTransaction.PayeeBankAccountNumber.IsEmpty)
						{
							error = Res.GetString("befe1edf-708c-4a52-a590-6238588ec2de", @"The Direct Debit Bank Account Number (in Organizations Master Files) for {0} cannot be empty for the DDR File System.
							To correct the error, select the transaction, right click your mouse and select “Edit Payment Organization Detail” to correct the BSB Number.", directTransaction.Header.OH_Code.Trim());
						}
						else
						{
							error = Res.GetString("342b9fc6-4024-4dee-b49a-dc145d60c001", @"The Direct Debit Bank Account Number (in Organizations Master Files) for {0} is too long for the DDR File System.
							To correct the error, select the transaction, right click your mouse and select “Edit Payment Organization Detail” to correct the BSB Number.", directTransaction.Header.OH_Code.Trim());
						}
					}
					else
					{
						if (directTransaction.PayeeBankAccountNumber.IsEmpty)
						{
							error = Res.GetString("28eb1e08-bc1e-4b64-839c-603c4e3d2ac6", "The Direct Debit Bank Account Number setup in Direct Payment Number {0} cannot be empty for the DDR File System.\r\nReverse the transaction and re-enter it with a Bank Account Number of less than 9 characters", directTransaction.AH_TransactionNum);
						}
						else
						{
							error = Res.GetString("087c5837-bb06-42e2-aa03-9b41f186ef0c", "The Direct Debit Bank Account Number setup in Direct Payment Number {0} is too long for the DDR File System.\r\nReverse the transaction and re-enter it with a Bank Account Number of less than 9 characters", directTransaction.AH_TransactionNum);
						}
					}
				}
			}

			return error;
		}

		internal static ZString GetBankBSBNumberError(IDirectDebitBatchTransaction directTransaction)
		{
			ZString error = "";

			if (!directTransaction.BankAccount.IsValidBSBNumber(directTransaction.PayeeBankBSB))
			{
				if (directTransaction.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.ASB || directTransaction.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.WNZ
					|| directTransaction.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.BNZ
					|| (directTransaction.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.ANZ && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand))
				{
					if (directTransaction.AH_Ledger != LedgerTypes.CashBook && directTransaction.Header != null)
					{
						error = GetInvalidBSBNumberASBErrorMsg(directTransaction.Header.OH_Code.Trim());
					}
					else
					{
						error = ZString.Format(InvalidBSBNumberASBDirectPaymentErrorMsg, directTransaction.AH_TransactionNum);
					}
				}
				else if (directTransaction.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.BCS)
				{
					error = Res.GetString("4d36bfa4-564f-48b0-8497-3f6c999cb27a", "The sort code format required for BACS DDR Files is 'XXXXXX'");
				}
				else
				{
					if (directTransaction.AH_Ledger != LedgerTypes.CashBook && directTransaction.Header != null)
					{
						error = GetInvalidBSBNumberNonASBErrorMsg(directTransaction.Header.OH_Code.Trim());
					}
					else
					{
						error = GetInvalidBSBNumberNonASBDirectPaymentErrorMsg(directTransaction.AH_TransactionNum);
					}
				}
			}

			return error;
		}

		#endregion
		#endregion
	}
}
