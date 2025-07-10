using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC025A;
using Enterprise.Customs.FR.Business;
using NctsTransitStatusList = Enterprise.Customs.FR.Business.NctsTransitStatusList;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC025AProcessorTest : DTBaseProcessorTest<Cc025AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT025A_MESSAGE_TEMPLATE.xml").Replace("{IrrHEA1020}", "0"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewArrivalStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.NoIrregularities,
					ExpectedNewMessageInterpretation = @"
<p>New arrival status: Goods Released From Transit Upon Arrival</p>
<p>New detailed departure status: No Irregularities</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>Release Date: 17/01/2018</p>
<p>Irregularities: No</p>"
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT025A_MESSAGE_TEMPLATE.xml").Replace("{IrrHEA1020}", "1"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewArrivalStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.Irregularities,
					ExpectedNewMessageInterpretation = @"
<p>New arrival status: Goods Released From Transit Upon Arrival</p>
<p>New detailed departure status: Irregularities</p>
<p>Status granted on: 11/04/2019 11:28</p>
<p>Release Date: 17/01/2018</p>
<p>Irregularities: Yes</p>"
				};
			}
		}
	}
}
