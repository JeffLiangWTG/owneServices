
using CargoWise.Data;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.PAVE.MENT.Shared
{
	public class DataCollectionStrategy : IDataCollectionStrategy
	{
		string IDataCollectionStrategy.QueryText
		{
			get { return QueryText; }
		}

		protected virtual string QueryText
		{
			get { return string.Empty; }
		}

		void IDataCollectionStrategy.PerformPreQueryOperation(DbConnection readerConnect)
		{
			PerformPreQueryOperation(readerConnect);
		}

		protected virtual void PerformPreQueryOperation(DbConnection readerConnect)
		{
		}

		void IDataCollectionStrategy.PerformPostQueryOperation(DbConnection readerConnect)
		{
			PerformPostQueryOperation(readerConnect);
		}

		protected virtual void PerformPostQueryOperation(DbConnection readerConnect)
		{
		}
	}
}
