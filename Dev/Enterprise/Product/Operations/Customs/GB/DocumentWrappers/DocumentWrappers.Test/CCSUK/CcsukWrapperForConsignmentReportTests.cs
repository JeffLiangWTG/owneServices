using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk.Testing
{
	public class CcsukWrapperForConsignmentReportTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConstructor_Mawb()
		{
			var mawb = Factory.New<CusMAWB>();
			var wrapper = CcsukWrapperForConsignmentReport.New(mawb, Factory);
			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();
			wrapper = CcsukWrapperForConsignmentReport.New(mawb, Factory);
		}

		[ExpectNoExceptions]
		public void TestConstructor_Hawb()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();
			var wrapper = CcsukWrapperForConsignmentReport.New(hawb1, Factory);

			var split1 = hawb1.Splits.AddNew();
			var split2 = hawb1.Splits.AddNew();
			wrapper = CcsukWrapperForConsignmentReport.New(hawb1, Factory);
		}

		[ExpectNoExceptions]
		public void TestConstructor_Basic()
		{
			var basic = Factory.New<CusMAWB>();
			var wrapper = CcsukWrapperForConsignmentReport.New(basic, Factory);
			var split1 = basic.Splits.AddNew();
			var split2 = basic.Splits.AddNew();
			wrapper = CcsukWrapperForConsignmentReport.New(basic, Factory);
		}

		[ExpectNoExceptions]
		public void TestConstructor_SplitHawb()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();
			var split1 = hawb1.Splits.AddNew();
			var split2 = hawb1.Splits.AddNew();
			var wrapper = CcsukWrapperForConsignmentReport.New(split2, Factory);
		}

		[ExpectNoExceptions]
		public void TestConstructor_SplitBasic()
		{
			var basic = Factory.New<CusMAWB>();
			var split1 = basic.Splits.AddNew();
			var split2 = basic.Splits.AddNew();
			var wrapper = CcsukWrapperForConsignmentReport.New(split2, Factory);
		}

		public void TestHousesCollectionAndIsConsol()
		{
			var mawb = Factory.New<CusMAWB>();
			var wrapper = CcsukWrapperForConsignmentReport.New(mawb, Factory);
			AssertEquals(false, wrapper.ISCONSOL);
			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();
			AssertEquals(true, wrapper.ISCONSOL);
			AssertEquals(2, wrapper.Houses.Count);
		}

		public void TestSplitsCollectionAndHasSplitsBasic()
		{
			var basic = Factory.New<CusMAWB>();
			var wrapper = CcsukWrapperForConsignmentReport.New(basic, Factory);
			AssertEquals(false, wrapper.HASSPLITS);
			var split1 = basic.Splits.AddNew();
			var split2 = basic.Splits.AddNew();
			AssertEquals(true, wrapper.HASSPLITS);
			AssertEquals(2, wrapper.Splits.Count);
		}

		public void TestSplitsCollectionAndHasSplitsHawb()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();
			var wrapper = CcsukWrapperForConsignmentReport.New(hawb1, Factory);
			AssertEquals(false, wrapper.HASSPLITS);
			var split1 = hawb1.Splits.AddNew();
			var split2 = hawb1.Splits.AddNew();
			AssertEquals(true, wrapper.HASSPLITS);
			AssertEquals(2, wrapper.Splits.Count);
			wrapper = CcsukWrapperForConsignmentReport.New(hawb2, Factory);
			AssertEquals(false, wrapper.HASSPLITS);
			AssertEquals(0, wrapper.Splits.Count);
		}

		[TestDate(1986, 3, 12, 4, 1, 2)]
		public void TestAllProperties()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "12387654321";
			basic.Profile = "CUKFFW98000XXX"; // to allow creation of declaration
			var w = CcsukWrapperForConsignmentReport.New(basic, Factory);
			AssertEquals("Prioxied from basic class", "123-87654321", w.MAWBHAWBSPLIT);
			AssertEquals("", w.STATUSONEDATE);
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 10;
			basic.Status1Date = new ZDateTime(1986, 3, 12, 4, 1, 2);
			AssertEquals("12-Mar-1986 04:01", w.STATUSONEDATE);
			AssertEquals(" ", w.FLIGHTANDDATE);
			basic.CM_FlightNo = "BA001";
			AssertEquals("BA001 ", w.FLIGHTANDDATE);
			basic.CM_ArrivalDate = ZDateTime.BrettsBirthday;
			AssertEquals("BA001 18-Sep-1971", w.FLIGHTANDDATE);
			AssertEquals(false, w.HASDECLARATIONUCR);
			var dec = ((ICcsukCusAwb)basic).CreateNewStandaloneCDSDeclaration();
			dec.JE_UCR = "Poopy";
			AssertEquals(true, w.HASDECLARATIONUCR);
			AssertEquals(dec.PK, w.Declaration.PK);
			basic.Messages.AddNew().EM_SystemCreateUser = "~BP";
			AssertEquals("", w.USER);
			AssertEquals("Yes", w.STATUS2);
			AssertEquals("12/03/86 04:01", w.CREATEDDETAILS);
			AssertEquals("12/03/86 04:01", w.EDITDETAILS);
			AssertEquals(false, w.HASOUTTURNS);
			basic.OutTurns.AddNew();
			AssertEquals(true, w.HASOUTTURNS);
		}

		public void TestFreightReference()
		{
			var consol = Factory.New<ForwardingConsol>();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			consol.JK_UniqueConsignRef = "C-poop";
			var mawbWrapper = CcsukWrapperForConsignmentReport.New(mawb, Factory);
			AssertEquals("C-poop", mawbWrapper.FREIGHTREFERENCE);
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S-poop";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			var hawbWrapper = CcsukWrapperForConsignmentReport.New(hawb, Factory);
			AssertEquals("S-poop", hawbWrapper.FREIGHTREFERENCE);
			var splitHawb = hawb.Splits.AddNew();
			var splitHawbWrapper = CcsukWrapperForConsignmentReport.New(splitHawb, Factory);
			AssertEquals("S-poop", splitHawbWrapper.FREIGHTREFERENCE);
			mawb.ChildBills.RemoveAndDeleteAll();
			var splitBasic = mawb.Splits.AddNew();
			var splitBasicWrapper = CcsukWrapperForConsignmentReport.New(splitBasic, Factory);
			AssertEquals("C-poop", splitBasicWrapper.FREIGHTREFERENCE);
		}

		public void TestFindJobDecOnSplitByMucr()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Facilities, "Test Facility Code");
			var shed = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.Facilities, "LHRBAC", "Test Shed", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			shed.Attributes.Add(helper.CreateNewOrGetExistingCusCodeListAttribute(shed.PK, EU.Business.UniversalReferenceConstants.ShedAttributes.ACPCode, "H"));

			Factory.Save();

			var basic = Factory.New<CusMAWB>();

			basic.CM_MAWB = "11122222222";
			basic.CargoTerminalOperatorAirportAndShed = shed.ZZD_Code;

			var split1 = basic.Splits.AddNew();
			var split2 = basic.Splits.AddNew();
			var split3 = basic.Splits.AddNew();

			split2.SplitReference = "02";

			var wrapper = CcsukWrapperForConsignmentReport.New(split2, Factory);

			var jobDec = Factory.New<JobDeclaration>();
			jobDec.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			jobDec.JE_MasterUCR = "HBAC11122222222        02";
			jobDec.MasterUCR.CE_IssueDate = ZDateTime.Today.AddYears(-1);

			Factory.Save();

			var foundJobDec = wrapper.Declaration;

			AssertEquals("Should be finding the created JobDec", jobDec, foundJobDec);

			var jobDecNewer = Factory.New<JobDeclaration>();
			jobDecNewer.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			jobDecNewer.JE_MasterUCR = "HBAC11122222222        02";
			jobDecNewer.MasterUCR.CE_IssueDate = ZDateTime.Today;

			Factory.Save();

			foundJobDec = wrapper.Declaration;

			AssertEquals("Should be finding the newer JobDec", jobDecNewer, foundJobDec);
		}
	}

	[TestedType(typeof(CcsukWrapperForConsignmentReportHouseLineCollection))]
	public class CcsukWrapperForConsignmentReportHouseLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CcsukWrapperForConsignmentReportHouseLineCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(CcsukWrapperForConsignmentReportHouseLineCollection);
		}

		protected override CcsukWrapperForConsignmentReportHouseLineCollection GetCollectionToTest()
		{
			return new CcsukWrapperForConsignmentReportHouseLineCollection(Mawb, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return CcsukWrapperForConsignmentReportHouseLine.New(Mawb.ChildBills.AddNew(), Factory);
		}

		CusMAWB mawb;
		CusMAWB Mawb
		{
			get { return mawb ?? (mawb = Factory.New<CusMAWB>()); }
		}
	}

	[TestedType(typeof(CcsukWrapperForConsignmentReportSplitLineCollection))]
	public class CcsukWrapperForConsignmentReportSplitLineCollectionTest_House : NonPersistentBusinessObjectCollectionTestCase<CcsukWrapperForConsignmentReportSplitLineCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(CcsukWrapperForConsignmentReportSplitLineCollection);
		}

		protected override CcsukWrapperForConsignmentReportSplitLineCollection GetCollectionToTest()
		{
			return new CcsukWrapperForConsignmentReportSplitLineCollection(Hawb, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var split = Hawb.Splits.AddNew();
			split.SplitReference = "01";
			return CcsukWrapperForConsignmentReportSplitLine.New(split, Factory);
		}

		CusMAWB Mawb
		{
			get { return mawb ?? (mawb = Factory.New<CusMAWB>()); }
		}

		CusHAWB Hawb
		{
			get { return hawb ?? (hawb = Mawb.ChildBills.AddNew()); }
		}

		CusMAWB mawb;
		CusHAWB hawb;
	}

	[TestedType(typeof(CcsukWrapperForConsignmentReportSplitLineCollection))]
	public class CcsukWrapperForConsignmentReportSplitLineCollectionTest_Basic : NonPersistentBusinessObjectCollectionTestCase<CcsukWrapperForConsignmentReportSplitLineCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(CcsukWrapperForConsignmentReportSplitLineCollection);
		}

		protected override CcsukWrapperForConsignmentReportSplitLineCollection GetCollectionToTest()
		{
			return new CcsukWrapperForConsignmentReportSplitLineCollection(Basic, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var split = Basic.Splits.AddNew();
			split.SplitReference = "01";
			return CcsukWrapperForConsignmentReportSplitLine.New(split, Factory);
		}

		CusMAWB Basic
		{
			get { return basic ?? (basic = Factory.New<CusMAWB>()); }
		}

		CusMAWB basic;
	}
}
