using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.CusDec;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.CusDecCreator.UkCinv.Testing
{
	class UkCinvCreatorTEST : TestCaseWithFactory
	{
		[TestDate(2010, 12, 11)]
		public void TestCreateEAC()
		{
			string expectedAss = @"UNH+<<MSGNO PLACEHOLDER>>+UKCINV:D:00A:UN:109001+<<SYSCAR>>'BGM+EAC:105:109'RFF+ABO:<<UCRREF>>:<<UCRPART>>'RFF+UCN:MUCR'UNS+D'UNS+S'UNT+7+<<MSGNO PLACEHOLDER>>'";
			string expectedDis = @"UNH+<<MSGNO PLACEHOLDER>>+UKCINV:D:00A:UN:109001+<<SYSCAR>>'BGM+EAC:105:109'RFF+ABO:<<UCRREF>>:<<UCRPART>>'UNS+D'UNS+S'UNT+6+<<MSGNO PLACEHOLDER>>'";
			string expectedCls = @"UNH+<<MSGNO PLACEHOLDER>>+UKCINV:D:00A:UN:109001+<<SYSCAR>>'BGM+EAC:105:109'RFF+UCN:MUCR'UNS+D'UNS+S'UNT+6+<<MSGNO PLACEHOLDER>>'";
			ErrorCollector ec = new ErrorCollector();
			CusDecCreatorUkcinv cusDecCreatorUkcinv = new CusDecCreatorUkcinv(dec, entry, new GbDes242MessageFunction.MucrAssociate(), ec);
			string actualAss = cusDecCreatorUkcinv.Create();
			AssertEquals(expectedAss, actualAss);

			cusDecCreatorUkcinv = new CusDecCreatorUkcinv(dec, entry, new GbDes242MessageFunction.MucrDisAssociate(), ec);
			string actualDis = cusDecCreatorUkcinv.Create();
			AssertEquals(expectedDis, actualDis);

			cusDecCreatorUkcinv = new CusDecCreatorUkcinv(dec, entry, new GbDes242MessageFunction.MucrClose(), ec);
			string actualCls = cusDecCreatorUkcinv.Create();
			AssertEquals(expectedCls, actualCls);
		}

		[TestDate(2010, 12, 11)]
		public void TestCreateEAA()
		{
			string expectedEaaDeclaration = @"UNH+<<MSGNO PLACEHOLDER>>+UKCINV:D:00A:UN:109001+<<SYSCAR>>'BGM+EAA:105:109'RFF+ABO:<<UCRREF>>:<<UCRPART>>'LOC+14+LHR:156:109:BAC'TDT+13++1+++++:::HMS DANIEL:GB'UNS+D'UNS+S'UNT+8+<<MSGNO PLACEHOLDER>>'";
			string expectedEaaMaster = @"UNH+<<MSGNO PLACEHOLDER>>+UKCINV:D:00A:UN:109001+<<SYSCAR>>'BGM+EAA:105:109'RFF+ABO:MUCR'LOC+14+LHR:156:109:BAC'TDT+13++1+++++:::HMS DANIEL:GB'UNS+D'UNS+S'UNT+8+<<MSGNO PLACEHOLDER>>'";

			ErrorCollector ec = new ErrorCollector();
			CusDecCreatorUkcinv cusDecCreatorUkcinv = new CusDecCreatorUkcinv(dec, entry, new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration), ec);
			string actualEaaDec = cusDecCreatorUkcinv.Create();
			AssertEquals(expectedEaaDeclaration, actualEaaDec);

			cusDecCreatorUkcinv = new CusDecCreatorUkcinv(dec, entry, new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master), ec);
			string actualEaaMaster = cusDecCreatorUkcinv.Create();
			AssertEquals(expectedEaaMaster, actualEaaMaster);

			dec.JE_MasterUCR = "";
			cusDecCreatorUkcinv = new CusDecCreatorUkcinv(dec, entry, new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master), ec);
			cusDecCreatorUkcinv.Create();
			AssertEquals(1, ec.ErrorCount);  // needs mucr if sending at mucr level
		}

		[TestDate(2010, 12, 11)]
		public void TestCreateEAL()
		{
			string expectedEalDeclaration = @"UNH+<<MSGNO PLACEHOLDER>>+UKCINV:D:00A:UN:109001+<<SYSCAR>>'BGM+EAL:105:109'RFF+ABO:<<UCRREF>>:<<UCRPART>>'LOC+14+LHR:156:109:BAC'DTM+178:197109180000:203'TDT+13++1+++++:::HMS DANIEL:GB'UNS+D'UNS+S'UNT+9+<<MSGNO PLACEHOLDER>>'";
			string expectedEalMaster = @"UNH+<<MSGNO PLACEHOLDER>>+UKCINV:D:00A:UN:109001+<<SYSCAR>>'BGM+EAL:105:109'RFF+ABO:MUCR'LOC+14+LHR:156:109:BAC'DTM+178:197109180000:203'TDT+13++1+++++:::HMS DANIEL:GB'UNS+D'UNS+S'UNT+9+<<MSGNO PLACEHOLDER>>'";
			ErrorCollector ec = new ErrorCollector();
			CusDecCreatorUkcinv cusDecCreatorUkcinv = new CusDecCreatorUkcinv(dec, entry, new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration), ec);
			string actualEaaDec = cusDecCreatorUkcinv.Create();
			AssertEquals(expectedEalDeclaration, actualEaaDec);

			cusDecCreatorUkcinv = new CusDecCreatorUkcinv(dec, entry, new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master), ec);
			string actualEaaMaster = cusDecCreatorUkcinv.Create();
			AssertEquals(expectedEalMaster, actualEaaMaster);

			dec.JE_LocationOfGoods = "";
			cusDecCreatorUkcinv = new CusDecCreatorUkcinv(dec, entry, new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master), ec);
			cusDecCreatorUkcinv.Create();
			AssertEquals(1, ec.ErrorCount);  // needs location of goods
		}

		[TestDate(2010, 12, 11)]
		public void TestCreateEDL()
		{
			string expectedEdlDeclaration = @"UNH+<<MSGNO PLACEHOLDER>>+UKCINV:D:00A:UN:109001+<<SYSCAR>>'BGM+EDL:105:109'RFF+ABO:<<UCRREF>>:<<UCRPART>>'LOC+14+LHR:156:109:BAC'DTM+189:19710919:102'TDT+13++1+++++:::HMS DANIEL:GB'UNS+D'UNS+S'UNT+9+<<MSGNO PLACEHOLDER>>'";
			string expectedEdlMaster = @"UNH+<<MSGNO PLACEHOLDER>>+UKCINV:D:00A:UN:109001+<<SYSCAR>>'BGM+EDL:105:109'RFF+ABO:MUCR'LOC+14+LHR:156:109:BAC'DTM+189:19710919:102'TDT+13++1+++++:::HMS DANIEL:GB'UNS+D'UNS+S'UNT+9+<<MSGNO PLACEHOLDER>>'";
			ErrorCollector ec = new ErrorCollector();
			CusDecCreatorUkcinv cusDecCreatorUkcinv = new CusDecCreatorUkcinv(dec, entry, new GbInventoryManagementMessageFunction.Departure(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration), ec);
			string actualEaaDec = cusDecCreatorUkcinv.Create();
			AssertEquals(expectedEdlDeclaration, actualEaaDec);

			cusDecCreatorUkcinv = new CusDecCreatorUkcinv(dec, entry, new GbInventoryManagementMessageFunction.Departure(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master), ec);
			string actualEaaMaster = cusDecCreatorUkcinv.Create();
			AssertEquals(expectedEdlMaster, actualEaaMaster);
		}

		protected override void SetUp()
		{
			dec = Factory.New<JobDeclaration>();
			dec.JE_MasterUCR = "MUCR";
			entry = dec.CustomsEntryHeaders.AddNew();
			dec.JE_LocationOfGoods = "LHR";
			dec.SubLocation = "BAC";
			dec.JE_TransportMode = "SEA";
			dec.JE_VesselName = "HMS Daniel";
			dec.ZG_LCPInspect = ZDateTime.BrettsBirthday;  // arrival
			dec.ZG_LCPDepart = ZDateTime.BrettsBirthday.AddDays(1); // departure
			dec.JE_RN_NKTransportNationality = "GB";
		}

		JobDeclaration dec;
		CusEntryHeader entry;
	}
}
