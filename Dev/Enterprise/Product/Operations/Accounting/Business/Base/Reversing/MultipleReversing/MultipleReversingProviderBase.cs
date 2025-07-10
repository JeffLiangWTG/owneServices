using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public abstract class MultipleReversingProviderBase : NonPersistentBusinessObject, IEnumerable, IEnumerator, IObsoleteValidation, IHandleDeleteError
	{
		protected MultipleReversingProviderBase() : base(new BusinessObjectFactory())
		{
		}

		public abstract BusinessObjectCollection BizObjectsForReversing { get; }

		public abstract BusinessObjectCollection BizObjectsAlreadyReversed { get; }

		public abstract BusinessObject GetWrappedBusinessEntity(BusinessObject bizObjectAlreadyReversed);

		public bool RollbackAfterDeleteError => false;

		public bool RebindAfterDeleteError => false;

		public bool DisableFormOnDeleteConcurrencyError => true;

		protected int CurrentTransactionsForReversingIndex
		{
			get;
			set;
		}

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			Reset();
			return this;
		}

		#endregion

		#region IEnumerator Members

		public BusinessObject Current => BizObjectsForReversing.ElementAt(CurrentTransactionsForReversingIndex);

		object IEnumerator.Current => Current;

		public bool MoveNext()
		{
			CurrentTransactionsForReversingIndex++;
			return CurrentTransactionsForReversingIndex < BizObjectsForReversing.Count;
		}

		public void Reset()
		{
			CurrentTransactionsForReversingIndex = -1;
		}

		#endregion
	}
}
