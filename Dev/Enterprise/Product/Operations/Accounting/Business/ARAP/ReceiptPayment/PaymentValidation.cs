using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class PaymentValidation : ReceiptPaymentBaseValidation
	{
		public PaymentValidation(Payment parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		protected new Payment Parent;

		public void ValidateChequeBook()
		{
			ValidateCalculatedProperty(Parent.ChequeBookInfo);
		}

		protected virtual void CheckChequeBook()
		{
			if (Parent.AH_ReceiptType == ReceiptTypes.Cheque)
			{
				MandatoryValidation.CheckEntered(Parent.ChequeBookInfo);
				TypeValidation.CheckValidGuid(Parent.ChequeBookInfo);
			}
		}

		protected override void CheckAH_ChequeOrReference()
		{
			if (!((IChequeNumberAutoAllocation)Parent).IsAutoAllocationEnabled)
			{
				base.CheckAH_ChequeOrReference();
				if (!Parent.AH_ChequeOrReferenceInfo.HasErrors() && Parent.ChequeBook.IsValid &&
					Parent.AH_ReceiptType == ReceiptTypes.Cheque)
				{
					AccChequeBook loadedChequeBook = Parent.Factory.Load<AccChequeBook>(Parent.ChequeBook);
					if (loadedChequeBook != null)
					{
						string chequeNumberErrorMessage = ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(loadedChequeBook, Parent.AH_ChequeOrReference);
						if (!string.IsNullOrEmpty(chequeNumberErrorMessage))
						{
							Parent.AH_ChequeOrReferenceInfo.AddError(chequeNumberErrorMessage);
						}
						else
						{
							if (loadedChequeBook.BankAccount != null)
							{
								bool isChequeNumberInUse;

								if (Parent.RelatedPaymentApproval != null && Parent.RelatedPaymentApproval.NewPayment == Parent)
								{
									isChequeNumberInUse = loadedChequeBook.BankAccount.HasChequeNumberBeenUsedOnAPaymentApproval(Parent.AH_ChequeOrReference, new List<ZGuid> { Parent.RelatedPaymentApproval.PK });
								}
								else
								{
									isChequeNumberInUse = loadedChequeBook.BankAccount.HasChequeNumberBeenUsed(Parent.AH_ChequeOrReference, Parent.PK);
								}

								if (isChequeNumberInUse)
								{
									Parent.AH_ChequeOrReferenceInfo.AddError(ChequeOrReferenceValidationHelper.GetInUseErrorMessage(Parent.AH_ChequeOrReference, loadedChequeBook.BankAccount, Parent.Factory));
								}
							}

							if (!Parent.AH_ChequeOrReferenceInfo.HasErrors() && Parent.BankAccount != null)
							{
								ValidationHelper.ValidateChequeDigits(Parent.AH_ChequeOrReferenceInfo, Parent.BankAccount.AB_ChequeNumDigits);
							}
						}
					}
				}
			}
		}

		protected override void CheckAH_OSExTaxAmount()
		{
			MandatoryValidation.CheckEntered(Parent.AH_OSExTaxAmountInfo);
			if (Parent.AH_OSExTaxAmount < 0)
			{
				Parent.AH_OSExTaxAmountInfo.AddError(Res.GetString("49ee95e6-7cd1-4137-a32a-30f6ccdbc3f8", "Overseas amount must be greater than 0"));
			}
		}

		protected override void CheckAH_ReceiptType()
		{
			base.CheckAH_ReceiptType();
			MandatoryValidation.CheckEntered(Parent.AH_ReceiptTypeInfo, Res.GetString("1b0289a4-d92c-4282-96f6-ad3b08992a86", "Payment Type"));
			if (!Parent.IsInDatabase)
			{
				ListValidation.ErrorIfInvalidCode(Parent.AH_ReceiptTypeInfo, Parent.PaymentMethods);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.AH_ReceiptTypeInfo, PaymentMethods);
			}

			if (!Parent.AH_ReceiptTypeInfo.HasErrors())
			{
				CheckAH_ReceiptTypeSecurity();
			}

			ValidateAH_OH();
		}

		protected override void CheckEPaymentReceiptType()
		{
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CheckAH_ReceiptTypeSecurity()
		{
			if (Parent.RelatedPaymentApproval == null || Parent.RelatedPaymentApproval.IsInDatabase)
			{
				bool isAllowed = true;
				switch (Parent.AH_ReceiptType)
				{
					case ReceiptTypes.Cheque:
						isAllowed = (Parent is ARPayment) ? Env.Security.NewReceivablesPaymentCheque.IsAllowed : Env.Security.NewPayablesPaymentCheque.IsAllowed;
						break;

					case ReceiptTypes.Cash:
						isAllowed = (Parent is ARPayment) ? Env.Security.NewReceivablesPaymentCash.IsAllowed : Env.Security.NewPayablesPaymentCash.IsAllowed;
						break;

					case ReceiptTypes.CreditCard:
						isAllowed = (Parent is ARPayment) ? Env.Security.NewReceivablesPaymentCreditCard.IsAllowed : Env.Security.NewPayablesPaymentCreditCard.IsAllowed;
						break;

					case ReceiptTypes.DirectDebit:
						isAllowed = (Parent is ARPayment) ? Env.Security.NewReceivablesPaymentDirectDebit.IsAllowed : Env.Security.NewPayablesPaymentDirectDebit.IsAllowed;
						break;

					case ReceiptTypes.EFT:
						isAllowed = (Parent is ARPayment) ? Env.Security.NewReceivablesPaymentEFT.IsAllowed : Env.Security.NewPayablesPaymentEFT.IsAllowed;
						break;

					case ReceiptTypes.ScheduledEFT:
						isAllowed = (Parent is ARPayment) ? Env.Security.NewReceivablesPaymentSFT.IsAllowed : Env.Security.NewPayablesPaymentSFT.IsAllowed;
						break;

					case ReceiptTypes.CollectionRequest:
						isAllowed = (Parent is ARPayment) ? Env.Security.NewReceivablesPaymentCRQ.IsAllowed : Env.Security.NewPayablesPaymentCRQ.IsAllowed;
						break;
				}

				if (!isAllowed)
				{
					Parent.AH_ReceiptTypeInfo.AddError(Res.GetString("190de071-4016-49d3-b0e4-8914b28172f4", "You do not have appropriate security rights to select this payment type."));
				}
			}
		}

		protected override void CheckAH_OH()
		{
			base.CheckAH_OH();
			ValidateOrganisationDDRDetail();
		}

		protected override void CheckAH_AB()
		{
			base.CheckAH_AB();
			ListValidation.ErrorIfInvalidPK(Parent.AH_ABInfo, Parent.BankAccounts);
			ValidateAH_OH();
		}

		void ValidateOrganisationDDRDetail()
		{
			if (Parent.AH_ReceiptType == ReceiptTypes.DirectDebit)
			{
				if (Parent.BankAccount != null && Parent.BankAccount.AB_AllowAutoDDR)
				{
					if (Parent.Header != null)
					{
						GetOrganisationDDRWarning();
					}
				}
			}
		}

		void GetOrganisationDDRWarning()
		{
			var warningMessage = new StringBuilder();
			if (Parent.AccountDetailsFound)
			{
				if (!Parent.AllowAutoDDR)
				{
					warningMessage.Append("   ").Append(Res.GetString("9dffa77c-44f6-4da3-b988-4c6441646431", "- Auto Direct Debit")).Append(System.Environment.NewLine);
				}

				if (Parent.PayeeBankBSB.IsEmpty)
				{
					warningMessage.Append("   ").Append(Res.GetString("8ed30907-df81-43b5-a765-a030a6603468", "- BSB Number")).Append(System.Environment.NewLine);
				}
				else
				{
					ValidateBSBFormat(warningMessage);
				}

				if (Parent.AccountTitle.IsEmpty)
				{
					warningMessage.Append("   ").Append(Res.GetString("2e0a4396-1a67-45e4-836f-d6dd978037ab", "- Account Name")).Append(System.Environment.NewLine);
				}

				if (Parent.PayeeBankAccountNumber.IsEmpty)
				{
					warningMessage.Append("   ").Append(Res.GetString("808238f7-748a-449e-a11d-f704f858f694", "- Bank Account")).Append(System.Environment.NewLine);
				}

				if (warningMessage.Length > 0)
				{
					warningMessage.Insert(0, System.Environment.NewLine);
					warningMessage.Insert(0, Res.GetString("f257029c-a628-416f-867e-c5c0299d02ed", "Organization {0} has following Banking Details incorrectly setup.", Parent.Header.OH_Code));
					warningMessage.Append(Res.GetString("406b0d5d-2b28-4276-9e9e-5475fd6633dc", "You'll need to correct this before you can generate DDR file."));

					Parent.AH_OHInfo.AddWarning(warningMessage.ToString());
				}
			}
			else
			{
				ZString currencyCode = Parent.TransactionCurrency != null ? Parent.AH_RX_NKTransactionCurrency : GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				ZString errorMessage = Res.GetString("4a4c096a-3b07-4ac0-8aea-5a04da934392", "An AP Bank Account could not be found with currency {0} and payment type DDR for the payee {1}.\r\n\r\nPlease set up an AP Account for the organization {1} under the AP Details tab, by right-clicking this grid and selecting \"Edit Payment Organization Detail\", with the currency {0} and payment type of DDR.", currencyCode, Parent.Header.OH_Code);
				Parent.AH_OHInfo.AddError(errorMessage);
			}
		}

		void ValidateBSBFormat(StringBuilder warningMessage)
		{
			if (!Parent.BankAccount.IsValidBSBNumber(Parent.PayeeBankBSB))
			{
				if (Parent.BankAccount.AB_AutoDDRFormat != Constants.DDRFileFormat.ASB)
				{
					warningMessage.Append("   ").Append(Res.GetString("71211fcc-b039-48ec-a2a8-fda687ca8088", "- BSB Number is in incorrect format. It should be in XXX-XXX format")).Append("\r\n");
				}
				else
				{
					warningMessage.Append("   ").Append(Res.GetString("c168ef60-e147-4387-ab22-752fe0dfca16", "- BSB Number is in incorrect format. It should be in XXXXXX format")).Append("\r\n");
				}
			}
		}

		CodeDescriptionPairList fPaymentMethods;
		protected CodeDescriptionPairList PaymentMethods
		{
			get
			{
				if (fPaymentMethods == null)
				{
					fPaymentMethods = Parent.PaymentMethods;
					fPaymentMethods.AddPair(ReceiptTypes.DirectDebitLine, Res.GetString("dc6959e4-1fa6-4d6d-af28-48b9ff68a33a", "Direct Debit Line"));
				}
				return fPaymentMethods;
			}
		}

		#region DirectDebit Batch Validation Implementation

		public void ValidateAllowAutoDDR()
		{
			ValidateCalculatedProperty(Parent.AllowAutoDDRInfo);
		}

		protected void CheckAllowAutoDDR()
		{
			if (!Parent.AllowAutoDDR)
			{
				Parent.AllowAutoDDRInfo.AddError(Res.GetString("cb5f9322-ce33-4fc8-ad60-79173735687e", "You cannot generate DDR file for non-Auto DDR Payments"));
			}
		}

		public void ValidatePayeeBankBSB()
		{
			ValidateCalculatedProperty(Parent.PayeeBankBSBInfo);
		}

		protected void CheckPayeeBankBSB()
		{
			if (Parent.IncludeInTheBatch)
			{
				ZString error = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(Parent);
				if (!error.IsEmpty)
				{
					Parent.PayeeBankBSBInfo.AddError(error);
				}
			}
		}

		public void ValidatePayeeBankAccountNumber()
		{
			ValidateCalculatedProperty(Parent.PayeeBankAccountNumberInfo);
		}

		protected void CheckPayeeBankAccountNumber()
		{
			if (Parent.IncludeInTheBatch)
			{
				ZString error = DirectDebitBatchHeaderValidation.GetBankAccountNumberError(Parent);
				if (!error.IsEmpty)
				{
					Parent.PayeeBankAccountNumberInfo.AddError(error);
				}
			}
		}

		#endregion

		#region Implementation

		public AccValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new AccValidationHelper();
				}
				return fValidationHelper;
			}
		}
		AccValidationHelper fValidationHelper;

		#endregion
	}
}
