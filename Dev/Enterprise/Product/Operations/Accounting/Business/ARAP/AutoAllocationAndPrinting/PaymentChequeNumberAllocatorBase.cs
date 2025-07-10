using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.AutoAllocationAndPrinting
{
	public partial class PaymentChequeNumberAllocatorBase : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PaymentChequeNumberAllocatorBase(IChequeNumberAutoAllocation autoAllocationObject, BusinessObjectFactory bizObjFactory)
			: base(bizObjFactory)
		{
			this.AutoAllocationObject = autoAllocationObject;
			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
		}

		#region Allocation

		protected virtual ZBool CanContinueWithAllocation
		{
			get
			{
				return !AutoAllocationObject.IsAllocationPerformed;
			}
		}

		protected virtual void AllocateChequeNumbers()
		{
			var nextChequeNumber = AutoAllocateChequeNumber(AutoAllocationObject.ChequeBook);
			if (nextChequeNumber != ZDecimal.Zero)
			{
				AutoAllocationObject.AssignChequeNumber(nextChequeNumber.ToString());
			}
		}

		public virtual ITransactionParticipant[] GetFactoriesWithAllocationCodeToBeCalledOnSaving(params ITransactionParticipant[] factories)
		{
			if (factories.Length == 0)
			{
				factories = new ITransactionParticipant[1];
				factories[0] = Factory;
			}
			return factories;
		}

		#endregion

		#region Implementation

		protected ZDecimal AutoAllocateChequeNumber(AccChequeBook chequeBook)
		{
			return Utils.GetNextChequeNumberFromActiveChequeBookWithLock(chequeBook, Factory);
		}

		protected AccountingUtils Utils
		{
			get
			{
				if (fUtils == null)
				{
					fUtils = new AccountingUtils();
				}
				return fUtils;
			}
		}
		AccountingUtils fUtils;

		void Factory_Saving(BusinessObjectFactory factory)
		{
			if (CanContinueWithAllocation && !Factory.HasContext(Enterprise.Integration.Accounting.BusinessContext.SavingPaymentApprovalAsDraft))
			{
				try
				{
					AllocateChequeNumbers();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					AllocationOrPrintingFailed = true;
					throw;
				}
			}
		}

		protected void CheckAllocationOrPrintingFailed()
		{
			if (AllocationOrPrintingFailed)
			{
				OnAllocationOrSavingFailed();
			}
		}

		protected virtual void OnAllocationOrSavingFailed()
		{
			AutoAllocationObject.ChequeIsAutoPrinted = ZBool.False;
			AutoAllocationObject.AllocationOrPrintingFailed();
		}

		protected IChequeNumberAutoAllocation AutoAllocationObject;
		protected bool AllocationOrPrintingFailed;

		#endregion

		#region Test
#if DEBUG

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class DummyPaymentChequeNumberAllocatorBase : PaymentChequeNumberAllocatorBase
		{
			public DummyPaymentChequeNumberAllocatorBase(IChequeNumberAutoAllocation autoAllocationObject, BusinessObjectFactory factory)
				: base(autoAllocationObject, factory)
			{
			}

			public ITransactionParticipant[] GetFactoriesForTest()
			{
				return GetFactoriesWithAllocationCodeToBeCalledOnSaving();
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
