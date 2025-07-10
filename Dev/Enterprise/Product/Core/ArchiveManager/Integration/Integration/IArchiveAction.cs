using CargoWise.Integration;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveAction : ITransactionStarter
	{
		void Execute();
	}
}
