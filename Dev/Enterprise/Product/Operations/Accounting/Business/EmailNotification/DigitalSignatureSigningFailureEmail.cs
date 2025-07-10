using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class DigitalSignatureSigningFailureEmail : AccountingEmailDef
	{
		public DigitalSignatureSigningFailureEmail(string failureReason)
		{
			ContentType = EmailContentTypes.HTML;
			this.failureReason = failureReason;
		}

		public DigitalSignatureSigningFailureEmail(InvoicingBase invoice, string failureReason)
		{
			ContentType = EmailContentTypes.HTML;
			ledger = invoice.AH_Ledger;
			transactionNum = invoice.AH_TransactionNum;
			transactionType = invoice.AH_TransactionType;
			this.failureReason = failureReason;
		}

		#region Implementation

		readonly string ledger;
		readonly string transactionType;
		readonly string transactionNum;
		readonly string failureReason;

		#region Overrides

		protected override GuidRegistryItem Recipient
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.ComplianceInvoiceBookAllocaltionFailureNotificationGroup;
			}
		}

		protected override string GetBody()
		{
			if (!string.IsNullOrEmpty(transactionNum))
			{
				return Res.GetString("c8ede953-4801-49a0-a805-d544c67a77e2", @"When post transaction, CW1 failed to sign transaction {0} {1} {2} with a valid digital Signature due to the following reason:
{3}",
				ledger,
				transactionType,
				transactionNum,
				failureReason);
			}
			else
			{
				return Res.GetString("537c31fc-2950-47ec-b855-d0b401c0def9", @"When post transactions, CW1 failed to sign transactions with valid digital Signatures due to the following reason:
{0}",
				failureReason);
			}
		}

		protected override string GetSubject()
		{
			if (!string.IsNullOrEmpty(transactionNum))
			{
				return Res.GetString("a356ff9b-56f1-4c23-8f22-dc7c71e894d0", "Failed to sign transaction {0} {1} {2} with a valid digital Signature", ledger, transactionType, transactionNum);
			}
			else
			{
				return Res.GetString("a4e311b3-c473-491e-8f4c-1fa5217da17c", "Failed to sign transactions with valid digital Signatures");
			}
		}

		#endregion

		#endregion
	}
}