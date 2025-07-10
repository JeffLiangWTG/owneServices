using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	sealed public class CreditNoteAmendingHelper
	{
		CreditNoteAmendingHelper()
		{
		}

		public delegate void CreditNoteAmendingErrorHandler(string message, string caption);

		public static IAmending AmendARTransaction(ZString transactionType, TransactionHeader aTransaction, JobInvoicingSecurityHelper securityHelper, CreditNoteAmendingErrorHandler securityRightsErrorHandler,
			CreditNoteAmendingErrorHandler errorHandler)
		{
			IAmending amending = null;

			if (aTransaction != null && (transactionType == TransactionTypes.Invoice || transactionType == TransactionTypes.CreditNote))
			{
				var checkPoint = (transactionType == TransactionTypes.Invoice) ? SecurityCore.AmendTransactionWInvoice : SecurityCore.AmendTransactionWCreditNote;

				if (securityHelper != null && securityHelper.PlugInSecurity != Env.Security.None && !securityHelper.CheckIsAllowedForSecurityCheckPoint(checkPoint))
				{
					securityRightsErrorHandler(checkPoint, null);
				}
				else
				{
					var originalTransaction = aTransaction as InvoicingBase;
					var message = ZString.Empty;
					var caption = Res.GetString("d5a9aa19-a358-43b4-bb8f-1a060f1f7239", "Cannot amend transaction");
					if (originalTransaction != null && !originalTransaction.IsReversed)
					{
						var original = originalTransaction as IAmending;

						if (original != null && original.IsOriginalTransaction)
						{
							var taxRateTypes = new ZString[] { AccTaxRate.Types.Rated, AccTaxRate.Types.Exempt, AccTaxRate.Types.CapitalRated };
							if (transactionType == TransactionTypes.CreditNote && AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.Value)
							{
								if (originalTransaction.Lines.OfType<InvoicingLineBase>().Any(x => x.TaxRate != null && taxRateTypes.Contains(x.TaxRate.AT_Type) && x.ComplianceDocumentNumber.IsEmpty))
								{
									message = Res.GetString("6254CC06-1A1F-431A-ADB3-4BD5421D1182", "Please create compliance document record and allocate compliance document number before proceeding to amend with credit note.");
									errorHandler(message, caption);
								}
								var allComplianceDocumentHeaders = originalTransaction.GetAllTransactionGeneratedComplianceDocument();
								if (allComplianceDocumentHeaders.Any(currentComplianceDocumentHeader => currentComplianceDocumentHeader.ADH_ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE && (currentComplianceDocumentHeader.EInvoicingStatus.IsEmpty || currentComplianceDocumentHeader.EInvoicingStatus != Constants.EInvoicingPivotState.Succeed)))
								{
									message = Res.GetString("712D3346-4013-4A2E-B1A6-7E8CBD1258FA", "Credit Note can only be created after the Original TXE documents have been successfully uploaded (i.e., E-Reporting Status = SUC).");
									errorHandler(message, caption);
								}
							}
							else if (transactionType == TransactionTypes.CreditNote && AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsReceivable, Env.CurrentCompany.PK))
							{
								message = Res.GetString("E7DDEF91-3652-4375-BF21-77C065767A15", "Cannot amend selected transaction with Credit Note as {0}", AccountingMasterFilesUtils.ARCreditNoteDisallowedMessage);
								errorHandler(message, caption);
							}
							else
							{
								var additionalValidation = CountryComplianceEInvoicingHelper.GetAdditionalValidation(originalTransaction.Company);
								message = additionalValidation?.GetCantAmendErrorMessage(originalTransaction, transactionType) ?? ZString.Empty;
								if (!message.IsEmpty)
								{
									errorHandler(message, caption);
								}
							}

							if (message.IsEmpty)
							{
								amending = original.GenerateAmendingTransaction(transactionType);
							}
						}
						else if (originalTransaction != null && originalTransaction.IsAmendingTransaction)
						{
							message = Res.GetString("c1f94833-d5ca-4d85-aa7e-be516f11d50d", "Cannot amend selected transaction as this is an amendment transaction.");
							errorHandler(message, caption);
						}
						else
						{
							message = Res.GetString("4b932563-f4b4-46cb-8835-9b6bdd0abf76", "Cannot amend selected transaction.");
							errorHandler(message, caption);
						}
					}
					else
					{
						message = Res.GetString("2a0c55ea-051f-4a65-bb5c-68a6f8c33ca9", "Cannot amend selected transaction as this has been reversed.");
						errorHandler(message, caption);
					}
				}
			}

			return amending;
		}

		public static IAmending AmendAPTransaction(TransactionHeader aTransaction, JobInvoicingSecurityHelper securityHelper, CreditNoteAmendingErrorHandler securityRightsErrorHandler, CreditNoteAmendingErrorHandler errorHandler)
		{
			IAmending amending = null;

			if (aTransaction != null)
			{
				var message = string.Empty;
				var caption = Res.GetString("d5a9aa19-a358-43b4-bb8f-1a060f1f7239", "Cannot amend transaction");
				var originalTransaction = aTransaction as InvoicingBase;

				if (!(originalTransaction is APInvoice))
				{
					message = Res.GetString("4b184547-1ad2-4afa-b80f-d9b34ae073d7", "Only AP Invoice can be amended.");
					errorHandler(message, caption);
				}
				else if (securityHelper != null && !securityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.APAmendWithCreditNote))
				{
					securityRightsErrorHandler(SecurityCore.APAmendWithCreditNote, null);
				}
				else if (originalTransaction != null && !originalTransaction.IsReversed)
				{
					var original = originalTransaction as IAmending;

					if (original != null && original.IsOriginalTransaction)
					{
						if (AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsPayable, Env.CurrentCompany.PK))
						{
							message = Res.GetString("FD2A8FE7-11A4-43E1-95BF-A955E20B53C6", "Cannot amend selected transaction with Credit Note as {0}", AccountingMasterFilesUtils.APCreditNoteDisallowedMessage);
							errorHandler(message, caption);
						}
						else
						{
							amending = original.GenerateAmendingTransaction(TransactionTypes.CreditNote);
						}
					}
					else if (originalTransaction != null && originalTransaction.IsAmendingTransaction)
					{
						message = Res.GetString("c1f94833-d5ca-4d85-aa7e-be516f11d50d", "Cannot amend selected transaction as this is an amendment transaction.");
						errorHandler(message, caption);
					}
					else
					{
						message = Res.GetString("4b932563-f4b4-46cb-8835-9b6bdd0abf76", "Cannot amend selected transaction.");
						errorHandler(message, caption);
					}
				}
				else
				{
					message = Res.GetString("2a0c55ea-051f-4a65-bb5c-68a6f8c33ca9", "Cannot amend selected transaction as this has been reversed.");
					errorHandler(message, caption);
				}
			}

			return amending;
		}
	}
}
