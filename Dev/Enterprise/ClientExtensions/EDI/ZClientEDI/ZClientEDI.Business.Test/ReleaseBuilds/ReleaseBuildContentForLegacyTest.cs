using System;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;

namespace Enterprise.Client.EDI.ReleaseBuilds.Test
{
	// Use old simple logic of exe dates and check-in tasks to avoid rewriting all existing tests to use DAT data
	public class ReleaseBuildContentForLegacyTest : IReleaseBuildContent
	{
		public static void Enable() => ReleaseBuildContent.Factory.Value = _ => new ReleaseBuildContentForLegacyTest();

		public bool IsPatchedTo(NewWorkItem workItem, ReleaseBuild releaseBuild)
		{
			var ringCode = releaseBuild.HL_ReleaseStatus;
			var task = workItem.GetClosedShelfCheckInTasksForReleaseRing(ringCode);
			var isPatched = task != null && releaseBuild.HL_ExeVersionDate >= task.CompletedTimeLocal.ToSmallDateTimeFloor();
			if (!isPatched)
			{
				task = null;
				do
				{
					ringCode = ReleaseRingsLookup.Instance.GetPreviousRing(ringCode);
					task = workItem.GetClosedShelfCheckInTasksForReleaseRing(ringCode);
				}
				while (!string.IsNullOrEmpty(ringCode) && task == null);

				isPatched = task != null && releaseBuild.HL_ExeVersionDate >= task.CompletedTimeLocal;
			}
			return isPatched;
		}

		public bool IsCargoWiseOneChange(NewWorkItem workItem)
		{
			return workItem.HasClosedCheckInTasks && IsCargowiseOneChangeOverride;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread-static")]
		public static bool IsCargowiseOneChangeOverride = true;

		public bool IsPatchedTo(NewWorkItem workItem, Version currentVersion)
		{
			throw new NotImplementedException();
		}
	}
}
