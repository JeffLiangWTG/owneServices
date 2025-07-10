using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestDate(2023, 12, 12, 6, 0, 0)]
	sealed class IMDMessagingTriggerActionProcessorTest : TestCaseWithFactory
	{
		public void TestQueuedEntry_LodgeWithPay()
		{
			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				jobDeclaration.JE_EDITransmitDate = ZDateTime.Now;
				Factory.Save();

				var notify = new NotificationBuffer();
				var logger = new SimpleLogger();
				var processor = new IMDMessagingTriggerActionProcessor(jobDeclaration, CMRMessageTypes.LodgeWithPay);
				processor.Process(notify, logger, GlbStaff.CurrentUser);

				AssertEquals(string.Empty, notify.AsString);
				AssertContains($"Generating entry lodgement messages for Job {jobDeclaration.JobNumber}", logger.ToString());

				AssertEquals(1, entryHeader.Messages.Count);
				AssertContains("message is LodgeWithPay (has GIS+EPA)", "'DTM+260:20231212:102'GIS+EPA:109:95'GIS+Y:153:95'", entryHeader.Messages[0].EM_MessageText);
			}
		}

		public void TestQueuedEntry_LodgeWithoutPay()
		{
			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("AUS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				jobDeclaration.JE_EDITransmitDate = ZDateTime.Now;
				Factory.Save();

				var notify = new NotificationBuffer();
				var logger = new SimpleLogger();
				var processor = new IMDMessagingTriggerActionProcessor(jobDeclaration, CMRMessageTypes.LodgeWithoutPay);
				processor.Process(notify, logger, GlbStaff.CurrentUser);

				AssertEquals(string.Empty, notify.AsString);
				AssertContains($"Generating entry lodgement messages for Job {jobDeclaration.JobNumber}", logger.ToString());

				AssertEquals(1, entryHeader.Messages.Count);
				AssertContains("message is LodgeWithoutPay (missing GIS+EPA)", "'DTM+260:20231212:102'GIS+Y:153:95'", entryHeader.Messages[0].EM_MessageText);
			}
		}

		public void TestQueuedEntry_Payment()
		{
			var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postMasterGroup.Staff[0].GS_EmailAddress = "test@cargowise.com";

			entryHeader.ScheduledPaymentDate = ZDateTime.Now;
			entryHeader.AddInfo.ZA_CustomsPayNow_Hidden = 100m;

			var imdrResponse = Factory.New<CMRIMDRMessage>();
			imdrResponse.EM_MessageText = TestMessages.IMDRMessageText;
			imdrResponse.EM_LinkedObject = entryHeader;
			imdrResponse.EM_ReceiveTransmit = CMRIMDRMessage.Direction.Receive;
			Factory.Save();

			var notify = new NotificationBuffer();
			var logger = new SimpleLogger();
			var processor = new IMDMessagingTriggerActionProcessor(jobDeclaration, CMRMessageTypes.Payment);
			var mockQuerier = new Mock<IServiceManagerQuerier>();

			mockQuerier.Setup(m => m.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				processor.Process(notify, logger, GlbStaff.CurrentUser);

				AssertEquals(string.Empty, notify.AsString);
				AssertContains($"Generating payment messages for Job {jobDeclaration.JobNumber}", logger.ToString());

				AssertEquals(2, entryHeader.Messages.Count);
				var messageText = entryHeader.Messages[1].EM_MessageText;
				AssertContains("message is Payment", "'BGM+481:::PAYSTD+", messageText);
				AssertContains("message has Payment Amount", "'MOA+128:100.00", messageText);
			}
		}

		JobDeclaration jobDeclaration;
		CusEntryHeader entryHeader;

		protected override void SetUp()
		{
			base.SetUp();

			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "AAA";
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();

			var brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;

			jobDeclaration = JobDeclaration.New(Factory);
			jobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_CustomsDischargePort = "AUSYN";
			jobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			jobDeclaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var importer = Factory.New<OrgHeader>();
			importer.LocalBusinessRegNo = GlbCompany.CurrentCompany.GC_BusinessRegNo;
			importer.OH_Code = "ABC";
			jobDeclaration.JE_OH_Importer = importer.PK;

			entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAA";

			var invoiceLine = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}
	}
}
