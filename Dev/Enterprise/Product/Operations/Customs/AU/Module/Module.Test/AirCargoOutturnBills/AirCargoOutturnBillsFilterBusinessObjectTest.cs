using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AirCargoOutturnBillsFilterBusinessObject))]
	sealed class AirCargoOutturnBillsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestApplicationCodeFilter()
		{
			underbond3.C4_FlightNo = "AYYYA";
			underbond4.C4_FlightNo = "AXXXC";
			underbond4.C4_ApplicationCode = "@#@";
			Factory.Save();
			var flightNoFilter = (ModuleNumberFilter)filterBO[FilterConstants.NumberTypes.Flight];
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			flightNoFilter.Property = "a";
			flightNoFilter.IsActive = true;
			var filter = filterBO.Filter;
			AssertEquals(true, underbond3.MatchesFilter(filter));
			AssertEquals(false, underbond4.MatchesFilter(filter));
		}

		public void TestSendersMsgRefNumberFilter()
		{
			underbond1.C4_SendersMessageReference = "U00003671";
			underbond2.C4_SendersMessageReference = "U00003702";
			underbond3.C4_SendersMessageReference = "U00003714";
			underbond4.C4_SendersMessageReference = "U00003719";
			outturn1.C5_C4_Underbond = underbond1.PK;
			outturn2.C5_C4_Underbond = underbond2.PK;
			outturn3.C5_C4_Underbond = underbond3.PK;
			outturn4.C5_C4_Underbond = underbond4.PK;
			outturn1.C5_ParentID = hAWB1.PK;
			outturn2.C5_ParentID = hAWB2.PK;
			outturn3.C5_ParentID = hAWB3.PK;
			outturn4.C5_ParentID = hAWB4.PK;
			Factory.Save();
			var sendersRefNumberFilter = (ModuleNumberFilter)filterBO[FilterConstants.NumberTypes.SendersRef];
			sendersRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			sendersRefNumberFilter.Property = "U000036";
			sendersRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection not to contain Underbond4", !filterCollection.Contains(underbond4));
			sendersRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			sendersRefNumberFilter.Property = "U00003719";
			sendersRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection to contain Underbond4", filterCollection.Contains(underbond4));
			sendersRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			sendersRefNumberFilter.Property = "0036";
			sendersRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection not to contain Underbond4", !filterCollection.Contains(underbond4));
			sendersRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			sendersRefNumberFilter.Property = "ZZZ";
			sendersRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection not to contain Underbond4", !filterCollection.Contains(underbond4));
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(CusHAWB.Schema.CS_HAWBMaxLength), sendersRefNumberFilter.MaxLength);
		}

		public void TestOutturnMsgRefNumberFilter()
		{
			outturn1.C5_C4_Underbond = underbond1.PK;
			outturn2.C5_C4_Underbond = underbond2.PK;
			outturn3.C5_C4_Underbond = underbond3.PK;
			outturn4.C5_C4_Underbond = underbond4.PK;
			outturn1.C5_ParentID = hAWB1.PK;
			outturn2.C5_ParentID = hAWB2.PK;
			outturn3.C5_ParentID = hAWB3.PK;
			outturn4.C5_ParentID = hAWB4.PK;
			hAWB1.CS_MessageReference = "S1IAKL100000919";
			hAWB2.CS_MessageReference = "S1IAKL100000923";
			hAWB3.CS_MessageReference = "S1IAKL100000924";
			hAWB4.CS_MessageReference = "S1IAKL100000929";
			Factory.Save();
			var outturnSendersRefNumberFilter = (ModuleNumberFilter)filterBO[FilterConstants.NumberTypes.OutturnSendersRef];
			outturnSendersRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			outturnSendersRefNumberFilter.Property = "S1IAKL10000091";
			outturnSendersRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection not to contain Underbond4", !filterCollection.Contains(underbond4));
			outturnSendersRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			outturnSendersRefNumberFilter.Property = "S1IAKL100000929";
			outturnSendersRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection to contain Underbond4", filterCollection.Contains(underbond4));
			outturnSendersRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			outturnSendersRefNumberFilter.Property = "0000091";
			outturnSendersRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection not to contain Underbond4", !filterCollection.Contains(underbond4));
			outturnSendersRefNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			outturnSendersRefNumberFilter.Property = "ZZZ";
			outturnSendersRefNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection not to contain Underbond4", !filterCollection.Contains(underbond4));
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(CusHAWB.Schema.CS_HAWBMaxLength), outturnSendersRefNumberFilter.MaxLength);
		}

		public void TestHouseBillNumberFilter()
		{
			outturn1.C5_C4_Underbond = underbond1.PK;
			outturn2.C5_C4_Underbond = underbond2.PK;
			outturn3.C5_C4_Underbond = underbond3.PK;
			outturn4.C5_C4_Underbond = underbond4.PK;
			outturn1.C5_ParentID = hAWB1.PK;
			outturn2.C5_ParentID = hAWB2.PK;
			hAWB1.CS_HAWB = "AXXXA";
			hAWB2.CS_HAWB = "BYYYB";
			outturn3.C5_HouseBill = "AYYYA";
			outturn4.C5_HouseBill = "CXXXC";
			Factory.Save();
			ModuleNumberFilter houseBillNumberFilter = (ModuleNumberFilter)filterBO[FilterConstants.NumberTypes.Housebill];
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			houseBillNumberFilter.Property = "A";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection not to contain Underbond4", !filterCollection.Contains(underbond4));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			houseBillNumberFilter.Property = "CXXXC";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection to contain Underbond4", filterCollection.Contains(underbond4));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			houseBillNumberFilter.Property = "XXX";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection not to contain Underbond4", !filterCollection.Contains(underbond4));
			houseBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			houseBillNumberFilter.Property = "ZZZ";
			houseBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection not to contain Underbond4", !filterCollection.Contains(underbond4));
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(CusHAWB.Schema.CS_HAWBMaxLength), houseBillNumberFilter.MaxLength);
		}

		public void TestMasterBillNumberFilter()
		{
			underbond1.C4_ParentID = mAWB1.PK;
			underbond2.C4_ParentID = mAWB2.PK;
			mAWB1.CM_MAWB = "AXXXA";
			mAWB2.CM_MAWB = "BYYYB";
			underbond3.C4_MAWB = "AYYYA";
			underbond4.C4_MAWB = "CXXXC";
			Factory.Save();
			ModuleNumberFilter masterBillNumberFilter = (ModuleNumberFilter)filterBO[FilterConstants.NumberTypes.Masterbill];
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			masterBillNumberFilter.Property = "A";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection not to contain Underbond4", !filterCollection.Contains(underbond4));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			masterBillNumberFilter.Property = "CXXXC";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection to contain Underbond4", filterCollection.Contains(underbond4));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			masterBillNumberFilter.Property = "XXX";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection not to contain Underbond4", !filterCollection.Contains(underbond4));
			masterBillNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			masterBillNumberFilter.Property = "ZZZ";
			masterBillNumberFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			Assert("Expect collection not to contain Underbond4", !filterCollection.Contains(underbond4));
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(CusMAWB.Schema.CM_MAWBMaxLength), masterBillNumberFilter.MaxLength);
		}

		public void TestFlightNoFilter()
		{
			underbond1.C4_FlightNo = "XAAAX";
			underbond2.C4_FlightNo = "YAAAY";
			Factory.Save();
			ModuleNumberFilter flightNoFilter = (ModuleNumberFilter)filterBO[FilterConstants.NumberTypes.Flight];
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			flightNoFilter.Property = "XAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			flightNoFilter.Property = "YAAAY";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection to contain Underbond2", filterCollection.Contains(underbond2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "AAA";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1", filterCollection.Contains(underbond1));
			Assert("Expect collection to contain Underbond2", filterCollection.Contains(underbond2));
			flightNoFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			flightNoFilter.Property = "ZZZ";
			flightNoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
		}

		public void TestArrivalDateFilter()
		{
			underbond1.C4_ArrivalDate = new ZDateTime(2000, 1, 1, 11, 0, 0); // 1 Jan 11:00
			underbond2.C4_ArrivalDate = new ZDateTime(2000, 2, 2, 22, 0, 0); // 2 Feb 22:00
			Factory.Save();
			ModuleDateFilter arrivalDateFilter = (ModuleDateFilter)filterBO[FilterConstants.DateTypes.ArrivalDate];
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.Property2 = ZDateTime.Empty;
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1.", !filterCollection.Contains(underbond1));
			Assert("Expect collection to contain Underbond2.", filterCollection.Contains(underbond2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = ZDateTime.Empty;
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1.", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2.", !filterCollection.Contains(underbond2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 2);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1.", filterCollection.Contains(underbond1));
			Assert("Expect collection to contain Underbond2.", filterCollection.Contains(underbond2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1.", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2.", !filterCollection.Contains(underbond2));
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.Property1 = new ZDateTime(2000, 1, 2);
			arrivalDateFilter.Property2 = new ZDateTime(2000, 2, 1);
			arrivalDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1.", !filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2.", !filterCollection.Contains(underbond2));
		}

		public void TestOutturnDateFilter()
		{
			underbond1.C4_Outurned = new ZDateTime(2000, 1, 1, 11, 0, 0); // 1 Jan 11:00
			underbond2.C4_Outurned = new ZDateTime(2000, 2, 2, 22, 0, 0); // 2 Feb 22:00
			Factory.Save();
			ModuleDateFilter outturnDateFilter = (ModuleDateFilter)filterBO[FilterConstants.DateTypes.OutturnDate];
			outturnDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			outturnDateFilter.Property1 = new ZDateTime(2000, 2, 2);
			outturnDateFilter.Property2 = ZDateTime.Empty;
			outturnDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1.", !filterCollection.Contains(underbond1));
			Assert("Expect collection to contain Underbond2.", filterCollection.Contains(underbond2));
			outturnDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			outturnDateFilter.Property1 = ZDateTime.Empty;
			outturnDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			outturnDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1.", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2.", !filterCollection.Contains(underbond2));
			outturnDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			outturnDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			outturnDateFilter.Property2 = new ZDateTime(2000, 2, 2);
			outturnDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1.", filterCollection.Contains(underbond1));
			Assert("Expect collection to contain Underbond2.", filterCollection.Contains(underbond2));
			outturnDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			outturnDateFilter.Property1 = new ZDateTime(2000, 1, 1);
			outturnDateFilter.Property2 = new ZDateTime(2000, 1, 1);
			outturnDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1.", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2.", !filterCollection.Contains(underbond2));
			outturnDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			outturnDateFilter.Property1 = new ZDateTime(2000, 1, 2);
			outturnDateFilter.Property2 = new ZDateTime(2000, 2, 1);
			outturnDateFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1.", !filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2.", !filterCollection.Contains(underbond2));
		}

		public void TestStatusListProperty()
		{
			AssertEquals(29, filterBO.StatusList.Count);
		}

		public void TestJobInvoicingStatusFilter()
		{
			var job1 = new JobHeader.Loader(underbond1).TryLoadOrCreate();
			AssertNotNull(job1);
			AssertEquals(JobHeaderStatus.Working.Code, job1.JH_Status);
			Factory.Save();
			var filter = (ModuleTextBaseFilter)filterBO["Invoicing Job Status"];
			filter.Property = JobHeaderStatus.Working.Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			AssertCollectionContains(underbond1, filterCollection);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filterCollection.Load(filterBO.Filter);
			AssertCollectionNotContains(underbond1, filterCollection);
		}

		public void TestCargoStatusFilter()
		{
			outturn1.C5_C4_Underbond = underbond1.PK;
			outturn2.C5_C4_Underbond = underbond2.PK;
			outturn3.C5_C4_Underbond = underbond3.PK;
			outturn1.C5_CustomsStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			outturn2.C5_CustomsStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			outturn3.C5_CustomsStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[FilterConstants.StatusTypes.Cargo];
			statusFilter.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection to contain Underbond3", filterCollection.Contains(underbond3));
			statusFilter.Property = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection to contain Underbond2", filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			statusFilter.Property = CMRBaseStatuses.Codes.NotSent;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
		}

		public void TestOutturnStatusFilter()
		{
			entryNumber1.CE_ParentID = underbond1.PK;
			entryNumber2.CE_ParentID = underbond2.PK;
			entryNumber3.CE_ParentID = underbond3.PK;
			entryNumber1.CE_EntryType = CusEntryNumber.EntryType.OutturnStatus;
			entryNumber2.CE_EntryType = CusEntryNumber.EntryType.OutturnStatus;
			entryNumber3.CE_EntryType = CusEntryNumber.EntryType.UnderbondStatus;
			entryNumber1.CE_EntryStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			entryNumber2.CE_EntryStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			entryNumber3.CE_EntryStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			Factory.Save();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filterBO[FilterConstants.StatusTypes.Outturn];
			statusFilter.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection to contain Underbond1", filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			statusFilter.Property = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection to contain Underbond2", filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
			statusFilter.Property = CMRBaseStatuses.Codes.NotSent;
			statusFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			Assert("Expect collection not to contain Underbond1", !filterCollection.Contains(underbond1));
			Assert("Expect collection not to contain Underbond2", !filterCollection.Contains(underbond2));
			Assert("Expect collection not to contain Underbond3", !filterCollection.Contains(underbond3));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AirCargoOutturnBillsFilterBusinessObject();

		CusMAWB mAWB1;
		CusMAWB mAWB2;
		CusHAWB hAWB1;
		CusHAWB hAWB2;
		CusHAWB hAWB3;
		CusHAWB hAWB4;
		CusUnderbond underbond1;
		CusUnderbond underbond2;
		CusUnderbond underbond3;
		CusUnderbond underbond4;
		CusEntryNumber entryNumber1;
		CusEntryNumber entryNumber2;
		CusEntryNumber entryNumber3;
		Business.CusOutturn outturn1;
		Business.CusOutturn outturn2;
		Business.CusOutturn outturn3;
		Business.CusOutturn outturn4;
		AirCargoOutturnBillsFilterBusinessObject filterBO;
		AirOrStandAloneCusUnderbondCollection filterCollection;
		protected override void SetUp()
		{
			base.SetUp();
			mAWB1 = Factory.NewWithValidTestData<CusMAWB>();
			mAWB2 = Factory.NewWithValidTestData<CusMAWB>();
			hAWB1 = Factory.NewWithValidTestData<CusHAWB>();
			hAWB2 = Factory.NewWithValidTestData<CusHAWB>();
			hAWB3 = Factory.NewWithValidTestData<CusHAWB>();
			hAWB4 = Factory.NewWithValidTestData<CusHAWB>();
			underbond1 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond2 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond3 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond4 = Factory.NewWithValidTestData<CusUnderbond>();
			entryNumber1 = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber1.CE_ParentTable = "CusUnderbond";
			entryNumber2 = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber2.CE_ParentTable = "CusUnderbond";
			entryNumber3 = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber3.CE_ParentTable = "CusUnderbond";
			outturn1 = Factory.NewWithValidTestData<Business.CusOutturn>();
			outturn2 = Factory.NewWithValidTestData<Business.CusOutturn>();
			outturn3 = Factory.NewWithValidTestData<Business.CusOutturn>();
			outturn4 = Factory.NewWithValidTestData<Business.CusOutturn>();
			filterBO = (AirCargoOutturnBillsFilterBusinessObject)GetNewFilterStripBusinessObject();
			filterCollection = new AirOrStandAloneCusUnderbondCollection(Factory);
		}
	}
}
