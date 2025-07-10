using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.TNT.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(IQDownAirCargo))]
	public class IQDownAirCargoTest : ImportAirCargoTestCase
	{
		public void TestConstructor()
		{
			OrgProxyPortMappingTestHelper portMappingHelper = new OrgProxyPortMappingTestHelper(Factory);
			portMappingHelper.OrgProxy.PatternMatchOverrides_ForBinding.RemoveAndDeleteAll();
			portMappingHelper.AddPortMappingToOrgProxy("WHR", portMappingHelper.AUSYDUnLoco);
			portMappingHelper.AddPortMappingToOrgProxy("SIN", portMappingHelper.SGSINUnLoco);
			Factory.Save();
			AssertNotNull(AirCargo);
			AssertEquals("AirCargo.FlightDetail", FlightRec, AirCargo.FlightDetail);
			AssertEqualsIgnoreTrailingSpaces("MasterBill", FlightRec.MasterBill, AirCargo.MasterBill);
			AssertEqualsIgnoreTrailingSpaces("FlightNo", FlightRec.FlightNumber, AirCargo.FlightNo);
			AssertEquals("ArrivalDate", FlightRec.FlightDate, AirCargo.ArrivalDate);
			AssertEqualsIgnoreTrailingSpaces("PortOfLoading", portMappingHelper.SGSINUnLoco.Code, AirCargo.PortOfLoading);
			AssertEqualsIgnoreTrailingSpaces("PortOfDischarge", portMappingHelper.AUSYDUnLoco.Code, AirCargo.PortOfDischarge);
			AssertEqualsIgnoreTrailingSpaces("SearchMasterbill", AirCargo.MasterBill, AirCargo.SearchMasterBill);
			AssertEqualsIgnoreTrailingSpaces("SearchFlightNo", AirCargo.FlightNo, AirCargo.SearchFlightNo);
			AssertEquals("SearchArrivalDate", AirCargo.ArrivalDate, AirCargo.SearchArrivalDate);
			AssertEqualsIgnoreTrailingSpaces("SearchPortOfLoading", AirCargo.PortOfLoading, AirCargo.SearchPortOfLoading);
			AssertEqualsIgnoreTrailingSpaces("SearchPortOfDischarge", AirCargo.PortOfDischarge, AirCargo.SearchPortOfDischarge);
			AssertEquals("AirCargo.MatchingAirCargos.Count is zero", 0, AirCargo.MatchingAirCargos.Count);
			AssertEquals("Branch", SydBranch.PK, AirCargo.Branch.PK);
		}

		#region TestLoadSearchData
		public void TestLoadSearchData()
		{
			TestCaseHelper.ClearTable(CusMAWBSchema.Constants.TableName);
			CusMAWB mawb1 = CreateDummyCusMAWB("MAWB1", "QF001", new ZDateTime(2005, 08, 29), "SGSIN", "AUSYD");
			CusMAWB mawb2 = CreateDummyCusMAWB("MAWB2", "QF002", new ZDateTime(2005, 08, 28), "SGSIN", "AUSYD");
			CusMAWB mawb3 = CreateDummyCusMAWB("MAWB3", "QF002", new ZDateTime(2005, 08, 29), "NZAKL", "AUSYD");
			CusMAWB mawb4 = CreateDummyCusMAWB("MAWB4", "QF001", new ZDateTime(2005, 08, 28), "USLAX", "AUMEL");
			CusMAWB mawb5 = CreateDummyCusMAWB("MAWB5", "QF001", new ZDateTime(2005, 08, 29), "SGSIN", "AUSYD");
			Factory.Save();
			AssertNotNull(AirCargo);
			AirCargo.SearchMasterBill = "";
			AirCargo.SearchFlightNo = "";
			AirCargo.SearchArrivalDate = ZDateTime.Empty;
			AirCargo.SearchPortOfLoading = "";
			AirCargo.SearchPortOfDischarge = "";
			AirCargo.LoadSearchData();
			AssertEquals("AirCargo.MatchingAirCargos.Count", 5, AirCargo.MatchingAirCargos.Count);
			AirCargo.SearchPortOfDischarge = "AUSYD";
			AirCargo.LoadSearchData();
			AssertEquals("AirCargo.MatchingAirCargos.Count", 4, AirCargo.MatchingAirCargos.Count);
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb1", true, AirCargo.MatchingAirCargos.Contains(mawb1.PK));
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb2", true, AirCargo.MatchingAirCargos.Contains(mawb2.PK));
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb3", true, AirCargo.MatchingAirCargos.Contains(mawb3.PK));
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb5", true, AirCargo.MatchingAirCargos.Contains(mawb5.PK));
			AirCargo.SearchPortOfLoading = "SGSIN";
			AirCargo.LoadSearchData();
			AssertEquals("AirCargo.MatchingAirCargos.Count", 3, AirCargo.MatchingAirCargos.Count);
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb1", true, AirCargo.MatchingAirCargos.Contains(mawb1.PK));
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb2", true, AirCargo.MatchingAirCargos.Contains(mawb2.PK));
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb5", true, AirCargo.MatchingAirCargos.Contains(mawb5.PK));
			AirCargo.SearchArrivalDate = new ZDateTime(2005, 08, 29);
			AirCargo.LoadSearchData();
			AssertEquals("AirCargo.MatchingAirCargos.Count", 2, AirCargo.MatchingAirCargos.Count);
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb1", true, AirCargo.MatchingAirCargos.Contains(mawb1.PK));
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb5", true, AirCargo.MatchingAirCargos.Contains(mawb5.PK));
			AirCargo.SearchPortOfLoading = "";
			AirCargo.LoadSearchData();
			AssertEquals("AirCargo.MatchingAirCargos.Count", 3, AirCargo.MatchingAirCargos.Count);
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb1", true, AirCargo.MatchingAirCargos.Contains(mawb1.PK));
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb3", true, AirCargo.MatchingAirCargos.Contains(mawb3.PK));
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb5", true, AirCargo.MatchingAirCargos.Contains(mawb5.PK));
			AirCargo.SearchFlightNo = "QF001";
			AirCargo.LoadSearchData();
			AssertEquals("AirCargo.MatchingAirCargos.Count", 2, AirCargo.MatchingAirCargos.Count);
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb1", true, AirCargo.MatchingAirCargos.Contains(mawb1.PK));
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb5", true, AirCargo.MatchingAirCargos.Contains(mawb5.PK));
			AirCargo.SearchMasterBill = "MAWB1";
			AirCargo.LoadSearchData();
			AssertEquals("AirCargo.MatchingAirCargos.Count", 1, AirCargo.MatchingAirCargos.Count);
			AssertEquals("AirCargo.MatchingAirCargos should containt Mawb1", true, AirCargo.MatchingAirCargos.Contains(mawb1.PK));
		}

		[ExpectNoExceptions]
		public void TestInvalidSearchArrivalDate()
		{
			TestCaseHelper.ClearTable(CusMAWBSchema.Constants.TableName);
			CusMAWB mawb1 = CreateDummyCusMAWB("MAWB1", "QF001", new ZDateTime(2005, 08, 29), "SGSIN", "AUSYD");
			CusMAWB mawb2 = CreateDummyCusMAWB("MAWB2", "QF002", new ZDateTime(2005, 08, 28), "SGSIN", "AUSYD");
			CusMAWB mawb3 = CreateDummyCusMAWB("MAWB3", "QF002", new ZDateTime(2005, 08, 29), "NZAKL", "AUSYD");
			CusMAWB mawb4 = CreateDummyCusMAWB("MAWB4", "QF001", new ZDateTime(2005, 08, 28), "USLAX", "AUMEL");
			CusMAWB mawb5 = CreateDummyCusMAWB("MAWB5", "QF001", new ZDateTime(2005, 08, 29), "SGSIN", "AUSYD");
			Factory.Save();
			AirCargo.SearchMasterBill = "";
			AirCargo.SearchFlightNo = "";
			AirCargo.SearchArrivalDate = new ZDateTime(" ");
			AirCargo.SearchPortOfLoading = "";
			AirCargo.SearchPortOfDischarge = "";
			AirCargo.LoadSearchData();
		}

		#endregion
		public void TestHasSameFlightDetail()
		{
			FlightRecord flightRec1 = new FlightRecord(FlightDetailLine);
			AssertNotNull(AirCargo);
			bool retVal = AirCargo.HasSameFlightDetail(flightRec1);
			AssertEquals("HasSameFlightDetail should be true", true, AirCargo.HasSameFlightDetail(flightRec1));
			flightRec1.MasterBill = "NewMasterBill";
			AssertEquals("HasSameFlightDetail should be false, as MasterBill is different", false, AirCargo.HasSameFlightDetail(flightRec1));
			flightRec1.MasterBill = FlightRec.MasterBill;
			AssertEquals("PreCondition: HasSameFlightDetail should be true", true, AirCargo.HasSameFlightDetail(flightRec1));
			flightRec1.FlightNumber = "QFzzzz";
			AssertEquals("HasSameFlightDetail should be false, as FlightNumber is different", false, AirCargo.HasSameFlightDetail(flightRec1));
			flightRec1.FlightNumber = FlightRec.FlightNumber;
			AssertEquals("PreCondition: HasSameFlightDetail should be true", true, AirCargo.HasSameFlightDetail(flightRec1));
			flightRec1.FlightDate = new ZDateTime(2001, 1, 1);
			AssertEquals("HasSameFlightDetail should be false, as FlightDate is different", false, AirCargo.HasSameFlightDetail(flightRec1));
			flightRec1.FlightDate = FlightRec.FlightDate;
			AssertEquals("PreCondition: HasSameFlightDetail should be true", true, AirCargo.HasSameFlightDetail(flightRec1));
			flightRec1.PortOfLoading = "NKZZZ";
			AssertEquals("HasSameFlightDetail should be false, as PortOfLoading is different", false, AirCargo.HasSameFlightDetail(flightRec1));
			flightRec1.PortOfLoading = FlightRec.PortOfLoading;
			AssertEquals("PreCondition: HasSameFlightDetail should be true", true, AirCargo.HasSameFlightDetail(flightRec1));
			flightRec1.PortOfDischarge = "NKZZZ";
			AssertEquals("HasSameFlightDetail should be false, as PortOfDischarge is different", false, AirCargo.HasSameFlightDetail(flightRec1));
			flightRec1.PortOfDischarge = FlightRec.PortOfDischarge;
		}

		public void TestCopySearchData()
		{
			AssertNotNull(AirCargo);
			AirCargo.SearchMasterBill = "NewMAWB";
			AirCargo.SearchFlightNo = "QF433";
			AirCargo.SearchArrivalDate = new ZDateTime(2004, 2, 3);
			AirCargo.SearchPortOfLoading = "NZAKL";
			AirCargo.SearchPortOfDischarge = "USLAX";
			AssertEquals("PreCondition: Search MasterBill is different", false, AirCargo.MasterBill == AirCargo.SearchMasterBill);
			AssertEquals("PreCondition: Search FlightNo is different", false, AirCargo.FlightNo == AirCargo.SearchFlightNo);
			AssertEquals("PreCondition: Search ArrivalDate is different", false, AirCargo.ArrivalDate == AirCargo.SearchArrivalDate);
			AssertEquals("PreCondition: Search PortOfLoading is different", false, AirCargo.PortOfLoading == AirCargo.SearchPortOfLoading);
			AssertEquals("PreCondition: Search PortOfDischarge is different", false, AirCargo.PortOfDischarge == AirCargo.SearchPortOfDischarge);
			AirCargo.CopySearchData();
			AssertEqualsIgnoreTrailingSpaces("Search MasterBill is the same", AirCargo.SearchMasterBill, AirCargo.MasterBill);
			AssertEqualsIgnoreTrailingSpaces("Search FlightNo is the same", AirCargo.SearchFlightNo, AirCargo.FlightNo);
			AssertEquals("Search ArrivalDate is the same", AirCargo.SearchArrivalDate, AirCargo.ArrivalDate);
			AssertEqualsIgnoreTrailingSpaces("Search PortOfLoading is the same", AirCargo.SearchPortOfLoading, AirCargo.PortOfLoading);
			AssertEqualsIgnoreTrailingSpaces("Search PortOfDischarge is the same", AirCargo.SearchPortOfDischarge, AirCargo.PortOfDischarge);
		}

		public void TestAddConsignmentDetail()
		{
			AssertNotNull(AirCargo);
			AssertEquals("PreCondition: AirCargo.NoOfHouseBills is zero", 0, AirCargo.NoOfHouseBills);
			ConsignmentRecord consignmentRec = new ConsignmentRecord(ConsignmentLine);
			AirCargo.AddConsignmentDetail(consignmentRec);
			AssertEquals("AirCargo.NoOfHouseBills", 1, AirCargo.NoOfHouseBills);
		}

		public void TestAddConsignmentNotes()
		{
			AssertNotNull(AirCargo);
			AssertEquals("PreCondition: AirCargo.ConsignmentNotes.Count is zero", 0, AirCargo.ConsignmentNotes.Count);
			ConsignmentNoteRecord consignmentNoteRec = new ConsignmentNoteRecord(ConsignmentNoteLine);
			AirCargo.AddConsignmentNotes(consignmentNoteRec);
			AssertEquals("AirCargo.ConsignmentNotes.Count", 1, AirCargo.ConsignmentNotes.Count);
		}

		#region TestSave
		public void TestSave()
		{
			ZString oldSetting = Env.Registry.AUCustomsImportsMessagingMode;
			try
			{
				OrgProxyPortMappingTestHelper portMappingHelper = new OrgProxyPortMappingTestHelper(Factory);
				portMappingHelper.OrgProxy.PatternMatchOverrides_ForBinding.RemoveAndDeleteAll();
				portMappingHelper.AddPortMappingToOrgProxy("SYD", portMappingHelper.AUSYDUnLoco);
				portMappingHelper.AddPortMappingToOrgProxy("SIN", portMappingHelper.SGSINUnLoco);
				Factory.Save();
				FlightRecord flightRec = new FlightRecord(FlightDetailLine);
				flightRec.PortOfDischarge = "SYD";
				flightRec.PortOfLoading = "SIN";
				IQDownAirCargo airCargo = new IQDownAirCargo(Factory, flightRec, SydBranch.GB_Code);
				ConsignmentRecord consignmentRec = new ConsignmentRecord(ConsignmentLine);
				consignmentRec.Origin = "SIN";
				consignmentRec.Destination = "SYD";
				airCargo.AddConsignmentDetail(consignmentRec);
				ConsignmentNoteRecord consignmentNoteRec = new ConsignmentNoteRecord(ConsignmentNoteLine);
				airCargo.AddConsignmentNotes(consignmentNoteRec);
				Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				AssertSave(airCargo, flightRec, consignmentRec, consignmentNoteRec, "PPD");
				Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				AssertSave(airCargo, flightRec, consignmentRec, consignmentNoteRec, "PO");
			}
			finally
			{
				Env.Registry.AUCustomsImportsMessagingMode = oldSetting;
			}
		}

		void AssertSave(IQDownAirCargo airCargo, FlightRecord flightRec, ConsignmentRecord consignmentRec, ConsignmentNoteRecord consignmentNoteRec, ZString termsOfPayment)
		{
			CusMAWB masterBill = Factory.New<CusMAWB>();
			AssertEquals("PreCondition: AirCargo Not linked to MasterBill", true, airCargo.LinkedMasterBill == null || airCargo.LinkedMasterBill.PK != masterBill.PK);
			airCargo.PopulateNewData(masterBill);
			Factory.Save();
			AssertPopulateCusMAWB(masterBill, airCargo);
			AssertEquals("1 HouseBill should have been created", 1, masterBill.ChildBills.Count);
			CusHAWB houseBill = masterBill.ChildBills[0];
			AssertNotNull("Housebill should not be null", houseBill);
			int declarationCount = Factory.GetDatabaseCount(typeof(JobDeclaration));
			AssertEquals("PreCondition: HouseBill is not linked to Declaration", ZGuid.Empty, houseBill.CS_JE_CustomsFormalEntry);
			airCargo.Save();
			AssertNotNull("AirCargo's linked MasterBill should not be null", airCargo.LinkedMasterBill);
			AssertEquals("AirCargo is linked to MasterBill", masterBill.PK, airCargo.LinkedMasterBill.PK);
			AssertEquals("AirCargo's Linked MasterBill Ref", masterBill.UnderbondHumanReadableName, airCargo.LinkedMasterBillRef);
		}

		#endregion
		#region TestPopulateNewData
		public void TestPopulateNewData()
		{
			ZString oldSetting = Env.Registry.AUCustomsImportsMessagingMode;
			try
			{
				AssertNotNull(AirCargo);
				ConsignmentRecord consignmentRec = new ConsignmentRecord(ConsignmentLine);
				consignmentRec.TermsOfPayment = "R";
				consignmentRec.DocumentIndicator = "N";
				AirCargo.AddConsignmentDetail(consignmentRec);
				consignmentRec.ConsigneeName = "";
				consignmentRec.ConsigneeAddress1 = "";
				consignmentRec.ConsigneeCity = "ADELAIDE";
				consignmentRec.ConsigneeState = "SA";
				consignmentRec.ConsigneeCountry = Core.Constants.CountryCodes.Australia;
				consignmentRec.ConsigneeContactPhone = "";
				consignmentRec.ConsigneePhone = "SHIP PHONE";
				consignmentRec.DeliveryContactPhone = "DLV CONTACTPHONE";
				consignmentRec.ConsignorAddress1 = "";
				consignmentRec.ConsignorAddress2 = "";
				consignmentRec.ConsignorContactPhone = "SHIP CONTACTPHONE";
				consignmentRec.PickupAddress1 = "";
				consignmentRec.PickupAddress2 = "COL ADDRESSS 2";
				consignmentRec.PickupContactPhone = "";
				consignmentRec.PickupPhone = "COL PHONE";
				ConsignmentNoteRecord consignmentNoteRec = new ConsignmentNoteRecord(ConsignmentNoteLine);
				consignmentNoteRec.Description1 = "";
				consignmentNoteRec.Description2 = "";
				consignmentNoteRec.Description3 = "";
				AirCargo.AddConsignmentNotes(consignmentNoteRec);
				Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				AssertPopulateNewData(AirCargo, FlightRec, consignmentRec, consignmentNoteRec, "CC");
			}
			finally
			{
				Env.Registry.AUCustomsImportsMessagingMode = oldSetting;
			}
		}

		void AssertPopulateNewData(IQDownAirCargo airCargo, FlightRecord flightRec, ConsignmentRecord consignmentRec, ConsignmentNoteRecord consignmentNoteRec, ZString termsOfPayment)
		{
			CusMAWB masterBill = Factory.New<CusMAWB>();
			try
			{
				airCargo.Progress += new TNTProgressEventHandler(Progress);
				ProgressCalled = false;
				airCargo.PopulateNewData(masterBill);
				AssertPopulateCusMAWB(masterBill, airCargo);
				AssertEquals("1 HouseBill should have been created", 1, masterBill.ChildBills.Count);
				CusHAWB houseBill = masterBill.ChildBills[0];
				AssertHouseBill(houseBill, consignmentRec.HouseBill, consignmentRec.Origin, houseBill.MAWB.CM_RL_NKDischargePort, consignmentRec.Weight, consignmentRec.GoodsValue, consignmentRec.GoodsCurrency, ConsignmentUpdator.DefaultEmptyGoodsDescriptionText, consignmentRec.PackageCount, termsOfPayment, "STD");
				AssertConsignee(houseBill, consignmentRec.ConsigneeContactName, ConsignmentUpdator.NotSuppliedString, consignmentRec.ConsigneeAddress2, consignmentRec.ConsigneeCity, consignmentRec.ConsigneeState, consignmentRec.ConsigneePostCode, consignmentRec.ConsigneeCountry, consignmentRec.ConsigneePhone);
				AssertDelivery(houseBill, consignmentRec.DeliveryContactName, consignmentRec.DeliveryName, consignmentRec.DeliveryAddress1, consignmentRec.DeliveryAddress2, consignmentRec.DeliveryCity, consignmentRec.DeliveryState, consignmentRec.DeliveryPostCode, consignmentRec.DeliveryContactPhone);
				AssertConsignor(houseBill, consignmentRec.ConsignorContactName, consignmentRec.ConsignorName, ConsignmentUpdator.NotSuppliedString, consignmentRec.ConsignorCity, consignmentRec.ConsignorState, consignmentRec.ConsignorPostCode, consignmentRec.ConsignorCountry, consignmentRec.ConsignorContactPhone);
				AssertPickup(houseBill, consignmentRec.PickupContactName, consignmentRec.PickupName, consignmentRec.PickupAddress2, "", consignmentRec.PickupCity, consignmentRec.PickupState, consignmentRec.PickupPostCode, consignmentRec.PickupPhone);
				AssertUpdateQuantumOriginalValue(houseBill, consignmentRec, flightRec);
				AssertEquals("Progress Called", true, ProgressCalled);
			}
			finally
			{
				airCargo.Progress -= new TNTProgressEventHandler(Progress);
			}
		}

		#endregion
		#region TestPopulateData
		public void TestPopulateData()
		{
			ZString oldSetting = Env.Registry.AUCustomsImportsMessagingMode;
			try
			{
				AssertNotNull(AirCargo);
				ConsignmentRecord consignmentRec = new ConsignmentRecord(ConsignmentLine);
				consignmentRec.TermsOfPayment = "S";
				consignmentRec.DocumentIndicator = "D";
				AirCargo.AddConsignmentDetail(consignmentRec);
				consignmentRec.ConsignorName = "";
				consignmentRec.ConsignorAddress1 = "";
				consignmentRec.ConsignorContactPhone = "";
				consignmentRec.ConsignorPhone = "SHIP PHONE";
				consignmentRec.PickupContactPhone = "COL CONTACTPHONE";
				consignmentRec.ConsigneeAddress1 = "";
				consignmentRec.ConsigneeAddress2 = "";
				consignmentRec.ConsigneeContactPhone = "RCV CONTACTPHONE";
				consignmentRec.DeliveryAddress1 = "";
				consignmentRec.DeliveryAddress2 = "COL ADDRESSS 2";
				consignmentRec.DeliveryContactPhone = "";
				consignmentRec.DeliveryPhone = "DLV PHONE";
				ConsignmentNoteRecord consignmentNoteRec = new ConsignmentNoteRecord(ConsignmentNoteLine);
				consignmentNoteRec.Description1 = "";
				consignmentNoteRec.Description2 = "";
				consignmentNoteRec.Description3 = "";
				AirCargo.AddConsignmentNotes(consignmentNoteRec);
				Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				AssertPopulateData(AirCargo, FlightRec, consignmentRec, consignmentNoteRec, "PO");
			}
			finally
			{
				Env.Registry.AUCustomsImportsMessagingMode = oldSetting;
			}
		}

		void AssertPopulateData(IQDownAirCargo airCargo, FlightRecord flightRec, ConsignmentRecord consignmentRec, ConsignmentNoteRecord consignmentNoteRec, ZString termsOfPayment)
		{
			CusMAWB masterBill = Factory.New<CusMAWB>();
			masterBill.CM_RL_NKDischargePort = consignmentRec.Destination;
			airCargo.PopulateData(masterBill);
			AssertEquals("1 HouseBill should have been created", 1, masterBill.ChildBills.Count);
			CusHAWB houseBill = masterBill.ChildBills[0];
			AssertHouseBill(houseBill, consignmentRec.HouseBill, consignmentRec.Origin, consignmentRec.Destination, consignmentRec.Weight, consignmentRec.GoodsValue, consignmentRec.GoodsCurrency, ConsignmentUpdator.DefaultEmptyGoodsDescriptionText, consignmentRec.PackageCount, termsOfPayment, "DOC");
			AssertConsignor(houseBill, consignmentRec.ConsignorContactName, ConsignmentUpdator.NotSuppliedString, consignmentRec.ConsignorAddress2, consignmentRec.ConsignorCity, consignmentRec.ConsignorState, consignmentRec.ConsignorPostCode, consignmentRec.ConsignorCountry, consignmentRec.ConsignorPhone);
			AssertPickup(houseBill, consignmentRec.PickupContactName, consignmentRec.PickupName, consignmentRec.PickupAddress1, consignmentRec.PickupAddress2, consignmentRec.PickupCity, consignmentRec.PickupState, consignmentRec.PickupPostCode, consignmentRec.PickupContactPhone);
			AssertConsignee(houseBill, consignmentRec.ConsigneeContactName, consignmentRec.ConsigneeName, ConsignmentUpdator.NotSuppliedString, consignmentRec.ConsigneeCity, consignmentRec.ConsigneeState, consignmentRec.ConsigneePostCode, consignmentRec.ConsigneeCountry, consignmentRec.ConsigneeContactPhone);
			AssertDelivery(houseBill, consignmentRec.DeliveryContactName, consignmentRec.DeliveryName, consignmentRec.DeliveryAddress2, "", consignmentRec.DeliveryCity, consignmentRec.DeliveryState, consignmentRec.DeliveryPostCode, consignmentRec.DeliveryPhone);
			AssertUpdateQuantumOriginalValue(houseBill, consignmentRec, flightRec);
		}

		#endregion
		#region TestPopulateData_ExistingHouseBill
		public void TestPopulateData_ExistingHouseBill()
		{
			ZString oldSetting = Env.Registry.AUCustomsImportsMessagingMode;
			try
			{
				AssertNotNull(AirCargo);
				ConsignmentRecord consignmentRec = new ConsignmentRecord(ConsignmentLine);
				AirCargo.AddConsignmentDetail(consignmentRec);
				ConsignmentNoteRecord consignmentNoteRec = new ConsignmentNoteRecord(ConsignmentNoteLine);
				AirCargo.AddConsignmentNotes(consignmentNoteRec);
				Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				AssertPopulateData_ExistingHouseBill(AirCargo, FlightRec, consignmentRec, consignmentNoteRec, "PO");
			}
			finally
			{
				Env.Registry.AUCustomsImportsMessagingMode = oldSetting;
			}
		}

		void AssertPopulateData_ExistingHouseBill(IQDownAirCargo airCargo, FlightRecord flightRec, ConsignmentRecord consignmentRec, ConsignmentNoteRecord consignmentNoteRec, ZString termsOfPayment)
		{
			CusMAWB masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_HAWB = consignmentRec.HouseBill.Trim();
			StmNote note = houseBill.Notes.AddNew(true, "Blah Blah Blah", "Blah Blah");
			StmNoteCollection notes = new StmNoteCollection(houseBill, Factory);
			notes.Load();
			int notesCount = notes.Count;
			airCargo.PopulateData(masterBill);
			AssertEquals("No new HouseBill should have been created", 1, masterBill.ChildBills.Count);
			AssertDefaultData(masterBill.ChildBills[0], airCargo, flightRec, consignmentRec, consignmentNoteRec, termsOfPayment);
			notes.Load();
			AssertEquals("1 new note should have been created for the HouseBill", notesCount + 1, notes.Count);
			AssertEquals("Existing Note 'Blah Blah Blah' should not have been changed", "Blah Blah", note.ST_NoteDataAsText);
			masterBill = Factory.New<CusMAWB>();
			houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_HAWB = consignmentRec.HouseBill.Trim();
			note = houseBill.Notes.AddNew(true, ConsignmentUpdator.OriginalQuantumSectorNoteDescription, "Blah Blah");
			notes = new StmNoteCollection(houseBill, Factory);
			notes.Load();
			notesCount = notes.Count;
			airCargo.PopulateData(masterBill);
			AssertEquals("No new HouseBill should have been created", 1, masterBill.ChildBills.Count);
			AssertDefaultData(masterBill.ChildBills[0], airCargo, flightRec, consignmentRec, consignmentNoteRec, termsOfPayment);
			notes.Load();
			AssertEquals("No new note were created for the HouseBill as existing one exist", notesCount, notes.Count);
			AssertEquals("Existing Note '" + ConsignmentUpdator.OriginalQuantumSectorNoteDescription + "' should have been updated; Note:" + System.Environment.NewLine + note.ST_NoteDataAsText, false, note.ST_NoteDataAsText == "Blah Blah");
		}

		#endregion
		public void TestPopulateData_DuplicateHouseBill()
		{
			AssertNotNull(AirCargo);
			ConsignmentRecord consignmentRec = new ConsignmentRecord(ConsignmentLine);
			consignmentRec.PickupAddress1 = "";
			consignmentRec.DeliveryAddress1 = "";
			AirCargo.AddConsignmentDetail(consignmentRec);
			ConsignmentNoteRecord consignmentNoteRec = new ConsignmentNoteRecord(ConsignmentNoteLine);
			AirCargo.AddConsignmentNotes(consignmentNoteRec);
			CusMAWB masterBill = Factory.New<CusMAWB>();
			masterBill.CM_MAWB = "MasterBill";
			CusHAWB houseBill1 = masterBill.ChildBills.AddNew();
			houseBill1.CS_HAWB = consignmentRec.HouseBill.Trim();
			CusHAWB houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.CS_HAWB = consignmentRec.HouseBill.Trim();
			UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
			AssertNotNull("UserNotification should not be null", userNotification);
			userNotification.ClearMessagesAndAnswers();
			AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
			userNotification.AddAnswer(DialogResult.Cancel);
			AirCargo.PopulateData(masterBill);
			string expectedMessage = string.Format("{0} has duplicated Housebills.{1}They can not be updated. Skip this Housebill and import the rest.", masterBill.UnderbondHumanReadableName, System.Environment.NewLine);
			var lastMessage = userNotification.LastMessage.Text ?? string.Empty;
			AssertEquals("Expected 1 new error message", 2, userNotification.PreviousMessages.Length);
			AssertEquals("Last Error Message:" + System.Environment.NewLine + lastMessage, true, lastMessage.IndexOf(expectedMessage) >= 0);
			AssertEquals("No new HouseBill should have been created", 2, masterBill.ChildBills.Count);
			AssertEquals("Buffer should error as DialogResult is 'Cancel' - Buffer contains:" + System.Environment.NewLine + AirCargo.Buffer.AsString, true, AirCargo.Buffer.HasErrors);
			AssertEquals("Buffer should have errors - Buffer:" + System.Environment.NewLine + AirCargo.Buffer.AsString, true, AirCargo.Buffer.ContainsNotificationType(TNTErrorType.MoreThan1NKMatch));
			string errorMessage = "Housebill: " + consignmentRec.HouseBill;
			AssertEquals("Buffer should have error message '" + errorMessage + "' - Buffer:" + System.Environment.NewLine + AirCargo.Buffer.AsString, true, AirCargo.Buffer.AsString.IndexOf(errorMessage) >= 0);
			userNotification.ClearMessagesAndAnswers();
			AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
			userNotification.AddAnswer(DialogResult.OK);
			AirCargo.PopulateData(masterBill);
			AssertEquals("Expected 1 new error message", 2, userNotification.PreviousMessages.Length);
			AssertEquals("Last Error Message:" + System.Environment.NewLine + lastMessage, true, lastMessage.IndexOf(expectedMessage) >= 0);
			AssertEquals("No new HouseBill should have been created", 2, masterBill.ChildBills.Count);
			AssertEquals("Buffer should no error as DialogResult is 'OK' - Buffer contains:" + System.Environment.NewLine + AirCargo.Buffer.AsString, false, AirCargo.Buffer.HasErrors);
		}

		#region Implementation
		#region Assertions
		void AssertDefaultData(CusHAWB houseBill, IQDownAirCargo airCargo, FlightRecord flightRec, ConsignmentRecord consignmentRec, ConsignmentNoteRecord consignmentNoteRec, ZString termsOfPayment)
		{
			AssertNotNull("HouseBill is not null", houseBill);
			AssertHouseBill(houseBill, consignmentRec.HouseBill, consignmentRec.Origin, houseBill.MAWB.CM_RL_NKDischargePort, consignmentRec.Weight, consignmentRec.GoodsValue, consignmentRec.GoodsCurrency, consignmentNoteRec.NoteText, consignmentRec.PackageCount, termsOfPayment, "STD");
			AssertConsignor(houseBill, consignmentRec.ConsignorContactName, consignmentRec.ConsignorName, consignmentRec.ConsignorAddress1, consignmentRec.ConsignorCity, consignmentRec.ConsignorState, consignmentRec.ConsignorPostCode, consignmentRec.ConsignorCountry, consignmentRec.ConsignorContactPhone);
			AssertPickup(houseBill, consignmentRec.PickupContactName, consignmentRec.PickupName, consignmentRec.PickupAddress1, consignmentRec.PickupAddress2, consignmentRec.PickupCity, consignmentRec.PickupState, consignmentRec.PickupPostCode, consignmentRec.PickupContactPhone);
			AssertConsignee(houseBill, consignmentRec.ConsigneeContactName, consignmentRec.ConsigneeName, consignmentRec.ConsigneeAddress1, consignmentRec.ConsigneeCity, consignmentRec.ConsigneeState, consignmentRec.ConsigneePostCode, consignmentRec.ConsigneeCountry, consignmentRec.ConsigneeContactPhone);
			AssertDelivery(houseBill, consignmentRec.DeliveryContactName, consignmentRec.DeliveryName, consignmentRec.DeliveryAddress1, consignmentRec.DeliveryAddress2, consignmentRec.DeliveryCity, consignmentRec.DeliveryState, consignmentRec.DeliveryPostCode, consignmentRec.DeliveryContactPhone);
			AssertUpdateQuantumOriginalValue(houseBill, consignmentRec, flightRec);
		}

		void AssertHouseBill(CusHAWB houseBill, ZString hAWB, ZString origin, ZString destination, ZDecimal weight, ZDecimal goodsValue, ZString currency, ZString goodsDescription, ZShort packageCount, ZString termsOfPayment, ZString shipmentType)
		{
			AssertNotNull("HouseBill is not null", houseBill);
			AssertEqualsIgnoreTrailingSpaces("HouseBill No", hAWB, houseBill.CS_HAWB);
			AssertEqualsIgnoreTrailingSpaces("Origin", origin, houseBill.CS_RL_NKOrigin);
			AssertEqualsIgnoreTrailingSpaces("Destination", destination, houseBill.CS_RL_NKDestination);
			AssertEquals("Weight", weight, houseBill.CS_Weight);
			AssertEquals("Chargable Weight and Weight should be the same", houseBill.CS_Weight, houseBill.CS_ChargableWeight);
			AssertEqualsIgnoreTrailingSpaces("Weight Unit Quantity", Core.Constants.Weight.Kilograms, houseBill.CS_WeightUQ);
			AssertEquals("Goods Value", goodsValue, houseBill.CS_GoodsValue);
			AssertEqualsIgnoreTrailingSpaces("Currency", currency, houseBill.CS_RX_NKGoodsCurrency);
			AssertEqualsIgnoreTrailingSpaces("Goods Description", goodsDescription.Left(CusHAWBSchema.CS_GoodsDescription.MaxLength), houseBill.CS_GoodsDescription);
			AssertEquals("Manifested PackageCount", packageCount, houseBill.CS_PiecesManifested);
			AssertEqualsIgnoreTrailingSpaces("Payment Terms", termsOfPayment, houseBill.CS_FreightPrepaidCollect);
			AssertEqualsIgnoreTrailingSpaces("Shipment Type", shipmentType, houseBill.CS_ShipmentType);
		}

		void AssertConsignor(CusHAWB houseBill, ZString contactName, ZString name, ZString street, ZString city, ZString state, ZString post, ZString country, ZString phone)
		{
			AssertNotNull("HouseBill is not null", houseBill);
			AssertEquals("CS_OH_Consignor", ZGuid.Empty, houseBill.CS_OH_Consignor);
			AssertEqualsIgnoreTrailingSpaces("Consignor Contact Name", contactName, houseBill.CS_ConsignorContactName);
			AssertEqualsIgnoreTrailingSpaces("Consignor Name", name, houseBill.CS_ConsignorName);
			AssertEqualsIgnoreTrailingSpaces("Consignor Street", street, houseBill.CS_ConsignorStreet);
			AssertEqualsIgnoreTrailingSpaces("Consignor City", city, houseBill.CS_ConsignorCity);
			AssertEqualsIgnoreTrailingSpaces("Consignor State", state.Left(CusHAWBSchema.CS_ConsigneeState.MaxLength), houseBill.CS_ConsignorState);
			AssertEqualsIgnoreTrailingSpaces("Consignor Post", post, houseBill.CS_ConsignorPostcode);
			AssertEqualsIgnoreTrailingSpaces("Consignor Country", country, houseBill.CS_RN_NKConsignorCountry);
			AssertEqualsIgnoreTrailingSpaces("Consignor Phone", phone, houseBill.CS_ConsignorPhone);
		}

		void AssertPickup(CusHAWB houseBill, ZString contactName, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString post, ZString phone)
		{
			AssertNotNull("HouseBill is not null", houseBill);
			ZQuery pickupFilter = CusHawbQuery(houseBill.PK, DocAddressTypes.Codes.SupplierPickupDeliveryAddress);
			JobDocAddress pickup = Factory.LoadTop1<JobDocAddress>(pickupFilter);
			AssertNotNull("Pickup is not null", pickup);
			AssertEqualsIgnoreTrailingSpaces("Pickup Contact Name", contactName, pickup.E2_Contact);
			AssertEqualsIgnoreTrailingSpaces("Pickup Name", name, pickup.E2_CompanyName);
			AssertEqualsIgnoreTrailingSpaces("Pickup Address 1", address1, pickup.E2_Address1);
			AssertEqualsIgnoreTrailingSpaces("Pickup Address 2", address2, pickup.E2_Address2);
			AssertEqualsIgnoreTrailingSpaces("Pickup City", city, pickup.E2_City);
			AssertEqualsIgnoreTrailingSpaces("Pickup State", state.Left(JobDocAddressSchema.E2_State.MaxLength), pickup.E2_State);
			AssertEqualsIgnoreTrailingSpaces("Pickup Post", post, pickup.E2_Postcode);
			AssertEqualsIgnoreTrailingSpaces("Pickup Phone", phone, pickup.E2_Phone);
		}

		void AssertConsignee(CusHAWB houseBill, ZString contactName, ZString name, ZString street, ZString city, ZString state, ZString post, ZString country, ZString phone)
		{
			AssertNotNull("HouseBill is not null", houseBill);
			AssertEquals("CS_OH_Consignee", ZGuid.Empty, houseBill.CS_OH_Consignee);
			AssertEqualsIgnoreTrailingSpaces("Consignee Contact Name", contactName, houseBill.CS_ConsigneeContactName);
			AssertEqualsIgnoreTrailingSpaces("Consignee Name", name, houseBill.CS_ConsigneeName);
			AssertEqualsIgnoreTrailingSpaces("Consignee Street", street, houseBill.CS_ConsigneeStreet);
			AssertEqualsIgnoreTrailingSpaces("Consignee City", city, houseBill.CS_ConsigneeCity);
			AssertEqualsIgnoreTrailingSpaces("Consignee State", state.Left(CusHAWBSchema.CS_ConsigneeState.MaxLength), houseBill.CS_ConsigneeState);
			AssertEqualsIgnoreTrailingSpaces("Consignee Post", post, houseBill.CS_ConsigneePostcode);
			AssertEqualsIgnoreTrailingSpaces("Consignee Country", country, houseBill.CS_RN_NKConsigneeCountry);
			AssertEqualsIgnoreTrailingSpaces("Consignee Phone", phone, houseBill.CS_ConsigneePhone);
		}

		void AssertDelivery(CusHAWB houseBill, ZString contactName, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString post, ZString phone)
		{
			AssertNotNull("HouseBill is not null", houseBill);
			ZQuery deliveryFilter = CusHawbQuery(houseBill.PK, DocAddressTypes.Codes.ImporterPickupDeliveryAddress);
			JobDocAddress delivery = Factory.LoadTop1<JobDocAddress>(deliveryFilter);
			AssertNotNull("Delivery is not null", delivery);
			AssertEqualsIgnoreTrailingSpaces("Delivery Contact Name", contactName, delivery.E2_Contact);
			AssertEqualsIgnoreTrailingSpaces("Delivery Name", name, delivery.E2_CompanyName);
			AssertEqualsIgnoreTrailingSpaces("Delivery Address 1", address1, delivery.E2_Address1);
			AssertEqualsIgnoreTrailingSpaces("Delivery Address 2", address2, delivery.E2_Address2);
			AssertEqualsIgnoreTrailingSpaces("Delivery City", city, delivery.E2_City);
			AssertEqualsIgnoreTrailingSpaces("Delivery State", state.Left(JobDocAddressSchema.E2_State.MaxLength), delivery.E2_State);
			AssertEqualsIgnoreTrailingSpaces("Delivery Post", post, delivery.E2_Postcode);
			AssertEqualsIgnoreTrailingSpaces("Delivery Phone", phone, delivery.E2_Phone);
		}

		void AssertUpdateQuantumOriginalValue(CusHAWB houseBill, ConsignmentRecord consignment, FlightRecord flightRec)
		{
			AssertNotNull("HouseBill is not null", houseBill);
			AssertNotNull("Consignment is not null", consignment);
			string expectedValue = ZString.Format("{0}-{1}-{2}-{3}", flightRec.RecordKey, consignment.Origin.Right(3).PadRight(3), consignment.Destination.Right(3).PadRight(3), SydBranch.GB_Code);
			StmNote[] sectorNotes = houseBill.Notes.FindByDescription(ConsignmentUpdator.OriginalQuantumSectorNoteDescription);
			AssertEquals("Housebill should have 1 Original Sector Note", 1, sectorNotes.Length);
			AssertEqualsIgnoreTrailingSpaces("Folio Reference", expectedValue, sectorNotes[0].ST_NoteDataAsText);
		}

		ZQuery CusHawbQuery(ZGuid housebillPK, ZString docAddressType)
		{
			ZQuery query = new ZQuery(JobDocAddressSchema.E2_ParentID, housebillPK);
			query.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(CusHAWBSchema.Constants.TableName));
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, docAddressType);
			return query;
		}

		#endregion
		#region Progress;
		void Progress(object sender, TNTProgressEventArgs e)
		{
			ProgressCalled = true;
		}

		bool ProgressCalled;
		#endregion
		#region AirCargo
		IQDownAirCargo AirCargo
		{
			get
			{
				if (fAirCargo == null)
				{
					fAirCargo = new IQDownAirCargo(Factory, FlightRec, SydBranch.GB_Code);
				}

				return fAirCargo;
			}
		}

		IQDownAirCargo fAirCargo;
		#endregion
		protected override BusinessObject GetNewBusinessObject()
		{
			return new IQDownAirCargo(Factory, new FlightRecord(FlightDetailLine), SydBranch.GB_Code);
		}
		#endregion
	}
}
