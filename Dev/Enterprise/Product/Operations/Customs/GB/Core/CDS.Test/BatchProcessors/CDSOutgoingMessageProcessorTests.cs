using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	sealed class CDSOutgoingMessageProcessorTests : TestCaseWithFactory
	{
		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestProcess()
		{
			CDSMessageSenderTestHelper.TestProcess(() =>
			{
				var processor = new CDSOutgoingMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(CancellationToken.None);
			});
		}

		public void TestOutgoingMessageTypes()
		{
			var messageTypeList = CDSOutgoingMessageProcessor.GetOutgoingMessageTypes().ToList<string>();

			CombineAssertions(() =>
			{
				Assert("OutgoingMessageTypes should contain NEW", messageTypeList.Contains(CDSEDIMessageTypeList.Codes.NewDeclaration));
				Assert("OutgoingMessageTypes should contain AMD", messageTypeList.Contains(CDSEDIMessageTypeList.Codes.AmendDeclaration));
				Assert("OutgoingMessageTypes should contain CAN", messageTypeList.Contains(CDSEDIMessageTypeList.Codes.CancelDeclaration));
				Assert("OutgoingMessageTypes should contain LCQ", messageTypeList.Contains(CDSEDIMessageTypeList.Codes.InventoryLinkingConsolidationRequest));
				Assert("OutgoingMessageTypes should contain LMQ", messageTypeList.Contains(CDSEDIMessageTypeList.Codes.InventoryLinkingMovementRequest));
				Assert("OutgoingMessageTypes should contain LQQ", messageTypeList.Contains(CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest));
				Assert("OutgoingMessageTypes should contain ARR", messageTypeList.Contains(CDSEDIMessageTypeList.Codes.ArrivalNotification));
				Assert("OutgoingMessageTypes should contain FEC", messageTypeList.Contains(CDSEDIMessageTypeList.Codes.FecChallenge));
				Assert("OutgoingMessageTypes should contain NIL", messageTypeList.Contains(CDSEDIMessageTypeList.Codes.NilAmendment));
				Assert("OutgoingMessageTypes should contain CAR", messageTypeList.Contains(CDSEDIMessageTypeList.Codes.CDSPentantAcaMessage));
				Assert("OutgoingMessageTypes should contain LQM", messageTypeList.Contains(CDSEDIMessageTypeList.Codes.MasterQueryDeclaration));
			});
		}

		public void TestFailDirectInventoryLinkingMessageWhenConsolHasInvalidSendingAgent()
		{
			RunInventoryLinkingMessageWhenConsolHasInvalidSendingAgent("ZPE", expected1: false, expected2: false, expected3: true);
		}

		public void TestIndirectInventoryLinkingMessageWhenConsolHasInvalidSendingAgent()
		{
			MawbTestHelper.MakeBadge("ZPG", GatewayList.Codes.CNS_CUSDECOnly, "CUKFFW98000", true, "ZPG", false, false, BadgeDirectionList.Codes.EXP, "GBLHR", MucrGenerationStyles.Codes.Air);
			RunInventoryLinkingMessageWhenConsolHasInvalidSendingAgent("ZPG", expected1: true, expected2: true, expected3: true);
		}

		void RunInventoryLinkingMessageWhenConsolHasInvalidSendingAgent(string badge, bool expected1, bool expected2, bool expected3)
		{
			var consol = GbCDSConsolIntegrationWrapperTests.CreateSampleWrapper(Factory);
			var consolWrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consolWrapper.MawbExportHelper.ME_Profile = badge;

			var logger = new LoggingInformation();
			var processor = new CDSOutgoingMessageProcessor(logger);

			var message = (CDSInventoryLinkingQueryRequestEDIMessage)consol.Messages.AddNew(typeof(CDSInventoryLinkingQueryRequestEDIMessage));
			message.EM_MessageOwner = "ME";
			Factory.Save();

			ProcessAndAssertResult("No sending agent", expected1);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			message = (CDSInventoryLinkingQueryRequestEDIMessage)consol.Messages.AddNew(typeof(CDSInventoryLinkingQueryRequestEDIMessage));
			message.EM_MessageOwner = "ME";
			Factory.Save();

			ProcessAndAssertResult("Sending agent has no EORI", expected2);

			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678");
			message = (CDSInventoryLinkingQueryRequestEDIMessage)consol.Messages.AddNew(typeof(CDSInventoryLinkingQueryRequestEDIMessage));
			message.EM_MessageOwner = "ME";
			Factory.Save();

			ProcessAndAssertResult("Sending agent has EORI", expected3);

			void ProcessAndAssertResult(string assertMessage, bool expected)
			{
				var query = new ZQuery(EDIInterchangeSchema.EI_GB, Env.CurrentBranchPK);
				var interchangeCount = Factory.Load<EDIInterchange>(query).Length;

				logger.ClearLogs();
				processor.ProcessMessage(CancellationToken.None);
				message.Reload();

				CombineAssertions(assertMessage, () =>
				{
					string interchangeAssertMessage;
					if (expected)
					{
						AssertEquals(EDIMessageStatusList.Codes.Sent, message.EM_Status);
						++interchangeCount;
						interchangeAssertMessage = "Interchange created";
					}
					else
					{
						var allMessages = string.Join(" ", logger.Logs.Select(x => x.Message));
						AssertContains($"Message {message.EM_MessageNum} cannot be packaged for delivery directly to CDS via eHub, as a suitable credentials key could not be determined. Most likely consol {consol.JK_UniqueConsignRef} lacks a sending forwarder with an EORI. The message is being set to failed", allMessages);
						AssertEquals(EDIMessageStatusList.Codes.Failed, message.EM_Status);
						interchangeAssertMessage = "Interchange not created";
					}

					AssertEquals(interchangeAssertMessage, interchangeCount, Factory.Load<EDIInterchange>(query).Length);
				});
			}
		}
	}
}
