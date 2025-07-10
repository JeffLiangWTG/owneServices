using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.TNT.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(ImportAirCargoTestClass))]
	public class ImportAirCargoTest : ImportAirCargoTestCase
	{
		#region TestConstructor
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor_WithException()
		{
			ImportAirCargoTestClass airCargo = new ImportAirCargoTestClass(Factory, null, SydBranch.GB_Code);
		}

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
			AssertEquals("DepartureDate", FlightRec.FlightDate, AirCargo.DepartureDate);
			AssertEqualsIgnoreTrailingSpaces("PortOfLoading", portMappingHelper.SGSINUnLoco.Code, AirCargo.PortOfLoading);
			AssertEqualsIgnoreTrailingSpaces("PortOfDischarge", portMappingHelper.AUSYDUnLoco.Code, AirCargo.PortOfDischarge);
			AssertEqualsIgnoreTrailingSpaces("SearchMasterbill", AirCargo.MasterBill, AirCargo.SearchMasterBill);
			AssertEqualsIgnoreTrailingSpaces("SearchFlightNo", AirCargo.FlightNo, AirCargo.SearchFlightNo);
			AssertEquals("SearchArrivalDate", AirCargo.ArrivalDate, AirCargo.SearchArrivalDate);
			AssertEqualsIgnoreTrailingSpaces("SearchPortOfLoading", AirCargo.PortOfLoading, AirCargo.SearchPortOfLoading);
			AssertEqualsIgnoreTrailingSpaces("SearchPortOfDischarge", AirCargo.PortOfDischarge, AirCargo.SearchPortOfDischarge);
			AssertEquals("Branch", SydBranch.PK, AirCargo.Branch.PK);
		}

		#endregion
		public void TestSave()
		{
			AssertNotNull(AirCargo);
			AirCargo.SaveCoreWasCalled = false;
			AirCargo.Save();
			AssertEquals("SaveCoreWasCalled is true", true, AirCargo.SaveCoreWasCalled);
		}

		public void TestPopulateNewData()
		{
			OrgProxyPortMappingTestHelper portMappingHelper = new OrgProxyPortMappingTestHelper(Factory);
			portMappingHelper.OrgProxy.PatternMatchOverrides_ForBinding.RemoveAndDeleteAll();
			portMappingHelper.AddPortMappingToOrgProxy("WHR", portMappingHelper.AUSYDUnLoco);
			portMappingHelper.AddPortMappingToOrgProxy("SIN", portMappingHelper.SGSINUnLoco);
			Factory.Save();
			CusMAWB masterBill = Factory.New<CusMAWB>();
			masterBill.CM_MAWB = "MAWB1";
			AssertNotNull(AirCargo);
			AirCargo.PopulateDataCoreWasCalled = false;
			AirCargo.Buffer.Notify(new ErrorNotification(ErrorType.Error, "This Message should be cleared"));
			AirCargo.PopulateNewData(masterBill);
			AssertEquals("Buffer should contain not errors: Buffer contains:" + System.Environment.NewLine + AirCargo.Buffer.AsString, false, AirCargo.Buffer.HasErrors);
			AssertEquals("PopulateDataCoreWasCalled is true", true, AirCargo.PopulateDataCoreWasCalled);
			AssertPopulateCusMAWB(masterBill, AirCargo);
		}

		public void TestPopulateData()
		{
			CusMAWB masterBill = Factory.New<CusMAWB>();
			masterBill.CM_MAWB = "MAWB1";
			Factory.Save();
			AssertNotNull(AirCargo);
			AirCargo.PopulateDataCoreWasCalled = false;
			AirCargo.Buffer.Notify(new ErrorNotification(ErrorType.Error, "This Message should be cleared"));
			AirCargo.PopulateData(masterBill);
			AssertEquals("Buffer should not contain errors: Buffer contains:" + System.Environment.NewLine + AirCargo.Buffer.AsString, false, AirCargo.Buffer.HasErrors);
			AssertEquals("PopulateDataCoreWasCalled is true", true, AirCargo.PopulateDataCoreWasCalled);
			AssertEquals("MasterBill should have not changes", false, masterBill.HasChanges);
		}

		public void TestSearchFilter()
		{
			ZQuery expectedFilter = new ZQuery();
			AssertNotNull(AirCargo);
			AirCargo.SearchMasterBill = "";
			AirCargo.SearchFlightNo = "";
			AirCargo.SearchArrivalDate = ZDateTime.Empty;
			AirCargo.SearchPortOfLoading = "";
			AirCargo.SearchPortOfDischarge = "";
			AssertEquals("All active search", expectedFilter.LiteralTextADO, AirCargo.SearchFilter.LiteralTextADO);
			AirCargo.SearchMasterBill = "08123423238";
			expectedFilter.AddToFilter(CusMAWBSchema.CM_MAWB, AirCargo.SearchMasterBill);
			AssertEquals(expectedFilter.LiteralTextADO, AirCargo.SearchFilter.LiteralTextADO);
			AirCargo.SearchArrivalDate = ZDateTime.Now;
			expectedFilter.AddToFilter(CusMAWBSchema.CM_ArrivalDate, AirCargo.SearchArrivalDate);
			AssertEquals(expectedFilter.LiteralTextADO, AirCargo.SearchFilter.LiteralTextADO);
			AirCargo.SearchFlightNo = "QF1234";
			expectedFilter.AddToFilter(CusMAWBSchema.CM_FlightNo, AirCargo.SearchFlightNo);
			AssertEquals(expectedFilter.LiteralTextADO, AirCargo.SearchFilter.LiteralTextADO);
			AirCargo.SearchPortOfLoading = "NZAKL";
			expectedFilter.AddToFilter(CusMAWBSchema.CM_RL_NKLoadPort, AirCargo.SearchPortOfLoading);
			AssertEquals(expectedFilter.LiteralTextADO, AirCargo.SearchFilter.LiteralTextADO);
			AirCargo.SearchPortOfDischarge = "NZAKL";
			expectedFilter.AddToFilter(CusMAWBSchema.CM_RL_NKDischargePort, AirCargo.SearchPortOfDischarge);
			AssertEquals(expectedFilter.LiteralTextADO, AirCargo.SearchFilter.LiteralTextADO);
		}

		#region Implementation
		#region AirCargo
		ImportAirCargoTestClass AirCargo
		{
			get
			{
				if (fAirCargo == null)
				{
					fAirCargo = new ImportAirCargoTestClass(Factory, FlightRec, SydBranch.GB_Code);
				}

				return fAirCargo;
			}
		}

		ImportAirCargoTestClass fAirCargo;
		#endregion
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportAirCargoTestClass(Factory, new FlightRecord(FlightDetailLine), SydBranch.GB_Code);
		}

		public class ImportAirCargoTestClass : ImportAirCargo
		{
			public ImportAirCargoTestClass(BusinessObjectFactory factory, FlightRecord flightDetail, ZString branchCode) : base(factory, flightDetail, branchCode)
			{
				PopulateDataCoreWasCalled = false;
				SaveCoreWasCalled = false;
			}

			public bool PopulateDataCoreWasCalled;
			protected override void PopulateDataCore(CusMAWB existingCusMawb)
			{
				PopulateDataCoreWasCalled = true;
			}

			public bool SaveCoreWasCalled;
			protected override void SaveCore()
			{
				SaveCoreWasCalled = true;
			}
		}
		#endregion
	}
}
