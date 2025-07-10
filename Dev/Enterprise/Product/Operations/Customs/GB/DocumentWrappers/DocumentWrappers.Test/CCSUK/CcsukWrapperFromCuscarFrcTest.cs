using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk.Testing
{
	class CcsukWrapperFromCuscarFrcTest : TestCaseWithFactory
	{
		public void TestConstructorSplitHawb()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = Factory.New<CusHAWB>();
			RunConstructorTestSplit(hawb);
		}

		public void TestConstructorSplitBasic()
		{
			var basic = Factory.New<CusMAWB>();
			RunConstructorTestSplit(basic);
		}

		public void TestConstructorHawb()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = Factory.New<CusHAWB>();
			RunConstructorTestSplit(hawb);
		}

		public void TestConstructorBasic()
		{
			var basic = Factory.New<CusMAWB>();
			RunConstructorTestSplit(basic);
		}

		void RunConstructorTestSplit(ICcsukCusAwb awb)
		{
			var split1 = awb.Splits.AddNew();
			var split2 = awb.Splits.AddNew();
			split1.SplitReference = "01";
			split2.SplitReference = "02";
			split1.SetCustomsActionCode("CW", ZDateTime.BrettsBirthday);
			var inboundFsnTextIrrelevant02 = "UNH+306+CIMFSN:0:0:Z1:IATA+306/U00000092'FTX+CIM+++FSN:LHRCWE:190-42011006-HAWB0001:CSN/CW-02/10/19APR1001/00000092/OK TRANSFER SYD'UNT+3+306'";
			var inboundFsnTextIrrelevant01 = "UNH+306+CIMFSN:0:0:Z1:IATA+306/U00000093'FTX+CIM+++FSN:LHRCWE:190-42011006-HAWB0001:CSN/CA-01/10/19APR1001/00000093/REQUEST TRANSFER LGW'UNT+3+306'";
			var inboundFsnTextGood = "UNH+306+CIMFSN:0:0:Z1:IATA+306/U00000094'FTX+CIM+++FSN:LHRCWE:190-42011006-HAWB0001:CSN/CW-01/10/19APR1001/00000094/OK TRANSFER LGW'UNT+3+306'";
			MakeAlreadyProcessedInboundFsnMessage(awb, inboundFsnTextIrrelevant02, "02");
			MakeAlreadyProcessedInboundFsnMessage(awb, inboundFsnTextIrrelevant01, "01");
			MakeAlreadyProcessedInboundFsnMessage(awb, inboundFsnTextGood, "01");

			var frcMessage = MakeCurrentInboundFrcMessage(awb, "UNH+122+CUSCAR:2:912:UN:109503+1BCEE0F9764D464F9B8A5765E9E69A0D'BGM+:::FRC+ANYTHING+++HWB:WHATEVER:99'GIS+S2Y'GIS+T:121'TDT+20+8++++BH:172:3'LOC+11:LHR:145:3::CWE:129:ZZZ+84:ATL:145:3+85:LHR:145:3'NAD+CB+CAR'GID+0'FTX+AAA+++STUFF'QTY+118:44'QTY+48:0'MEA+WT++KGM:1.000'UNT+13+122'");

			var wrapper = new CcsukWrapperFromCuscarFrc(frcMessage, Factory);
			AssertEquals("Wrapper wraps split 01", split1.ReferenceNumber, wrapper.MAWBHAWBSPLIT);
			AssertEquals("Wrapper extracts from correct FSN", "00000094", wrapper.AGENTREF);
		}

		EDIMessage MakeCurrentInboundFrcMessage(ICcsukCusAwb awb, string inboundFrcText)
		{
			var frcMessage = awb.Messages.AddNew();
			frcMessage.EM_MessageText = inboundFrcText;
			frcMessage.EM_ApplicationReference = "01";
			frcMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			return frcMessage;
		}

		void MakeAlreadyProcessedInboundFsnMessage(ICcsukCusAwb awb, string inboundFsnText, ZString splitNumberForApplicationReference)
		{
			var fsnMessage = awb.Messages.AddNew();
			fsnMessage.EM_MessageText = inboundFsnText;
			if (!splitNumberForApplicationReference.IsEmpty)
			{
				fsnMessage.EM_ApplicationReference = splitNumberForApplicationReference;
			}
			fsnMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			fsnMessage.EM_MessageType = CcsukTransmissionMessageFunction.CIM.Code;
			fsnMessage.EM_MessageSubType = CcsukTransmissionMessageFunction.CIM.CUKFSR.FSN.Subcode;
			fsnMessage.EM_Status = EDIMessage.Status.Received;
		}
	}
}
