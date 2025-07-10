using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.DataTransfer
{
	public class PaymentReceiptRemittanceFileProcessor : TxnHeaderProcessorBase
	{
		public PaymentReceiptRemittanceFileProcessor(BusinessObjectFactory factory, INotifications notifications) : base(factory, notifications)
		{
		}

		List<Guid> ReceiptPaymentGuids => receiptPaymentGuids ?? (receiptPaymentGuids = new List<Guid>());
		List<Guid> receiptPaymentGuids;

		protected override void SetupContext()
		{
			ReceiptPaymentGuids.Clear();
			NotificationBuffer = new NotificationBuffer(Notifications);
			NotificationManager = new NotificationManager(NotificationBuffer);
			CurrentFactory = new BusinessObjectFactory();
			CurrentFactory.SetContext(BusinessContext.RemittanceFileImport);

			if (FactoryToCollectChildFactories != null)
			{
				//only when auto importing remittance import file by the xms service task, factory is not null. when manually importing factory is null
				FactoryToCollectChildFactories.ChildFactories.Add(CurrentFactory);
			}
		}

		protected override void RemoveCurrentFactory()
		{
			FactoryToCollectChildFactories.ChildFactories.Remove(CurrentFactory);
		}

		protected override void PaymentApprovalAllocateCheckNumberAndSave(PaymentApprovalBase paymentApproval, bool isAutoAllocationEnabled)
		{
			if (isAutoAllocationEnabled && OnSavePaymentWithChequeNumberAllocator != null)
			{
				OnSavePaymentWithChequeNumberAllocator(paymentApproval, EventArgs.Empty);
			}
			else
			{
				paymentApproval.Factory.Save();
			}
		}

		public event EventHandler OnSavePaymentWithChequeNumberAllocator;

		protected override void PrintReceiptAndPayment()
		{
			if (ReceiptPaymentGuids.Count > 0 && RiseOnPrintMatchingDocument())
			{
				using (var printUtility = new ReceiptPrint())
				{
					printUtility.Print(AccountingUtils.AccountingDocumentTitles.ReceiptMatching, ReceiptPaymentGuids.ToArray());
				}
			}
		}

		protected override void AddReceiptAndPaymentToPrintList()
		{
			ReceiptPaymentGuids.Add(TransactionHeader.PK.ToGuid());
		}

		internal bool RiseOnPrintMatchingDocument()
		{
			bool response = false;
			if (OnPrintMatchingDocument != null)
			{
				var eventArgs = new BoolResponseEventArgs(true);
				OnPrintMatchingDocument(this, eventArgs);
				response = eventArgs.Response;
			}
			return response;
		}

		public event EventHandler<BoolResponseEventArgs> OnPrintMatchingDocument;
	}
}
