using System;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class FeatureControlTestHelper
	{
		public class FeatureControlManagerDateTimeProvider : TimeProvider
		{
			public DateTime CurrentUtcDateTimeOverride { get; set; }

			public override DateTimeOffset GetUtcNow()
			{
				return CurrentUtcDateTimeOverride;
			}
		}
	}
}
