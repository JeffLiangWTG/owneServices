using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.CL.Manifest.ServiceTasks.Testing
{
	[TestedType(typeof(MessageSenderService))]
	sealed class MessageSenderServiceTaskTest : ServiceTaskTestCase<MessageSenderService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "CCS", hostedServiceAttribute.Code);
				AssertEquals("Description", "CCS Chilean Customs Sender", hostedServiceAttribute.Description);
				AssertEquals("Category", "CHL", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "1Minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Chile, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunTask()
		{
			CLCustomsDataRegistry.Instance.CLSMSMessageSending.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CreateSMSMessageSending());

			var message1 = CreateMessage(MessageTypes.Codes.CHB, "12345", "Message Text");
			var message2 = CreateMessage(MessageTypes.Codes.CHC, "12351", "Message Text");
			var message3 = CreateMessage(MessageTypes.Codes.CHE, "19960", "Message Text");

			var task = new MessageSenderService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			CombineAssertions(() =>
			{
				var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("NumberOfInterchanges", 3, interchangesCreated.Length);

				var interchange = GetLinkedInterchange(message1.PK);

				AssertEquals("EI_ApplicationCode", "CLC", interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeNum", message1.EM_MessageNum, interchange.EI_InterchangeNum);
				AssertEquals("EI_InterchangeType", "CHB", interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("EI_To", "CW1KNZ", interchange.EI_To);
				AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("EI_Status", "QUE", interchange.EI_Status);
				AssertEquals("EI_BodyText", "Message Text", interchange.EI_BodyText);

				message1.Reload();
				AssertEquals("EM_EI", interchange.PK, message1.EM_EI);
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message1.EM_Status);

				interchange = GetLinkedInterchange(message2.PK);

				AssertEquals("EI_ApplicationCode", "CLC", interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeNum", message2.EM_MessageNum, interchange.EI_InterchangeNum);
				AssertEquals("EI_InterchangeType", "CHC", interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("EI_To", "CW1KNZ", interchange.EI_To);
				AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("EI_Status", "QUE", interchange.EI_Status);

				message2.Reload();
				AssertEquals("EM_EI", interchange.PK, message2.EM_EI);
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message2.EM_Status);

				interchange = GetLinkedInterchange(message3.PK);

				AssertEquals("EI_ApplicationCode", "CLC", interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeNum", message3.EM_MessageNum, interchange.EI_InterchangeNum);
				AssertEquals("EI_InterchangeType", "CHE", interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("EI_To", "CW1KNZ", interchange.EI_To);
				AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("EI_Status", "QUE", interchange.EI_Status);
				AssertEquals("EI_BodyText", "Message Text", interchange.EI_BodyText);

				message3.Reload();
				AssertEquals("EM_EI", interchange.PK, message3.EM_EI);
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message3.EM_Status);
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Descriptions.CCS,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CLCustoms,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		EDIMessage CreateMessage(ZString messageType, ZString messageNumber, ZString bodyText)
		{
			var message = Factory.New<CLMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CLCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageNum = messageNumber;
			message.EM_MessageText = bodyText;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();
			return message;
		}

		CLSMSMessageSending CreateSMSMessageSending()
		{
			var sms = new CLSMSMessageSending(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			sms.MachineName = "Machine Name";
			sms.ApplicationNodePassword = "1234";
			sms.RunningIntervalInSeconds = 15;
			sms.SendFolder = @"D:\Folders\SendFolder";
			sms.UnknownFolder = @"D:\Folders\UnknownFolder";
			sms.InvalidFolder = @"D:\Folders\InvalidFolder";
			sms.RejectedFolder = @"D:\Folders\RejectedFolder";
			sms.ReceiveFolder = @"D:\Folders\ReceiveFolder";
			sms.AcceptedFolder = @"D:\Folders\AcceptedFolder";
			sms.ApplicationNodeName = "CW1KNZ";

			Factory.Save();
			return sms;
		}

		EDIInterchange GetLinkedInterchange(ZGuid messagePK)
		{
			var zquery = new ZDBOnlyQuery(typeof(EDIInterchange));
			var ediMessageQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_EI);
			ediMessageQuery.AddToFilter(EDIMessageSchema.PK, messagePK);
			zquery.AddSubQuery(ediMessageQuery, JoinCondition.And);
			return Factory.LoadTop1<EDIInterchange>(zquery);
		}
	}
}
