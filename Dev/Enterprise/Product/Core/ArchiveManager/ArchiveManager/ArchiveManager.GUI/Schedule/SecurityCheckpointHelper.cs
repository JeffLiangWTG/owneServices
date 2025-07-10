using System.Collections.Generic;
using Enterprise.ArchiveManager.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.ArchiveManager.GUI
{
	public static class SecurityCheckpointHelper
	{
		static Dictionary<string, SecurityCheckpoint> CheckPoint
		{
			get => new()
			{
				{ ArchiveManagerConstants.Codes.PDR, Env.Security.ArchiveSchedulePurge },
				{ ArchiveManagerConstants.Codes.PAR, Env.Security.ArchiveRecordsPurge  },
				{ ArchiveManagerConstants.Codes.PDO, Env.Security.ArchiveSchedulePurge },
				{ ArchiveManagerConstants.Codes.EST, Env.Security.ArchiveSchedulePurge },
				{ ArchiveManagerConstants.Codes.PAL, Env.Security.ArchiveSchedulePurge },
				{ ArchiveManagerConstants.Codes.RED, Env.Security.ArchiveSchedulePurge },
			};
		}

		public static SecurityCheckpoint GetCheckpoint(string code)
			=> CheckPoint.TryGetValue(code, out var value) ? value : null;
	}
}
