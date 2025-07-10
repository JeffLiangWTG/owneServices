using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.CN.ServiceTasks.Testing
{
	[TestedType(typeof(MessageSenderServiceTask))]
	class MessageSenderServiceTaskTest : ServiceTaskTestCase<MessageSenderServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "CNO", hostedServiceAttribute.Code);
				AssertEquals("Description", "China Customs Message Sender", hostedServiceAttribute.Description);
				AssertEquals("Category", "CNC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "60Second", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.China, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestHostedServiceRequirementIsApplied()
		{
			var methodInfo = typeof(MessageSenderServiceTask).GetMethod(nameof(MessageSenderServiceTask.CheckCNSWClientSetting));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			ServiceTaskEnvironmentCheckerTest.TestCheckCNSWClientSetting(Factory, MessageSenderServiceTask.CheckCNSWClientSetting);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Codes.CNMessageSenderServiceTask,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
					),
				};
			}
		}

		public void TestRunTask()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "DCN";
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;

			var branch1 = company1.Branches.AddNew();
			branch1.FillWithValidTestData();
			branch1.GB_Code = "SHA";
			branch1.GB_RL_NKHomePort = "CNSHA";

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "DTW";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

			var branch2 = company2.Branches.AddNew();
			branch2.FillWithValidTestData();
			branch2.GB_Code = "TPE";
			branch2.GB_RL_NKHomePort = "TWTPE";

			var message1 = Factory.New<CNEDIMessage>();
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.CNCustomsSingleWindow;
			message1.EM_GB = branch1.PK;

			var message2 = Factory.New<CNEDIMessage>();
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.TWCustoms;
			message2.EM_GB = branch1.PK;

			var message3 = Factory.New<CNEDIMessage>();
			message3.EM_ApplicationCode = ApplicationCodeList.Codes.CNCustomsSingleWindow;
			message3.EM_GB = branch2.PK;

			var message4 = Factory.New<CNEDIMessage>();
			message4.EM_ApplicationCode = ApplicationCodeList.Codes.TWCustoms;
			message4.EM_GB = branch2.PK;

			Factory.Save();

			var serviceTask = new MessageSenderServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);

			message1.Reload();
			CombineAssertions("Sent: CSW CN SHA Message", () =>
			{
				AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
				AssertNotNull(Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_InterchangeNum, message1.EM_MessageNum)));
			});

			message2.Reload();
			CombineAssertions("Not Sent: TWC CN SHA Message", () =>
			{
				AssertEquals(EDIMessage.Status.Queued, message2.EM_Status);
				AssertNull(Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_InterchangeNum, message2.EM_MessageNum)));
			});

			message3.Reload();
			CombineAssertions("Not Sent: CSW TW TPE Message", () =>
			{
				AssertEquals(EDIMessage.Status.Queued, message3.EM_Status);
				AssertNull(Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_InterchangeNum, message3.EM_MessageNum)));
			});

			message4.Reload();
			CombineAssertions("Not Sent: TWC TW TPE Message", () =>
			{
				AssertEquals(EDIMessage.Status.Queued, message4.EM_Status);
				AssertNull(Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_InterchangeNum, message4.EM_MessageNum)));
			});
		}
	}
}
