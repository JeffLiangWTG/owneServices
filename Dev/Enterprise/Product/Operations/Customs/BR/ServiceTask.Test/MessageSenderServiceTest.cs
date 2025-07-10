using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
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
	[TestedType(typeof(MessageSenderService))]
	class MessageSenderServiceTest : ServiceTaskTestCase<MessageSenderService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "BRS", hostedServiceAttribute.Code);
				AssertEquals("Description", "Brazil Customs Interchange Sender", hostedServiceAttribute.Description);
				AssertEquals("Category", "BRC", hostedServiceAttribute.Category);
				AssertEquals("RequiresCompanyInCountry", Enterprise.Core.Constants.CountryCodes.Brazil, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("MinimumPeriod", "30seconds", hostedServiceAttribute.MinimumPeriod);
			});
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(MessageSenderService).GetMethod(nameof(MessageSenderService.IsRequired));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			CertificateRequirementChecker.ResetForTesting();
			var isRequired = "";
			AssertNoExceptionThrown(() => isRequired = MessageSenderService.IsRequired());
			AssertEquals("There is no Certificate configured in Brazil.", MessageSenderService.IsRequired());
			GlbExternalPasswordHelperTest.SetupGlbExternalPassword_CCT(Factory);
			CertificateRequirementChecker.ResetForTesting();
			AssertEquals(string.Empty, MessageInterchangeProcessorService.IsRequired());
		}

		public void TestRunTask()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			var message1 = CreateMessage(Factory, entryHeader, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDI, Business.EDIMessageSubTypeList.Codes.Original);
			var message2 = CreateMessage(Factory, entryHeader, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDE, Business.EDIMessageSubTypeList.Codes.Original);

			Factory.Save();

			CombineAssertions(() =>
			{
				InitialiseAndRunTaskSchedule(new MessageSenderService());

				message1.Reload();
				AssertEquals("EI_Status", EDIInterchange.Status.Queued, Factory.Load<EDIInterchange>(message1.EM_EI).EI_Status);
				message2.Reload();
				AssertEquals("EI_Status", EDIInterchange.Status.Queued, Factory.Load<EDIInterchange>(message2.EM_EI).EI_Status);
			});
		}

		BREDIMessage CreateMessage(BusinessObjectFactory factory, CusEntryHeader entryHeader, ZString direction, ZString status, ZString type, ZString subType)
		{
			var message = factory.New<BREDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			message.EM_ApplicationReference = "APRN0000001";
			message.EM_MessageNum = "001";
			message.EM_MessageType = type;
			message.EM_MessageSubType = subType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_IsActive = true;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_LinkedObject = entryHeader;
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
						"Brazil Customs Interchange Sender",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.BRCustoms),
				};
			}
		}
	}
}
