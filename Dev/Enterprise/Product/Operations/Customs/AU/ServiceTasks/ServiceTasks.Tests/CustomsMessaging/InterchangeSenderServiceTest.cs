using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.AU.ServiceTasks.Testing
{
	[TestedType(typeof(InterchangeSenderService))]
	sealed class InterchangeSenderServiceTest : BaseMultiCompanyCustomsMessagingServiceTest<InterchangeSenderService>
	{
		public void TestInterchangeSenderServiceOverride()
		{
			AssertEquals(typeof(AUCInterchangeSender), testServiceTask.ProcessTypeForTesting);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestHandleEmailNotificationWithoutRecipient()
		{
			// Setup a compnay with site id
			// Setup a postmaster group with a staff which doesn't have an email
			// Run service task and ensure that a log exists in the service task notifying users of the issue.

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "GS3";
			staff.GS_FullName = "BOB THE BUILDER";
			var postmasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postmasterGroup.Staff.RemoveAndDeleteAll();
			postmasterGroup.Staff.Add(staff);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_CustomsRegistrationNo = ZString.Empty;
			var message = Factory.New<CMRMessage>();
			message.EM_ApplicationCode = CMRMessage.ApplicationCodes.CMR;
			message.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			message.EM_ReceiveTransmit = CMRMessage.Direction.Transmit;
			message.EM_Status = CMRMessage.Status.Queued;
			message.EM_MessageText = "UNH+1+CONTRL:D:3:UN'UCI+44+CUSTOMS::CUSTOMS+COMPANY+8'UNT+3+1'";
			Factory.Save();
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();

			testServiceTask.RunTask();
			AssertMultilineASCIIEquals("Service Log", @"Warning|There is no Customs interchange sender id set up,
for Company - EDI, Branch - BNE
Please follow the instruction here to set it up.
Please check with Customs, obtain a site ID and enter the ID on the Config > System > Companies > Current Company > Customs Reg No.
The interchange/message text:
Error|Email (Subject: 'Batch Processor problems while processing interchanges and messages', For Group: Notification -> Company Notification Group 'PMG - Post Masters') must have at least one recipient, CC or BCC", testServiceTask.ServiceLogger.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestCanRunInAnyBranch()
		{
			var initialUserContext = Env.CurrentUserContext;
			try
			{
				using (Env.Instance.TemporaryServiceTaskContext(Enterprise.Customs.AU.ServiceTasks.InterchangeSenderService.Code, canRunInAnyBranch: true))
				{
					testServiceTask.RunTask();
				}
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
				AssertEquals("No Errors", false, ErrorReporter.LastMessageReported.Contains("Direct access to Env.CurrentBranch is not allowed from service tasks"));
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"AU Customs CMR exclude ACR And SCR messages outbound",
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CMR,
						EDIMessageSchema.Constants.EM_MessageType     + "!=ACR",
						EDIMessageSchema.Constants.EM_MessageType     + "!=SCR"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"AU Customs CMR ACR And SCR messages outbound",
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CMR,
						EDIMessageSchema.Constants.EM_MessageType     + "=ACR",
						EDIMessageSchema.Constants.EM_MessageType     + "=SCR"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"AU Customs PRA messages outbound",
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.OneStop),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"AU Customs ExDocs messages outbound",
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.EXDOC),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"AU Customs NEXDocs messages outbound",
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.NEXDOCS),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"AU Customs COLS messages outbound",
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.COLS)
				};
			}
		}
	}
}
