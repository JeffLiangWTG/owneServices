using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.BE.ServiceTasks.Testing;

[TestedType(typeof(MessageRetrieverService))]
sealed class MessageRetrieverServiceTest : ServiceTaskTestCase<MessageRetrieverService>
{
	public void TestHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
		var hostedServiceAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Code", "BER", hostedServiceAttribute.Code);
			AssertEquals("Description", "BE Customs interchanges inbound", hostedServiceAttribute.Description);
			AssertEquals("Category", "BEC", hostedServiceAttribute.Category);
			AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Belgium, hostedServiceAttribute.RequiresCompanyInCountry);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
		});
	}

	public void TestSuccessfulInterchange()
	{
		var interchange = CreateInterchangeAndRunService("<CC004C><messageType>CC004C</messageType></CC004C>");

		CombineAssertions(() =>
		{
			AssertEquals("The interchange should be successful", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Log should not exist that message failed.", false, interchange.Logs.Find(l => l.Event.SE_Code == "ERR").Any());
			var message = (EDIMessage)interchange.ContainedMessages.Single();
			AssertEquals("Message should be queued", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("Message should have correct message type", "004", message.EM_MessageSubType);
			AssertEquals("EM_MessageNum", interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength), message.EM_MessageNum);
			AssertEquals("EM_MessageText", "<CC004C><messageType>CC004C</messageType></CC004C>", message.EM_MessageText);
		});
	}

	public void TestEmptyTextInterchange()
	{
		var interchange = CreateInterchangeAndRunService(ZString.Empty);

		CombineAssertions(() =>
		{
			AssertEquals("The interchange should have failed", EDIInterchange.Status.Error, interchange.EI_Status);
			AssertNotNull("Log should exist that message failed.", interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport, "The Message XML is Empty, the Message creation failed"));
			AssertEquals("No messages should be created", 0, interchange.ContainedMessages.Count);
		});
	}

	public void TestInvalidMessageInterchange()
	{
		var interchange = CreateInterchangeAndRunService("<element>value</wrongelement>");

		CombineAssertions(() =>
		{
			AssertEquals("The interchange should have failed", EDIInterchange.Status.Error, interchange.EI_Status);
			AssertNotNull("Log should exist that message failed.", interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport, "The Message XML is not a valid XML, the Message creation failed"));
			AssertEquals("No messages should be created", 0, interchange.ContainedMessages.Count);
		});
	}

	public void TestIncorrectMessageTypeInterchange()
	{
		var interchange = CreateInterchangeAndRunService("<CC007C><messageType>invalidMessageType</messageType></CC007C>");

		CombineAssertions(() =>
		{
			AssertEquals("The interchange should have failed", EDIInterchange.Status.Error, interchange.EI_Status);
			AssertNotNull("Log should exist that message failed.", interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport, $"Interchange {interchange.EI_InterchangeNum}: Can not determine Message Sub Type for the Received Interchange"));
			AssertEquals("No messages should be created", 0, interchange.ContainedMessages.Count);
		});
	}
	public void TestHostedServiceRequirement()
	{
		var hostedServiceRequirementMethod = typeof(MessageRetrieverService).GetMethods().FirstOrDefault(p => Attribute.IsDefined(p, typeof(HostedServiceRequirementAttribute)));
		AssertNotNull("Method with Attribute HostedServiceRequirement should exist on MessageRetrieverService", hostedServiceRequirementMethod);

		CombineAssertions(() =>
		{
			var combinations = new List<(ZString passwordType, string requiredExpected)>
									{ ("", "There is no Credential configured in Belgium."),
									  (PasswordTypesList.Codes.BEC, string.Empty) };
			var credential = BE.Business.BEGlbCompanyWrapper.GetWrapper<BE.Business.BEGlbCompanyWrapper>(GlbCompany.CurrentCompany).Credential;

			foreach (var combination in combinations)
			{
				credential.GP_PasswordType = combination.passwordType;
				credential.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
				credential.Factory.Save();
				CertificateRequirementChecker.ResetForTesting();
				AssertEquals($"Credential Password Type: {combination.passwordType}", combination.requiredExpected, hostedServiceRequirementMethod.Invoke(null, null));
			}
		});
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[]
	{
		new TaskNudgeInformationForTest(
			EDIInterchangeSchema.Constants.TableName,
			"BE Customs interchanges inbound",
			EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
			EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
			EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
			EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + ApplicationCodeList.Codes.BECustoms),
	};

	EDIInterchange CreateInterchangeAndRunService(ZString interchangeBodyText)
	{
		var interchange = Factory.NewWithValidTestData<BECInterchange>();
		interchange.EI_ApplicationCode = "BEC";
		interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange.EI_Status = EDIInterchange.Status.Queued;
		interchange.EI_IsActive = true;
		interchange.ForceDeprecatedNTextUsageForTesting = true;
		interchange.EI_BodyText = interchangeBodyText;
		Factory.Save();

		var service = new MessageRetrieverService();
		InitialiseTaskSchedule(service);
		using (Env.Instance.TemporaryServiceTaskContext(MessageRetrieverService.Code, canRunInAnyBranch: true))
		{
			AssertNoExceptionThrown(service.RunTask);
		}

		return new BusinessObjectFactory().Load<EDIInterchange>(interchange.PK);
	}
}
