using System.IO;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	sealed class MXCInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMesssage()
		{
			var airBodyText = GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.AirAcceptedFinalResponse));
			var seaBodyText = GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.SeaRejectedWithEnvelopeFirstResponse));

			var interchange = CreateInterchange(seaBodyText, MessageTypes.Codes.MXA, MXMessageConstants.MXCustomsForSeaMode);
			var interchange3 = CreateInterchange(airBodyText, MessageTypes.Codes.MXF, MXMessageConstants.MXCustomsForAirMode);
			var interchange4 = CreateInterchange(airBodyText, MessageTypes.Codes.MXG, MXMessageConstants.MXCustomsForAirMode);

			Factory.Save();

			var processor = new MXCInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.MXCustoms });
			processor.ExecuteBatch();

			var processedInterchange = interchange;
			processedInterchange.Reload();
			var processedInterchange3 = interchange3;
			processedInterchange3.Reload();
			var processedInterchange4 = interchange4;
			processedInterchange4.Reload();

			CombineAssertions(() =>
			{
				AssertMessage(processedInterchange);
				AssertMessage(processedInterchange3);
				AssertMessage(processedInterchange4);
			});
		}

		public void TestCreateMesssageWithEmptyBody()
		{
			var interchange = CreateInterchange("", MessageTypes.Codes.MXA, MXMessageConstants.MXCustomsForSeaMode);
			var interchange2 = CreateInterchange("", MessageTypes.Codes.MXD, MXMessageConstants.MXCustomsForSeaMode);
			var interchange3 = CreateInterchange("", MessageTypes.Codes.MXF, MXMessageConstants.MXCustomsForAirMode);
			var interchange4 = CreateInterchange("", MessageTypes.Codes.MXG, MXMessageConstants.MXCustomsForAirMode);

			Factory.Save();

			var processor = new MXCInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.MXCustoms });
			processor.ExecuteBatch();

			var processedInterchange = interchange;
			processedInterchange.Reload();
			var processedInterchange2 = interchange2;
			processedInterchange2.Reload();
			var processedInterchange3 = interchange3;
			processedInterchange3.Reload();
			var processedInterchange4 = interchange4;
			processedInterchange4.Reload();

			CombineAssertions(() =>
			{
				AssertEmptyMessage(processedInterchange);
				AssertEmptyMessage(processedInterchange2);
				AssertEmptyMessage(processedInterchange3);
				AssertEmptyMessage(processedInterchange4);
			});
		}

		MXInterchange CreateInterchange(string bodyText, string interchangeType, string eiFrom)
		{
			var interchange = Factory.New<MXInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.MXCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = eiFrom;
			interchange.EI_To = "eHub";
			interchange.EI_BodyText = bodyText;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;

			return interchange;
		}

		void AssertMessage(MXInterchange interchange)
		{
			var message = interchange.ContainedMessages[0];
			AssertEquals(message.EM_ApplicationCode, ApplicationCodeList.Codes.MXCustoms);
			AssertEquals(message.EM_MessageType, interchange.EI_InterchangeType);
			AssertEquals(message.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive);
			AssertEquals(message.EM_Status, EDIMessageStatusList.Codes.Queued);
			AssertEquals(message.EM_MessageText, interchange.EI_BodyText);
			AssertEquals(message.EM_GB, interchange.EI_GB);
			AssertEquals(message.EM_GE, GlbDepartment.CurrentDepartment.PK);
			AssertEquals(message.EM_MessageNum, interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength));
			AssertEquals(message.EM_EI, interchange.PK);
		}

		void AssertEmptyMessage(MXInterchange interchange)
		{
			AssertEquals(EDIInterchange.Status.Error, interchange.EI_Status);
			AssertEquals("NO MX CUSTOMS DATA", interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
			AssertEquals(0, interchange.ContainedMessages.Count);
		}

		public static ZString GetExpectedMessageXML(ZString path)
		{
			var doc = new XmlDocument();
			doc.Load(path);

			return doc.OuterXml;
		}
	}
}
