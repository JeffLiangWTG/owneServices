
using CargoWise.Data;

namespace Enterprise.BufferManagement.Integration
{
	public interface IDataCollectionStrategy
	{
		string QueryText { get; }

		void PerformPreQueryOperation(DbConnection readerConnect);
		void PerformPostQueryOperation(DbConnection readerConnect);
	}
}
