using System;
using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Module.Testing
{
	[TestedType(typeof(JPAFRBillFilterStrip))]
	class JPAFRBillFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestBranchFilter()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var branch2 = currentCompany.Branches.AddNew();
			branch2.GB_Code = "B#@";
			branch2.GB_BranchName = "BRANCH 2 Testing";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			var shippingLineHeader = Factory.New<JPAFRHeader>();
			shippingLineHeader.JPH_GB_Branch = branch2.PK;
			shippingLineHeader.JPH_IsShippingLineEntry = ZBool.True;
			var shippingLineBill = shippingLineHeader.Bills.AddNew();
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_IsShippingLineEntry = ZBool.False;
			header1.JPH_GB_Branch = branch2.PK;
			var header1Bill = header1.Bills.AddNew();
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_IsShippingLineEntry = ZBool.False;
			header2.JPH_GB_Branch = GlbBranch.CurrentBranch.PK;
			var header2Bill = header2.Bills.AddNew();
			Factory.Save();

			var filterObj = new JPAFRBillFilterStrip();
			var filter = (ModuleGuidFilter)filterObj[JPAFRBillFilterStrip.FilterConstants.Branch];
			filter.IsActive = true;
			filter.Property = branch2.PK;
			Assert(header1Bill.MatchesFilter(filterObj.Filter));
			Assert(!header2Bill.MatchesFilter(filterObj.Filter));
			Assert(!shippingLineBill.MatchesFilter(filterObj.Filter));
		}

		public void TestVesselCallSignFilter()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TEST VESSEL 1";
			vessel.RV_RadioCallSign = "AB11111";
			Factory.Save();

			var shippingLineHeader = Factory.New<JPAFRHeader>();
			shippingLineHeader.JPH_IsShippingLineEntry = ZBool.True;
			shippingLineHeader.JPH_VesselName = "TEST VESSEL 1";
			var shippingLineBill = shippingLineHeader.Bills.AddNew();
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_IsShippingLineEntry = ZBool.False;
			header1.JPH_VesselName = "TEST VESSEL 1";
			var header1Bill = header1.Bills.AddNew();
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_IsShippingLineEntry = ZBool.False;
			header2.JPH_VesselName = "TEST VESSEL 2";
			var header2Bill = header2.Bills.AddNew();
			Factory.Save();

			var filterObj = new JPAFRBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[JPAFRBillFilterStrip.FilterConstants.VesselCallSign];
			filter.IsActive = true;
			filter.Property = "AB11111";
			Assert(header1Bill.MatchesFilter(filterObj.Filter));
			Assert(!header2Bill.MatchesFilter(filterObj.Filter));
			Assert(!shippingLineBill.MatchesFilter(filterObj.Filter));
		}

		public void TestDateFilters()
		{
			AssertHeaderDateFilter(JPAFRBillFilterStrip.FilterConstants.EstimatedTimeArrival, JPAFRHeaderSchema.JPH_ETA);
			AssertHeaderDateFilter(JPAFRBillFilterStrip.FilterConstants.EstimatedTimeDeparture, JPAFRHeaderSchema.JPH_ETD);
			AssertHeaderDateFilter(JPAFRBillFilterStrip.FilterConstants.JobCreatedTime, JPAFRHeaderSchema.JPH_SystemCreateTimeUtc);
		}

		public void TestJobReferenceFilter()
		{
			var shippingLineHeader = Factory.New<JPAFRHeader>();
			shippingLineHeader.JPH_JobReference = "1A";
			shippingLineHeader.JPH_IsShippingLineEntry = ZBool.True;
			var shippingLineBill = shippingLineHeader.Bills.AddNew();
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_IsShippingLineEntry = ZBool.False;
			header1.JPH_JobReference = "1B";
			var header1Bill = header1.Bills.AddNew();
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_IsShippingLineEntry = ZBool.False;
			header2.JPH_JobReference = "2B";
			var header2Bill = header2.Bills.AddNew();
			Factory.Save();

			var filterObj = new JPAFRBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[JPAFRBillFilterStrip.FilterConstants.JobReference];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.StartsWith;
			filter.Property = "1";
			Assert(header1Bill.MatchesFilter(filterObj.Filter));
			Assert(!header2Bill.MatchesFilter(filterObj.Filter));
			Assert(!shippingLineBill.MatchesFilter(filterObj.Filter));
		}

		public void TestLoadDischargeFilter()
		{
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_RL_NKLoading = "AUSYD";
			header1.JPH_RL_NKDischarge = "JPTKY";
			var bill1 = header1.Bills.AddNew();
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_RL_NKLoading = "AUMEL";
			header2.JPH_RL_NKDischarge = "SGSIN";
			var bill2 = header2.Bills.AddNew();
			Factory.Save();

			var filterObj = new JPAFRBillFilterStrip();
			var filter = (ModuleLocationFilter)filterObj[JPAFRBillFilterStrip.FilterConstants.LoadDischarge];
			AssertEquals("Load", filter.ItemDescription1.Caption);
			AssertEquals("Discharge", filter.ItemDescription2.Caption);
			filter.IsActive = true;

			filter.Property1 = "AU";
			filter.Property2 = ZString.Empty;
			AssertEquals("bill1 match filter", true, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals("bill2 match filter", true, bill2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "SGSIN";
			AssertEquals("bill1 match filter", false, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals("bill2 match filter", true, bill2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "USLAX";
			AssertEquals("bill1 match filter", false, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals("bill2 match filter", false, bill2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "JP";
			AssertEquals("bill1 match filter", true, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals("bill2 match filter", false, bill2.MatchesFilter(filterObj.Filter));

			filter.Property1 = "AUMEL";
			AssertEquals("bill1 match filter", false, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals("bill2 match filter", false, bill2.MatchesFilter(filterObj.Filter));

			filter.Property1 = ZString.Empty;
			AssertEquals("bill1 match filter", true, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals("bill2 match filter", false, bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestTextFilters()
		{
			AssertHeaderTextFilter(JPAFRBillFilterStrip.FilterConstants.CarrierCode, JPAFRHeaderSchema.JPH_CarrierCode);
			AssertHeaderTextFilter(JPAFRBillFilterStrip.FilterConstants.VesselName, JPAFRHeaderSchema.JPH_VesselName);
			AssertHeaderTextFilter(JPAFRBillFilterStrip.FilterConstants.VoyageNumber, JPAFRHeaderSchema.JPH_Voyage);
			AssertHeaderTextFilter(JPAFRBillFilterStrip.FilterConstants.MasterBillOfLading, JPAFRHeaderSchema.JPH_MasterBillNumber);
			AssertHeaderTextFilter(JPAFRBillFilterStrip.FilterConstants.JobCreatedBy, JPAFRHeaderSchema.JPH_SystemCreateUser);

			AssertBillTextFilter(JPAFRBillFilterStrip.FilterConstants.BillOfLading, JPAFRBillsSchema.JPB_BillNumber);
			AssertBillTextFilter(JPAFRBillFilterStrip.FilterConstants.ReleaseStatus, JPAFRBillsSchema.JPB_ReleaseStatus);
			AssertBillTextFilter(JPAFRBillFilterStrip.FilterConstants.MessageStatus, JPAFRBillsSchema.JPB_MessageStatus);
		}

		void AssertHeaderDateFilter(ZString filterName, SchemaDateTimeColumn schemaCol)
		{
			var shippingLineHeader = Factory.New<JPAFRHeader>();
			shippingLineHeader[schemaCol] = ZDateTime.Today.AddDays(1);
			var shippingLineHeaderBill = shippingLineHeader.Bills.AddNew();
			shippingLineHeader.JPH_IsShippingLineEntry = ZBool.True;
			var header1 = Factory.New<JPAFRHeader>();
			header1[schemaCol] = ZDateTime.Today.AddDays(1);
			header1.JPH_IsShippingLineEntry = ZBool.False;
			var header1Bill = header1.Bills.AddNew();
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_IsShippingLineEntry = ZBool.False;
			header2[schemaCol] = ZDateTime.Today.AddDays(100);
			var header2Bill = header2.Bills.AddNew();
			Factory.Save();

			var filterObj = new JPAFRBillFilterStrip();
			var filter = (ModuleDateFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = ZDateTime.Today.AddDays(7);
			Assert(string.Format("Should match header1Bill for '{0}' and '{1}'", filterName, schemaCol.Name), header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header2Bill for '{0}' and '{1}'", filterName, schemaCol.Name), !header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match shippingLineHeaderBill for '{0}' and '{1}'", filterName, schemaCol.Name), !shippingLineHeaderBill.MatchesFilter(filterObj.Filter));
		}

		void AssertHeaderTextFilter(ZString filterName, SchemaStringColumn schemaCol)
		{
			var shippingLineHeader = Factory.New<JPAFRHeader>();
			shippingLineHeader[schemaCol] = "1";
			shippingLineHeader.JPH_IsShippingLineEntry = ZBool.True;
			var shippingLineBill = shippingLineHeader.Bills.AddNew();
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_IsShippingLineEntry = ZBool.False;
			header1[schemaCol] = "1";
			var header1Bill = header1.Bills.AddNew();
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_IsShippingLineEntry = ZBool.False;
			header2[schemaCol] = "2";
			var header2Bill = header2.Bills.AddNew();
			Factory.Save();

			var filterObj = new JPAFRBillFilterStrip();
			var filter = (ModuleTextBaseFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(string.Format("Should match header1Bill for '{0}' and '{1}'", filterName, schemaCol.Name), header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header2Bill for '{0}' and '{1}'", filterName, schemaCol.Name), !header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match shippingLineBill for '{0}' and '{1}'", filterName, schemaCol.Name), !shippingLineBill.MatchesFilter(filterObj.Filter));
		}

		void AssertBillTextFilter(ZString filterName, SchemaStringColumn schemaCol)
		{
			var header1 = Factory.New<JPAFRHeader>();
			var header1Bill = header1.Bills.AddNew();
			header1.JPH_IsShippingLineEntry = ZBool.False;
			header1Bill[schemaCol] = "1";
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_IsShippingLineEntry = ZBool.False;
			var header2Bill = header2.Bills.AddNew();
			header2Bill[schemaCol] = "2";
			Factory.Save();

			var filterObj = new JPAFRBillFilterStrip();
			var filter = (ModuleTextBaseFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(string.Format("Should match header1Bill for '{0}' and '{1}'", filterName, schemaCol.Name), header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header2Bill for '{0}' and '{1}'", filterName, schemaCol.Name), !header2Bill.MatchesFilter(filterObj.Filter));
		}

		public void TestHousebillRegistrationCompletedFilter()
		{
			var header1 = Factory.New<JPAFRHeader>();
			header1.LogBillRegistrationCompletion();
			var bill1 = header1.Bills.AddNew();
			var header2 = Factory.New<JPAFRHeader>();
			var bill2 = header2.Bills.AddNew();
			Factory.Save();

			var filterObj = new JPAFRBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[JPAFRBillFilterStrip.FilterConstants.HousebillRegistrationCompleted];
			filter.IsActive = true;
			filter.Property = YesNoList.Codes.Yes;
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));

			filter.Property = YesNoList.Codes.No;
			Assert(!bill1.MatchesFilter(filterObj.Filter));
			Assert(bill2.MatchesFilter(filterObj.Filter));

			filter.Property = "D";
			Assert(!bill1.MatchesFilter(filterObj.Filter));
			Assert(bill2.MatchesFilter(filterObj.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JPAFRBillFilterStrip();
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheck();
			result.Add(TableFilter(JPAFRHeaderSchema.Constants.TableName, JPAFRBillFilterStrip.FilterConstants.HousebillRegistrationCompleted));
			result.Add(TableFilter(StmALogSchema.Constants.TableName, JPAFRBillFilterStrip.FilterConstants.HousebillRegistrationCompleted));
			return result;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(JPAFRHeaderSchema.Constants.TableName, JPAFRBillFilterStrip.FilterConstants.HousebillRegistrationCompleted));
			return result;
		}
	}
}
