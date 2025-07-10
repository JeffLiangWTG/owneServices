using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AR.Manifest.Business;
using Enterprise.Customs.AR.Manifest.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.AR.Manifest.ServiceTasks.Testing
{
	[TestedType(typeof(MessageRetrieverService))]
	sealed class MessageRetrieverServiceTaskTest : ServiceTaskTestCase<MessageRetrieverService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "ARR", hostedServiceAttribute.Code);
				AssertEquals("Description", "Argentina Receive Processor", hostedServiceAttribute.Description);
				AssertEquals("Category", "ARC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Argentina, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunTask()
		{
			var seaAcceptedResponse = ARMessageTestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, ARMessageTestingConstants.SeaAcceptedResponse));
			var airAcceptedResponse = ARMessageTestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, ARMessageTestingConstants.AirAcceptedResponse));
			var airManifestInterchange = CreateInterchange(MessageTypes.Codes.ARE, ARMessageConstants.ARCustomsAirMode, airAcceptedResponse);
			var seaManifestInterchange = CreateInterchange(MessageTypes.Codes.ARB, ARMessageConstants.ARCustomsSeaMode, seaAcceptedResponse);

			var task = new MessageRetrieverService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			var messagesCreated = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("NumberOfInterchanges", 2, messagesCreated.Length);

			AssertMessage(airManifestInterchange);
			AssertMessage(seaManifestInterchange);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Descriptions.ARR,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.ARCustoms),
				};
			}
		}

		void AssertMessage(EDIInterchange interchange)
		{
			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			CombineAssertions(() =>
			{
				AssertEquals(EDIMessageSchema.Constants.EM_ApplicationCode, interchange.EI_ApplicationCode, message.EM_ApplicationCode);
				AssertEquals(EDIMessageSchema.Constants.EM_MessageType, interchange.EI_InterchangeType, message.EM_MessageType);
				AssertEquals(EDIMessageSchema.Constants.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive, message.EM_ReceiveTransmit);
				AssertEquals(EDIMessageSchema.Constants.EM_Status, EDIMessageStatusList.Codes.Queued, message.EM_Status);
				AssertEquals(EDIMessageSchema.Constants.EM_MessageText, interchange.EI_BodyText, message.EM_MessageText);
				AssertEquals(EDIMessageSchema.Constants.EM_GB, interchange.EI_GB, message.EM_GB);
				AssertEquals(EDIMessageSchema.Constants.EM_GE, GlbDepartment.CurrentDepartment.PK, message.EM_GE);
				AssertEquals(EDIMessageSchema.Constants.EM_MessageNum, interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength), message.EM_MessageNum);
				AssertEquals(EDIMessageSchema.Constants.EM_EI, interchange.PK, message.EM_EI);

				interchange.Reload();
				AssertEquals(EDIMessageSchema.Constants.EM_EI, message.EM_EI, interchange.PK);
				AssertEquals(EDIInterchangeSchema.Constants.EI_Status, EDIInterchange.Status.Received, interchange.EI_Status);
			});
		}

		EDIInterchange CreateInterchange(ZString interchangeType, ZString from, ZString bodyText)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.ARCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = from;
			interchange.EI_To = "eHub";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = bodyText;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();
			return interchange;
		}
	}
}
