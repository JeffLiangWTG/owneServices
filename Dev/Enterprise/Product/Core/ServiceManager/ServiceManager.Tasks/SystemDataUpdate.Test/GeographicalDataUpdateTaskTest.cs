using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.SystemDataUpdate.Testing
{
	[TestedType(typeof(GeographicalDataUpdateTask))]
	sealed class GeographicalDataUpdateTaskTest : ServiceTaskTestCase<GeographicalDataUpdateTask>
	{
		[ExpectNoExceptions]
		public void TestRunTask_WhenInvalidUrl_ShouldNotThrowException()
		{
			const string invalidUrl = "https://foobar";

			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Primary.ServiceUri = invalidUrl, Factory))
			{
				var testTask = new GeographicalDataUpdateTask
				{
					ServiceLogger = new TestServiceLogger()
				};

				testTask.RunTask(CancellationToken.None);
			}
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "GDU", hostedServiceAttribute.Code);
				AssertEquals("Description", "Geographical Data Update Service", hostedServiceAttribute.Description);
				AssertEquals("Category", "SYS", hostedServiceAttribute.Category);
				AssertEquals("Type", typeof(GeographicalDataUpdateTask).FullName, hostedServiceAttribute.TypeName);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "1week", hostedServiceAttribute.MinimumPeriod);
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
