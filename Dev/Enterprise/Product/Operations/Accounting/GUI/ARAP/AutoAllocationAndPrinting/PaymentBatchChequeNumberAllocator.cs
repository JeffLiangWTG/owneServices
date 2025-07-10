using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting
{
	public class PaymentBatchChequeNumberAllocator : PaymentChequeNumberAllocator, IObsoleteValidation
	{
		public PaymentBatchChequeNumberAllocator(ICollection transactions, BusinessObjectFactory bizObjFactory)
			: base(null, PrintingMode.DirectPayment, bizObjFactory)
		{
			this.Transactions = transactions;
		}

		#region AutoAllocation

		protected override ZBool CanContinueWithAllocation
		{
			get
			{
				if (SortedPaymentCollections.Count > 0)
				{
					foreach (var paymentChequeBookGroup in SortedPaymentCollections)
					{
						foreach (IChequeNumberAutoAllocation transaction in paymentChequeBookGroup)
						{
							if (transaction.IsAllocationPerformed)
							{
								return ZBool.False;
							}
						}
					}
				}
				return ZBool.True;
			}
		}

		protected override void AllocateChequeNumbers()
		{
			if (SortedPaymentCollections.Count > 0)
			{
				foreach (var paymentChequeBookGroup in SortedPaymentCollections)
				{
					AutoAllocateChequeNumbersForPaymentCollection(paymentChequeBookGroup);
				}
			}
		}

		void AutoAllocateChequeNumbersForPaymentCollection(IEnumerable<TransactionHeader> paymentCollection)
		{
			AccChequeBook currentChequeBook = null;
			foreach (IChequeNumberAutoAllocation payment in paymentCollection)
			{
				if (currentChequeBook == null)
				{
					currentChequeBook = payment.ChequeBook;
				}

				var nextChequeNumber = AutoAllocateChequeNumber(currentChequeBook);
				if (nextChequeNumber != ZDecimal.Zero)
				{
					payment.AssignChequeNumber(nextChequeNumber.ToString());
				}
			}
		}

		#endregion

		#region Printing

		protected override void AutoPrintCheques_Core()
		{
			if (SortedPaymentCollections.Count > 0)
			{
				foreach (var paymentChequeBookGroup in SortedPaymentCollections)
				{
					AutoPrintChequesForPaymentCollection(paymentChequeBookGroup);
				}
			}
		}

		protected override ZBool CanContinueWithPrinting
		{
			get
			{
				if (SortedPaymentCollections.Count > 0)
				{
					foreach (var paymentChequeBookGroup in SortedPaymentCollections)
					{
						foreach (IChequeNumberAutoAllocation transaction in paymentChequeBookGroup)
						{
							if (transaction.ChequeIsAutoPrinted)
							{
								return ZBool.False;
							}
						}
					}
				}
				return ZBool.True;
			}
		}

		void AutoPrintChequesForPaymentCollection(IEnumerable<TransactionHeader> paymentCollection)
		{
			PaymentBatchPrintManager printManager = GetNewPrintManager(paymentCollection);
			printManager.PrintChequesDirectly(((IChequeNumberAutoAllocation)paymentCollection.First()).Printing_PrinterPK);
		}

		protected virtual PaymentBatchPrintManager GetNewPrintManager(IEnumerable<TransactionHeader> paymentCollection)
		{
			return new PaymentBatchPrintManager(paymentCollection, null, ZArchitecture.Core.TransactionTypes.Payment, Factory);
		}

		#endregion

		#region Implementation

		readonly ICollection Transactions;

		List<TransactionHeader[]> SortedPaymentCollections
		{
			get
			{
				if (fSortedPaymentCollections == null)
				{
					fSortedPaymentCollections = GetSortedPaymentCollections();
				}
				return fSortedPaymentCollections;
			}
		}
		List<TransactionHeader[]> fSortedPaymentCollections;

		List<TransactionHeader[]> GetSortedPaymentCollections()
		{
			var result = new List<TransactionHeader[]>();
			foreach (var collection in Utils.SplitTransactionBatchOnCollectionByChequeBookParameter(Transactions, Factory))
			{
				if (collection.Any(x => !x.AH_ChequeOrReference.IsEmpty))
				{
					result.Add(collection.OrderBy(x => x.AH_ChequeOrReference).ToArray());
				}
				else
				{
					result.Add(collection.ToArray());
				}
			}
			return result;
		}

		protected override void OnAllocationOrSavingFailed()
		{
			foreach (var paymentBatch in SortedPaymentCollections)
			{
				foreach (IChequeNumberAutoAllocation payment in paymentBatch)
				{
					payment.ChequeIsAutoPrinted = ZBool.False;
				}
			}
		}

		#endregion

		#region Test
#if DEBUG
		#region TestHelpers

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class DummyPaymentBatchChequeNumberAllocator : PaymentBatchChequeNumberAllocator
		{
			public DummyPaymentBatchChequeNumberAllocator(ICollection transactions, BusinessObjectFactory bizObjFactory)
				: base(transactions, bizObjFactory)
			{ }

			public ITransactionParticipant[] GetFactoriesForTest(params ITransactionParticipant[] factories)
			{
				return GetFactoriesWithAllocationCodeToBeCalledOnSaving(factories);
			}

			protected override PaymentBatchPrintManager GetNewPrintManager(IEnumerable<TransactionHeader> paymentCollection)
			{
				CollectionsPassedForPrinting.Add(paymentCollection);
				PaymentBatchPrintManager.TestPaymentPrintManager paymentPrintManager = new PaymentBatchPrintManager.TestPaymentPrintManager(paymentCollection, null, TransactionTypes.Payment, Factory);
				PrintManagerCollection.Add(paymentPrintManager);
				return paymentPrintManager;
			}

			protected override void AllocateChequeNumbers()
			{
				if (fSetChequeBookToInactiveOnSaving)
				{
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					ZGuid chequeBookPK = ((Business.ARAP.ReceiptPayment.Payment)(SortedPaymentCollections[0]).First()).ChequeBookBizO.PK;
					AccChequeBook chequeBook = newFactory.Load<AccChequeBook>(chequeBookPK);
					chequeBook.AK_IsActive = ZBool.False;
					newFactory.Save();
				}
				base.AllocateChequeNumbers();
				AllocationCalled_Counter++;
				if (fRaiseErrorOnAllocation)
				{
					throw new Exception("Exception for test");
				}
			}
			public ZInt AllocationCalled_Counter;

			protected override void AutoPrintCheques_Core()
			{
				base.AutoPrintCheques_Core();
				PrintingCalled_Counter++;
				if (RaiseErrorOnPrinting)
				{
					throw new Exception("Exception for test");
				}
			}
			public ZInt PrintingCalled_Counter;

			public ZBool ChequeWasAutoPrinted
			{
				get
				{
					ZBool fChequeWasAutoPrinted = ZBool.True;
					foreach (PaymentBatchPrintManager.TestPaymentPrintManager printManager in PrintManagerCollection)
					{
						fChequeWasAutoPrinted &= ((PaymentBatchPrintManager.MockPaymentPrint)printManager.PaymentPrinter_ForTestOnly).IsAutoChequePrinted;
					}
					return fChequeWasAutoPrinted;
				}
			}

			public List<ZGuid> PrintersPassedForAutoPrinting
			{
				get
				{
					if (fPrintersPassedForAutoPrinting == null)
					{
						fPrintersPassedForAutoPrinting = new List<ZGuid>();
						foreach (PaymentBatchPrintManager.TestPaymentPrintManager printManager in PrintManagerCollection)
						{
							fPrintersPassedForAutoPrinting.Add(((PaymentBatchPrintManager.MockPaymentPrint)printManager.PaymentPrinter_ForTestOnly).PrinterPKPassedForAutoPrinting);
						}
					}
					return fPrintersPassedForAutoPrinting;
				}
			}
			List<ZGuid> fPrintersPassedForAutoPrinting;

			protected override void OnAllocationOrSavingFailed()
			{
				base.OnAllocationOrSavingFailed();
				//In order for testing.
				//In the real situation, the rollback of cheque numbers is not needed for the batch
				foreach (var paymentBatch in SortedPaymentCollections)
				{
					foreach (IChequeNumberAutoAllocation payment in paymentBatch)
					{
						payment.AllocationOrPrintingFailed();
					}
				}
			}

			public List<PaymentBatchPrintManager.TestPaymentPrintManager> PrintManagerCollection
			{
				get
				{
					if (fPrintManagerCollection == null)
					{
						fPrintManagerCollection = new List<PaymentBatchPrintManager.TestPaymentPrintManager>();
					}
					return fPrintManagerCollection;
				}
			}
			List<PaymentBatchPrintManager.TestPaymentPrintManager> fPrintManagerCollection;

			public List<IEnumerable<TransactionHeader>> CollectionsPassedForPrinting
			{
				get
				{
					if (fCollectionsPassedForPrinting == null)
					{
						fCollectionsPassedForPrinting = new List<IEnumerable<TransactionHeader>>();
					}
					return fCollectionsPassedForPrinting;
				}
			}
			List<IEnumerable<TransactionHeader>> fCollectionsPassedForPrinting;

			public ZBool SetChequeBookToInactiveOnSaving
			{
				get
				{
					return SetChequeBookToInactiveOnSaving;
				}
				set
				{
					fSetChequeBookToInactiveOnSaving = value;
				}
			}
			ZBool fSetChequeBookToInactiveOnSaving;

			public ZBool RaiseErrorOnPrinting
			{
				get
				{
					return fRaiseErrorOnPrinting;
				}
				set
				{
					fRaiseErrorOnPrinting = value;
				}
			}
			ZBool fRaiseErrorOnPrinting;

			public ZBool RaiseErrorOnAllocation
			{
				get
				{
					return fRaiseErrorOnAllocation;
				}
				set
				{
					fRaiseErrorOnAllocation = value;
				}
			}
			ZBool fRaiseErrorOnAllocation;
		}

		#endregion
#endif
		#endregion

	}
}
