using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SEAIARMessageBuilderTest : ImpendingArrivalReportBuilderAbstractTest
	{
		public override void TestDocumentName()
		{
			AssertEquals("DocumentName", "SEAIAR", ((SEAIARMessageBuilder)Builder).DocumentName);
		}

		public void TestFirstPortOfArrival()
		{
			Assert("FirstPortOfArrival", GeneratedMessage.Contains("LOC+60+AUSYD::6'"));
		}

		public void TestStevadoreID()
		{
			oceanBill.StevadoreID = "9121B";
			Assert("StevadoreID", GeneratedMessage.Contains("NAD+UP+9121B::95'"));
		}

		public override void TestTransportDetails()
		{
			Assert("TransportDetails", GeneratedMessage.Contains("TDT+20+123++11++++8811924::11'"));
		}

		public override void TestLastDateOfDeparture()
		{
			Assert(true);
		}

		public override void TestEstimatedDateOfArrival()
		{
			Assert(true);
		}

		public override void TestDischargeCTOID()
		{
			oceanBill.DischargeCTOID = "9122M";
			Assert("DischargeCTOID", GeneratedMessage.Contains("NAD+TR+9122M::95'"));
		}

		public void TestWithdrawEndToEnd()
		{
			messageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			AssertEquals("Message", "UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:99B:UN'BGM+98:::SEAIAR+<<SENDERS REFERENCE PLACE HOLDER>>/DAT0:1+50'TDT+20+123++11++++8811924::11'UNT+4+<<MSGNO PLACEHOLDER>>'", GeneratedMessage);
		}

		public void TestAmendmentEndToEnd()
		{
			var tranHead = Factory.New<CusSeaManTranHead>();
			tranHead.BT_PortOfLastForeignPortATD = new ZDateTime(2005, 10, 09); // daylight savings time
			tranHead.BT_PrincipalID = "12345";
			tranHead.BT_ResponsiblePartyID = "12345";
			tranHead.BT_RL_NKPortOfLastForeignPort = "NZAKL";
			tranHead.BT_VoyageNum = "321";
			tranHead.BT_VesselName = "ADMIRALENGRACHT";

			var org = OrgHeader.New(Factory);
			org.FillWithValidTestData();
			org.LocalBusinessRegNo = "FOO";
			org.MainAddress.LocalControlledPremisesID = "BAR";

			var port = tranHead.Arrivals.AddNew();
			port.BA_IsFirstArrival = true;
			port.BA_RL_NKArrivalPort = "AUSYD";
			port.BA_DischargeIndicator = true;
			port.BA_BerthCode = "SPAM";
			port.BA_ArrivalPortETA = new ZDateTime(2005, 10, 10);       // daylight savings time
			port.BA_OA_CTOAddress = org.MainAddress.PK;

			var messageBuilder = new SEAIARMessageBuilder(tranHead);
			messageBuilder.Messages = tranHead.Messages;
			messageBuilder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertMultilineEquals("Original message with one port", @"UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:99B:UN
BGM+98:::SEAIAR+<<SENDERS REFERENCE PLACE HOLDER>>/DAT1:1+9
RFF+ABP:12345
LOC+125+NZAKL::6
LOC+79+AUSYD::6
DTM+136:20051008:102
DTM+136:1100:401
TDT+20+321++11++++8811924::11
LOC+60+AUSYD::6
DTM+132:20051009:102
DTM+132:1400:401
NAD+TR+BAR::95
NAD+UP+FOO::95
STS++Y:63:95
FTX+LIN+I
UNT+16+<<MSGNO PLACEHOLDER>>".Replace("\r\n", "'"), messageBuilder.GeneratedMessageStrings[0], '\'');
			var originalMessage = Factory.New<CMRSEAIARMessage>();
			originalMessage.EM_MessageText = messageBuilder.MessageText;
			tranHead.Messages.Add(originalMessage);
			AssertEquals(1, tranHead.Messages.Count);
			Factory.Save();

			org = OrgHeader.New(Factory);
			org.FillWithValidTestData();
			org.LocalBusinessRegNo = "BACON";
			org.MainAddress.LocalControlledPremisesID = "GHERKIN";

			port = tranHead.Arrivals.AddNew();
			port.BA_RL_NKArrivalPort = "AUMEL";
			port.BA_DischargeIndicator = true;
			port.BA_BerthCode = "ONION";
			port.BA_ArrivalPortETA = new ZDateTime(2005, 10, 11);
			port.BA_OA_CTOAddress = org.MainAddress.PK;

			messageBuilder = new SEAIARMessageBuilder(tranHead);
			messageBuilder.Messages = tranHead.Messages;
			messageBuilder.MessageSubType = Customs.Common.MessageBuilders.MessageSubTypes.Change;
			AssertMultilineEquals("Amending with added port", @"UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:99B:UN
BGM+98:::SEAIAR+<<SENDERS REFERENCE PLACE HOLDER>>/DAT0:2+4
RFF+ABP:12345
LOC+125+NZAKL::6
LOC+79+AUSYD::6
DTM+136:20051008:102
DTM+136:1100:401
TDT+20+321++11++++8811924::11
LOC+60+AUMEL::6
DTM+132:20051010:102
DTM+132:1400:401
NAD+TR+GHERKIN::95
NAD+UP+BACON::95
STS++Y:63:95
FTX+LIN+I
UNT+16+<<MSGNO PLACEHOLDER>>".Replace("\r\n", "'"), messageBuilder.GeneratedMessageStrings[0], '\'');
		}

		protected override ArrivalReportBuilder Builder
		{
			get
			{
				var result = new SEAIARMessageBuilder(oceanBill);
				result.MessageSubType = messageSubType;
				result.Messages = oceanBill.MessageCollection;
				return result;
			}
		}

		protected override ZString DateTimeCodeQualifier => "253";

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "41065894724");
			oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_Voyage = "123";
			oceanBill.CB_VesselName = "ADMIRALENGRACHT";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_RL_NKPortOfLoading = "NZAKL";
		}

		CusSCAOceanBill oceanBill;
	}
}
