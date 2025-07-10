using System;

namespace CargoWise.Integration
{
	public interface ITransactionManager : IDisposable
	{
		void CommitTransaction();

		void RollbackTransaction();
	}
}