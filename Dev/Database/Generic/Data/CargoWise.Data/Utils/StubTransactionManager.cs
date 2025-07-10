using CargoWise.Integration;

namespace CargoWise.Data
{
	public sealed class StubTransactionManager : ITransactionManager
	{
		public void Dispose() { }
		public void CommitTransaction() { }
		public void RollbackTransaction() { }
	}
}
