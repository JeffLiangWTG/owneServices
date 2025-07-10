using System.Collections.Generic;

namespace Enterprise.DbUpgrader.Transformation.Common;

public interface ISessionWaitStatsReporter
{
	void StartRecording();
	IReadOnlyCollection<SessionWaitStats> GetSessionWaits();
}
