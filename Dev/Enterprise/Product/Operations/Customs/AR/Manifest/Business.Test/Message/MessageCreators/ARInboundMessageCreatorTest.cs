using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	sealed class ARInboundMessageCreatorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMesssage()
		{
			var airAcceptedResponse = ARMessageTestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, ARMessageTestingConstants.AirAcceptedResponse));
			var seaAcceptedResponse = ARMessageTestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, ARMessageTestingConstants.SeaAcceptedResponse));

			ProcessMessage(CreateInterchange(MessageTypes.Codes.ARE, ARMessageConstants.ARCustomsAirMode, airAcceptedResponse));
			ProcessMessage(CreateInterchange(MessageTypes.Codes.ARB, ARMessageConstants.ARCustomsSeaMode, seaAcceptedResponse));
		}

		void ProcessMessage(EDIInterchange interchange)
		{
			var creator = new ARInboundMessageCreator();
			creator.CreateMessagesForInterchange(interchange);
			interchange.Reload();
			AssertMessage(interchange);
		}

		void AssertMessage(EDIInterchange interchange)
		{
			EDIMessage message = interchange.ContainedMessages[0];
			CombineAssertions(() =>
			{
				AssertNotNull(message.EM_GE);

				AssertEquals(message.EM_GB, interchange.EI_GB);
				AssertEquals(message.EM_EI, interchange.PK);
				AssertEquals(message.EM_MessageNum, interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength));
			});
		}

		EDIInterchange CreateInterchange(ZString interchangeType, ZString from, ZString bodyText)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.ARCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = from;
			interchange.EI_To = "eHub";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_BodyText = bodyText;

			Factory.Save();
			return interchange;
		}
	}
}
