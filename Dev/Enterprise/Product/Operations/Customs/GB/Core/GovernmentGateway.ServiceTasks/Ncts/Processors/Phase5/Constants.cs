namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	static class Constants
	{
		public static class ReleaseIndicator
		{
			public const string FullRelease = "1";
			public const string PartialRelease = "2";
			public const string PartialReleaseClosed = "3";
			public const string NoRelease = "4";
		}

		public static class ReleaseType
		{
			public const string PartialRelease = "1";
			public const string FullRelease = "2";
		}

		public static class ServiceTypes
		{
			public const string CTL = "CTL";
		}
	}
}
