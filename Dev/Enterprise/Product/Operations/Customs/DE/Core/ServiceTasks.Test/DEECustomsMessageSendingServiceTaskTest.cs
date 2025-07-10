using System.Collections.Generic;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Customs.DE.ServiceTasks.Testing.TestHelper;

namespace Enterprise.Customs.DE.ServiceTasks.Testing
{
	[TestedType(typeof(DEECustomsMessageSendingServiceTask))]
	sealed class DEECustomsMessageSendingServiceTaskTest : ServiceTaskTestCase<DEECustomsMessageSendingServiceTask>
	{
		public void TestEndToEnd()
		{
			SetupDataForTesting();
			Factory.Save();
			serviceTask.RunTask();
			AssertEndToEndResult();
		}

		public void TestUnicodeEI_BodyText()
		{
			SetupDataForTesting();
			transferAndSettlementMessage.EM_MessageText = "HAUSN㐿ÇËŠÏ№Ø";
			Factory.Save();
			serviceTask.RunTask();
			transferAndSettlementMessage.Reload();
			AssertEquals("HAUSN㐿ÇËŠÏ№Ø", transferAndSettlementMessage.Interchange.EI_BodyText);
		}

		public void TestHostedServiceBindingAttribute()
		{
			TestHelper.AssertSingleHostedServiceAttribute<DEECustomsMessageSendingServiceTask>(ServiceTaskApplicationCodeList.Codes.DEEMessageSending,
				ServiceTaskApplicationCodeList.Descriptions.DEEMessageSending,
				"DEC",
				typeof(DEECustomsMessageSendingServiceTask),
				"60Seconds",
				Core.Constants.CountryCodes.Germany,
				true);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new TaskNudgeInformationForTest[]
		{
			new TaskNudgeInformationForTest(
				EDIMessageSchema.Constants.TableName,
				"DE Aes Message Sending",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
				EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
				EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.DECustomsAesSystem,
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
		};

		protected override void SetUpCore()
		{
			base.SetUpCore();
			serviceTask = new DEECustomsMessageSendingServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
		}
		DEECustomsMessageSendingServiceTask serviceTask;

		void AssertEndToEndResult()
		{
			CombineAssertions(() =>
			{
				transferAndSettlementMessage.Reload();
				AssertEquals("TransferAndSettlementMessage Sent", EDIMessage.Status.Sent, transferAndSettlementMessage.EM_Status);
				var transferAndSettlementInterchange = transferAndSettlementMessage.Interchange;
				AssertEquals("TransferAndSettlementMessage - Interchange EI_ApplicationCode", EDIInterchange.ApplicationCodes.GenericMessageDelivery, transferAndSettlementInterchange.EI_ApplicationCode);
				AssertEquals("TransferAndSettlementMessage - Interchange EI_InterchangeType", EDIInterchange.ApplicationCodes.DECustomsAesSystem, transferAndSettlementInterchange.EI_InterchangeType);
				AssertEquals("TransferAndSettlementMessage - Interchange EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, transferAndSettlementInterchange.EI_ReceiveTransmit);
				AssertEquals("TransferAndSettlementMessage - Interchange EI_From", companiesAndBranchesTestData.DECompany1.LicenceKeyIdentifier, transferAndSettlementInterchange.EI_From);
				AssertEquals("TransferAndSettlementMessage - Interchange EI_To", "DECustomsTest", transferAndSettlementInterchange.EI_To);
				AssertEquals("TransferAndSettlementMessage - Interchange EI_Status", EDIInterchange.Status.eHubQueued, transferAndSettlementInterchange.EI_Status);
				AssertEquals("TransferAndSettlementMessage - Interchange EI_GB", companiesAndBranchesTestData.BERBranch.PK, transferAndSettlementInterchange.EI_GB);

				monitoringMessage.Reload();
				AssertEquals("MonitoringMessage Sent", EDIMessage.Status.Sent, monitoringMessage.EM_Status);
				var monitoringInterchange = monitoringMessage.Interchange;
				AssertEquals("MonitoringMessage - Interchange EI_ApplicationCode", EDIInterchange.ApplicationCodes.GenericMessageDelivery, monitoringInterchange.EI_ApplicationCode);
				AssertEquals("MonitoringMessage - Interchange EI_InterchangeType", EDIInterchange.ApplicationCodes.DECustomsAesSystem, monitoringInterchange.EI_InterchangeType);
				AssertEquals("MonitoringMessage - Interchange EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, monitoringInterchange.EI_ReceiveTransmit);
				AssertEquals("MonitoringMessage - Interchange EI_From", companiesAndBranchesTestData.DECompany1.LicenceKeyIdentifier, monitoringInterchange.EI_From);
				AssertEquals("MonitoringMessage - Interchange EI_To", "DECustomsTest", monitoringInterchange.EI_To);
				AssertEquals("MonitoringMessage - Interchange EI_Status", EDIInterchange.Status.eHubQueued, monitoringInterchange.EI_Status);
				AssertEquals("MonitoringMessage - Interchange EI_GB", companiesAndBranchesTestData.BERBranch.PK, monitoringInterchange.EI_GB);

				aesMessageDifferentCompany.Reload();
				AssertEquals("AesMessageDifferentCompany Sent", EDIMessage.Status.Sent, aesMessageDifferentCompany.EM_Status);
				var differentCompanyinterchange = aesMessageDifferentCompany.Interchange;
				AssertEquals("AesMessageDifferentCompany - Interchange EI_ApplicationCode", EDIInterchange.ApplicationCodes.GenericMessageDelivery, differentCompanyinterchange.EI_ApplicationCode);
				AssertEquals("AesMessageDifferentCompany - Interchange EI_InterchangeType", EDIInterchange.ApplicationCodes.DECustomsAesSystem, differentCompanyinterchange.EI_InterchangeType);
				AssertEquals("AesMessageDifferentCompany - Interchange EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, differentCompanyinterchange.EI_ReceiveTransmit);
				AssertEquals("AesMessageDifferentCompany - Interchange EI_From", companiesAndBranchesTestData.DECompany2.LicenceKeyIdentifier, differentCompanyinterchange.EI_From);
				AssertEquals("AesMessageDifferentCompany - Interchange EI_To", "DECustomsTest", differentCompanyinterchange.EI_To);
				AssertEquals("AesMessageDifferentCompany - Interchange EI_Status", EDIInterchange.Status.eHubQueued, differentCompanyinterchange.EI_Status);
				AssertEquals("AesMessageDifferentCompany - Interchange EI_GB", aesMessageDifferentCompany.EM_GB, differentCompanyinterchange.EI_GB);

				notAesMessage.Reload();
				AssertEquals("Not Aes unchanged status", EDIMessage.Status.Queued, notAesMessage.EM_Status);
				AssertNull("Not Aes - Interchange Not Created as DEE", notAesMessage.Interchange);
			});
		}

		void SetupDataForTesting()
		{
			companiesAndBranchesTestData = new CompaniesAndBranchesTestData(Factory);

			transferAndSettlementMessage = Factory.New<AesEDIMessage>();
			transferAndSettlementMessage.EM_ApplicationReference = "DEXPDF";
			transferAndSettlementMessage.EM_MessageSubType = ExportMessageSubTypeList.Codes.EXP;
			transferAndSettlementMessage.EM_MessageType = EDIMessageTypeList.Codes.AES;
			transferAndSettlementMessage.EM_GB = companiesAndBranchesTestData.BERBranch.PK;

			monitoringMessage = Factory.New<AesEDIMessage>();
			monitoringMessage.EM_ApplicationReference = "DEXTDE";
			monitoringMessage.EM_MessageSubType = ExportMessageSubTypeList.Codes.EXT;
			monitoringMessage.EM_MessageType = EDIMessageTypeList.Codes.AES;
			monitoringMessage.EM_GB = companiesAndBranchesTestData.SITBranch.PK;

			notAesMessage = Factory.New<AtlasEDIMessage>();
			notAesMessage.EM_ApplicationReference = "SCPRLI";
			notAesMessage.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.SummaryDeclarationAfterPresentation;
			notAesMessage.EM_MessageType = EDIMessageTypeList.Codes.TemporaryStorage;
			notAesMessage.EM_GB = companiesAndBranchesTestData.BERBranch.PK;

			aesMessageDifferentCompany = Factory.New<AesEDIMessage>();
			aesMessageDifferentCompany.EM_ApplicationReference = "DEXPEE";
			aesMessageDifferentCompany.EM_MessageSubType = ExportMessageSubTypeList.Codes.EXP;
			aesMessageDifferentCompany.EM_MessageType = EDIMessageTypeList.Codes.AES;
			aesMessageDifferentCompany.EM_GB = companiesAndBranchesTestData.BYBBranch.PK;
		}
		CompaniesAndBranchesTestData companiesAndBranchesTestData;
		AesEDIMessage transferAndSettlementMessage;
		AesEDIMessage monitoringMessage;
		AtlasEDIMessage notAesMessage;
		AesEDIMessage aesMessageDifferentCompany;
	}
}
