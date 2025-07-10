using System;

namespace Enterprise.Integration.Billing
{
	public interface IStlItemRegistrySettings
	{
		bool HasHighWaterMarkBeenSet { get; }
		DateTime HighWaterMark { get; set; }

		void ClearHighWaterMark();
	}
}
