using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.MX.Manifest.Business;
using Enterprise.Customs.MX.Manifest.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.MX.Manifest.ServiceTasks.Testing
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
				AssertEquals("Code", "MXR", hostedServiceAttribute.Code);
				AssertEquals("Description", "Mexican Receive Processor", hostedServiceAttribute.Description);
				AssertEquals("Category", "MXC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Mexico, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunTask()
		{
			var interchange2 = PopulateEDIMessage(MessageTypes.Codes.MXD, MXMessageConstants.MXCustomsForSeaMode);
			var interchange3 = PopulateEDIMessage(MessageTypes.Codes.MXF, MXMessageConstants.MXCustomsForAirMode);
			var interchange4 = PopulateEDIMessage(MessageTypes.Codes.MXG, MXMessageConstants.MXCustomsForAirMode);

			Factory.Save();

			var task = new MessageRetrieverService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			var messagesCreated = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("NumberOfInterchanges", 3, messagesCreated.Length);

			AssertMessage(interchange2, MessageTypes.Codes.MXD);
			AssertMessage(interchange3, MessageTypes.Codes.MXF);
			AssertMessage(interchange4, MessageTypes.Codes.MXG);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Descriptions.MXR,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.MXCustoms),
				};
			}
		}

		void AssertMessage(EDIInterchange interchange, ZString messageType)
		{
			CombineAssertions(() =>
			{
				var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
				AssertEquals(EDIMessageSchema.Constants.EM_ApplicationCode, "MXC", message.EM_ApplicationCode);
				AssertEquals(EDIMessageSchema.Constants.EM_MessageType, messageType, message.EM_MessageType);
				AssertEquals(EDIMessageSchema.Constants.EM_ReceiveTransmit, "RCV", message.EM_ReceiveTransmit);
				AssertEquals(EDIMessageSchema.Constants.EM_Status, "QUE", message.EM_Status);
				AssertEquals(EDIMessageSchema.Constants.EM_MessageText, BodyText, message.EM_MessageText);
				AssertEquals(EDIMessageSchema.Constants.EM_GB, interchange.EI_GB, message.EM_GB);
				AssertEquals(EDIMessageSchema.Constants.EM_GE, GlbDepartment.CurrentDepartment.PK, message.EM_GE);
				AssertEquals(EDIMessageSchema.Constants.EM_MessageNum, interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength), message.EM_MessageNum);
				AssertEquals(EDIMessageSchema.Constants.EM_EI, interchange.PK, message.EM_EI);

				interchange.Reload();
				AssertEquals(EDIMessageSchema.Constants.EM_EI, interchange.PK, message.EM_EI);
				AssertEquals(EDIInterchangeSchema.Constants.EI_Status, "RCV", interchange.EI_Status);
			});
		}

		EDIInterchange PopulateEDIMessage(ZString interchangeType, string eiFrom)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.MXCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = eiFrom;
			interchange.EI_To = "eHub";
			interchange.EI_BodyText = BodyText;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;

			return interchange;
		}

		static ZString GetExpectedMessageXML(ZString path)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);

			return doc.OuterXml;
		}

		string BodyText => bodyText ?? (bodyText = GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.SeaAcceptedFinalResponse)));
		string bodyText;
	}
}
