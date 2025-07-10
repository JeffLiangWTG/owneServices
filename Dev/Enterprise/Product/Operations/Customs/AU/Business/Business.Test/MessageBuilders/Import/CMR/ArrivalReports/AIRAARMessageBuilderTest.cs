using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AIRAARMessageBuilderTest : ArrivalReportBuilderAbstractTest
	{
		public override void TestDocumentName()
		{
			AssertEquals("DocumentName", "AIRAAR", ((AIRAARMessageBuilder)Builder).DocumentName);
		}

		public override void TestTransportDetails()
		{
			Assert("TransportDetails", GeneratedMessage.Contains("TDT+20+123++6+5X::3'"));
		}

		public void TestLastOverseasPortOfDeparture()
		{
			Assert("LastOverseasPortOfDeparture", GeneratedMessage.Contains("LOC+125+NZAKL::6'"));
		}

		public void TestEstimatedDateOfArrival()
		{
			Assert("EstimatedDateOfArrival", GeneratedMessage.Contains("DTM+132:20050124:102'"));
		}

		public void TestLastDateOfDeparture()
		{
			Assert("LastDateOfDeparture", GeneratedMessage.Contains("DTM+" + DateTimeCodeQualifier + ":20050124:102'"));
		}

		protected override ArrivalReportBuilder Builder
		{
			get
			{
				var result = new AIRAARMessageBuilder(mawb);
				result.MessageSubType = messageSubType;
				result.Messages = mawb.Messages;
				return result;
			}
		}

		protected override ZString DateTimeCodeQualifier => "253";

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "41065894724");
			mawb = Factory.New<CusMAWB>();
			mawb.CM_ResponsiblePartyID = "41065894724";
			mawb.CM_FlightNo = "5X-123";
			mawb.CM_MAWB = "08144014703";
			mawb.CM_ArrivalDate = new ZDateTime(2005, 1, 24, 18, 0, 0);
			mawb.CM_RL_NKDischargePort = "AUSYD";
			mawb.CM_RL_NKLoadPort = "NZAKL";
		}

		CusMAWB mawb;
	}
}
