using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public interface IReversing : ITransaction
	{
		bool IsReversing { get; }
		bool IsReversed { get; }
		bool IsReverseTransaction { get; }
		void GenerateReverseTransaction(bool mustTransform);
		IReversing ReverseTransaction { get; }
		void SetCancellationFlag(bool cancel);
		void SetTransactionBelongsToGroupField(ZGuid groupingGuidValue);
		void SetDescription(ZString descriptionToSet);
		void SetNumberOfSupportingDocuments(ZByte numberOfSupportingDocumentsToSet);
		void ApplyWorkflowTemplatesOnReverseTransaction();
		ZString ReversingReason { get; set; }
		ZString ReversingCode { get; set; }
		bool IsClearedInCashbook { get; }
		string[] MultipleReversingErrors { get; set; }
	}
}

	#region Test
#if DEBUG
namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	using CargoWise.EntityFramework;
	using Enterprise.Accounting.Business.Base.Transaction;

	public class TestIReverseTransaction : TestITransaction, IReversingForTests
	{
		#region IReversing Members

		#region IsReversing

		public bool IsReversing
		{
			get { return fIsReversing; }
		}

		public void SetIsReversing(bool value)
		{
			fIsReversing = value;
		}
		protected bool fIsReversing;

		#endregion

		#region IsReversed

		public bool IsReversed
		{
			get { return fIsReversed; }
		}

		public void SetIsReversed(bool value)
		{
			fIsReversed = value;
		}
		protected bool fIsReversed;

		#endregion

		#region IsReverseTransaction

		protected bool fIsReverseTransaction;
		public bool IsReverseTransaction
		{
			get { return fIsReverseTransaction; }
		}

		#endregion

		#region SetReverseTransactionToBeGenerated

		public void SetReverseTransactionToBeGenerated(IReversing reverseTransaction)
		{
			fReverseTransaction = reverseTransaction;
		}

		#endregion

		#region GenerateReverseTransaction

		public void GenerateReverseTransaction(bool mustTransform)
		{
		}

		#endregion

		#region SetDescription

		public void SetDescription(ZString descriptionToSet)
		{
		}

		#endregion
		public void SetNumberOfSupportingDocuments(ZByte numberOfSupportingDocumentsToSet)
		{
		}

		public void ApplyWorkflowTemplatesOnReverseTransaction()
		{
		}

		#region ReversingReason

		public ZString ReversingReason
		{
			get { return fReversingReason; }
			set { fReversingReason = value; }
		}
		protected ZString fReversingReason;

		#endregion

		#region ReversingCode

		public ZString ReversingCode
		{
			get { return fReversingCode; }
			set { fReversingCode = value; }
		}
		protected ZString fReversingCode;

		#endregion

		#region ReverseTransaction

		protected IReversing fReverseTransaction;
		public IReversing ReverseTransaction
		{
			get { return fReverseTransaction; }
		}

		#endregion

		#region SetCancellationFlag

		public void SetCancellationFlag(bool cancel)
		{
		}

		#endregion

		#region SetTransactionBelongsToGroupField

		public void SetTransactionBelongsToGroupField(ZGuid groupingGuidValue)
		{
		}

		#endregion

		#region IsClearedInCashbook

		public bool IsClearedInCashbook
		{
			get { return fIsClearedInCashBook; }
		}

		public void SetIsClearedInCashbook(bool value)
		{
			fIsClearedInCashBook = value;
		}
		bool fIsClearedInCashBook;

		#endregion

		bool IBusiness.CanContinueWithSave
		{
			get { return true; }
		}

		public string[] MultipleReversingErrors
		{
			get
			{
				throw new System.NotImplementedException();
			}
			set
			{
				throw new System.NotImplementedException();
			}
		}

		#endregion
	}

	public interface IReversingTest
	{
		void TestIsReversedImplementation();
		void TestSetCancellationFlag();
		void TestReverseTransaction();
		void TestSetTransactionBelongsToGroupField();
		void TestSetDescription();
		void TestReversingReason();
	}

	public interface IReversingForTests : IReversing, ITransactionForTests
	{
		void SetIsClearedInCashbook(bool value);
		void SetIsReversed(bool value);
		void SetIsReversing(bool value);
		void SetReverseTransactionToBeGenerated(IReversing reverseTransaction);
	}
}
#endif
	#endregion
