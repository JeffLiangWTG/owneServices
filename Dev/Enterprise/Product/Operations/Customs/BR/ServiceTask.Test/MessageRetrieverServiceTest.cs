using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.BR.ServiceTasks.Testing
{
	[TestedType(typeof(MessageRetrieverService))]
	class MessageRetrieverServiceTest : ServiceTaskTestCase<MessageRetrieverService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "BRP", hostedServiceAttribute.Code);
				AssertEquals("Description", "Brazil Customs Message Processor", hostedServiceAttribute.Description);
				AssertEquals("Category", "BRC", hostedServiceAttribute.Category);
				AssertEquals("RequiresCompanyInCountry", Enterprise.Core.Constants.CountryCodes.Brazil, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("MinimumPeriod", "30seconds", hostedServiceAttribute.MinimumPeriod);
			});
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(MessageRetrieverService).GetMethod(nameof(MessageRetrieverService.IsRequired));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			CertificateRequirementChecker.ResetForTesting();
			var isRequired = "";
			AssertNoExceptionThrown(() => isRequired = MessageRetrieverService.IsRequired());
			AssertEquals("There is no Certificate configured in Brazil.", MessageRetrieverService.IsRequired());
			GlbExternalPasswordHelperTest.SetupGlbExternalPassword_CCT(Factory);
			CertificateRequirementChecker.ResetForTesting();
			AssertEquals(string.Empty, MessageInterchangeProcessorService.IsRequired());
		}

		public void TestRunTask()
		{
			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var caBranch = Factory.NewWithValidTestData<GlbBranch>();
			caCompany.Branches.Add(caBranch);
			Factory.Save();

			var message1 = CreateMessage(EDIMessage.ApplicationCodes.BRCustoms, MessageTypeList.Codes.CDE, Business.EDIMessageSubTypeList.Codes.Success);
			var message2 = CreateMessage(EDIMessage.ApplicationCodes.BRCustoms, MessageTypeList.Codes.CDE, Business.EDIMessageSubTypeList.Codes.Error);
			var message3 = CreateMessage(EDIMessage.ApplicationCodes.CACustoms, MessageTypeList.Codes.CDE, Business.EDIMessageSubTypeList.Codes.Success);
			var message4 = CreateMessage(EDIMessage.ApplicationCodes.BRCustoms, MessageTypeList.Codes.CDE, Business.EDIMessageSubTypeList.Codes.Success);
			message4.EM_GB = caBranch.PK;
			Factory.Save();

			CombineAssertions(() =>
			{
				InitialiseAndRunTaskSchedule(new MessageRetrieverService());

				var anotherFactory = new BusinessObjectFactory();
				AssertNotEquals("BRC|CDE|SUC processed", EDIMessage.Status.Queued, anotherFactory.Load<EDIMessage>(message1.PK).EM_Status);
				AssertNotEquals("BRC|CDE|ERR processed", EDIMessage.Status.Queued, anotherFactory.Load<EDIMessage>(message2.PK).EM_Status);
				AssertEquals("CAC|CDE|SUC not processed", EDIMessage.Status.Queued, anotherFactory.Load<EDIMessage>(message3.PK).EM_Status);
				AssertEquals("BRC|CDE|SUC in CA branch not processed", EDIMessage.Status.Queued, anotherFactory.Load<EDIMessage>(message4.PK).EM_Status);
			});
		}

		EDIMessage CreateMessage(string applicationCode, string messageType, string messageSubType)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_IsActive = true;
			message.EM_MessageText = "TEST";
			return message;
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"BR Customs messages inbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.BRCustoms),
				};
			}
		}
	}
}
