using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class DepositBatchCreator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DepositBatchCreator(IDepositBatch originalReceipt)
			: base(originalReceipt.Factory)
		{
			this.OriginalReceipt = originalReceipt;
		}

		#region Create Deposit Batch

		public void CreateDepositBatch()
		{
			if (OriginalReceipt.IsDirectCreditAndNotOpeningReceipt)
			{
				RelatedDepositBatch = Factory.New<DepositBatch>();
				RelatedDepositBatch.SetOriginalReceipt(OriginalReceipt);
				RelatedDepositBatch.AH_OSTotal = -OriginalReceipt.AH_OSTotal;
				RelatedDepositBatch.AH_InvoiceAmount = -OriginalReceipt.AH_InvoiceAmount;
				RelatedDepositBatch.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				RelatedDepositBatch.AH_PostDate = OriginalReceipt.AH_PostDate;
				RelatedDepositBatch.AH_InvoiceDate = OriginalReceipt.AH_InvoiceDate;
				RelatedDepositBatch.AH_DueDate = OriginalReceipt.AH_DueDate;
				RelatedDepositBatch.AH_AB = OriginalReceipt.AH_AB;
				RelatedDepositBatch.AH_OH = OriginalReceipt.AH_OH;

				if (OriginalReceipt.AH_ReceiptType == ReceiptTypes.eNettDirectCredit)
				{
					ZString dateDesc = ZString.Empty;

					if (OriginalReceipt.CompayReceiptBatchDate.IsValid)
					{
						dateDesc = OriginalReceipt.CompayReceiptBatchDate.ToString("ddMMyyyy");
						RelatedDepositBatch.AH_PostDate = OriginalReceipt.CompayReceiptBatchDate;
						RelatedDepositBatch.AH_InvoiceDate = OriginalReceipt.CompayReceiptBatchDate;
					}

					RelatedDepositBatch.AH_Desc = string.Format((NoResString)"COMPAY-CC-{0} CREDIT-{1}", AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.PadLeft(6, '0'), dateDesc);
				}
			}
		}

		DepositBatch RelatedDepositBatch;
		readonly IDepositBatch OriginalReceipt;

		#endregion
	}
}
