using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc182c;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class CC182CProcessorTest : NctsBaseProcessorTest<Cc182CType>
	{
		protected override IEnumerable<MessageProcessorTestCase> ProcessorTestCases
		{
			get
			{
				CreateZZRefTestValuesIfNeeded("GB000246", Core.Constants.CountryCodes.UnitedKingdom, ["DES"], "UK South Auth Consignor/nees");
				CreateZZRefTestValuesIfNeeded("XI000142", Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, ["DES"], "Belfast");
				yield return new MessageProcessorTestCase
				{
					MRN = "23GB000246T9DW2SJ9",
					IncomingMessageText = responseHelper.GetEmbeddedResourceFile("CC182C_Message.xml"),
					MessageSubType = "182",
					ExpectedNewDeclarationStatus = NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered,
					ExpectedNewMessageStatus = LogicalStatusList.Codes.Accepted,
					ExpectedPhase = NCTS5DeparturePhaseList.Codes.Declaration,
					ExpectedNewMessageInterpretation = @"<h2>Events during the journey</h2><table><tr><td>MRN:</td><td>23GB000246T9DW2SJ9.</td></tr><tr><td>Date:</td><td>2023-07-20 14:24:45</td></tr><tr><td>Customs Office Departure:</td><td>GB000246 UK South Auth Consignor/nees</td></tr><tr><td>Customs Office Incident:</td><td>XI000142 Belfast</td></tr></table><h3>Incident</h3><table><tr><td>Number:</td><td>1</td></tr><tr><td>Code:</td><td>Under the supervision of the customs authority, goods are transferred from one means of transport to another means of transport.</td></tr><tr><td>Text:</td><td>Change of tractor unit</td></tr></table><h4>Endorsement</h4><table><tr><td>Authority:</td><td>XIHMRC</td></tr><tr><td>Country:</td><td>XI</td></tr><tr><td>Date:</td><td>20-07-2023</td></tr><tr><td>Place:</td><td>GBBEL</td></tr></table><h4>Location</h4><table><tr><td>Qualifier:</td><td>U</td></tr><tr><td>UNLOCO:</td><td>GBBEL</td></tr><tr><td>GNNS Latitude:</td><td>&nbsp;</td></tr><tr><td>GNNS Longitude:</td><td>&nbsp;</td></tr></table><h4>Transhipment</h4><table><tr><td>Container:</td><td>Yes</td></tr><tr><td>Transport Means:</td><td>AF 0012 Registration Number of the Road Vehicle</td></tr></table><h4>Transport Equipment</h4><table><tr><td>Number:</td><td>1</td></tr><tr><td>Container:</td><td>WGPCGR</td></tr><tr><td>Number of Seals:</td><td>1</td></tr><tr><td>Seals:</td><td>1234</td></tr></table>",
					HeaderAssertion = header => AssertEquals("Y", header.BH_ExportFlag),
					ExpectedMovementType = NctsMovementType.Codes.Departure,
					ExpectedAcceptMovementType = NctsMovementType.Codes.Departure,
				};
			}
		}

		protected override IEnumerable<MessageShouldBeDiscardedTestCase> MessageShouldBeDiscardedTestCases
		{
			get
			{
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=MRN",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
					},
					ExpectedResult = false,
				};
				yield return new MessageShouldBeDiscardedTestCase
				{
					Description = "BM_CustomsStatus=WRO",
					Setup = header =>
					{
						header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;
					},
					ExpectedResult = true,
					ExpectedReasonText = "The message was discarded, because the Status at Customs of the declaration was WRO",
				};
			}
		}
	}
}
