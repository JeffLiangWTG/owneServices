using System.Collections.Generic;
using System.Linq;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ElectronicPayment.EPaymentStaffToken.Testing
{
	[TestedType(typeof(EPaymentStaffTokenServiceTask))]
	public class EPaymentStaffTokenServiceTaskTest : ServiceTaskTestCase<EPaymentStaffTokenServiceTask>
	{
		public void TestInitialiseTask()
		{
			AssertEquals("15minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestSerivceTaskIsMandatoryAndScheduleReadOnly()
		{
			var attribute = GetHostedServiceAttributes().SingleOrDefault();
			AssertNotNull(attribute);
			Assert($"{attribute.Code} service task must be mandatory", attribute.IsMandatory);
			Assert($"{attribute.Code} service task schedule must be readonly", attribute.IsScheduleReadOnly);
		}

		public void TestRunTask()
		{
			var serviceTask = new EPaymentStaffTokenServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			var log = logger.ToString();

			AssertContains("Information|Process EPayment Provider Staff Token service task started.", log);
			AssertContains("Information|Process EPayment Provider Staff Token service task completed.", log);

			using (Env.Instance.TemporaryServiceTaskContext(EPaymentStaffTokenServiceTask.Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						null,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + EDIInterchangeTypeList.Codes.Configuration,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.OFX),
				};
			}
		}
	}
}
