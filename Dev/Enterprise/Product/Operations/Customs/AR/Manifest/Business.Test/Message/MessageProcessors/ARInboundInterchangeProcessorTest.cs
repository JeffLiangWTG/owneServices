using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	sealed class ARInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMesssage()
		{
			var processor = new ARInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.ARCustoms });

			var interchange = CreateInterchange(MessageTypes.Codes.ARE, ARMessageConstants.ARCustomsAirMode, airAcceptedResponse);
			processor.ExecuteBatch();
			var processedInterchange = interchange;
			processedInterchange.Reload();
			AssertMessage(processedInterchange);

			interchange = CreateInterchange(MessageTypes.Codes.ARB, ARMessageConstants.ARCustomsSeaMode, seaAcceptedResponse);
			processor.ExecuteBatch();
			processedInterchange = interchange;
			processedInterchange.Reload();
			AssertMessage(processedInterchange);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMesssageWithEmptyBody()
		{
			var interchange = CreateInterchange(MessageTypes.Codes.ARE, ARMessageConstants.ARCustomsAirMode, ZString.Empty);
			var processor = new ARInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.ARCustoms });
			processor.ExecuteBatch();
			var processedInterchange = interchange;
			processedInterchange.Reload();
			AssertEmptyMessage(processedInterchange);
		}

		void AssertMessage(EDIInterchange interchange)
		{
			var message = interchange.ContainedMessages[0];
			AssertEquals(message.EM_ApplicationCode, ApplicationCodeList.Codes.ARCustoms);
			AssertEquals(message.EM_MessageType, interchange.EI_InterchangeType);
			AssertEquals(message.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive);
			AssertEquals(message.EM_Status, EDIMessageStatusList.Codes.Queued);
			AssertEquals(message.EM_MessageText, interchange.EI_BodyText);
			AssertEquals(message.EM_GB, interchange.EI_GB);
			AssertEquals(message.EM_GE, GlbDepartment.CurrentDepartment.PK);
			AssertEquals(message.EM_MessageNum, interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength));
			AssertEquals(message.EM_EI, interchange.PK);
		}

		EDIInterchange CreateInterchange(ZString interchangeType, ZString from, ZString bodyText)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.ARCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = from;
			interchange.EI_To = "eHub";
			interchange.EI_BodyText = bodyText;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();
			return interchange;
		}

		public void AssertEmptyMessage(EDIInterchange interchange)
		{
			AssertEquals(EDIInterchange.Status.Error, interchange.EI_Status);
			AssertEquals("NO AR CUSTOMS DATA", interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
			AssertEquals(0, interchange.ContainedMessages.Count);
		}

		readonly ZString airAcceptedResponse = ARMessageTestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, ARMessageTestingConstants.AirAcceptedResponse));
		readonly ZString seaAcceptedResponse = ARMessageTestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, ARMessageTestingConstants.SeaAcceptedResponse));
	}
}
