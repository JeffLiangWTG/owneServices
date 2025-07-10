using System;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using NUnit.Framework;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	class WinFormPowerBiReportLinkBuilderTest : TestCase
	{
		public void TestFactory()
		{
			var factory = new WinFormPowerBiReportLinkBuilder.Factory();
			var linkBuilder = factory.NewLinkBuilder(registration, new Uri("http://TestUrl"));
			AssertEquals("WinFormPowerBiReportLinkBuilder.Factory creates instances of WinFormPowerBiReportLinkBuilder", true, linkBuilder is WinFormPowerBiReportLinkBuilder);
		}

		public void TestGetReportPath()
		{
			var testReport = new PowerBiReportTestCase();
			var factory = new WinFormPowerBiReportLinkBuilder.Factory();
			var linkBuilder = factory.NewLinkBuilder(registration, new Uri("http://TestUrl"));
			AssertEquals("testReport Report Link", $"http://testurl//{testReport.ResourceType}/{registration.Key.EnterpriseCode}/{registration.Key.ServerCode}/Analytics/{testReport.BusinessArea}/TestName?rs:Embed=true", linkBuilder.GetReportPath(testReport));
		}

		readonly IProductRegistration registration = ObjectFactory.Get<IProductRegistration>();
	}
}
