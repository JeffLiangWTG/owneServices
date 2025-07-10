using System.Collections.Generic;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine
{
	class ArchiveStepResult : IArchiveStepResult
	{
		#region IArchiveStepResult Members

		public List<string> ErrorsEncountered { get; set; } = new List<string>();

		#endregion
	}
}
