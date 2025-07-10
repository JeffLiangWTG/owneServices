using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IImportResult : IEntityID, ISimpleLogResult
	{
		string ToString();
		bool WasSuccessful { get; }
		IEnumerable<IEntityID> LinkedJobs { get; }
	}
}
