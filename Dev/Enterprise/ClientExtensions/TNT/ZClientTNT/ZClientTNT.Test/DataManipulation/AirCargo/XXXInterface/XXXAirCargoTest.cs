using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.TNT.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(XXXAirCargo))]
	public class XXXAirCargoTest : ImportAirCargoTestCase
	{
		#region TestConstructor
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor_WithException()
		{
			XXXAirCargo airCargo = new XXXAirCargo(Factory, FlightRec, null, SydBranch.GB_Code);
		}

		[TestDate(2005, 8, 23)]
		public void TestConstructor()
		{
			OrgProxyPortMappingTestHelper portMappingHelper = new OrgProxyPortMappingTestHelper(Factory);
			portMappingHelper.OrgProxy.PatternMatchOverrides_ForBinding.RemoveAndDeleteAll();
			portMappingHelper.AddPortMappingToOrgProxy("WHR", portMappingHelper.AUSYDUnLoco);
			portMappingHelper.AddPortMappingToOrgProxy("SIN", portMappingHelper.SGSINUnLoco);
			Factory.Save();
			AssertNotNull(AirCargo);
			AssertEquals("AirCargo.FlightDetail", FlightRec, AirCargo.FlightDetail);
			AssertEquals("AirCargo.Consignment", Consignment, AirCargo.Consignment);
			AssertEqualsIgnoreTrailingSpaces("Masterbill", FlightRec.MasterBill, AirCargo.MasterBill);
			AssertEqualsIgnoreTrailingSpaces("FlightNo", FlightRec.FlightNumber, AirCargo.FlightNo);
			AssertEquals("ArrivalDate", FlightRec.FlightDate, AirCargo.ArrivalDate);
			AssertEqualsIgnoreTrailingSpaces("PortOfLoading", portMappingHelper.SGSINUnLoco.Code, AirCargo.PortOfLoading);
			AssertEqualsIgnoreTrailingSpaces("PortOfDischarge", portMappingHelper.AUSYDUnLoco.Code, AirCargo.PortOfDischarge);
			AssertEqualsIgnoreTrailingSpaces("SearchMasterbill", AirCargo.MasterBill, AirCargo.SearchMasterBill);
			AssertEqualsIgnoreTrailingSpaces("SearchFlightNo", AirCargo.FlightNo, AirCargo.SearchFlightNo);
			AssertEquals("SearchArrivalDate", AirCargo.ArrivalDate, AirCargo.SearchArrivalDate);
			AssertEqualsIgnoreTrailingSpaces("SearchPortOfLoading", AirCargo.PortOfLoading, AirCargo.SearchPortOfLoading);
			AssertEqualsIgnoreTrailingSpaces("SearchPortOfDischarge", AirCargo.PortOfDischarge, AirCargo.SearchPortOfDischarge);
			AssertEquals("MatchedMasterBill", "", AirCargo.MatchedMasterBill);
			AssertEqualsIgnoreTrailingSpaces("HouseBill", Consignment.HouseBill, AirCargo.HouseBill);
			AssertEqualsIgnoreTrailingSpaces("Origin", Consignment.Origin, AirCargo.Origin);
			AssertEqualsIgnoreTrailingSpaces("Destination", Consignment.Destination, AirCargo.Destination);
			AssertEqualsIgnoreTrailingSpaces("Consignee", Consignment.ConsigneeName, AirCargo.Consignee);
			AssertEqualsIgnoreTrailingSpaces("Consignor", Consignment.ConsignorName, AirCargo.Consignor);
			AssertEquals("Package Count", Consignment.PackageCount, AirCargo.PackageCount);
			AssertEquals("AirCargo.ConsignmentNotes.Count is zero", 0, AirCargo.ConsignmentNotes.Count);
			AssertEquals("LinkedHouseBillRef", "", AirCargo.LinkedHouseBillRef);
			AssertNull("LinkedHouseBill", AirCargo.LinkedHouseBill);
			FlightRec.FlightDate = ZDateTime.Empty;
			FlightRec.PortOfLoading = "";
			FlightRec.PortOfDischarge = "";
			Consignment.Origin = "NZAKL";
			Consignment.Destination = "AUMEL";
			XXXAirCargo newAirCargo = new XXXAirCargo(Factory, FlightRec, Consignment, SydBranch.GB_Code);
			AssertNotNull(newAirCargo);
			AssertEquals("SearchArrivalDate", ZDateTime.Now, newAirCargo.SearchArrivalDate);
			AssertEqualsIgnoreTrailingSpaces("SearchPortOfLoading", Consignment.Origin, newAirCargo.SearchPortOfLoading);
			AssertEqualsIgnoreTrailingSpaces("SearchPortOfDischarge", Consignment.Destination, newAirCargo.SearchPortOfDischarge);
		}

		#endregion
		public void TestAddConsignmentNotes()
		{
			AssertEquals("PreCondition: AirCargo.ConsignmentNotes.Count is zero", 0, AirCargo.ConsignmentNotes.Count);
			AirCargo.AddConsignmentNotes(ConsignmentNote);
			AssertEquals("AirCargo.AddConsignmentNotes.Count", 1, AirCargo.ConsignmentNotes.Count);
		}

		#region TestPopulateData
		public void TestPopulateData()
		{
			Consignment.ConsigneeCountry = "NZ";
			AirCargo.AddConsignmentNotes(ConsignmentNote);
			CusMAWB masterBill = CreateDummyCusMAWB("MASTERBILL", "QF1234", new ZDateTime(2005, 8, 30), "NZAKL", "AUSYD");
			AssertEquals("PreConditon: MasterBill should have no housebills", 0, masterBill.ChildBills.Count);
			AirCargo.PopulateData(masterBill);
			AssertEquals("MasterBill should have 1 new housebill", 1, masterBill.ChildBills.Count);
			CusHAWB houseBill = masterBill.ChildBills[0];
			AssertEqualsIgnoreTrailingSpaces("HouseBill No", Consignment.HouseBill, houseBill.CS_HAWB);
			AssertEqualsIgnoreTrailingSpaces("Origin", Consignment.Origin, houseBill.CS_RL_NKOrigin);
			AssertEqualsIgnoreTrailingSpaces("Destination", Consignment.Destination, houseBill.CS_RL_NKDestination);
			AssertEquals("Weight", Consignment.Weight, houseBill.CS_Weight);
			AssertEquals("Goods Value", Consignment.GoodsValue, houseBill.CS_GoodsValue);
			AssertEqualsIgnoreTrailingSpaces("Currency", Consignment.GoodsCurrency, houseBill.CS_RX_NKGoodsCurrency);
			AssertEquals("Manifested PackageCount", Consignment.PackageCount, houseBill.CS_PiecesManifested);
			StmNote[] sectorNotes = houseBill.Notes.FindByDescription(ConsignmentUpdator.OriginalQuantumSectorNoteDescription);
			AssertEquals("Housebill should have 1 Original Sector Note", 1, sectorNotes.Length);
			AssertHousebillIsMarkedAsSurplusConsignment(houseBill);
		}

		void AssertHousebillIsMarkedAsSurplusConsignment(CusHAWB housebill)
		{
			AssertNotNull("Housebill should not be null", housebill);
			AssertEquals("Housebill shoudl have 1 new Underbond created", 1, housebill.AllUnderbonds.Count);
			CusUnderbond underbond = housebill.AllUnderbonds[0];
			AssertNotNull("Underbond should not be null", underbond);
			AssertEquals("Underbond's Parent ID should be the Housebill PK", housebill.PK, underbond.C4_ParentID);
			AssertEquals("Underbond should be an Air Movement", CMRUnderbondModeOfMovement.Codes.Air, underbond.C4_ModeOfMovement);
			AssertEquals("There should be 1 Outturn for this Underbond", 1, underbond.Outturns.Count);
			CusOutturn outTurn = underbond.Outturns[0];
			AssertNotNull("OutTurn should not be null", outTurn);
			AssertEquals("OutTurn Landed Pieces should be Housebill's landed pieces", housebill.CS_PiecesLanded, outTurn.C5_PackagesOutturned);
			AssertEquals("OutTurn's Parent should be Housebill", ((IOutturnableLine)housebill).LinkPK, outTurn.Parent.LinkPK);
			AssertEquals("OutTurn's type should be Surplus Consignment", CMROutturnResultType.Codes.SurplusConsignment, outTurn.C5_OutturnResultType);
		}

		public void TestPopulateData_HouseBillAlreadyExist()
		{
			Consignment.ConsigneeCountry = "NZ";
			AirCargo.AddConsignmentNotes(ConsignmentNote);
			UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
			AssertNotNull("UserNotification should not be null", userNotification);
			CusMAWB masterBill = CreateDummyCusMAWB("MASTERBILL", "QF1234", new ZDateTime(2005, 8, 30), "NZAKL", "AUSYD");
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_HAWB = Consignment.HouseBill;
			Factory.Save();
			AssertEquals("PreCondition: MasterBill should have only 1 housebill", 1, masterBill.ChildBills.Count);
			userNotification.ClearMessagesAndAnswers();
			AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
			string expectedMessage = string.Format("Housebill '{0}' already exist.{1}It will be ignored", Consignment.HouseBill, System.Environment.NewLine);
			AirCargo.PopulateData(masterBill);
			AssertEquals("MasterBill should still have only 1 housebill", 1, masterBill.ChildBills.Count);
			AssertEquals("MasterBill shoudl have not changes", false, masterBill.HasChanges);
			AssertEquals("HouseBill shoudl have not changes", false, houseBill.HasChanges);
			var lastMessage = userNotification.LastMessage.Text ?? string.Empty;
			AssertEquals("Expected 1 new error message", 2, userNotification.PreviousMessages.Length);
			AssertEquals("Last Error Message:" + System.Environment.NewLine + lastMessage, true, lastMessage.IndexOf(expectedMessage) >= 0);
		}

		#endregion
		public void TestClearSearchData()
		{
			AssertNotNull(AirCargo);
			AirCargo.SearchMasterBill = "MasterBill";
			AirCargo.SearchFlightNo = "FlightNo";
			AirCargo.SearchArrivalDate = ZDateTime.Now;
			AirCargo.SearchPortOfLoading = "Load";
			AirCargo.SearchPortOfDischarge = "Disc";
			AirCargo.ClearSearchData();
			AssertEqualsIgnoreTrailingSpaces("SearchMasterbill", "", AirCargo.SearchMasterBill);
			AssertEqualsIgnoreTrailingSpaces("SearchFlightNo", "", AirCargo.SearchFlightNo);
			AssertEquals("SearchArrivalDate", ZDateTime.Empty, AirCargo.SearchArrivalDate);
			AssertEqualsIgnoreTrailingSpaces("SearchPortOfLoading", "", AirCargo.SearchPortOfLoading);
			AssertEqualsIgnoreTrailingSpaces("SearchPortOfDischarge", "", AirCargo.SearchPortOfDischarge);
		}

		public void TestSave()
		{
			AirCargo.AddConsignmentNotes(ConsignmentNote);
			CusMAWB masterBill = CreateDummyCusMAWB("MASTERBILL", "QF1234", new ZDateTime(2005, 8, 30), "NZAKL", "AUSYD");
			AssertNull("PreCondition: AirCargo.LinkedHouseBill", AirCargo.LinkedHouseBill);
			AssertEquals("PreCondition: MasterBill should have no housebill", 0, masterBill.ChildBills.Count);
			AirCargo.PopulateData(masterBill);
			AssertEquals("MasterBill should still have 1 housebill", 1, masterBill.ChildBills.Count);
			AssertNull("PreCondition: AirCargo.LinkedHouseBill", AirCargo.LinkedHouseBill);
			AirCargo.Save();
			AssertNotNull("AirCargo.LinkedHouseBill", AirCargo.LinkedHouseBill);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new XXXAirCargo(Factory, new FlightRecord(FlightDetailLine), new ConsignmentRecord(ConsignmentLine), SydBranch.GB_Code);
		}

		#region AirCargo
		XXXAirCargo AirCargo
		{
			get
			{
				if (fAirCargo == null)
				{
					fAirCargo = new XXXAirCargo(Factory, FlightRec, Consignment, SydBranch.GB_Code);
				}

				return fAirCargo;
			}
		}

		XXXAirCargo fAirCargo;
		#endregion
		#endregion
	}
}
