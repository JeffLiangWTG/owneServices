using System;
using System.Linq;
using CargoWise.Common.JSON.Extensions;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class RAJGlobalElectronicInvoiceBuilderForVietnam : GlobalElectronicInvoiceBuilderForVietnam
	{
		public RAJGlobalElectronicInvoiceBuilderForVietnam(string batchNumber, TransactionBatch universalTransactionBatch, AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo)
			: base(batchNumber, additionalTransactionInfo)
		{
			if (universalTransactionBatch?.TransactionCollection?.Count != 1)
			{
				throw new ArgumentException("Each transaction batch can have only one transaction in order to generate a Vietnam Adjustment Electronic Invoice.");
			}

			Transaction = universalTransactionBatch.TransactionCollection.First();
		}

		readonly UniversalTransactionInfo Transaction;

		protected override ZString GetPayload(INotifications notifications)
		{
			return new VietnamEInvoiceAdjustmentCreator(Transaction, AdditionalTransactionInfo).Create()?.ToJSON();
		}

		protected override string SchemaResourceName => "Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice.AdjustInvoice.VietnamEInvoiceAdjustmentSchema.json";
		protected override ZString MessageType => VietnamEInvoiceAPICommandList.Codes.AdjustReceivablesInvoice;
	}
}
