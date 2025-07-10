using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.Telematics.ServiceTasks;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace ZClientEDI.Test.Telematics.ServiceTasks
{
	[TestedType(typeof(TelematicsRimEnrolmentReportServiceTask))]
	class TelematicsRimEnrolmentReportServiceTaskTest : ServiceTaskTestCase<TelematicsRimEnrolmentReportServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
		}

		public void TestCode()
		{
			AssertEquals("RIM", TelematicsRimEnrolmentReportServiceTask.Code);
		}

		public void TestHostedServiceAttributeParameters()
		{
			// Arrange
			var attributes = typeof(TelematicsRimEnrolmentReportServiceTask)
				.Assembly
				.GetCustomAttributes(true)
				.OfType<HostedServiceAttribute>();

			// Act
			var result = attributes
				.Single(attribute => attribute.Code == TelematicsRimEnrolmentReportServiceTask.Code);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("TEL", result.Category);
				AssertEquals(typeof(TelematicsRimEnrolmentReportServiceTask), result.Type);
			});
		}

		public void TestHostedServiceBusinessObjectBindingAttributeParameters()
		{
			// Arrange
			var attributes = typeof(TelematicsRimEnrolmentReportServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceBusinessObjectBindingAttribute>();

			// Act
			var result = attributes
				.SingleOrDefault(attribute => attribute.ServiceTaskCode == TelematicsRimEnrolmentReportServiceTask.Code);

			// Assert
			AssertNull(result);
		}

		public void TestHostedServiceMinimumPeriod()
		{
			// Arrange
			var result = GetHostedServiceAttributes()
				.SingleOrDefault()
				.MinimumPeriod;

			// Act
			// Assert
			AssertEquals("1month", result);
		}

		public void TestHostedServiceMaximumPeriod()
		{
			// Arrange
			var result = GetHostedServiceAttributes()
				.SingleOrDefault()
				.MaximumPeriod;

			// Act
			// Assert
			AssertEquals("1month", result);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
