using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AR.Manifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.AR.Manifest.ServiceTasks.Testing
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
				AssertEquals("Code", "ARS", hostedServiceAttribute.Code);
				AssertEquals("Description", "Argentina Customs Sender", hostedServiceAttribute.Description);
				AssertEquals("Category", "ARC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Argentina, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestRunTask()
		{
			var manifest1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest1.AMA_JobReference = "MAN0000070";
			var message1 = CreateMessage(manifest1, "MESSAGE1", MessageTypes.Codes.ARA);

			var manifest2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest2.AMA_JobReference = "MAN0000071";
			var message2 = CreateMessage(manifest2, "MESSAGE2", MessageTypes.Codes.ARD);

			var task = new MessageSenderService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("NumberOfInterchanges", 2, interchangesCreated.Length);

			AssertInterchange(message1, ARMessageConstants.ARCustomsSeaMode);
			AssertInterchange(message2, ARMessageConstants.ARCustomsAirMode);
		}

		void AssertInterchange(EDIMessage message, ZString to)
		{
			var zquery = new ZDBOnlyQuery(typeof(EDIInterchange));
			var ediMessageQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_EI);
			ediMessageQuery.AddToFilter(EDIMessageSchema.PK, message.PK);
			zquery.AddSubQuery(ediMessageQuery, JoinCondition.And);
			var interchange = Factory.LoadTop1<EDIInterchange>(zquery);

			CombineAssertions(() =>
			{
				AssertEquals("EI_ApplicationCode", EDIMessage.ApplicationCodes.ARCustoms, interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeNum", message.EM_MessageNum, interchange.EI_InterchangeNum);
				AssertEquals("EI_InterchangeType", message.EM_MessageType, interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", EDIMessage.Direction.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("EI_To", to, interchange.EI_To);
				AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("EI_Status", EDIInterchange.Status.Queued, interchange.EI_Status);
				AssertEquals("EI_TransportType", EDIInterchange.TransportType.xT, interchange.EI_TransportType);

				AssertNotNull(interchange.EI_SessionGUID);

				message.Reload();
				AssertEquals("EM_EI", interchange.PK, message.EM_EI);
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message.EM_Status);
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
						ServiceTaskApplicationCodeList.Descriptions.ARS,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.ARCustoms,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y"),
				};
			}
		}

		ARMessage CreateMessage(AsycudaManifestHeader manifest, ZString messageText, ZString messageType)
		{
			var message = Factory.New<ARMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ARCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = manifest.PK;
			message.EM_MessageText = messageText;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();
			return message;
		}
	}
}
