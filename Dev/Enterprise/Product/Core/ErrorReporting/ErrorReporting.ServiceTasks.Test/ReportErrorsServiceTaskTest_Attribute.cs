using System.Linq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ErrorReporting.ServiceTasks.Test
{
	sealed class ReportErrorsServiceTaskTest_Attribute : TestCase
	{
		public void TestHostedServiceAttributes()
		{
			// Arrange
			var attributes = typeof(ReportErrorsServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>();

			// Act
			var attribute = attributes.Single(a => a.TypeName == typeof(ReportErrorsServiceTask).FullName);

			// Assert
			AssertEquals("HostedService attribute Code should be RET", attribute.Code, ReportErrorsServiceTask.Code);
			AssertEquals("HostedService attribute Category should be SYS", attribute.Category, "SYS");
			AssertEquals("HostedService attribute IsMandatory should be true", attribute.IsMandatory, true);
		}
	}
}
