using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	enum ReportRunningType
	{
		Document,
		Report,
		NormalScheduledReport,
		OneOffScheduledReport,
		DocumentReferenceGuide,
		ReportReferenceGuide
	}

	public static class ReportRunningConstants
	{
		public static ZGuid DocumentReferenceMenuItemPk => new ZGuid("97a00227-89fa-42ea-a8a1-fc3d5063ec15");
		public static ZGuid ReportReferenceMenuItemPk => new ZGuid("bb4a440c-4b39-4d4c-801e-a6ca21e6f65f");
	}
}
