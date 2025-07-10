using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SEAAARMessageBuilderTest : ArrivalReportBuilderAbstractTest
	{
		public override void TestDocumentName()
		{
			AssertEquals("DocumentName", "SEAAAR", ((SEAAARMessageBuilder)Builder).DocumentName);
		}

		public override void TestTransportDetails()
		{
			Assert("TransportDetails", GeneratedMessage.Contains("TDT+20+123++11++++8811924::11'"));
		}

		public void TestBerthCode()
		{
			Assert("BerthCode", GeneratedMessage.Contains("LOC+164+AGL-M::95'"));
		}

		public void TestDischargeCTOID()
		{
			Assert("DischargeCTOID", GeneratedMessage.Contains("NAD+TR+9122M::95'"));
		}

		public void TestStevadoreID()
		{
			Assert("StevadoreID", GeneratedMessage.Contains("NAD+UP+9121B::95'"));
		}

		public void TestWithdrawMessage()
		{
			messageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			AssertMultilineEquals("Message", "UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:99B:UN'BGM+98:::SEAAAR+<<SENDERS REFERENCE PLACE HOLDER>>/DAT0:1+50'NAD+TR+9122M::95'TDT+20+123++11++++8811924::11'UNT+5+<<MSGNO PLACEHOLDER>>'", GeneratedMessage, '\'');
		}

		protected override ArrivalReportBuilder Builder
		{
			get
			{
				var result = new SEAAARMessageBuilder(oceanBill);
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
			oceanBill.BerthCode = "AGL-M";
			oceanBill.DischargeCTOID = "9122M";
			oceanBill.StevadoreID = "9121B";
		}

		CusSCAOceanBill oceanBill;
	}
}
