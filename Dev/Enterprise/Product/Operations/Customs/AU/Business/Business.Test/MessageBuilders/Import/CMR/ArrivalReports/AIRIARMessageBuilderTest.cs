using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AIRIARMessageBuilderTest : ImpendingArrivalReportBuilderAbstractTest
	{
		public override void TestDocumentName()
		{
			AssertEquals("DocumentName", "AIRIAR", ((AIRIARMessageBuilder)Builder).DocumentName);
		}

		public override void TestDischargeCTOID()
		{
			mawb.DischargeCTOID = "9163D";
			Assert("DischargeCTOID", GeneratedMessage.Contains("NAD+TR+9163D::95'"));
		}

		public override void TestTransportDetails()
		{
			Assert("TransportDetails", GeneratedMessage.Contains("TDT+20+333++6+99::3'"));
		}

		protected override ArrivalReportBuilder Builder
		{
			get
			{
				var result = new AIRIARMessageBuilder(mawb);
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
			mawb.CM_FlightNo = "99 333";
			mawb.CM_MAWB = "08144014703";
			mawb.CM_ArrivalDate = new ZDateTime(2005, 1, 24, 18, 0, 0);
			mawb.CM_RL_NKDischargePort = "AUSYD";
			mawb.CM_RL_NKLoadPort = "NZAKL";
		}

		CusMAWB mawb;
	}
}
