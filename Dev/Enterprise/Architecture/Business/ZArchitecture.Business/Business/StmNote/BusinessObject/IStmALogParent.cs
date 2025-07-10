using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	/// <summary>
	/// Its like IStmALogProvider, except with more cruft!
	/// </summary>
	public interface IStmALogParent : IBusiness, IStmALogProvider
	{
		bool IsDeleted { get; }
		ZGuid LogsParentPK { get; }
		string LogsParentTableName { get; }

		void ProcessLog(IStmALog log);
		BusinessObject[] BusinessObjectsWithRelatedEvents { get; }
		bool DeferFiringWorkflow { get; }
	}
}
