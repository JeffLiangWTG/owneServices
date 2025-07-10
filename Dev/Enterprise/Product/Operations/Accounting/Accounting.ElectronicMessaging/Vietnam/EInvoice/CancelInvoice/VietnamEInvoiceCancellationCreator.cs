using System;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class VietnamEInvoiceCancellationCreator
	{
		public VietnamEInvoiceCancellationCreator(AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo)
		{
			this.additionalTransactionInfo = additionalTransactionInfo;
		}

		readonly AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo;

		#region SuppressResourceStringsCheckRegion

		public VietnamEInvoiceCancellation Create()
		{
			var companyPk = additionalTransactionInfo.CompanyPK.ToGuid();
			var branchPk = additionalTransactionInfo.BranchPK.ToGuid();
			var seriesPrefix = additionalTransactionInfo.ComplianceSequenceInfo.OriginalSeriesPrefix;
			var transactionReference = additionalTransactionInfo.OriginalSequenceNumber;

			var eInvoice = new VietnamEInvoiceCancellation
			{
				Language = AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.GetFallBackValueAtAllLevels(companyPk, branchPk, Guid.Empty),
				User = new User() { Username = "", Password = "" },
				Inv = new InvForCancel
				{
					Seq = $"{VietnamEInvoiceHelper.GetForm(additionalTransactionInfo)}-{seriesPrefix}-{transactionReference}",
					Adj = new Adj
					{
						Rdt = additionalTransactionInfo.RectifyDate,
						Rea = additionalTransactionInfo.RectifyReason,
						Ref = additionalTransactionInfo.RectifySupportingDocumentNumber
					}
				}
			};
			return eInvoice;
		}

		#endregion
	}
}
