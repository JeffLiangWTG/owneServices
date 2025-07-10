using System;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	public class StlItemRegistrySettingsStub : IStlItemRegistrySettings
	{
		public DateTime HighWaterMark { get; set; }
		public bool HasHighWaterMarkBeenSet { get; }

		public void ClearHighWaterMark() { }
	}
}
