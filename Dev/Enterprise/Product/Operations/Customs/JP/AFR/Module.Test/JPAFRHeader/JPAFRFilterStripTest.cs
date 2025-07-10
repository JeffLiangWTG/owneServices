using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Module.Testing
{
	[TestedType(typeof(JPAFRFilterStrip))]
	class JPAFRFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestDateFilters()
		{
			AssertDateFilter(JPAFRFilterStrip.FilterConstants.EstimatedTimeDeparture, JPAFRHeaderSchema.JPH_ETD);
			AssertDateFilter(JPAFRFilterStrip.FilterConstants.EstimatedTimeArrival, JPAFRHeaderSchema.JPH_ETA);
		}

		void AssertDateFilter(ZString filterName, SchemaDateTimeColumn schemaCol)
		{
			var header1 = Factory.New<JPAFRHeader>();
			header1[schemaCol] = ZDateTime.Today.AddDays(1);
			var header2 = Factory.New<JPAFRHeader>();
			header2[schemaCol] = ZDateTime.Today.AddDays(100);

			var filterObj = new JPAFRFilterStrip();
			var filter = (ModuleDateFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = ZDateTime.Today.AddDays(7);
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestTextFilters()
		{
			AssertTextFilter(JPAFRFilterStrip.FilterConstants.JobReference, JPAFRHeaderSchema.JPH_JobReference);
			AssertTextFilter(JPAFRFilterStrip.FilterConstants.CarrierCode, JPAFRHeaderSchema.JPH_CarrierCode);
			AssertTextFilter(JPAFRFilterStrip.FilterConstants.VesselName, JPAFRHeaderSchema.JPH_VesselName);
			AssertTextFilter(JPAFRFilterStrip.FilterConstants.VoyageNumber, JPAFRHeaderSchema.JPH_Voyage);
			AssertTextFilter(JPAFRFilterStrip.FilterConstants.MasterBillOfLading, JPAFRHeaderSchema.JPH_MasterBillNumber);
			AssertTextFilter(JPAFRFilterStrip.FilterConstants.MasterbillMessageStatus, JPAFRHeaderSchema.JPH_MessageStatus);
		}

		void AssertTextFilter(ZString filterName, SchemaStringColumn schemaCol)
		{
			var header1 = Factory.New<JPAFRHeader>();
			header1[schemaCol] = "1";
			var header2 = Factory.New<JPAFRHeader>();
			header2[schemaCol] = "2";

			var filterObj = new JPAFRFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestLoadDischargeFilter()
		{
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_RL_NKLoading = "AUSYD";
			header1.JPH_RL_NKDischarge = "JPTKY";
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_RL_NKLoading = "AUMEL";
			header2.JPH_RL_NKDischarge = "SGSIN";

			var filterObj = new JPAFRFilterStrip();
			var filter = (ModuleLocationFilter)filterObj[JPAFRFilterStrip.FilterConstants.LoadDischarge];
			AssertEquals("Load", filter.ItemDescription1.Caption);
			AssertEquals("Discharge", filter.ItemDescription2.Caption);
			filter.IsActive = true;

			filter.Property1 = "AU";
			filter.Property2 = ZString.Empty;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "SGSIN";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "USLAX";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(filterObj.Filter));

			filter.Property2 = "JP";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(filterObj.Filter));

			filter.Property1 = "AUMEL";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(filterObj.Filter));

			filter.Property1 = ZString.Empty;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(filterObj.Filter));
		}

		public void TestVesselCountryFilter()
		{
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_RN_NKCountryOfReg = "AU";
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_RN_NKCountryOfReg = "GB";

			var filterObj = new JPAFRFilterStrip();
			var filter = (ModuleNkFilter)filterObj[JPAFRFilterStrip.FilterConstants.VesselCountry];
			filter.IsActive = true;
			filter.Property = "AU";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
		}

		public void TestBranchFilter()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var branch2 = currentCompany.Branches.AddNew();
			branch2.GB_Code = "B#@";
			branch2.GB_BranchName = "BRANCH 2 Testing";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_GB_Branch = branch2.PK;
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_GB_Branch = GlbBranch.CurrentBranch.PK;

			var filterObj = new JPAFRFilterStrip();
			var filter = (ModuleGuidFilter)filterObj[JPAFRFilterStrip.FilterConstants.Branch];
			filter.IsActive = true;
			filter.Property = branch2.PK;
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestVesselCallSignFilter()
		{
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_VesselName = "TEST VESSEL 1";
			header1.JPH_RadioCallSign = "AB11111";
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_VesselName = "TEST VESSEL 2";
			header2.JPH_RadioCallSign = "AB22222";
			Factory.Save();

			var filterObj = new JPAFRFilterStrip();
			var filter = (ModuleTextFilter)filterObj[JPAFRFilterStrip.FilterConstants.VesselCallSign];
			filter.IsActive = true;
			filter.Property = "AB11111";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestMasterbillRegistrationCompletedFilter()
		{
			var header1 = Factory.New<JPAFRHeader>();
			header1.LogBillRegistrationCompletion();
			var header2 = Factory.New<JPAFRHeader>();
			Factory.Save();

			var filterObj = new JPAFRFilterStrip();
			var filter = (ModuleTextFilter)filterObj[JPAFRFilterStrip.FilterConstants.MasterbillRegistrationStatus];
			filter.IsActive = true;
			filter.Property = YesNoList.Codes.Yes;
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));

			filter.Property = YesNoList.Codes.No;
			Assert(!header1.MatchesFilter(filterObj.Filter));
			Assert(header2.MatchesFilter(filterObj.Filter));

			filter.Property = "D";
			Assert(!header1.MatchesFilter(filterObj.Filter));
			Assert(header2.MatchesFilter(filterObj.Filter));
		}

		public void TestHousebillMessageStatusFilter()
		{
			var header1 = Factory.New<JPAFRHeader>();
			header1.LogBillRegistrationCompletion();
			header1.Bills.AddNew().JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillAdd;
			header1.Bills.AddNew().JPB_MessageStatus = MessageStatusList.Codes.ErrorHouseBillAdd;
			var header2 = Factory.New<JPAFRHeader>();
			header2.Bills.AddNew().JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillAdd;
			header2.Bills.AddNew().JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillAdd;
			Factory.Save();

			var filterObj = new JPAFRFilterStrip();
			var filter = (ModuleTextFilter)filterObj[JPAFRFilterStrip.FilterConstants.HousebillMessageStatus];
			AssertEquals(2, filter.ComparisonOperator_List.Count);
			AssertNotNull(filter.ComparisonOperator_List.GetDescriptionFromCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			AssertNotNull(filter.ComparisonOperator_List.GetDescriptionFromCode(ModuleTextFilter.ComparisonConstants.Contains));

			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = MessageStatusList.Codes.ErrorHouseBillAdd;
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));

			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = MessageStatusList.Codes.AwaitingHouseBillAdd;
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(header2.MatchesFilter(filterObj.Filter));

			filter.IsActive = true;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.Property = MessageStatusList.Codes.AwaitingHouseBillAdd;
			Assert(!header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));

			header2.Bills.FirstOrDefault().JPB_MessageStatus = string.Empty;
			Factory.Save();
			Assert(!header1.MatchesFilter(filterObj.Filter));
			Assert(header2.MatchesFilter(filterObj.Filter));

			foreach (var bill in header2.Bills)
			{
				bill.JPB_MessageStatus = string.Empty;
			}
			Factory.Save();
			Assert(!header1.MatchesFilter(filterObj.Filter));
			Assert(header2.MatchesFilter(filterObj.Filter));
		}

		public void TestHousebillRegistrationStatusFilter()
		{
			var header1 = Factory.New<JPAFRHeader>();
			header1.LogBillRegistrationCompletion();
			header1.Bills.AddNew().JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header1.Bills.AddNew();
			var header2 = Factory.New<JPAFRHeader>();
			header2.Bills.AddNew().JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			header2.Bills.AddNew();
			Factory.Save();

			CombineAssertions(() =>
			{
				var filterObj = new JPAFRFilterStrip();
				var filter = (ModuleTextFilter)filterObj[JPAFRFilterStrip.FilterConstants.HousebillRegistrationStatus];
				AssertEquals(2, filter.ComparisonOperator_List.Count);
				AssertNotNull(filter.ComparisonOperator_List.GetDescriptionFromCode(ModuleTextFilter.ComparisonConstants.Exact));
				AssertNotNull(filter.ComparisonOperator_List.GetDescriptionFromCode(ModuleTextFilter.ComparisonConstants.Contains));

				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				filter.Property = AFRBillCustomsStatusList.Codes.NotRegistered;
				Assert("Contains NOT 1", header1.MatchesFilter(filterObj.Filter));
				Assert("Contains NOT 2", header2.MatchesFilter(filterObj.Filter));

				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				filter.Property = AFRBillCustomsStatusList.Codes.Registered;
				Assert("Contains REG 1", header1.MatchesFilter(filterObj.Filter));
				Assert("Contains REG 2", !header2.MatchesFilter(filterObj.Filter));

				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = AFRBillCustomsStatusList.Codes.NotRegistered;
				Assert("Equal NOT 1", !header1.MatchesFilter(filterObj.Filter));
				Assert("Equal NOT 2", header2.MatchesFilter(filterObj.Filter));

				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = AFRBillCustomsStatusList.Codes.Registered;
				Assert("Equal REG 1", !header1.MatchesFilter(filterObj.Filter));
				Assert("Equal REG 2", !header2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestIsShipplingLineEntryFilter()
		{
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_IsShippingLineEntry = true;
			var header2 = Factory.New<JPAFRHeader>();
			Factory.Save();

			var filterObj = new JPAFRFilterStrip();
			var filter = (ModuleTextFilter)filterObj[JPAFRFilterStrip.FilterConstants.IsShippingLineEntry];
			filter.IsActive = true;
			filter.Property = YesNoList.Codes.Yes;
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));

			filter.Property = YesNoList.Codes.No;
			Assert(!header1.MatchesFilter(filterObj.Filter));
			Assert(header2.MatchesFilter(filterObj.Filter));
		}

		public void TestGetCustomFilterStripsHelpersCore()
		{
			var header1 = Factory.New<JPAFRHeader>();
			var task1 = header1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Test Task Declaration";

			var consol = Factory.New<ForwardingConsol>();
			var task2 = consol.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "Test Task Shipment";
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_ParentId = consol.PK;

			var header3 = Factory.New<JPAFRHeader>();
			var task3 = header3.WorkflowItems.Tasks.AddNew();
			task3.P9_Description = "~Test Task Declaration";

			Factory.Save();

			var filterBizo = new JPAFRFilterStrip();
			var filter = filterBizo.AddFilterStrip<TasksModuleFilter>("Tasks");
			filter.SelectedFilters.AddTextFilterStrip("Description", "Test Task");

			var subFilterResult = Factory.Load<ProcessTask>(filter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { task1.PK, task2.PK }, subFilterResult.Select(x => x.PK));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var result = Factory.Load<JPAFRHeader>(filter.Query);
			AssertContainsExactElementsInAnyOrder("Any match: " + filter.Query.LiteralTextSqlFormatted, new[] { header1.PK, header2.PK }, result.Select(x => x.PK));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			result = Factory.Load<JPAFRHeader>(filter.Query);
			AssertContainsExactElementsInAnyOrder("None match: " + filter.Query.LiteralTextSqlFormatted, new[] { header3.PK }, result.Select(x => x.PK));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JPAFRFilterStrip();
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheck();
			result.Add(TableFilter(StmALogSchema.Constants.TableName, Enterprise.Customs.JP.AFR.Module.JPAFRFilterStrip.FilterConstants.MasterbillRegistrationStatus));
			result.Add(TableFilter(JPAFRBillsSchema.Constants.TableName, Enterprise.Customs.JP.AFR.Module.JPAFRFilterStrip.FilterConstants.HousebillMessageStatus));
			result.Add(TableFilter(JPAFRBillsSchema.Constants.TableName, Enterprise.Customs.JP.AFR.Module.JPAFRFilterStrip.FilterConstants.HousebillRegistrationStatus));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Milestone Completed"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Any Open Task Assigned To"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Next Task Assigned To"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Tasks"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Exceptions"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Milestones"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Triggers"));
			return result;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(JPAFRBillsSchema.Constants.TableName, Enterprise.Customs.JP.AFR.Module.JPAFRFilterStrip.FilterConstants.HousebillRegistrationStatus));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Any Open Task Assigned To"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Next Task Assigned To"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Tasks"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Exceptions"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Milestones"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Triggers"));
			return result;
		}
	}
}
