using System;
using CargoWise.Bi.Common;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	public class AnalyticsAPIForTest : TransactionedTestCase
	{
		public void TestVerifyAPIAserverThrowsExceptionWhenRegistryIsDsabled()
		{
			var originalValue = SystemDataRegistry.Instance.BiReportAPI.Value;
			using (SystemDataRegistry.Instance.BiReportAPI.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				try
				{
					BiServers.ClearBiServersCache();
					SystemDataRegistry.Instance.BiReportAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					AssertExceptionThrown<AnalyticsAPIException>(() => { AnalyticsAPI<TestModelHelper>.Instance.VerifyAPIIsEnabled(); });
					BiServers.ClearBiServersCache();
				}
				finally
				{
					SystemDataRegistry.Instance.BiReportAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
				}
			}
		}

		public void TestExceptionIsThrownWhenRegistryIsDisabled()
		{
			var originalValue = SystemDataRegistry.Instance.BiReportAPI.Value;
			using (SystemDataRegistry.Instance.BiReportAPI.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				try
				{
					BiServers.ClearBiServersCache();
					SystemDataRegistry.Instance.BiReportAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					AssertExceptionThrown<AnalyticsAPIException>(() => { AnalyticsAPI<TestModelHelper>.Instance.GetShipmentProfileReportData("S00001000", "EDI", "AU"); });
					BiServers.ClearBiServersCache();
				}
				finally
				{
					SystemDataRegistry.Instance.BiReportAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
				}
			}
		}
	}
}
