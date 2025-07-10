using System;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class VietnamEInvoiceDocumentCreator
	{
		#region SuppressResourceStringsCheckRegion

		public VietnamEInvoiceDocument Create(AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo)
		{
			var companyPK = additionalTransactionInfo.CompanyPK.ToGuid();
			var branchPK = additionalTransactionInfo.BranchPK.ToGuid();

			var eInvoiceDocument = new VietnamEInvoiceDocument
			{
				User = new User() { Username = "", Password = "" },
				Inv = new InvForDocumentReqeust
				{
					Stax = VietnamEInvoiceHelper.GetVietnamRegistrationNumber(companyPK, branchPK),
					Sid = additionalTransactionInfo.OriginalTransactionPK.ToString(),
					Type = AccountingConfigurationRegistry.Instance.VietnamEInvoicingReceivingFileType.GetFallBackValueAtAllLevels(additionalTransactionInfo.CompanyPK.ToGuid(), additionalTransactionInfo.BranchPK.ToGuid(), Guid.Empty),
				}
			};

			return eInvoiceDocument;
		}

		#endregion
	}
}
