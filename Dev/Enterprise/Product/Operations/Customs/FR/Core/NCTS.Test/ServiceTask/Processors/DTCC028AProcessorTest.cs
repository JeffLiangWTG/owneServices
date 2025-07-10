using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC028A;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC028AProcessorTest : DTBaseProcessorTest<Cc028AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT028A_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated,
					ExpectedNewDetailedDepartureStatus = ZString.Empty,
					ExpectedNewMessageInterpretation = @"
<p>New departure status: Declaration MRN Allocated</p>
<p>Status granted on: 11/04/2019 11:28</p>",
					HeaderAssertion = header =>
					{
						AssertEquals("MRN0035909", header.MovementReferenceNumber);
						AssertEquals(new ZDateTime(2021, 05, 20), header.MovementHeader.BM_EntryDate);
						AssertEquals(new ZDateTime(2021, 05, 20), header.MovementHeader.BM_ValuationDate);
					}
				};

				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT028A_MESSAGE.xml"),
					SetUpHeader = header => header.MovementHeader.BM_EntryDate = new ZDateTime(2021, 05, 01),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated,
					ExpectedNewDetailedDepartureStatus = ZString.Empty,
					ExpectedNewMessageInterpretation = @"
<p>New departure status: Declaration MRN Allocated</p>
<p>Status granted on: 11/04/2019 11:28</p>",
					HeaderAssertion = header =>
					{
						AssertEquals("MRN0035909", header.MovementReferenceNumber);
						AssertEquals("We don't set BM_EntryDate if BM_EntryDate has a value.", new ZDateTime(2021, 05, 01), header.MovementHeader.BM_EntryDate);
					}
				};
			}
		}
	}
}
