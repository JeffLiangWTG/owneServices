using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Module.AirCargo;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AirCTOImportFilterBusinessObject))]
	sealed class AirCTOImportFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestJobNumberFilter()
		{
			hAWB1.CS_MessageReference = "A00010001";
			hAWB2.CS_MessageReference = "A00010002";
			Factory.Save();
			ModuleNumberFilter jobNumberFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.JobNumber];
			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			jobNumberFilter.Property = "10001";
			jobNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			jobNumberFilter.Property = "A00010002";
			jobNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			jobNumberFilter.Property = "010";
			jobNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestMasterBillNumberFilter()
		{
			hAWB1.CS_HAWB = "ZZZAAAZZZZZ";
			hAWB2.CS_HAWB = "XXXXXAAAXXX";
			Factory.Save();
			ModuleNumberFilter masterBillNumberFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.MasterBillNumber];
			filterCollection.Load(filterBO.Filter);
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			masterBillNumberFilter.Property = "ZZZ";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			masterBillNumberFilter.Property = "XXXXXAAAXXX";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			masterBillNumberFilter.Property = "AAA";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			masterBillNumberFilter.Property = "YYYYYYYYYYY";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestFlightNoFilter()
		{
			mAWB1.CM_FlightNo = "XAAAX";
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB1.PK;
			partShip1.CG_CS = hAWB2.PK;
			partShip1.CG_FlightNo = "YAAAY";
			Factory.Save();
			ModuleNumberFilter flightNoFilter = (ModuleNumberFilter)filterBO[AirCargoFilterConstants.NumberFilterTypes.FlightNo];
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			flightNoFilter.Property = "XAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			flightNoFilter.Property = "YAAAY";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "AAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "ZZZ";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestLocationsProperty()
		{
			AssertNotNull(filterBO.Locations);
		}

		public void TestLoadFilter()
		{
			hAWB1.CS_RL_NKLoadPort = "KRSEL";
			hAWB2.CS_RL_NKLoadPort = "AUBNE";
			Factory.Save();
			ModuleLocationFilter loadFilter = (ModuleLocationFilter)filterBO[string.Format("{0} / {1}", AirCTOFilterConstants.PortFilterTypes.Load, AirCTOFilterConstants.PortFilterTypes.PortOfArrival)];
			loadFilter.Property1 = "KR";
			loadFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			loadFilter.Property1 = "AUBNE";
			loadFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			loadFilter.Property1 = "AUSYD";
			loadFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestOriginFilter()
		{
			hAWB1.CS_RL_NKOrigin = "KRSEL";
			hAWB2.CS_RL_NKOrigin = "AUBNE";
			Factory.Save();
			ModuleLocationFilter originFilter = (ModuleLocationFilter)filterBO[string.Format("{0} / {1}", AirCTOFilterConstants.PortFilterTypes.Origin, AirCTOFilterConstants.PortFilterTypes.Destination)];
			originFilter.Property1 = "KR";
			originFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			originFilter.Property1 = "AUBNE";
			originFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			originFilter.Property1 = "AUSYD";
			originFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestDestinationFilter()
		{
			hAWB1.CS_RL_NKDestination = "KRSEL";
			hAWB2.CS_RL_NKDestination = "AUBNE";
			Factory.Save();
			ModuleLocationFilter destinationFilter = (ModuleLocationFilter)filterBO[string.Format("{0} / {1}", AirCTOFilterConstants.PortFilterTypes.Origin, AirCTOFilterConstants.PortFilterTypes.Destination)];
			destinationFilter.Property2 = "KR";
			destinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			destinationFilter.Property2 = "AUBNE";
			destinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			destinationFilter.Property2 = "AUSYD";
			destinationFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestPortOfArrivalFilter()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			mAWB1.CM_RL_NKDischargePort = "KRSEL";
			mAWB2.CM_RL_NKDischargePort = "AUBNE";
			Factory.Save();
			ModuleLocationFilter arrivalFilter = (ModuleLocationFilter)filterBO[string.Format("{0} / {1}", AirCTOFilterConstants.PortFilterTypes.Load, AirCTOFilterConstants.PortFilterTypes.PortOfArrival)];
			arrivalFilter.Property2 = "KR";
			arrivalFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			arrivalFilter.Property2 = "AUBNE";
			arrivalFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			arrivalFilter.Property2 = "AUSYD";
			arrivalFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestPortOfFirstArrivalFilter()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			mAWB1.CM_RL_NKFirstArrivalPort = "KRSEL";
			mAWB2.CM_RL_NKFirstArrivalPort = "AUBNE";
			Factory.Save();
			ModuleNkFilter firstArrivalFilter = (ModuleNkFilter)filterBO[AirCTOFilterConstants.PortFilterTypes.PortOf1stArrival];
			firstArrivalFilter.Property = "KR";
			firstArrivalFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			firstArrivalFilter.Property = "AUBNE";
			firstArrivalFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			firstArrivalFilter.Property = "AUSYD";
			firstArrivalFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
		}

		public void TestJobInvoicingStatusFilter()
		{
			hAWB1.CS_CM = mAWB1.PK;
			mAWB1.CM_FlightNo = "12";
			var job1 = new JobHeader.Loader(mAWB1).TryLoadOrCreate();
			AssertNotNull(job1);
			AssertEquals(JobHeaderStatus.Working.Code, job1.JH_Status);
			Factory.Save();
			var filter = (ModuleTextBaseFilter)filterBO["Invoicing Job Status"];
			filter.Property = JobHeaderStatus.Working.Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			AssertCollectionContains(hAWB1, filterCollection);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filterCollection.Load(filterBO.Filter);
			AssertCollectionNotContains(hAWB1, filterCollection);
		}

		public void TestGetStatusList()
		{
			AssertEquals("List should contain everything", 80, filterBO.GetStatusList(null).Count);
			AssertEquals(29, filterBO.GetStatusList(AirCargoFilterConstants.StatusFilterType.Outturn).Count);
			AssertEquals(29, filterBO.GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRUnderbond).Count);
		}

		public void TestOutturnStatusFilter()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "9914N";
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			underbond1.C4_ParentID = mAWB1.PK;
			underbond2.C4_ParentID = mAWB2.PK;
			underbond3.C4_ParentID = hAWB3.PK;
			underbond4.C4_ParentID = hAWB4.PK;
			underbond2.C4_DestinationPremiseID = "9914N";
			underbond3.C4_DestinationPremiseID = "9914N";
			underbond4.C4_DestinationPremiseID = "9914N";
			underbond2.OutturnStatus.Code = CMRBaseStatuses.Codes.NotSent;
			underbond3.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			underbond4.OutturnStatus.Code = CMRBaseStatuses.Codes.NotSent;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.Outturn];
			statusFilter.Property = CMRBaseStatuses.Codes.NotSent;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			Assert("Expect collection not to contain HAWB3", !filterCollection.Contains(hAWB3));
			Assert("Expect collection to contain HAWB4", filterCollection.Contains(hAWB4));
		}

		public void TestUnderbondStatusFilter()
		{
			hAWB1.CS_CustomsStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			hAWB2.CS_CustomsStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			hAWB3.CS_CustomsStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRUnderbond];
			statusFilter.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			Assert("Expect collection to contain HAWB3", filterCollection.Contains(hAWB3));
			statusFilter.Property = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2", filterCollection.Contains(hAWB2));
			Assert("Expect collection not to contain HAWB3", !filterCollection.Contains(hAWB3));
			statusFilter.Property = CMRBaseStatuses.Codes.NotSent;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2", !filterCollection.Contains(hAWB2));
			Assert("Expect collection not to contain HAWB3", !filterCollection.Contains(hAWB3));
		}

		public void TestArrivalDateFilter()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			mAWB1.CM_ArrivalDate = new ZDateTime(2000, 1, 1, 11, 0, 0); // 1 Jan 11:00
			mAWB2.CM_ArrivalDate = new ZDateTime(2000, 2, 2, 22, 0, 0); // 2 Feb 22:00
			Factory.Save();
			ModuleDateFilter arrivalDateFilter = (ModuleDateFilter)filterBO[AirCTOFilterConstants.DateFilterType.EstimatedArrival];
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.Property2 = ZDateTime.Empty;
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1.", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2.", filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = ZDateTime.Empty;
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1.", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2.", !filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1.", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2.", filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1.", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2.", !filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 2);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1.", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2.", !filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Both have dates", false, filterCollection.Contains(hAWB1));
			AssertEquals("Both have dates", false, filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Both have dates", true, filterCollection.Contains(hAWB1));
			AssertEquals("Both have dates", true, filterCollection.Contains(hAWB2));
			mAWB1.CM_ArrivalDate = ZDateTime.Empty;
			mAWB2.CM_ArrivalDate = ZDateTime.Empty;
			Factory.Save();
			arrivalDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Both have dates", true, filterCollection.Contains(hAWB1));
			AssertEquals("Both have dates", true, filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Both have dates", false, filterCollection.Contains(hAWB1));
			AssertEquals("Both have dates", false, filterCollection.Contains(hAWB2));
		}

		public void TestFirstArrivalDateFilter()
		{
			hAWB1.CS_CM = mAWB1.PK;
			hAWB2.CS_CM = mAWB2.PK;
			mAWB1.CM_DateOfFirstArrival = new ZDateTime(2000, 1, 1, 11, 0, 0); // 1 Jan 11:00
			mAWB2.CM_DateOfFirstArrival = new ZDateTime(2000, 2, 2, 22, 0, 0); // 2 Feb 22:00
			Factory.Save();
			ModuleDateFilter arrivalDateFilter = (ModuleDateFilter)filterBO[AirCTOFilterConstants.DateFilterType.DateOf1stArrival];
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.Property2 = ZDateTime.Empty;
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1.", !filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2.", filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = ZDateTime.Empty;
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1.", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2.", !filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1.", filterCollection.Contains(hAWB1));
			Assert("Expect collection to contain HAWB2.", filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1.", filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2.", !filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 2);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain HAWB1.", !filterCollection.Contains(hAWB1));
			Assert("Expect collection not to contain HAWB2.", !filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Both have dates", false, filterCollection.Contains(hAWB1));
			AssertEquals("Both have dates", false, filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Both have dates", true, filterCollection.Contains(hAWB1));
			AssertEquals("Both have dates", true, filterCollection.Contains(hAWB2));
			mAWB1.CM_DateOfFirstArrival = ZDateTime.Empty;
			mAWB2.CM_DateOfFirstArrival = ZDateTime.Empty;
			Factory.Save();
			arrivalDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Both have dates", true, filterCollection.Contains(hAWB1));
			AssertEquals("Both have dates", true, filterCollection.Contains(hAWB2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Both have dates", false, filterCollection.Contains(hAWB1));
			AssertEquals("Both have dates", false, filterCollection.Contains(hAWB2));
		}

		public void TestWeDontLoadForwarderObjects()
		{
			CusHAWB nonCTOHAWB = Factory.NewWithValidTestData<CusHAWB>();
			hAWB1.CS_CustomsStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			nonCTOHAWB.CS_CustomsStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			hAWB2.CS_CustomsStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			nonCTOHAWB.CS_CustomsStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			hAWB2.CS_CM = mAWB2.PK;
			mAWB2.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[AirCargoFilterConstants.StatusFilterType.CMRUnderbond];
			statusFilter.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain HAWB1", filterCollection.Contains(hAWB1));
			Assert("Expect collection to not contain HAWB2", !filterCollection.Contains(hAWB2));
			Assert("Expect collection not to contain nonCTOHAWB", !filterCollection.Contains(nonCTOHAWB));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AirCTOImportFilterBusinessObject();

		CTOCusMAWB mAWB1;
		CTOCusMAWB mAWB2;
		CTOCusHAWB hAWB1;
		CTOCusHAWB hAWB2;
		CTOCusHAWB hAWB3;
		CTOCusHAWB hAWB4;
		CusPartShip partShip1;
		CusUnderbond underbond1;
		CusUnderbond underbond2;
		CusUnderbond underbond3;
		CusUnderbond underbond4;
		CTOCusHAWBModuleCollection filterCollection;
		AirCTOImportFilterBusinessObject filterBO;
		protected override void SetUp()
		{
			base.SetUp();
			mAWB1 = Factory.NewWithValidTestData<CTOCusMAWB>();
			mAWB2 = Factory.NewWithValidTestData<CTOCusMAWB>();
			hAWB1 = Factory.NewWithValidTestData<CTOCusHAWB>();
			hAWB2 = Factory.NewWithValidTestData<CTOCusHAWB>();
			hAWB3 = Factory.NewWithValidTestData<CTOCusHAWB>();
			hAWB4 = Factory.NewWithValidTestData<CTOCusHAWB>();
			partShip1 = Factory.NewWithValidTestData<CusPartShip>();
			underbond1 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond2 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond3 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond4 = Factory.NewWithValidTestData<CusUnderbond>();
			filterBO = (AirCTOImportFilterBusinessObject)GetNewFilterStripBusinessObject();
			filterCollection = new CTOCusHAWBModuleCollection(Factory);
		}
	}
}
