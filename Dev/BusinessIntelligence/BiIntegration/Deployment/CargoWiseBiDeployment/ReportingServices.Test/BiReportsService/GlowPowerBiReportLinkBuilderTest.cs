using System;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	class GlowPowerBiReportLinkBuilderTest : TestCase
	{
		public void TestFactory()
		{
			var factory = new GlowPowerBiReportLinkBuilder.Factory();
			var linkBuilder = factory.NewLinkBuilder(registration, new Uri("http://TestUrl"));
			AssertEquals("GlowPowerBiReportLinkBuilder.Factory creates instances of GlowPowerBiReportLinkBuilder", true, linkBuilder is GlowPowerBiReportLinkBuilder);
		}

		public void TestGetReportPath()
		{
			var testReport = new PowerBiReportTestCase();
			var glowURI = GlowRegistry.Instance.GlowServiceUriRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var factory = new GlowPowerBiReportLinkBuilder.Factory();
			var linkBuilder = factory.NewLinkBuilder(registration, new Uri("http://TestUrl"));
			AssertEquals("testReport Report Link", glowURI + $"cw1api/analytics/loadReport///{testReport.ResourceType}/{registration.Key.EnterpriseCode}/{registration.Key.ServerCode}/Analytics/{testReport.BusinessArea}/TestName?rs:Embed=true", linkBuilder.GetReportPath(testReport));
		}

		readonly IProductRegistration registration = ObjectFactory.Get<IProductRegistration>();
	}
}
