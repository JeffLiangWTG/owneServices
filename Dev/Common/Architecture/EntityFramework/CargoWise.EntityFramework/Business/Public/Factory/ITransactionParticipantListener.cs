using CargoWise.Integration;

namespace CargoWise.EntityFramework
{
	public interface ITransactionParticipantListener
	{
		void FactorySaveBeginning(ITransactionParticipant[] factories);
		void FactorySaveCompleted(ITransactionParticipant[] factories, bool successful);
	}
}
