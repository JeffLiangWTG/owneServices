using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageProcessors.Testing;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.FR.ServiceTasks.Testing
{
	[TestedType(typeof(FRCustomsSenderServiceTask))]
	sealed class FRCustomsSenderServiceTaskTest : ServiceTaskTestCase<FRCustomsSenderServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var attr = GetHostedServiceAttributes().FirstOrDefault();
			AssertEquals("60Seconds", attr.MinimumPeriod);
			AssertEquals(true, attr.CanRunInAnyBranch);
			AssertEquals("15minutes", attr.DefaultScheduleRunEvery);
		}

		public void TestCheckRecipientIDRegistrySetting_HostServiceRequirementIsDefined()
		{
			var methodInfo = typeof(FRCustomsSenderServiceTask).GetMethod(nameof(FRCustomsSenderServiceTask.CheckRecipientIDRegistrySetting));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		public void TestCheckRecipientIDRegistrySetting_ShouldReturnNoErrorMsg_WhenRegistryValueIsSet()
		{
			using (FRCustomsDataRegistry.Instance.RecipientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "EASYLOG2_EAD"))
			{
				var message = FRCustomsSenderServiceTask.CheckRecipientIDRegistrySetting();
				AssertEquals(message, string.Empty);
			}
		}

		public void TestCheckRecipientIDRegistrySetting_ShouldReturnErrorMsg_WhenRegistryValueIsNotSet()
		{
			using (FRCustomsDataRegistry.Instance.RecipientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var message = FRCustomsSenderServiceTask.CheckRecipientIDRegistrySetting();
				AssertEquals($"The registry setting '{FRCustomsDataRegistry.Instance.RecipientID.GetLocationInEnglish()}' has not been configured.", message);
			}
		}

		public void TestAllSupportingMessageTypeCanBeWellProcessed()
		{
			var processedCounter = 0;
			AssertMessageProcessedAndInterchangeCreated("FRC", "007", "DT", "FRC");
			AssertMessageProcessedAndInterchangeCreated("FRC", "013", "DT", "FRC");
			AssertMessageProcessedAndInterchangeCreated("FRC", "014", "DT", "FRC");
			AssertMessageProcessedAndInterchangeCreated("FRC", "015", "DT", "FRC");
			AssertMessageProcessedAndInterchangeCreated("FRC", "141", "DT", "FRC");
			AssertMessageProcessedAndInterchangeCreated("FRC", "F15", "DT", "FRC");
			AssertMessageProcessedAndInterchangeCreated("FRC", "COD", ZString.Empty, "FRC");
			AssertMessageProcessedAndInterchangeCreated("FRC", "DCG", ZString.Empty, "FRC");
			AssertMessageProcessedAndInterchangeCreated("FRC", "EXC", ZString.Empty, "FRC");
			AssertMessageProcessedAndInterchangeCreated("FRC", "EXD", ZString.Empty, "FRC");
			AssertMessageProcessedAndInterchangeCreated("FRC", "IMC", ZString.Empty, "FRC");
			AssertMessageProcessedAndInterchangeCreated("FRC", "IMD", ZString.Empty, "FRC");
			AssertMessageProcessedAndInterchangeCreated("FRP", "POR", ZString.Empty, "FRP");
			AssertMessageProcessedAndInterchangeCreated("FRC", "DEC", ZString.Empty, "FRI", @"{""ImportOperation"":{""LRN"":""0000005856""}}");
			AssertMessageProcessedAndInterchangeCreated("FRC", "STO", ZString.Empty, "FRS");
			AssertMessageProcessedAndInterchangeCreated("FRC", "745", ZString.Empty, "FRT");
			AssertMessageProcessedAndInterchangeCreated("FRC", "755", ZString.Empty, "FRT");
			AssertMessageProcessedAndInterchangeCreated("FRC", "CIN", ZString.Empty, "FRT");
			AssertMessageProcessedAndInterchangeCreated("FRC", "TP5", ZString.Empty, "FR5");

			void AssertMessageProcessedAndInterchangeCreated(string applicationCode, string messageType, string messageSubType, string expectedInterchangeType, string messageText = "")
			{
				var msg = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				applicationCode,
				ReceiveTransmitList.Codes.Transmit,
				EDIMessageStatusList.Codes.Queued,
				branch.PK,
				messageText,
				messageType,
				messageSubType
				);
				Factory.Save();

				var logger = new TestServiceLogger();
				var serviceTask = new FRCustomsSenderServiceTask();
				serviceTask.ServiceLogger = logger;
				serviceTask.RunTask(CancellationToken.None);

				msg.Reload();
				AssertEquals($"Message of type'{messageType}' & subtype'{messageSubType}' should be successfully processed.", "SNT", msg.EM_Status);

				var interchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery));

				var assertionComment = ZString.Empty;
				if (string.IsNullOrEmpty(messageSubType))
				{
					assertionComment = $"Interchange of type {expectedInterchangeType} should be successfully created for a message of type:{messageType} whatever the subtype.";
				}
				else
				{
					assertionComment = $"Interchange of type {expectedInterchangeType} should be successfully created for a message of type:{messageType} & subtype:{messageSubType}.";
				}

				AssertEquals(assertionComment, ++processedCounter, interchanges.Length);
				var interchange = interchanges.Cast<EDIInterchange>().First(x => x.PK == msg.EM_EI);
				MessageProcessorTestHelper.AssertEDIInterchangeProperties(interchange,
				expectedApplicationCode: "GMD",
				expectedInterchangeType: expectedInterchangeType,
				expectedReceiveTransmit: "TRX",
				expectedStatus: "HQU",
				expectedFrom: GlbCompany.CurrentCompany.LicenceKeyIdentifier,
				expectedTo: "ABCDEFG",
				expectedBodyText: messageType == "DEC" ? GetInterchangeBodyTextForDeltaIEMessages(messageSubType, messageText) : messageText,
				expectedHeaderText: $"</SenderID><RecipientID>ABCDEFG</RecipientID><InterchangeType>{expectedInterchangeType}</InterchangeType><InterchangeNumber>",
				message: $"Type:{messageType} & subtype:{messageSubType} Interchange properties"
				);
			}

			string GetInterchangeBodyTextForDeltaIEMessages(string messageSubType, string messageText) => $@"{{""SchemaId"":""IEXXX"",""TransactionId"":"""",""MessageJson"":{messageText}}}";
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			GlbCompany.CurrentCompany.SetCountry("FR");
			branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			FRCustomsDataRegistry.Instance.RecipientID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCDEFG");
			FRCustomsDataRegistry.Instance.CINSenderID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIN-UNIT-TEST");
		}
		GlbBranch branch;

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"FR Customs messages outbound",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.FRCustomsMessage,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"FR Port messages outbound",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.FRPortMessage,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL")
				};
			}
		}

		public void TestMessageSentForEachEntryOfEachDeclaration_InterchangeShouldBeUnique()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.FillWithValidTestData();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.FillWithValidTestData();
			entryHeader1_1.CorrelationID = "1";
			entryHeader1_1.MRN = "1";
			entryHeader1_1.CRN = "1";
			entryHeader1_1.AllEntryLines.AddNew().FillWithValidTestData();
			var entryHeader1_2 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_2.FillWithValidTestData();
			entryHeader1_2.CorrelationID = "2";
			entryHeader1_2.MRN = "2";
			entryHeader1_2.CRN = "2";
			entryHeader1_2.AllEntryLines.AddNew().FillWithValidTestData();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.FillWithValidTestData();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.FillWithValidTestData();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			entryHeader2_1.AllEntryLines.AddNew().FillWithValidTestData();
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ABC", declaration1.CountryCode);

			var cancellationMessage = new SendCancellationMessageDataObject();
			cancellationMessage.InvalidationMotivation = "AAA";
			cancellationMessage.InvalidationReason = "BBB";
			cancellationMessage.Targets = new BusinessObject[] { declaration1, declaration2 };

			var log = new DummyOperationalActionSectionLog();
			var runner = new SendCancellationMessageOperationalActionRunner(log, Factory);
			runner.SendCancellationMessages(cancellationMessage);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery()).ToArray();
			AssertEquals(3, ediMessages.Length);

			var logger = new TestServiceLogger();
			var serviceTask = new FRCustomsSenderServiceTask();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask(CancellationToken.None);

			CombineAssertions("Only 1 interchange should have been created with the 3 messages in it.", () =>
			{
				var ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery()).ToArray();
				AssertEquals(1, ediInterchanges.Length);
				AssertEquals(4, ediInterchanges.SingleOrDefault().EI_BodyText.Split("LRN").Length);
			});
		}
	}
}
