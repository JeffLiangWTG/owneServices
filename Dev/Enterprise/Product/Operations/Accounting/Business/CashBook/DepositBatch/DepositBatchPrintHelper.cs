using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using DepositBatchObj = Enterprise.Accounting.Business.CashBook.DepositBatch.DepositBatch;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public partial class DepositSlipPrintHelper
	{
		public DepositSlipPrintHelper()
		{
		}

		public void PrintDepositSlip(string depositBatchNum)
		{
			DepositBatchObj depositBatch = GetBatchTransactionHeaderFromNumber(depositBatchNum);
			if (depositBatch != null)
			{
				Print(depositBatch);
			}
		}

		#region Implementation

		DepositBatchObj GetBatchTransactionHeaderFromNumber(string batchNum)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_ReceiptBatchNo, batchNum);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			return Factory.LoadTop1<DepositBatchObj>(filter);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded template name")]
#if DEBUG
		protected virtual void Print(DepositBatchObj depositBatch)
#else
		protected void Print(DepositBatchObj depositBatch)
#endif
		{
			AccPrintingUtility utility = new AccPrintingUtility(Factory, Enterprise.Core.Constants.DataContext.DepositBatch);
			string[] templateNames;
			if (DocumentsDataRegistry.Instance.UseNewDocBuilderDepositBatch.Value)
			{
				if (depositBatch.CashTransactionSelectedCount > 0 || depositBatch.ChequeTransactionSelectedCount > 0 || depositBatch.CreditCardTransactionSelectedCount > 0)
				{
					templateNames = new string[] { "Docbuilder Deposit Slip Bank", "Docbuilder Deposit Slip Office" };
				}
				else
				{
					templateNames = new string[] { "Docbuilder Deposit Slip Office" };
				}
			}
			else
			{
				templateNames = new string[] { "Deposit Slip Bank", "Deposit Slip Office" };
			}
			utility.PrintDocuments(depositBatch, templateNames, AllowedDeliveryOptions.All, true);
		}

		readonly BusinessObjectFactory Factory = new BusinessObjectFactory();

		#endregion
	}
}
