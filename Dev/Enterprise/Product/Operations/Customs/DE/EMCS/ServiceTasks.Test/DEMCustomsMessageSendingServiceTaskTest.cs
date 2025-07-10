using System.Collections.Generic;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.EMCS.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Customs.DE.EMCS.ServiceTasks.Testing.TestHelper;

namespace Enterprise.Customs.DE.EMCS.ServiceTasks.Testing
{
	[TestedType(typeof(DEMCustomsMessageSendingServiceTask))]
	class DEMCustomsMessageSendingServiceTaskTest : ServiceTaskTestCase<DEMCustomsMessageSendingServiceTask>
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
			cancellationOfEADMessage.EM_MessageText = "HAUSN㐿ÇËŠÏ№Ø";
			Factory.Save();
			serviceTask.RunTask();
			cancellationOfEADMessage.Reload();
			AssertEquals("HAUSN㐿ÇËŠÏ№Ø", cancellationOfEADMessage.Interchange.EI_BodyText);
		}

		public void TestHostedServiceBindingAttribute()
		{
			TestHelper.AssertSingleHostedServiceAttribute<DEMCustomsMessageSendingServiceTask>(ServiceTaskApplicationCodeList.Codes.DEMMessageSending,
				ServiceTaskApplicationCodeList.Descriptions.DEMMessageSending,
				"DEC",
				typeof(DEMCustomsMessageSendingServiceTask),
				"60Seconds",
				Core.Constants.CountryCodes.Germany,
				true);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new TaskNudgeInformationForTest[]
		{
			new TaskNudgeInformationForTest(
				EDIMessageSchema.Constants.TableName,
				"DE Emcs Message Sending",
				EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.DECustomsEmcsSystem),
		};

		protected override void SetUpCore()
		{
			base.SetUpCore();
			serviceTask = new DEMCustomsMessageSendingServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
		}
		DEMCustomsMessageSendingServiceTask serviceTask;

		void AssertEndToEndResult()
		{
			CombineAssertions(() =>
			{
				cancellationOfEADMessage.Reload();
				AssertEquals("CancellationOfEAD Sent", EDIMessage.Status.Sent, cancellationOfEADMessage.EM_Status);
				var cancellationOfEADInterchange = cancellationOfEADMessage.Interchange;
				AssertEquals("CancellationOfEAD - Interchange EI_ApplicationCode", EDIInterchange.ApplicationCodes.GenericMessageDelivery, cancellationOfEADInterchange.EI_ApplicationCode);
				AssertEquals("CancellationOfEAD - Interchange EI_InterchangeType", EDIInterchange.ApplicationCodes.DECustomsEmcsSystem, cancellationOfEADInterchange.EI_InterchangeType);
				AssertEquals("CancellationOfEAD - Interchange EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, cancellationOfEADInterchange.EI_ReceiveTransmit);
				AssertEquals("CancellationOfEAD - Interchange EI_From", companiesAndBranchesTestData.DECompany1.LicenceKeyIdentifier, cancellationOfEADInterchange.EI_From);
				AssertEquals("CancellationOfEAD - Interchange EI_To", "DECustomsTest", cancellationOfEADInterchange.EI_To);
				AssertEquals("CancellationOfEAD - Interchange EI_Status", EDIInterchange.Status.eHubQueued, cancellationOfEADInterchange.EI_Status);
				AssertEquals("CancellationOfEAD - Interchange EI_GB", companiesAndBranchesTestData.BERBranch.PK, cancellationOfEADInterchange.EI_GB);

				reportOfReceiptMessage.Reload();
				AssertEquals("ReportOfReceipt Sent", EDIMessage.Status.Sent, reportOfReceiptMessage.EM_Status);
				var reportOfReceiptInterchange = reportOfReceiptMessage.Interchange;
				AssertEquals("ReportOfReceipt -Interchange EI_ApplicationCode", EDIInterchange.ApplicationCodes.GenericMessageDelivery, reportOfReceiptInterchange.EI_ApplicationCode);
				AssertEquals("ReportOfReceipt -Interchange EI_InterchangeType", EDIInterchange.ApplicationCodes.DECustomsEmcsSystem, reportOfReceiptInterchange.EI_InterchangeType);
				AssertEquals("ReportOfReceipt -Interchange EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, reportOfReceiptInterchange.EI_ReceiveTransmit);
				AssertEquals("ReportOfReceipt -Interchange EI_From", companiesAndBranchesTestData.DECompany1.LicenceKeyIdentifier, reportOfReceiptInterchange.EI_From);
				AssertEquals("ReportOfReceipt -Interchange EI_To", "DECustomsTest", reportOfReceiptInterchange.EI_To);
				AssertEquals("ReportOfReceipt -Interchange EI_Status", EDIInterchange.Status.eHubQueued, reportOfReceiptInterchange.EI_Status);
				AssertEquals("ReportOfReceipt -Interchange EI_GB", companiesAndBranchesTestData.BERBranch.PK, reportOfReceiptInterchange.EI_GB);

				emcsMessageDifferentCompany.Reload();
				AssertEquals("EmcsMessageDifferentCompany Sent", EDIMessage.Status.Sent, emcsMessageDifferentCompany.EM_Status);
				var differentCompanyInterchange = emcsMessageDifferentCompany.Interchange;
				AssertEquals("EmcsMessageDifferentCompany - Interchange EI_ApplicationCode", EDIInterchange.ApplicationCodes.GenericMessageDelivery, differentCompanyInterchange.EI_ApplicationCode);
				AssertEquals("EmcsMessageDifferentCompany - Interchange EI_InterchangeType", EDIInterchange.ApplicationCodes.DECustomsEmcsSystem, differentCompanyInterchange.EI_InterchangeType);
				AssertEquals("EmcsMessageDifferentCompany - Interchange EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, differentCompanyInterchange.EI_ReceiveTransmit);
				AssertEquals("EmcsMessageDifferentCompany - Interchange EI_From", companiesAndBranchesTestData.DECompany2.LicenceKeyIdentifier, differentCompanyInterchange.EI_From);
				AssertEquals("EmcsMessageDifferentCompany - Interchange EI_To", "DECustomsTest", differentCompanyInterchange.EI_To);
				AssertEquals("EmcsMessageDifferentCompany - Interchange EI_Status", EDIInterchange.Status.eHubQueued, differentCompanyInterchange.EI_Status);
				AssertEquals("EmcsMessageDifferentCompany - Interchange EI_GB", emcsMessageDifferentCompany.EM_GB, differentCompanyInterchange.EI_GB);

				notEmcsMessage.Reload();
				AssertEquals("Not Emcs unchanged status", EDIMessage.Status.Queued, notEmcsMessage.EM_Status);
				AssertNull("Not Emcs - Interchange Not Created as DEM", notEmcsMessage.Interchange);
			});
		}

		void SetupDataForTesting()
		{
			companiesAndBranchesTestData = new CompaniesAndBranchesTestData(Factory);

			cancellationOfEADMessage = Factory.New<EmcsEDIMessage>();
			cancellationOfEADMessage.EM_ApplicationReference = "ED810C";
			cancellationOfEADMessage.EM_MessageSubType = EmcsMessageSubTypeList.Codes.Eme;
			cancellationOfEADMessage.EM_MessageType = EDIMessageTypeList.Codes.EMCS;
			cancellationOfEADMessage.EM_GB = companiesAndBranchesTestData.BERBranch.PK;

			reportOfReceiptMessage = Factory.New<EmcsEDIMessage>();
			reportOfReceiptMessage.EM_ApplicationReference = "ED818C";
			reportOfReceiptMessage.EM_MessageSubType = EmcsMessageSubTypeList.Codes.Emb;
			reportOfReceiptMessage.EM_MessageType = EDIMessageTypeList.Codes.EMCS;
			reportOfReceiptMessage.EM_GB = companiesAndBranchesTestData.SITBranch.PK;

			notEmcsMessage = Factory.New<AesEDIMessage>();
			notEmcsMessage.EM_ApplicationReference = "DEXPDF";
			notEmcsMessage.EM_MessageSubType = ExportMessageSubTypeList.Codes.EXP;
			notEmcsMessage.EM_MessageType = EDIMessageTypeList.Codes.AES;
			notEmcsMessage.EM_GB = companiesAndBranchesTestData.BERBranch.PK;

			emcsMessageDifferentCompany = Factory.New<EmcsEDIMessage>();
			emcsMessageDifferentCompany.EM_ApplicationReference = "ED810C";
			emcsMessageDifferentCompany.EM_MessageSubType = EmcsMessageSubTypeList.Codes.Eme;
			emcsMessageDifferentCompany.EM_GB = companiesAndBranchesTestData.BYBBranch.PK;
		}
		CompaniesAndBranchesTestData companiesAndBranchesTestData;
		EmcsEDIMessage cancellationOfEADMessage;
		EmcsEDIMessage reportOfReceiptMessage;
		EmcsEDIMessage emcsMessageDifferentCompany;
		AesEDIMessage notEmcsMessage;
	}
}
