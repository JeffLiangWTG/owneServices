using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.Customs.CL.Manifest.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.CL.Manifest.ServiceTasks.Testing
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
				AssertEquals("Code", "CCR", hostedServiceAttribute.Code);
				AssertEquals("Description", "Chilean Customs Receiver", hostedServiceAttribute.Description);
				AssertEquals("Category", "CHL", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Chile, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunTask()
		{
			var airAcceptedResponse = GetExpectedMessageXML(Path.Combine(BaseSourcePath, CLMessagingConstants.AirAcceptedResponse));
			var seaAcceptedResponse = GetExpectedMessageXML(Path.Combine(BaseSourcePath, CLMessagingConstants.SeaAcceptedResponse));

			var interchange1 = CreateInterchange(MessageTypes.Codes.CHB, CLMessageConstants.CLCustomsForSeaMode, seaAcceptedResponse);
			var interchange2 = CreateInterchange(MessageTypes.Codes.CHE, CLMessageConstants.CLCustomsForAirMode, airAcceptedResponse);

			var task = new MessageRetrieverService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			var messagesCreated = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("NumberOfInterchanges", 2, messagesCreated.Length);

			AssertMessage(interchange1, MessageTypes.Codes.CHB, seaAcceptedResponse);
			AssertMessage(interchange2, MessageTypes.Codes.CHE, airAcceptedResponse);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Descriptions.CCR,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CLCustoms),
				};
			}
		}

		void AssertMessage(EDIInterchange interchange, ZString interchangeType, ZString messageText)
		{
			CombineAssertions(() =>
			{
				var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
				AssertEquals(EDIMessageSchema.Constants.EM_ApplicationCode, ApplicationCodeList.Codes.CLCustoms, message.EM_ApplicationCode);
				AssertEquals(EDIMessageSchema.Constants.EM_ReceiveTransmit, EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
				AssertEquals(EDIMessageSchema.Constants.EM_Status, EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals(EDIMessageSchema.Constants.EM_MessageText, messageText, message.EM_MessageText);
				AssertEquals(EDIMessageSchema.Constants.EM_GB, interchange.EI_GB, message.EM_GB);
				AssertEquals(EDIMessageSchema.Constants.EM_GE, GlbDepartment.CurrentDepartment.PK, message.EM_GE);
				AssertEquals(EDIMessageSchema.Constants.EM_MessageNum, interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength), message.EM_MessageNum);
				AssertEquals(EDIMessageSchema.Constants.EM_EI, interchange.PK, message.EM_EI);

				interchange.Reload();
				AssertEquals(EDIMessageSchema.Constants.EM_EI, interchange.PK, message.EM_EI);
				AssertEquals(EDIInterchangeSchema.Constants.EI_Status, EDIInterchange.Direction.Receive, interchange.EI_Status);
			});
		}

		EDIInterchange CreateInterchange(ZString interchangeType, ZString from, ZString bodyText)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CLCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = from;
			interchange.EI_To = CLMessageConstants.EHub;
			interchange.EI_BodyText = bodyText;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_HeaderText = @"{""custom.FileName"":""IMP-BL-1.0-0000000001.xml"",""custom.ClientID"":""HYECHLCMT_SMS""}";

			Factory.Save();
			return interchange;
		}

		static ZString GetExpectedMessageXML(ZString path)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);

			return doc.OuterXml;
		}
	}
}
