using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.HotCheque;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting
{
	public partial class PaymentChequeNumberAllocator : PaymentChequeNumberAllocatorBase
	{
		public enum PrintingMode
		{
			PaymentApproval,
			HotCheque,
			DirectPayment,
			Invoice
		}

		public PaymentChequeNumberAllocator(IChequeNumberAutoAllocation autoAllocationObject, PrintingMode printingMode, BusinessObjectFactory bizObjFactory)
			: base(autoAllocationObject, bizObjFactory)
		{
			this.Mode = printingMode;
		}

		public override ITransactionParticipant[] GetFactoriesWithAllocationCodeToBeCalledOnSaving(params ITransactionParticipant[] factories)
		{
			if (factories.Length > 0)
			{
				Array.Resize(ref factories, factories.Length + 1);
			}
			else
			{
				factories = new ITransactionParticipant[2];
				factories[0] = Factory;
			}

			factories[factories.Length - 1] = new SaveInTransactionWithRollBackAction(Factory, () => { AutoPrintCheques(); return ChangedTableNames.All; }, CheckAllocationOrPrintingFailed);
			return factories;
		}

		#region Printing

		protected virtual ZBool CanContinueWithPrinting
		{
			get
			{
				return !AutoAllocationObject.ChequeIsAutoPrinted;
			}
		}

		protected virtual void AutoPrintCheques_Core()
		{
			ZGuid printerPK = ZGuid.Empty;
			PaymentPrintManager paymentPrintManager;

			switch (Mode)
			{
				case PrintingMode.HotCheque:
					HotChequePrintManager printManager = GetHotChequePrintManager(AutoAllocationObject.Printing_ObjectPK);
					printManager.AutoPrintCheque(AutoAllocationObject.Printing_PrinterPK);
					AutoAllocationObject.ChequeIsAutoPrinted = ZBool.True;
					break;
				case PrintingMode.PaymentApproval:
				case PrintingMode.Invoice:
					paymentPrintManager = GetPaymentPrintManager(AutoAllocationObject.Printing_ObjectPK, TransactionTypes.Payment);
					paymentPrintManager.AutoPrintCheque(AutoAllocationObject.Printing_PrinterPK);
					AutoAllocationObject.ChequeIsAutoPrinted = ZBool.True;
					break;
				case PrintingMode.DirectPayment:
					paymentPrintManager = GetPaymentPrintManager(AutoAllocationObject.Printing_ObjectPK, TransactionTypes.DirectPayment);
					paymentPrintManager.AutoPrintCheque(AutoAllocationObject.Printing_PrinterPK);
					AutoAllocationObject.ChequeIsAutoPrinted = ZBool.True;
					break;
			}
		}

		protected virtual PaymentPrintManager GetPaymentPrintManager(Guid paymentPK, string transactionType)
		{
			return new PaymentPrintManager(paymentPK, transactionType, Factory);
		}

		protected virtual HotChequePrintManager GetHotChequePrintManager(Guid hotChequePK)
		{
			return new HotChequePrintManager(hotChequePK, Factory);
		}

		#endregion

		#region Implementation

		void AutoPrintCheques()
		{
			if (CanContinueWithPrinting)
			{
				try
				{
					AutoPrintCheques_Core();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					AllocationOrPrintingFailed = true;

					if (ex is ZCannotSaveException)
					{
						throw;
					}

					var message = Res.GetString("64ef3a67-751d-4272-ab86-9a55c1fb91c8", "Auto cheque printing failed. Please check Auto cheque printing settings.");
					var heading = Res.GetString("d075a279-d993-4d16-8097-1786671456cf", "Print Cheque");
					throw new ZCannotSaveException(message, heading, ex);
				}
			}
		}

		readonly PrintingMode Mode;

		#endregion

		#region Test

#if DEBUG

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class DummyPaymentChequeNumberAllocator : PaymentChequeNumberAllocator
		{
			public DummyPaymentChequeNumberAllocator(IChequeNumberAutoAllocation autoAllocationObject, PrintingMode printingMode, BusinessObjectFactory factory)
				: base(autoAllocationObject, printingMode, factory)
			{
			}

			public ITransactionParticipant[] GetFactoriesForTest()
			{
				return GetFactoriesWithAllocationCodeToBeCalledOnSaving();
			}

			protected override PaymentPrintManager GetPaymentPrintManager(Guid paymentPK, string transactionType)
			{
				PaymentPrintManager = new PaymentPrintManager.TestPaymentPrintManager(paymentPK, transactionType, Factory);
				return PaymentPrintManager;
			}

			protected override HotChequePrintManager GetHotChequePrintManager(Guid hotChequePK)
			{
				HotChequePrintManager = new HotChequePrintManager(hotChequePK, Factory);
				return HotChequePrintManager;
			}

			protected override void AllocateChequeNumbers()
			{
				if (fSetChequeBookToInactiveOnSaving)
				{
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					AccChequeBook chequeBook = newFactory.Load<AccChequeBook>(AutoAllocationObject.ChequeBook.PK);
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

			public PaymentPrintManager.TestPaymentPrintManager PaymentPrintManager;
			HotChequePrintManager HotChequePrintManager;

			public virtual ZBool ChequeWasAutoPrinted
			{
				get
				{
					if (HotChequePrintManager != null)
					{
						return HotChequePrintManager.ChequeIsAutoPrinted;
					}
					else if (PaymentPrintManager != null)
					{
						return ((PaymentPrintManager.MockPaymentPrint)PaymentPrintManager.PaymentPrinter_Exposed).IsChequeAutoPrinted;
					}
					else
					{
						return ZBool.False;
					}
				}
			}

			public virtual ZGuid PrinterPassedForAutoPrinting
			{
				get
				{
					if (HotChequePrintManager != null)
					{
						return HotChequePrintManager.PrinterPassedForPrinting;
					}
					else if (PaymentPrintManager != null)
					{
						return ((PaymentPrintManager.MockPaymentPrint)PaymentPrintManager.PaymentPrinter_Exposed).PrinterPKPassedForAutoPrinting;
					}
					else
					{
						return ZGuid.Empty;
					}
				}
			}

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

#endif
		#endregion
	}
}
