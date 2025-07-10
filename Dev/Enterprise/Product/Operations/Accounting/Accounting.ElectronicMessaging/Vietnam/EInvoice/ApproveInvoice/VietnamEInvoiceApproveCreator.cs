using System;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice
{
	public class VietnamEInvoiceApproveCreator
	{
		public VietnamEInvoiceApproveCreator(AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo)
		{
			AdditionalTransactionInfo = additionalTransactionInfo;
		}

		readonly AdditionalTransactionInfoForVietnamEInvoice AdditionalTransactionInfo;

		#region SuppressResourceStringsCheckRegion

		public VietnamEInvoiceApprove Create()
		{
			var companyPK = AdditionalTransactionInfo.CompanyPK.ToGuid();
			var branchPK = AdditionalTransactionInfo.BranchPK.ToGuid();

			var form = VietnamEInvoiceHelper.GetForm(AdditionalTransactionInfo);
			var seriesPrefix = AdditionalTransactionInfo.ComplianceSequenceInfo.SeriesPrefix;
			var sequenceNumber = AdditionalTransactionInfo.SequenceNumber;

			var lang = AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);

			var eInvoice = new VietnamEInvoiceApprove()
			{
				User = new UserForApprove() { Username = "", Password = "", Lang = lang },
				Inv =
				{
					Sid = AdditionalTransactionInfo.TransactionPK.ToString(),
					Form = form,
					Serial = AdditionalTransactionInfo.ComplianceSequenceInfo.SeriesPrefix,
					Seq = sequenceNumber,
					Stax =  VietnamEInvoiceHelper.GetVietnamRegistrationNumber(companyPK, branchPK),
					SendFile = 1
				}
			};

			return eInvoice;
		}

		#endregion
	}
}
