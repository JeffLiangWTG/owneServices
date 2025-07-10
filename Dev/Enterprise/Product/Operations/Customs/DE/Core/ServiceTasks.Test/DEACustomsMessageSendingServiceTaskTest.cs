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
	[TestedType(typeof(DEACustomsMessageSendingServiceTask))]
	sealed class DEACustomsMessageSendingServiceTaskTest : ServiceTaskTestCase<DEACustomsMessageSendingServiceTask>
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
			temporaryStorageMessage.EM_MessageText = "HAUSN㐿ÇËŠÏ№Ø";
			Factory.Save();
			serviceTask.RunTask();
			temporaryStorageMessage.Reload();
			AssertEquals("HAUSN㐿ÇËŠÏ№Ø", temporaryStorageMessage.Interchange.EI_BodyText);
		}

		public void TestHostedServiceBindingAttribute()
		{
			TestHelper.AssertSingleHostedServiceAttribute<DEACustomsMessageSendingServiceTask>(ServiceTaskApplicationCodeList.Codes.DEAMessageSending,
			ServiceTaskApplicationCodeList.Descriptions.DEAMessageSending,
			"DEC",
			typeof(DEACustomsMessageSendingServiceTask),
			"60Seconds",
			Core.Constants.CountryCodes.Germany,
			true);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new TaskNudgeInformationForTest[]
		{
			new TaskNudgeInformationForTest(
				EDIMessageSchema.Constants.TableName,
				"DE Atlas Message Sending",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
				EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
				EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.DECustomsAtlasSystem,
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
		};

		protected override void SetUpCore()
		{
			base.SetUpCore();
			serviceTask = new DEACustomsMessageSendingServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
		}
		DEACustomsMessageSendingServiceTask serviceTask;

		void AssertEndToEndResult()
		{
			CombineAssertions(() =>
			{
				temporaryStorageMessage.Reload();
				AssertEquals("AtlasTemporaryStorageMessage Sent", EDIMessage.Status.Sent, temporaryStorageMessage.EM_Status);
				var temporaryStorageInterchange = temporaryStorageMessage.Interchange;
				AssertEquals("TemporaryStorageMessage - Interchange EI_ApplicationCode", EDIInterchange.ApplicationCodes.GenericMessageDelivery, temporaryStorageInterchange.EI_ApplicationCode);
				AssertEquals("TemporaryStorageMessage - Interchange EI_InterchangeType", EDIInterchange.ApplicationCodes.DECustomsAtlasSystem, temporaryStorageInterchange.EI_InterchangeType);
				AssertEquals("TemporaryStorageMessage - Interchange EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, temporaryStorageInterchange.EI_ReceiveTransmit);
				AssertEquals("TemporaryStorageMessage - Interchange EI_From", companiesAndBranchesTestData.DECompany1.LicenceKeyIdentifier, temporaryStorageInterchange.EI_From);
				AssertEquals("TemporaryStorageMessage - Interchange EI_To", "DECustomsTest", temporaryStorageInterchange.EI_To);
				AssertEquals("TemporaryStorageMessage - Interchange EI_Status", EDIInterchange.Status.eHubQueued, temporaryStorageInterchange.EI_Status);
				AssertEquals("TemporaryStorageMessage - Interchange EI_GB", companiesAndBranchesTestData.BERBranch.PK, temporaryStorageInterchange.EI_GB);

				importMessage.Reload();
				AssertEquals("ImportMessage Sent", EDIMessage.Status.Sent, importMessage.EM_Status);
				var importInterchange = importMessage.Interchange;
				AssertEquals("ImportMessage - Interchange EI_ApplicationCode", EDIInterchange.ApplicationCodes.GenericMessageDelivery, importInterchange.EI_ApplicationCode);
				AssertEquals("ImportMessage - Interchange EI_InterchangeType", EDIInterchange.ApplicationCodes.DECustomsAtlasSystem, importInterchange.EI_InterchangeType);
				AssertEquals("ImportMessage - Interchange EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, importInterchange.EI_ReceiveTransmit);
				AssertEquals("ImportMessage - Interchange EI_From", companiesAndBranchesTestData.DECompany1.LicenceKeyIdentifier, importInterchange.EI_From);
				AssertEquals("ImportMessage - Interchange EI_To", "DECustomsTest", importInterchange.EI_To);
				AssertEquals("ImportMessage - Interchange EI_Status", EDIInterchange.Status.eHubQueued, importInterchange.EI_Status);
				AssertEquals("ImportMessage - Interchange EI_GB", companiesAndBranchesTestData.BERBranch.PK, importInterchange.EI_GB);

				atlasMessageDifferentCompany.Reload();
				AssertEquals("DifferentCompany Sent", EDIMessage.Status.Sent, atlasMessageDifferentCompany.EM_Status);
				var differentCompanyInterchange = atlasMessageDifferentCompany.Interchange;
				AssertEquals("DifferentCompany - Interchange EI_ApplicationCode", EDIInterchange.ApplicationCodes.GenericMessageDelivery, differentCompanyInterchange.EI_ApplicationCode);
				AssertEquals("DifferentCompany - Interchange EI_InterchangeType", EDIInterchange.ApplicationCodes.DECustomsAtlasSystem, differentCompanyInterchange.EI_InterchangeType);
				AssertEquals("DifferentCompany - Interchange EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, differentCompanyInterchange.EI_ReceiveTransmit);
				AssertEquals("DifferentCompany - Interchange EI_From", companiesAndBranchesTestData.DECompany2.LicenceKeyIdentifier, differentCompanyInterchange.EI_From);
				AssertEquals("DifferentCompany - Interchange EI_To", "DECustomsTest", differentCompanyInterchange.EI_To);
				AssertEquals("DifferentCompany - Interchange EI_Status", EDIInterchange.Status.eHubQueued, differentCompanyInterchange.EI_Status);
				AssertEquals("DifferentCompany - Interchange EI_GB", atlasMessageDifferentCompany.EM_GB, differentCompanyInterchange.EI_GB);

				notAtlasMessage.Reload();
				AssertEquals("Not Atlas unchanged status", EDIMessage.Status.Queued, notAtlasMessage.EM_Status);
				AssertNull("Not Atlas - Interchange Not Created as DEA expected", notAtlasMessage.Interchange);
			});
		}

		void SetupDataForTesting()
		{
			companiesAndBranchesTestData = new CompaniesAndBranchesTestData(Factory);

			temporaryStorageMessage = Factory.New<AtlasEDIMessage>();
			temporaryStorageMessage.EM_ApplicationReference = "SCPRLI";
			temporaryStorageMessage.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.SummaryDeclarationAfterPresentation;
			temporaryStorageMessage.EM_MessageType = EDIMessageTypeList.Codes.TemporaryStorage;
			temporaryStorageMessage.EM_GB = companiesAndBranchesTestData.BERBranch.PK;

			importMessage = Factory.New<AtlasEDIMessage>();
			importMessage.EM_ApplicationReference = "FCFCDE";
			importMessage.EM_MessageSubType = ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration;
			importMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			importMessage.EM_GB = companiesAndBranchesTestData.SITBranch.PK;

			notAtlasMessage = Factory.New<AesEDIMessage>();
			notAtlasMessage.EM_ApplicationReference = "DEXPDF";
			notAtlasMessage.EM_MessageSubType = ExportMessageSubTypeList.Codes.EXP;
			notAtlasMessage.EM_GB = companiesAndBranchesTestData.BERBranch.PK;

			atlasMessageDifferentCompany = Factory.New<AtlasEDIMessage>();
			atlasMessageDifferentCompany.EM_ApplicationReference = "SCPRLI";
			atlasMessageDifferentCompany.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.SummaryDeclarationAfterPresentation;
			atlasMessageDifferentCompany.EM_MessageType = EDIMessageTypeList.Codes.TemporaryStorage;
			atlasMessageDifferentCompany.EM_GB = companiesAndBranchesTestData.BYBBranch.PK;
		}
		CompaniesAndBranchesTestData companiesAndBranchesTestData;
		AtlasEDIMessage temporaryStorageMessage;
		AtlasEDIMessage importMessage;
		AesEDIMessage notAtlasMessage;
		AtlasEDIMessage atlasMessageDifferentCompany;
	}
}
