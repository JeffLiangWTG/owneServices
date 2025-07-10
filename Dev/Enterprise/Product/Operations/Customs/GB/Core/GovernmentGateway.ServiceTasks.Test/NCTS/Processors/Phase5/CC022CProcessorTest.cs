using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc022c;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC022CProcessorTest : NctsBaseProcessorTest<Cc022CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				yield return new MessageProcessorTestCase
				{
					MRN = "23GB000246ZPABRDJ3",
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC022C_Message.xml"),
					MessageSubType = "22C",
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Sent,
					ExpectedNewMessageInterpretation = "<q1:CC022C xmlns:q1=\"http://ncts.dgtaxud.ec\">\r\n  <messageType>CC022C</messageType>\r\n  <TransitOperation>\r\n    <MRN>23GB000246ZPABRDJ3</MRN>\r\n  </TransitOperation>\r\n</q1:CC022C>",
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
				};
			}
		}
	}
}
