using System.Collections.Generic;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveStepResult
	{
		List<string> ErrorsEncountered { get; }
	}
}
