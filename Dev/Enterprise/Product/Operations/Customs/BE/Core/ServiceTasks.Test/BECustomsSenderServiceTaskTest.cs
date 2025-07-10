using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.BE.ServiceTasks.Testing;

[TestedType(typeof(BECustomsSenderServiceTask))]
sealed class BECustomsSenderServiceTaskTest : MessagingServiceTaskAbstractTest<BECustomsSenderServiceTask>
{
	public void TestProcess()
	{
		var bemessage = Factory.New<BEMessage>();
		bemessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.BECustoms;
		bemessage.EM_MessageType = SendMessageTypes.Codes.NCT;
		bemessage.EM_MessageNum = "1";
		bemessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		bemessage.EM_Status = EDIMessage.Status.Queued;
		bemessage.EM_IsActive = true;
		bemessage.EM_MessageText = "<?xml version='1.0' encoding='utf - 8'?><MetaData xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Declaration:1'><WCOTypeCode>CC432A</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>EH00001</ApplicationReferenceID><CommunicationsAgreementID>18</CommunicationsAgreementID><PreparationDateTime formatCode='304'>20220224154823Z</PreparationDateTime><Recipient><ID>DMS.NL</ID></Recipient><Sender><ID>NL56785678</ID></Sender></CommunicationMetaData><Declaration><FunctionalReferenceID>EH00001</FunctionalReferenceID><ID>MRN123</ID><DeclarationOffice><ID>NL55566677</ID></DeclarationOffice><Agent><ID>NL43434343</ID><FunctionCode>DIR</FunctionCode></Agent><Declarant><Name>Declarant Full Name</Name><ID>NL56785678</ID><Address><CityName>Decapolis</CityName><CountryCode>NL</CountryCode><Line>Declarantenstraat 30</Line><PostcodeID>5890DW</PostcodeID></Address></Declarant><GoodsShipment><SequenceNumeric>1</SequenceNumeric><Consignment><GoodsLocation><TypeCode>A</TypeCode><IdentificationTypeCode>T</IdentificationTypeCode><Address><CountryCode>NL</CountryCode><PostcodeID>1234AB</PostcodeID><StreetNumberID>Kerkstraat 1</StreetNumberID></Address></GoodsLocation><TransportEquipment><SequenceNumeric>1</SequenceNumeric><ID>APLU8521458</ID><GoodsReference><SequenceNumeric>1</SequenceNumeric><GoodsItemNumeric>1</GoodsItemNumeric></GoodsReference><GoodsReference><SequenceNumeric>2</SequenceNumeric><GoodsItemNumeric>1</GoodsItemNumeric></GoodsReference></TransportEquipment><TransportEquipment><SequenceNumeric>2</SequenceNumeric><ID>MSCU0051257</ID><GoodsReference><SequenceNumeric>1</SequenceNumeric><GoodsItemNumeric>1</GoodsItemNumeric></GoodsReference><GoodsReference><SequenceNumeric>2</SequenceNumeric><GoodsItemNumeric>1</GoodsItemNumeric></GoodsReference></TransportEquipment></Consignment></GoodsShipment></Declaration></MetaData>";
		bemessage.EM_MessageOwner = "CW1_Test";
		Factory.Save();

		var service = new BECustomsSenderServiceTask();
		InitialiseTaskSchedule(service);
		using (Env.Instance.TemporaryServiceTaskContext(BECustomsSenderServiceTask.Code, canRunInAnyBranch: true))
		{
			AssertNoExceptionThrown(service.RunTask);
		}

		var factory = new BusinessObjectFactory();
		var messages = factory.Load<BEMessage>(bemessage.PK);
		var interchange = messages.Interchange;
		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Interchange No", "1", messages.EM_InterchangeNumber);
			AssertEquals("EDI Interchange - Status", EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("EDI Interchange - Header Text", "{\"custom.MessageSubType\":\"XXX\"}", interchange.EI_HeaderText);
			AssertEquals("EDI Interchange - Body Text", messages.EM_MessageText, interchange.EI_BodyText);
			AssertNullOrEmpty("EDI Interchange - Footer Text", interchange.EI_FooterText);
			AssertEquals("EDI Interchange - Direction", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("EDI Interchange - Retry Count", 0, interchange.EI_RetryCount);
			AssertEquals("EDI Interchange - Transport Type", EDIInterchange.TransportType.xT, interchange.EI_TransportType);
			AssertEquals("EDI Interchange - From", bemessage.Company.LicenceKeyIdentifier, interchange.EI_From);
			AssertEquals("EDI Interchange - Type", "NCT", interchange.EI_InterchangeType);
			AssertEquals("EDI Interchange - To", "LIVE", interchange.EI_To);
		});
	}

	public void TestInitialiseSchedule()
	{
		var testTask = new BECustomsSenderServiceTask();
		InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);

		CombineAssertions(() =>
		{
			AssertEquals("IsActive", ZBool.False, taskSchedule.SST_Active);
			Assert("TaskPeriod", taskSchedule.Recurrence.MinutesRange);
			AssertEquals("TaskPeriodCount", 15, taskSchedule.Recurrence.Period);
			AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("Is DailyStartTime empty?", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
		});
	}

	public override void TestLogging()
	{
		var testTask = new BECustomsSenderServiceTask();
		InitialiseAndRunTaskSchedule(testTask);
		var logger = testTask.ServiceLogger;
		logger.Log(LogType.Information, "Log line 1");
		logger.Log(LogType.Warning, "Log line 2 has a warning");
		logger.Log(LogType.Error, "Log line 3 has an error");
		AssertEquals("Message after adding 3 lines with different types", "Information|Log line 1\r\nWarning|Log line 2 has a warning\r\nError|Log line 3 has an error\r\n", logger.ToString());
	}

	public void TestHostedServiceRequirement()
	{
		var hostedServiceRequirementMethod = typeof(BECustomsSenderServiceTask).GetMethods().FirstOrDefault(p => Attribute.IsDefined(p, typeof(HostedServiceRequirementAttribute)));
		AssertNotNull("Method with Attribute HostedServiceRequirement should exist on BECustomsSenderServiceTask", hostedServiceRequirementMethod);

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

	protected override void AssertSpecificHostedServiceAttributeProperties(HostedServiceAttribute attribute)
	{
		CombineAssertions(() =>
		{
			AssertEquals("Code", "BES", attribute.Code);
			AssertEquals("Description", "BE Customs Message Sender", attribute.Description);
			AssertEquals("Category", "BEC", attribute.Category);
			AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Belgium, attribute.RequiresCompanyInCountry);
			AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
			AssertEquals("MinimumPeriod", "60Seconds", attribute.MinimumPeriod);
		});
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
	{
		get
		{
			return new[]
			{
				new TaskNudgeInformationForTest(
					EDIMessageSchema.Constants.TableName,
					"BE Customs Message Sender",
					EDIMessageSchema.Constants.EM_IsActive + "=Y",
					EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
					EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
					EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.BECustoms,
					EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
			};
		}
	}
}
