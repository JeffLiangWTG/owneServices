using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class TransactionBatchToGEIConverterForTurkey : GlobalTransactionBatchToGEIConverter
	{
		public TransactionBatchToGEIConverterForTurkey(ICountryEInvoicingObjectFactory countryFactory) : base(countryFactory)
		{
		}

		protected override void PerformBeforeConvert(AccEInvoicingBatch batch)
		{
			EInvoicingBatch = batch;
			var pivot = (AccEInvoicingTransactionPivot)batch.TransactionPivots.FirstOrDefault();
			if (pivot != null)
			{
				MessageType = TurkeyEInvoiceAPICommandList.GetMessageType(((AccEInvoicingTransactionPivot)batch.TransactionPivots.FirstOrDefault())?.AIP_ActionType ?? ZString.Empty);

				if (MessageType == TurkeyEInvoiceAPICommandList.Codes.SendReceivablesInvoice)
				{
					base.PerformBeforeConvert(batch);
				}
			}
			else
			{
				MessageType = TurkeyEInvoiceAPICommandList.Codes.GetInboxInvoiceList;
			}
		}

		protected override IGlobalElectronicInvoiceBuilder GetGEIBuilder(string batchNumber)
		{
			switch (MessageType)
			{
				case TurkeyEInvoiceAPICommandList.Codes.SendReceivablesInvoice:
					return new RINGlobalElectronicInvoiceBuilderForTurkey(batchNumber, UniversalBatch);

				case TurkeyEInvoiceAPICommandList.Codes.CancelReceivablesInvoice:
					return new RCNGlobalElectronicInvoiceBuilderForTurkey(batchNumber, BatchCompany, GovernmentAllocatedNumber, TransactionHeaderPk);

				case TurkeyEInvoiceAPICommandList.Codes.RequestPDFofReceivablesInvoice:
					return new GlobalElectronicInvoiceRequestByIdBuilderForTurkey(batchNumber, TurkeyEInvoiceAPICommandList.Codes.RequestPDFofReceivablesInvoice, BatchCompany, GovernmentAllocatedNumber);

				case TurkeyEInvoiceAPICommandList.Codes.StatusRequestForReceivablesInvoice:
					return new GlobalElectronicInvoiceRequestByIdBuilderForTurkey(batchNumber, TurkeyEInvoiceAPICommandList.Codes.StatusRequestForReceivablesInvoice, BatchCompany, GovernmentAllocatedNumber);

				case TurkeyEInvoiceAPICommandList.Codes.GetInboxInvoiceList:
				case TurkeyEInvoiceAPICommandList.Codes.SetInvoiceTaken:
				case TurkeyEInvoiceAPICommandList.Codes.SendApproveDocumentResponse:
				case TurkeyEInvoiceAPICommandList.Codes.SendRejectDocumentResponse:
					return new APGEIBuilderForTurkey(batchNumber, MessageType, BatchCompany, GovernmentAllocatedNumber);

				default:
					throw new InvalidOperationException($"Invalid Message Type: {MessageType}");
			}
		}

		protected string GovernmentAllocatedNumber => EInvoicingBatch?.AIB_GovernmentAllocatedNumber;
		protected GlbCompany BatchCompany => EInvoicingBatch?.Company;
		protected string MessageType;
		protected ZGuid TransactionHeaderPk => ((AccEInvoicingTransactionPivot)EInvoicingBatch?.TransactionPivots.FirstOrDefault())?.AIP_ParentID ?? ZGuid.Empty;
		protected AccEInvoicingBatch EInvoicingBatch;
	}
}
