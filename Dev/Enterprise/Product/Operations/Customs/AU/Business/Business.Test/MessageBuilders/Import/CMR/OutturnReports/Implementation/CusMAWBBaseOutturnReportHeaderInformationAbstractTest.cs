using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CusMAWBBaseOutturnReportHeaderInformationAbstractTest : CusUnderbondOutturnReportHeaderInformationAbstractTest
	{
		public void TestDateTimeOfOutturn()
		{
			Underbond.C4_Outurned = new ZDateTime(2005, 6, 21, 11, 22, 0);
			AssertEquals("DateTimeOfOutturn", new ZDateTime(2005, 6, 21, 1, 22, 0), HeaderInfo.DateTimeOfOutturn);
		}

		public void TestDateTimeOfOutturnNoException()
		{
			Underbond.C4_Outurned = new ZDateTime(2005, 6, 21, 11, 22, 0);
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertNoExceptionThrown(delegate
			{ var accessed = HeaderInfo.DateTimeOfOutturn; });
		}

		[TestDate(2005, 04, 04)]
		public void TestDateTimeOfOutturnUTC()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_RL_NKClosestPort = "AUSYD";
			var address = header.Addresses.AddNew();

			Underbond.C4_OA_DestinationAddress = address.PK;
			Underbond.C4_Outurned = new ZDateTime(2005, 6, 21, 11, 22, 0);
			AssertEquals("UTC DateTimeOfOutturn", new ZDateTime(2005, 6, 21, 1, 22, 0), HeaderInfo.DateTimeOfOutturn);
		}

		public void TestMatchingDateTimeOfOutturnDoesntCauseMessageRejection()
		{
			var dateTimeOfArrival = new ZDateTime(2006, 9, 18);
			var dateTimeOfOutturn = new ZDateTime(2006, 9, 18, 11, 11, 00);

			var mAWB1 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "M1";
			mAWB1.CM_FlightNo = "QF111";
			mAWB1.CM_ArrivalDate = dateTimeOfArrival;

			var underbond1 = mAWB1.Underbonds.AddNew();
			underbond1.C4_ParentID = mAWB1.PK;
			underbond1.C4_SendersMessageReference = "U00000001";
			underbond1.C4_Outurned = dateTimeOfOutturn;

			var outturn1 = underbond1.Outturns.AddNew();
			outturn1.C5_MasterBill = mAWB1.CM_MAWB;
			outturn1.C5_PackagesOutturned = 5;
			outturn1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			Factory.Save();

			var header1 = new CusMAWBOutturnReportHeaderInformation(mAWB1, underbond1);
			// Date is converted to UTC in the header for messaging
			AssertEquals(dateTimeOfOutturn.AddHours(-10), header1.DateTimeOfOutturn);

			var mAWB2 = Factory.New<CusMAWB>();
			mAWB2.CM_MAWB = "M2";
			mAWB2.CM_FlightNo = "QF111";
			mAWB2.CM_ArrivalDate = dateTimeOfArrival;

			var underbond2 = mAWB2.Underbonds.AddNew();
			underbond2.C4_ParentID = mAWB2.PK;
			underbond2.C4_SendersMessageReference = "U00000002";
			underbond2.C4_Outurned = dateTimeOfOutturn;

			var outturn2 = underbond2.Outturns.AddNew();
			outturn2.C5_HouseBill = mAWB2.CM_MAWB;
			outturn2.C5_PackagesOutturned = 4;
			outturn2.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			var header2 = new CusMAWBOutturnReportHeaderInformation(mAWB2, underbond2);
			// Date is converted to UTC in the header for messaging
			AssertEquals(dateTimeOfOutturn.AddHours(-10).AddMinutes(1), header2.DateTimeOfOutturn);
			AssertEquals(dateTimeOfOutturn.AddMinutes(1), underbond2.C4_Outurned);

			var mAWB3 = Factory.New<CusMAWB>();
			mAWB3.CM_MAWB = "M3";
			mAWB3.CM_FlightNo = "QF222";
			mAWB3.CM_ArrivalDate = new ZDateTime(2006, 9, 19);

			var underbond3 = mAWB3.Underbonds.AddNew();
			underbond3.C4_ParentID = mAWB3.PK;
			underbond3.C4_SendersMessageReference = "U00000003";
			underbond3.C4_Outurned = dateTimeOfOutturn;

			var outturn3 = underbond3.Outturns.AddNew();
			outturn3.C5_HouseBill = mAWB3.CM_MAWB;
			outturn3.C5_PackagesOutturned = 3;
			outturn3.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			var header3 = new CusMAWBOutturnReportHeaderInformation(mAWB3, underbond3);
			// Date is converted to UTC in the header for messaging
			AssertEquals(dateTimeOfOutturn.AddHours(-10), header3.DateTimeOfOutturn);
			AssertEquals(dateTimeOfOutturn, underbond3.C4_Outurned);
		}

		public void TestFlightNumber()
		{
			MAWB.CM_FlightNo = "QF123";
			AssertEquals("FlightNum", "QF123", HeaderInfo.FlightNumber);
		}

		public void TestOutturnForDifferentFlightNumber()
		{
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08165225436";
			mAWB.CM_FlightNo = "QF190";
			mAWB.CM_ArrivalDate = new ZDateTime(2006, 07, 24);
			var underbond1 = mAWB.Underbonds.AddNew();
			underbond1.C4_SendersMessageReference = "U00003023";
			underbond1.C4_OriginPremiseID = "9532M";
			underbond1.C4_DestinationPremiseID = "DP418";
			underbond1.C4_FlightNo = "QF190";
			underbond1.C4_ArrivalDate = new ZDateTime(2006, 07, 24);
			var outturnLine1 = underbond1.Outturns.AddNew();
			outturnLine1.C5_HouseBill = "H1";
			outturnLine1.C5_PackagesOutturned = 4;
			var underbond2 = mAWB.Underbonds.AddNew();
			underbond2.C4_SendersMessageReference = "U00003024";
			underbond2.C4_OriginPremiseID = "9532M";
			underbond2.C4_DestinationPremiseID = "DP418";
			underbond2.C4_FlightNo = "QF120";
			underbond2.C4_ArrivalDate = new ZDateTime(2006, 07, 25);
			var outturnLine2 = underbond2.Outturns.AddNew();
			outturnLine2.C5_HouseBill = "H2";
			outturnLine2.C5_PackagesOutturned = 2;
			var underbond3 = mAWB.Underbonds.AddNew();
			underbond3.C4_SendersMessageReference = "U00003024";
			underbond3.C4_OriginPremiseID = "9532M";
			underbond3.C4_DestinationPremiseID = "DP418";
			underbond3.C4_FlightNo = "";
			var outturnLine3 = underbond3.Outturns.AddNew();
			outturnLine3.C5_HouseBill = "H2";
			outturnLine3.C5_PackagesOutturned = 2;

			var header1 = new CusMAWBOutturnReportHeaderInformation(mAWB, underbond1);
			AssertEquals("QF190", header1.FlightNumber);
			AssertEquals(new ZDateTime(2006, 07, 24), header1.EstimatedDateOfArrival);
			var header2 = new CusMAWBOutturnReportHeaderInformation(mAWB, underbond2);
			AssertEquals("QF120", header2.FlightNumber);
			AssertEquals(new ZDateTime(2006, 07, 25), header2.EstimatedDateOfArrival);
			var header3 = new CusMAWBOutturnReportHeaderInformation(mAWB, underbond3);
			AssertEquals("QF190", header3.FlightNumber);
			AssertEquals(new ZDateTime(2006, 07, 24), header3.EstimatedDateOfArrival);
		}

		public void TestEstimatedDateOfArrival()
		{
			MAWB.CM_ArrivalDate = new ZDateTime(2005, 6, 21);
			AssertEquals("EstimatedDateOfArrival", new ZDateTime(2005, 6, 21), HeaderInfo.EstimatedDateOfArrival);
		}

		CusMAWBBase mawb;
		protected CusMAWBBase MAWB => mawb ?? (mawb = GetCusMAWB());

		protected abstract CusMAWBBase GetCusMAWB();

		CusMAWBBaseOutturnReportHeaderInformation HeaderInfo => (CusMAWBBaseOutturnReportHeaderInformation)GetHeaderInfo();
	}
}
