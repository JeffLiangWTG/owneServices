
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Interfaces
{
	public interface IDepositBatch
	{
		BusinessObjectFactory Factory { get; }
		ZDecimal AH_OSTotal { get; }
		ZDecimal AH_InvoiceAmount { get; }
		ZString AH_RX_NKTransactionCurrency { get; }
		ZDateTime AH_PostDate { get; }
		ZDateTime AH_InvoiceDate { get; }
		ZDateTime AH_DueDate { get; }
		ZGuid AH_AB { get; }
		ZGuid AH_OH { get; }
		ZBool IsDirectCreditAndNotOpeningReceipt { get; }
		ZString AH_ReceiptBatchNo { set; }
		ZString AH_ReceiptType { get; }
		ZDateTime CompayReceiptBatchDate { get; }
	}
}
