using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.HK.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.HK.ServiceTasks.Testing
{
	[TestedType(typeof(MessageProcessorServiceTask))]
	sealed class MessageProcessorServiceTaskTest : ServiceTaskTestCase<MessageProcessorServiceTask>
	{
		public void TestProcessForAllCompanies()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.HongKong;

			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "test@test.com.au";

			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "~1";
			group1.Staff.Add(GlbStaff.CurrentUser);

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.HongKong;
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "HK2";

			var group2 = Factory.New<GlbGroup>();
			group2.Staff.Add(GlbStaff.CurrentUser);
			group2.GG_Code = "~2";

			using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "1234"))
			using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, "2345"))
			{
				var transmitMessage1 = CreateMessage("119812", ZGuid.Empty, EDIMessage.Direction.Receive, EDIMessage.Status.Sent, "TEST");
				transmitMessage1.EM_GB = GlbBranch.CurrentBranch.PK;
				var interchange1 = CreateInterchange("1234");
				transmitMessage1.EM_EI = interchange1.PK;

				var responseMessage1 = CreateMessage("119812", ZGuid.Empty, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, TraxonMessageProcessorTest.TestCIMFMAMessageResponse);
				responseMessage1.EM_GB = GlbBranch.CurrentBranch.PK;

				var transmitMessage2 = CreateMessage("119813", ZGuid.Empty, EDIMessage.Direction.Receive, EDIMessage.Status.Sent, "TEST");
				transmitMessage2.EM_GB = branch2.PK;
				var interchange2 = CreateInterchange("2345");
				transmitMessage2.EM_EI = interchange2.PK;

				var responseMessage2 = CreateMessage("119813", ZGuid.Empty, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, TraxonMessageProcessorTest.TestCIMFNAMessageResponse);
				responseMessage2.EM_GB = branch2.PK;

				Factory.Save();
				transmitMessage1.EM_MessageNum = "119812";
				transmitMessage2.EM_MessageNum = "119813";
				transmitMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				transmitMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				Factory.Save();

				new MessageProcessorServiceTask().RunTask();

				responseMessage1.Reload();
				responseMessage2.Reload();

				AssertEquals("Status", EDIMessage.Status.Received, responseMessage1.EM_Status);
				AssertEquals("linked", transmitMessage1.EM_LinkUniqueID, responseMessage1.EM_LinkUniqueID);
				AssertEquals("Status", EDIMessage.Status.Received, responseMessage2.EM_Status);
				AssertEquals("linked", transmitMessage2.EM_LinkUniqueID, responseMessage2.EM_LinkUniqueID);
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
						"HK Customs messages inbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.Traxon,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		EDIInterchange CreateInterchange(ZString from)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = from;
			interchange.EI_To = "TRAXON";
			interchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.Traxon;
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			return interchange;
		}

		EDIMessage CreateMessage(ZString messageNum, ZGuid parentID, ZString direction, ZString status, ZString messageText)
		{
			var result = Factory.New<TraxonMessage>();
			result.EM_ReceiveTransmit = direction;
			result.EM_MessageNum = messageNum;
			result.EM_Status = status;
			result.EM_MessageText = messageText;
			result.EM_LinkTable = parentID == ZGuid.Empty ? string.Empty : ForwardingConsol.Schema.TableName;
			result.EM_LinkUniqueID = parentID;
			return result;
		}
	}
}
