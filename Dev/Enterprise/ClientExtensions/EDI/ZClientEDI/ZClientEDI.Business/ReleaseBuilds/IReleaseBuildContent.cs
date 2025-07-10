using System;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;

namespace Enterprise.Client.EDI.ReleaseBuilds
{
	public interface IReleaseBuildContent
	{
		bool IsPatchedTo(NewWorkItem workItem, ReleaseBuild releaseBuild);
		bool IsPatchedTo(NewWorkItem workItem, Version currentVersion);
		bool IsCargoWiseOneChange(NewWorkItem workItem);
	}
}
