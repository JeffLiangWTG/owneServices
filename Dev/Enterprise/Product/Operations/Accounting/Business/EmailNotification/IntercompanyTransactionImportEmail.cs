using System;
using System.Globalization;
using System.Text;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class IntercompanyTransactionImportEmail : AccountingEmailDef
	{
		public IntercompanyTransactionImportEmail(InvoicingBase importedTransaction)
			: base()
		{
			if (importedTransaction == null)
			{
				throw new ArgumentException("Imported transaction being passed cannot be null");
			}

			this.body = GetBodyForImportedTransaction(importedTransaction);
			this.subject = GetSubjectForImportedTransaction(importedTransaction);
		}

		public IntercompanyTransactionImportEmail(InvoicingBase transactionToImport, string errorMessage)
			: base()
		{
			this.body = GetBodyForTransactionToImport(transactionToImport, errorMessage);
			this.subject = GetSubjectForTransactionToImport(transactionToImport);
		}

		readonly string body;
		readonly string subject;

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.IntercompanyTransactionsImportNotifyGroup; }
		}

		string GetBodyForImportedTransaction(InvoicingBase importedTransaction)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine(Res.GetString("ab0edba3-8513-482a-aaa0-61c95a79716b", "An attempt to post an Intercompany Imported {0} {1} {2} for {3} {4} has failed because of the following validation errors:",
				importedTransaction.AH_Ledger,
				importedTransaction.AH_TransactionType,
				importedTransaction.AH_TransactionNum,
				importedTransaction.AH_RX_NKTransactionCurrency,
				importedTransaction.AH_OSTotalAmount.ToString(string.Format(CultureInfo.CurrentCulture, "N{0}", importedTransaction.TransactionCurrency.Decimals), CultureInfo.CurrentCulture)));
			result.AppendLine(string.Empty);

			foreach (string error in importedTransaction.Notifications.GetUniqueMessageList())
			{
				result.AppendLine(error);
			}

			if (importedTransaction.Lines.HasErrors())
			{
				result.AppendLine(string.Empty);

				result.AppendLine(Res.GetString("ae78dd0a-53a8-4851-934a-19fc849534fa", "Transaction Lines with validation errors:"));

				foreach (InvoicingLineBase line in importedTransaction.Lines)
				{
					if (line.HasErrors)
					{
						result.AppendLine(Res.GetString("f566ccae-b483-4837-b360-766fe2c3bc67", "Line {0}", line.AL_Sequence));
						foreach (string error in line.Notifications.GetUniqueMessageList())
						{
							result.AppendLine(error);
						}
					}
				}
			}

			return result.ToString();
		}

		string GetBodyForTransactionToImport(InvoicingBase transactionToImport, string errorMessage)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine(Res.GetString("4ed7a6a7-95a6-4815-baf6-051e21234258", "An attempt to import an invoice with organization {0} in company [{1}] branch [{2}] department [{3}] has failed because of the following error:",
									transactionToImport.Factory.Load<OrgHeader>(transactionToImport.AH_OH).OH_Code,
									transactionToImport.Company.GC_Code,
									transactionToImport.Branch.GB_Code,
									transactionToImport.Department.GE_Code));

			result.Append(errorMessage);

			return result.ToString();
		}

		protected sealed override string GetBody()
		{
			return body;
		}

#if DEBUG
		public string GetBody_ForTestOnly()
		{
			return GetBody();
		}
#endif

		string GetSubjectForImportedTransaction(InvoicingBase importedTransaction)
		{
			return Res.GetString("588e2faa-9d75-492b-944b-d9775621d6cc", "Failed to post {0} {1} {2} ({3} {4}) imported from other Group Company",
							importedTransaction.AH_Ledger,
							importedTransaction.AH_TransactionType,
							importedTransaction.AH_TransactionNum,
							importedTransaction.AH_RX_NKTransactionCurrency,
							importedTransaction.AH_OSTotalAmount.ToString(string.Format(CultureInfo.CurrentCulture, "N{0}", importedTransaction.TransactionCurrency.Decimals), CultureInfo.CurrentCulture));
		}

		string GetSubjectForTransactionToImport(InvoicingBase transactionToImport)
		{
			return Res.GetString("e2a87aab-a3a7-4216-a304-93d7b8e71287", "Failed to auto-import {0} {1} {2} ({3} {4}) from other Group Company",
																			transactionToImport.AH_Ledger,
																			transactionToImport.AH_TransactionType,
																			transactionToImport.AH_TransactionNum,
																			transactionToImport.AH_RX_NKTransactionCurrency,
																			transactionToImport.AH_OSTotalAmount.ToString(string.Format(CultureInfo.CurrentCulture, "N{0}", transactionToImport.TransactionCurrency.Decimals), CultureInfo.CurrentCulture));
		}

		protected sealed override string GetSubject()
		{
			return subject;
		}

#if DEBUG
		public string GetSubject_ForTestOnly()
		{
			return GetSubject();
		}
#endif

	}
}

