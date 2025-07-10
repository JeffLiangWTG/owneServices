using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirOutturnStandAloneReportHeaderTest : TestCaseWithFactory
	{
		public void TestFlightNumber()
		{
			Underbond.C4_FlightNo = "QF123";
			AssertEquals("QF123", ((IAirOutturnReportHeaderInformation)Header).FlightNumber);
		}

		public void TestOutturnResponsibleParty()
		{
			ZString localBusinessNumber = "21003980130";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = localBusinessNumber;
			AssertEquals(localBusinessNumber, ((IAirOutturnReportHeaderInformation)Header).ResponsiblePartyID);

			ZString localBusinessNumberOverride = "67094168242";
			Underbond.C4_DestinationPremiseID = "AA33N";
			CodeDescriptionPairList codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair("AA33N", "67094168242");
			FreightDataRegistry.Instance.OuturnResponsiblePartyIDOverride.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, codeDescriptionPairList);

			AssertEquals(localBusinessNumberOverride, ((IAirOutturnReportHeaderInformation)Header).ResponsiblePartyID);
		}

		public void TestMatchingDateTimeOfStandAloneOutturnDoesntCauseMessageRejection()
		{
			ZDateTime dateTimeOfArrival = new ZDateTime(2006, 9, 18);
			ZDateTime dateTimeOfOutturn = new ZDateTime(2006, 9, 18, 11, 11, 00);

			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			mAWB1.CM_MAWB = "M1";
			mAWB1.CM_FlightNo = "QF111";
			mAWB1.CM_ArrivalDate = dateTimeOfArrival;

			CusUnderbond underbond1 = mAWB1.Underbonds.AddNew();
			underbond1.C4_ParentID = mAWB1.PK;
			underbond1.C4_SendersMessageReference = "U00000001";
			underbond1.C4_Outurned = dateTimeOfOutturn;

			CusOutturn outturn1 = underbond1.Outturns.AddNew();
			outturn1.C5_MasterBill = mAWB1.CM_MAWB;
			outturn1.C5_PackagesOutturned = 5;
			outturn1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			Factory.Save();

			AirOutturnStandAloneReportHeader header1 = new AirOutturnStandAloneReportHeader(underbond1);
			AssertEquals(dateTimeOfOutturn.AddHours(-10), ((IAirOutturnReportHeaderInformation)header1).DateTimeOfOutturn);

			CusUnderbond underbond2 = Factory.New<CusUnderbond>();
			underbond2.C4_SendersMessageReference = "U00000002";
			underbond2.C4_Outurned = dateTimeOfOutturn;
			underbond2.C4_MAWB = "M2";
			underbond2.C4_FlightNo = "QF111";
			underbond2.C4_ArrivalDate = dateTimeOfArrival;

			CusOutturn outturn2 = underbond2.Outturns.AddNew();
			outturn2.C5_HouseBill = "M2";
			outturn2.C5_PackagesOutturned = 4;
			outturn2.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			AirOutturnStandAloneReportHeader header2 = new AirOutturnStandAloneReportHeader(underbond2);
			AssertEquals(dateTimeOfOutturn.AddHours(-10).AddMinutes(1), ((IAirOutturnReportHeaderInformation)header2).DateTimeOfOutturn);
			AssertEquals(dateTimeOfOutturn.AddMinutes(1), underbond2.C4_Outurned);

			CusUnderbond underbond3 = Factory.New<CusUnderbond>();
			underbond3.C4_MAWB = "M3";
			underbond3.C4_FlightNo = "QF222";
			underbond3.C4_ArrivalDate = new ZDateTime(2006, 9, 19);
			underbond3.C4_SendersMessageReference = "U00000003";
			underbond3.C4_Outurned = dateTimeOfOutturn;

			CusOutturn outturn3 = underbond3.Outturns.AddNew();
			outturn3.C5_HouseBill = "M3";
			outturn3.C5_PackagesOutturned = 3;
			outturn3.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			AirOutturnStandAloneReportHeader header3 = new AirOutturnStandAloneReportHeader(underbond3);
			AssertEquals(dateTimeOfOutturn.AddHours(-10), ((IAirOutturnReportHeaderInformation)header3).DateTimeOfOutturn);
			AssertEquals(dateTimeOfOutturn, underbond3.C4_Outurned);
		}

		public void TestMatchingDateTimeOfStandAloneOutturnDoesntCauseMessageRejectionWithExistingStandAloneOutturn()
		{
			ZDateTime dateTimeOfArrival = new ZDateTime(2006, 9, 18);
			ZDateTime dateTimeOfOutturn = new ZDateTime(2006, 9, 18, 11, 11, 00);

			CusUnderbond underbond1 = Factory.New<CusUnderbond>();
			underbond1.C4_MAWB = "M1";
			underbond1.C4_FlightNo = "QF111";
			underbond1.C4_ArrivalDate = dateTimeOfArrival;
			underbond1.C4_SendersMessageReference = "U00000001";
			underbond1.C4_Outurned = dateTimeOfOutturn;

			CusOutturn outturn1 = underbond1.Outturns.AddNew();
			outturn1.C5_MasterBill = "M1";
			outturn1.C5_PackagesOutturned = 5;
			outturn1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			Factory.Save();

			AirOutturnStandAloneReportHeader header1 = new AirOutturnStandAloneReportHeader(underbond1);
			AssertEquals(dateTimeOfOutturn.AddHours(-10), ((IAirOutturnReportHeaderInformation)header1).DateTimeOfOutturn);

			CusUnderbond underbond2 = Factory.New<CusUnderbond>();
			underbond2.C4_SendersMessageReference = "U00000002";
			underbond2.C4_Outurned = dateTimeOfOutturn;
			underbond2.C4_MAWB = "M2";
			underbond2.C4_FlightNo = "QF111";
			underbond2.C4_ArrivalDate = dateTimeOfArrival;

			CusOutturn outturn2 = underbond2.Outturns.AddNew();
			outturn2.C5_HouseBill = "M2";
			outturn2.C5_PackagesOutturned = 4;
			outturn2.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			AirOutturnStandAloneReportHeader header2 = new AirOutturnStandAloneReportHeader(underbond2);
			AssertEquals(dateTimeOfOutturn.AddHours(-10).AddMinutes(1), ((IAirOutturnReportHeaderInformation)header2).DateTimeOfOutturn);
			AssertEquals(dateTimeOfOutturn.AddMinutes(1), underbond2.C4_Outurned);

			CusUnderbond underbond3 = Factory.New<CusUnderbond>();
			underbond3.C4_MAWB = "M3";
			underbond3.C4_FlightNo = "QF222";
			underbond3.C4_ArrivalDate = new ZDateTime(2006, 9, 19);
			underbond3.C4_SendersMessageReference = "U00000003";
			underbond3.C4_Outurned = dateTimeOfOutturn;

			CusOutturn outturn3 = underbond3.Outturns.AddNew();
			outturn3.C5_HouseBill = "M3";
			outturn3.C5_PackagesOutturned = 3;
			outturn3.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			AirOutturnStandAloneReportHeader header3 = new AirOutturnStandAloneReportHeader(underbond3);
			AssertEquals(dateTimeOfOutturn.AddHours(-10), ((IAirOutturnReportHeaderInformation)header3).DateTimeOfOutturn);
			AssertEquals(dateTimeOfOutturn, underbond3.C4_Outurned);
		}

		public void TestDateTimeOfOutturn()
		{
			ZDateTime date = new ZDateTime(2004, 5, 12);
			Underbond.C4_Outurned = date;
			AssertEquals(date.AddHours(-10), ((IAirOutturnReportHeaderInformation)Header).DateTimeOfOutturn);
		}

		public void TestUnderbond_IsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new AirOutturnStandAloneReportHeader(null));
		}

		public void TestArrivalDate()
		{
			ZDateTime date = new ZDateTime(2006, 5, 12);
			Underbond.C4_ArrivalDate = date;
			AssertEquals(date, ((IAirOutturnReportHeaderInformation)Header).EstimatedDateOfArrival);
		}

		public void TestEstablishmentID()
		{
			Underbond.C4_DestinationPremiseID = "Gibbiceps";
			AssertEquals("Gibbiceps", ((IAirOutturnReportHeaderInformation)Header).EstablishmentID);
		}

		public void TestLines()
		{
			Underbond.Outturns.AddNew();
			AssertNotNull(((IAirOutturnReportHeaderInformation)Header).Lines);
		}

		AirOutturnStandAloneReportHeader header;
		AirOutturnStandAloneReportHeader Header => header ?? (header = new AirOutturnStandAloneReportHeader(Underbond));

		CusUnderbond underbond;
		CusUnderbond Underbond => underbond ?? (underbond = Factory.New<CusUnderbond>());
	}
}
