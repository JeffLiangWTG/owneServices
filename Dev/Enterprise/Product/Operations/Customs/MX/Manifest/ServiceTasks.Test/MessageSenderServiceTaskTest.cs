using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.MX.Manifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.MX.Manifest.ServiceTasks.Testing
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
				AssertEquals("Code", "MXS", hostedServiceAttribute.Code);
				AssertEquals("Description", "MCS Mexican Customs Sender", hostedServiceAttribute.Description);
				AssertEquals("Category", "MXC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "1Minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Mexico, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestRunTask()
		{
			var credential = Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_UserID = "USERNAME";
			credential.GP_CurrentPassword = "Password";

			var manifest1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest1.AMA_JobReference = "MAN0000070";
			var message1 = CreateMessage(manifest1.PK, MessageTypes.Codes.MXA, "MESSAGE1");

			var manifest2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest2.AMA_JobReference = "MAN0000071";
			var message2 = CreateMessage(manifest2.PK, MessageTypes.Codes.MXE, "MESSAGE2");

			var task = new MessageSenderService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			CombineAssertions(() =>
			{
				var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("NumberOfInterchanges", 2, interchangesCreated.Length);

				var zquery = new ZDBOnlyQuery(typeof(EDIInterchange));
				var ediMessageQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_EI);
				ediMessageQuery.AddToFilter(EDIMessageSchema.PK, message1.PK);
				zquery.AddSubQuery(ediMessageQuery, JoinCondition.And);
				var interchange = Factory.LoadTop1<EDIInterchange>(zquery);

				AssertEquals("EI_ApplicationCode", "MXC", interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeNum", message1.EM_MessageNum, interchange.EI_InterchangeNum);
				AssertEquals("EI_InterchangeType", "MXA", interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("EI_To", "MXCustomsSeaModeTesting", interchange.EI_To);
				AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("EI_Status", "QUE", interchange.EI_Status);
				AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);

				AssertNotNull(interchange.EI_SessionGUID);

				message1.Reload();
				AssertEquals("EM_EI", interchange.PK, message1.EM_EI);
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message1.EM_Status);

				zquery = new ZDBOnlyQuery(typeof(EDIInterchange));
				ediMessageQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_EI);
				ediMessageQuery.AddToFilter(EDIMessageSchema.PK, message2.PK);
				zquery.AddSubQuery(ediMessageQuery, JoinCondition.And);
				interchange = Factory.LoadTop1<EDIInterchange>(zquery);

				AssertEquals("EI_ApplicationCode", "MXC", interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeNum", message2.EM_MessageNum, interchange.EI_InterchangeNum);
				AssertEquals("EI_InterchangeType", "MXE", interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("EI_To", "MXCustomsAirMode", interchange.EI_To);
				AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("EI_Status", "QUE", interchange.EI_Status);
				AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);

				message2.Reload();
				AssertEquals("EM_EI", interchange.PK, message2.EM_EI);
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message2.EM_Status);
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
						ServiceTaskApplicationCodeList.Descriptions.MXS,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.MXCustoms,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		MXMessage CreateMessage(ZGuid manifestPK, ZString messageType, ZString bodyText)
		{
			var message = Factory.New<MXMessage>();

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.MXCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = manifestPK;
			message.EM_MessageText = bodyText;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();
			return message;
		}
	}
}
