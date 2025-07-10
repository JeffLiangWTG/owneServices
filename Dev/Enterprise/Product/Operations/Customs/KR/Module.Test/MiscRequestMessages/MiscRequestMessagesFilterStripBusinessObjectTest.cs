using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(MiscRequestMessagesFilterStripBusinessObject))]
	sealed class MiscRequestMessagesFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new MiscRequestMessagesFilterStripBusinessObject();

		public void TestFilters()
		{
			var filter = new MiscRequestMessagesFilterStripBusinessObject();
			AssertNotNull(filter[MiscRequestMessagesFilterStripBusinessObject.Schema.JobNumber]);
			AssertNotNull(filter[MiscRequestMessagesFilterStripBusinessObject.Schema.MessageType]);
			AssertNotNull(filter[MiscRequestMessagesFilterStripBusinessObject.Schema.Status]);
			AssertNotNull(filter[MiscRequestMessagesFilterStripBusinessObject.Schema.RequestDate]);
			AssertNotNull(filter[MiscRequestMessagesFilterStripBusinessObject.Schema.CustomsOffice]);
			AssertNotNull(filter[MiscRequestMessagesFilterStripBusinessObject.Schema.CustomsBroker]);
			AssertNotNull(filter[MiscRequestMessagesFilterStripBusinessObject.Schema.Branch]);
			AssertNotNull(filter[MiscRequestMessagesFilterStripBusinessObject.Schema.EntryNumber]);
			AssertNotNull(filter[MiscRequestMessagesFilterStripBusinessObject.Schema.EntryType]);
			AssertNotNull(filter[MiscRequestMessagesFilterStripBusinessObject.Schema.ApplicationStartPeriod]);
			AssertNotNull(filter[MiscRequestMessagesFilterStripBusinessObject.Schema.ReviewDate]);
		}

		public void TestApplicationNumberFilter()
		{
			var request1 = Factory.New<Business.CusMiscRequestHeader>();
			request1.CMR_JobNumber = "JOB001";
			request1.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			request1.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			request1.CMR_RequestDate = new ZDate(2021, 01, 01);
			request1.CMR_RequestDetails = "Detail 001";
			request1.CMR_CustomsOffice = "01001";
			request1.CMR_GS_NKBroker = "AAA";
			request1.CMR_GB = GlbBranch.CurrentBranch.PK;

			var request2 = Factory.New<Business.CusMiscRequestHeader>();
			request2.CMR_JobNumber = "JOB002";
			request2.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
			request2.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			request2.CMR_RequestDate = new ZDate(2021, 02, 01);
			request2.CMR_RequestDetails = "Detail 002";
			request2.CMR_CustomsOffice = "01002";
			request2.CMR_GS_NKBroker = "BBB";
			request2.CMR_GB = GlbBranch.CurrentBranch.PK;

			var entryNum1 = request1.CreateCusEntryNumber();
			entryNum1.CE_EntryNum = "1234520000045M";

			var entryNum2 = request2.CreateCusEntryNumber();
			entryNum2.CE_EntryNum = "1234520000046M";
			Factory.Save();
			var filter = new MiscRequestMessagesFilterStripBusinessObject();
			var applicationNumberFilter = (ModuleTextFilter)filter[MiscRequestMessagesFilterStripBusinessObject.Schema.ApplicationNumber];
			applicationNumberFilter.Property = "1234520000045M";
			applicationNumberFilter.IsActive = true;

			var coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("1234520000045M", coll.Cast<Business.CusMiscRequestHeader>().First().CusEntryNumber.CE_EntryNum);
			AssertEquals("12345-20-000045M", coll.Cast<Business.CusMiscRequestHeader>().First().FormattedApplicationNumber);

			applicationNumberFilter.Property = "12345200000";
			applicationNumberFilter.IsActive = true;
			coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(2, coll.Count);
			AssertEquals("1234520000045M", coll.Cast<Business.CusMiscRequestHeader>().First().CusEntryNumber.CE_EntryNum);
			AssertEquals("1234520000046M", coll.Cast<Business.CusMiscRequestHeader>().Last().CusEntryNumber.CE_EntryNum);
		}

		public void TestJobNumberFilter()
		{
			SetupData();
			var filter = new MiscRequestMessagesFilterStripBusinessObject();
			var jobNumberFilter = (ModuleTextFilter)filter[MiscRequestMessagesFilterStripBusinessObject.Schema.JobNumber];
			jobNumberFilter.Property = "JOB001";
			jobNumberFilter.IsActive = true;

			var coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("JOB001", coll.Cast<CusMiscRequestHeader>().First().CMR_JobNumber);
		}

		public void TestMessageTypeFilter()
		{
			SetupData();
			var filter = new MiscRequestMessagesFilterStripBusinessObject();
			var messageTypeFilter = (ModuleTextFilter)filter[MiscRequestMessagesFilterStripBusinessObject.Schema.MessageType];
			messageTypeFilter.Property = ElectronicDocumentTypeList.Codes._5AC;
			messageTypeFilter.IsActive = true;

			var coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("5AC", coll.Cast<CusMiscRequestHeader>().First().CMR_MessageType);
		}

		public void TestStatusFilter()
		{
			SetupData();
			var filter = new MiscRequestMessagesFilterStripBusinessObject();
			var statusFilter = (ModuleTextFilter)filter[MiscRequestMessagesFilterStripBusinessObject.Schema.Status];
			statusFilter.Property = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			statusFilter.IsActive = true;

			var coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("ORJ", coll.Cast<CusMiscRequestHeader>().First().CMR_Status);
		}

		public void TestRequestDateFilter()
		{
			SetupData();
			var filter = new MiscRequestMessagesFilterStripBusinessObject();
			var requestDateFilter = (ModuleDateFilter)filter[MiscRequestMessagesFilterStripBusinessObject.Schema.RequestDate];
			requestDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			requestDateFilter.Property2 = new ZDateTime(2021, 1, 30);
			requestDateFilter.IsActive = true;
			var coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(new ZDateTime(2021, 1, 1), coll.Cast<CusMiscRequestHeader>().First().CMR_RequestDate);
		}
		public void TestApplicationStartPeriodFilter()
		{
			var request1 = Factory.New<Business.CusMiscRequestHeader>();
			request1.CMR_JobNumber = "JOB001";
			request1.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			request1.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			request1.CMR_RequestDate = new ZDate(2022, 01, 01);
			request1.CMR_RequestDetails = "Detail 001";
			request1.CMR_CustomsOffice = "01001";
			request1.CMR_GS_NKBroker = "AAA";
			request1.CMR_GB = GlbBranch.CurrentBranch.PK;

			var request2 = Factory.New<Business.CusMiscRequestHeader>();
			request2.CMR_JobNumber = "JOB002";
			request2.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
			request2.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			request2.CMR_RequestDate = new ZDate(2022, 02, 01);
			request2.CMR_RequestDetails = "Detail 002";
			request2.CMR_CustomsOffice = "01002";
			request2.CMR_GS_NKBroker = "BBB";
			request2.CMR_GB = GlbBranch.CurrentBranch.PK;

			var request3 = Factory.New<Business.CusMiscRequestHeader>();
			request3.CMR_JobNumber = "JOB003";
			request3.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;
			request3.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			request3.CMR_RequestDate = new ZDate(2022, 02, 01);
			request3.CMR_RequestDetails = "Detail 003";
			request3.CMR_CustomsOffice = "01003";
			request3.CMR_GS_NKBroker = "CCC";
			request3.CMR_GB = GlbBranch.CurrentBranch.PK;

			var entryNum1 = request1.CreateCusEntryNumber();
			entryNum1.CE_EntryNum = "1234520000045M";
			entryNum1.CE_IssueDate = new ZDate(2021, 01, 01);

			var entryNum2 = request2.CreateCusEntryNumber();
			entryNum2.CE_EntryNum = "1234520000046M";
			entryNum2.CE_IssueDate = new ZDate(2021, 02, 01);

			var entryNum3 = request3.CreateCusEntryNumber();
			entryNum3.CE_EntryNum = "1234520000047M";
			entryNum3.CE_IssueDate = new ZDate(2021, 02, 01);
			Factory.Save();

			var filter = new MiscRequestMessagesFilterStripBusinessObject();
			var periodFilter = (ModuleDateFilter)filter[MiscRequestMessagesFilterStripBusinessObject.Schema.ApplicationStartPeriod];
			periodFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			periodFilter.Property1 = new ZDateTime(2021, 1, 30);
			periodFilter.IsActive = true;
			var coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(new ZDateTime(2021, 2, 1), coll.Cast<Business.CusMiscRequestHeader>().First().CusEntryNumber.CE_IssueDate);
			AssertEquals("01002", coll.Cast<Business.CusMiscRequestHeader>().First().CMR_CustomsOffice);

			filter = new MiscRequestMessagesFilterStripBusinessObject();
			periodFilter = (ModuleDateFilter)filter[MiscRequestMessagesFilterStripBusinessObject.Schema.ReviewDate];
			periodFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			periodFilter.Property1 = new ZDateTime(2021, 1, 30);
			periodFilter.IsActive = true;
			coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(new ZDateTime(2021, 2, 1), coll.Cast<Business.CusMiscRequestHeader>().First().CusEntryNumber.CE_IssueDate);
			AssertEquals("01003", coll.Cast<Business.CusMiscRequestHeader>().First().CMR_CustomsOffice);
		}

		public void TestCustomsOfficeFilter()
		{
			var request = Factory.New<CusMiscRequestHeader>();
			request.CMR_JobNumber = "JOB005";
			request.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			request.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			request.CMR_RequestDate = new ZDate(2021, 01, 01);
			request.CMR_RequestDetails = "Detail 001";
			request.CMR_CustomsOffice = "02001";
			request.CMR_GS_NKBroker = "AAA";
			request.CMR_GB = GlbBranch.CurrentBranch.PK;
			SetupData();

			var filter = new MiscRequestMessagesFilterStripBusinessObject();
			var statementProcessPortFilter = (ModuleNkFilter)filter[MiscRequestMessagesFilterStripBusinessObject.Schema.CustomsOffice];
			statementProcessPortFilter.Property = "010";
			statementProcessPortFilter.IsActive = true;
			var coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(4, coll.Count);
			AssertEquals("01001", coll.Cast<CusMiscRequestHeader>().FirstOrDefault(x => x.CMR_CustomsOffice == "01001").CMR_CustomsOffice);
			AssertEquals("01002", coll.Cast<CusMiscRequestHeader>().FirstOrDefault(x => x.CMR_CustomsOffice == "01002").CMR_CustomsOffice);
			AssertEquals("01003", coll.Cast<CusMiscRequestHeader>().FirstOrDefault(x => x.CMR_CustomsOffice == "01003").CMR_CustomsOffice);
			AssertEquals("01004", coll.Cast<CusMiscRequestHeader>().FirstOrDefault(x => x.CMR_CustomsOffice == "01004").CMR_CustomsOffice);

			statementProcessPortFilter.Property = "020";
			statementProcessPortFilter.IsActive = true;
			coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("02001", coll.Cast<CusMiscRequestHeader>().First().CMR_CustomsOffice);

			statementProcessPortFilter.Property = "030";
			statementProcessPortFilter.IsActive = true;
			coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(0, coll.Count);

			statementProcessPortFilter.Property = "040";
			statementProcessPortFilter.IsActive = false;
			coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(5, coll.Count);
		}

		public void TestCustomsBrokerFilter()
		{
			SetupData();
			var filter = new MiscRequestMessagesFilterStripBusinessObject();
			var customsBrokerFilter = (ModuleNkFilter)filter[MiscRequestMessagesFilterStripBusinessObject.Schema.CustomsBroker];
			customsBrokerFilter.Property = "CCC";
			customsBrokerFilter.IsActive = true;

			var coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("CCC", coll.Cast<CusMiscRequestHeader>().First().CMR_GS_NKBroker);
		}

		public void TestShowOnlyCurrentBranchRequests()
		{
			var branch1 = GlbCompany.CurrentCompany.Branches.AddNew();
			var request1 = Factory.New<CusMiscRequestHeader>();
			request1.CMR_GB = branch1.PK;
			var branch2 = GlbCompany.CurrentCompany.Branches.AddNew();
			var request2 = Factory.New<CusMiscRequestHeader>();
			request2.CMR_GB = branch2.PK;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch3 = company.Branches.AddNew();
			var request3 = Factory.New<CusMiscRequestHeader>();
			request3.CMR_GB = branch3.PK;

			var filterBizObj = new MiscRequestMessagesFilterStripBusinessObject();
			AssertEquals(true, request1.MatchesFilter(filterBizObj.Filter));
			AssertEquals(true, request2.MatchesFilter(filterBizObj.Filter));
			AssertEquals(false, request3.MatchesFilter(filterBizObj.Filter));
		}

		public void TestEntryTypeFilter()
		{
			SetupData();
			var filter = new MiscRequestMessagesFilterStripBusinessObject();
			var entryTypeFilter = (ModuleTextFilter)filter[MiscRequestMessagesFilterStripBusinessObject.Schema.EntryType];
			entryTypeFilter.Property = "IMP";
			entryTypeFilter.IsActive = true;

			var coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("JOB001", coll.Cast<CusMiscRequestHeader>().First().CMR_JobNumber);
		}

		public void TestEntryNumberFilter()
		{
			SetupData();
			var filter = new MiscRequestMessagesFilterStripBusinessObject();
			var entryNumberFilter = (ModuleTextFilter)filter[MiscRequestMessagesFilterStripBusinessObject.Schema.EntryNumber];
			entryNumberFilter.Property = "ENT002";
			entryNumberFilter.IsActive = true;

			var coll = new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("JOB002", coll.Cast<CusMiscRequestHeader>().First().CMR_JobNumber);
		}

		void SetupData()
		{
			var request1 = Factory.New<CusMiscRequestHeader>();
			request1.CMR_JobNumber = "JOB001";
			request1.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			request1.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			request1.CMR_RequestDate = new ZDate(2021, 01, 01);
			request1.CMR_RequestDetails = "Detail 001";
			request1.CMR_CustomsOffice = "01001";
			request1.CMR_GS_NKBroker = "AAA";
			request1.CMR_GB = GlbBranch.CurrentBranch.PK;
			var requestLine1 = Factory.New<CusMiscRequestLine>();
			requestLine1.CML_CMR = request1.PK;
			requestLine1.CML_EntryNumber = "ENT001";
			requestLine1.CML_EntryType = "IMP";
			var request2 = Factory.New<CusMiscRequestHeader>();
			request2.CMR_JobNumber = "JOB002";
			request2.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
			request2.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			request2.CMR_RequestDate = new ZDate(2021, 02, 01);
			request2.CMR_RequestDetails = "Detail 002";
			request2.CMR_CustomsOffice = "01002";
			request2.CMR_GS_NKBroker = "BBB";
			request2.CMR_GB = GlbBranch.CurrentBranch.PK;
			var requestLine2 = Factory.New<CusMiscRequestLine>();
			requestLine2.CML_CMR = request2.PK;
			requestLine2.CML_EntryNumber = "ENT002";
			requestLine2.CML_EntryType = "EXP";
			var request3 = Factory.New<CusMiscRequestHeader>();
			request3.CMR_JobNumber = "JOB003";
			request3.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			request3.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			request3.CMR_RequestDate = new ZDate(2021, 03, 01);
			request3.CMR_RequestDetails = "Detail 003";
			request3.CMR_CustomsOffice = "01003";
			request3.CMR_GS_NKBroker = "CCC";
			request3.CMR_GB = GlbBranch.CurrentBranch.PK;
			var request4 = Factory.New<CusMiscRequestHeader>();
			request4.CMR_JobNumber = "JOB004";
			request4.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;
			request4.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			request4.CMR_RequestDate = new ZDate(2021, 03, 01);
			request4.CMR_RequestDetails = "Detail 004";
			request4.CMR_CustomsOffice = "01004";
			request4.CMR_GS_NKBroker = "DDD";
			request4.CMR_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
		}
	}
}
