using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Business.ModuleFilterWithListAndComparisonOperators<CargoWise.Types.ZString>;

namespace Enterprise.Customs.EU.ExitControl.Module.Testing
{
	[TestedType(typeof(ExitControlReportFilterBusinessObject))]
	sealed class ExitControlReportFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestReportTypeFilterProperties()
		{
			var entryTypeFilter = (ModuleTextFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.Type];
			entryTypeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Report Type", entryTypeFilter.Description);
				AssertEquals("Category", FilterCategories.ModesAndTypes, entryTypeFilter.Category);
				AssertArrayEqualsByElements("ComparisonOperators", new[] { ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact }, entryTypeFilter.ComparisonOperator_List.GetAllCodes());
				AssertEquals("CodesAsString", new ExitReportTypeList().CodesAsString, ((CodeDescriptionPairList)entryTypeFilter.List).CodesAsString);
			});
		}

		public void TestReportTypeFilter()
		{
			var report1 = Factory.New<CusExitReport>();
			report1.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			var report2 = Factory.New<CusExitReport>();
			report2.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			var report3 = Factory.New<CusExitReport>();
			report3.CER_Type = ExitReportTypeList.Codes.Presentation;

			var entryTypeFilter = (ModuleTextFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.Type];
			entryTypeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				entryTypeFilter.Property = ExitReportTypeList.Codes.ExitNotification;
				AssertMatch("EXT", true, false, false);

				entryTypeFilter.Property = ExitReportTypeList.Codes.InformationOnNonExitedExport;
				AssertMatch("ALT", false, true, false);

				entryTypeFilter.Property = ExitReportTypeList.Codes.Presentation;
				AssertMatch("PRE", false, false, true);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->report1", match1, report1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->report2", match2, report2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->report3", match3, report3.MatchesFilter(filter.Filter));
			}
		}

		public void TestExitConsignmentFilterProperties()
		{
			var consignmentFilter = (ModuleTextFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.EntryConsignment];
			consignmentFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Entry/Consignment", consignmentFilter.Description);
				AssertEquals("Category", FilterCategories.NumbersAndReferences, consignmentFilter.Category);
			});
		}

		public void TestExitConsignmentFilter()
		{
			var report1 = GetExitReportForTesting();
			report1.Consignment.CXC_MovementReference = "DE172821";
			var report2 = GetExitReportForTesting();
			report2.Consignment.CXC_MovementReference = "DE932878";
			var report3 = GetExitReportForTesting();
			report3.Consignment.CXC_MovementReference = "AU718221";

			Factory.Save();

			var consignment = (ModuleTextFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.EntryConsignment];
			consignment.IsActive = true;

			CombineAssertions(() =>
			{
				consignment.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				consignment.Property = "DE932878";
				AssertMatch("Exact", false, true, false);

				consignment.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				consignment.Property = "DE";
				AssertMatch("StartsWith", true, true, false);

				consignment.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				consignment.Property = "21";
				AssertMatch("Contains", true, false, true);

				consignment.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				consignment.Property = "DE932878";
				AssertMatch("NotEqual", true, false, true);

				consignment.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				consignment.Property = "DE";
				AssertMatch("NotStartsWith", false, false, true);

				consignment.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				consignment.Property = "17";
				AssertMatch("NotContain", false, true, true);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->report1", match1, report1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->report2", match2, report2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->report3", match3, report3.MatchesFilter(filter.Filter));
			}
		}

		public void TestStatusFilterProperties()
		{
			var statusFilter = (ModuleNkFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.Status];
			statusFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Status", statusFilter.Description);
				AssertEquals("Category", FilterCategories.StatusAndFlags, statusFilter.Category);
				AssertArrayEqualsByElements("ComparisonOperators", new[] {
					ComparisonConstants.Exact,
					ComparisonConstants.NotEqual,
					ComparisonConstants.IsBlank,
					ComparisonConstants.IsNotBlank,
					ComparisonConstants.FiltersMatch,
				}, statusFilter.ComparisonOperator_List.GetAllCodes());
				AssertEquals("MaxLength", CusExitReportSchema.CER_Status.MaxLength, statusFilter.MaxLength);
			});
		}

		public void TestStatusFilter()
		{
			var report1 = Factory.New<CusExitReport>();
			report1.CER_Status = "AB";
			var report2 = Factory.New<CusExitReport>();
			report2.CER_Status = "CD";

			var entryTypeFilter = (ModuleNkFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.Status];
			entryTypeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				entryTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				entryTypeFilter.Property = "AB";
				AssertMatch("AB", true, false);

				entryTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				entryTypeFilter.Property = "CD";
				AssertMatch("CD", true, false);
			});

			void AssertMatch(ZString message, bool match1, bool match2)
			{
				AssertEquals(message + "->report1", match1, report1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->report2", match2, report2.MatchesFilter(filter.Filter));
			}
		}

		public void TestMessageStatusFilterProperties()
		{
			var messageStatusFilter = (ModuleTextFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.MessageStatus];
			messageStatusFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Message Status", messageStatusFilter.Description);
				AssertEquals("Category", FilterCategories.StatusAndFlags, messageStatusFilter.Category);
				AssertEquals("CodesAsString", new LogicalStatusList().CodesAsString, ((CodeDescriptionPairList)messageStatusFilter.List).CodesAsString);
			});
		}

		public void TestMessageStatusFilter()
		{
			var report1 = Factory.New<CusExitReport>();
			report1.CER_MessageStatus = LogicalStatusList.Codes.Accepted;
			var report2 = Factory.New<CusExitReport>();
			report2.CER_MessageStatus = LogicalStatusList.Codes.Sent;

			var entryTypeFilter = (ModuleTextFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.MessageStatus];
			entryTypeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				entryTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				entryTypeFilter.Property = LogicalStatusList.Codes.Accepted;
				AssertMatch("ACC", true, false);

				entryTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				entryTypeFilter.Property = LogicalStatusList.Codes.Sent;
				AssertMatch("SNT", true, false);
			});

			void AssertMatch(ZString message, bool match1, bool match2)
			{
				AssertEquals(message + "->report1", match1, report1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->report2", match2, report2.MatchesFilter(filter.Filter));
			}
		}

		public void TestOfficeOfExitFilterProperties()
		{
			var officeOfExitFilter = (ModuleNkFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.OfficeOfExit];
			officeOfExitFilter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals("Description", "Office Of Exit", officeOfExitFilter.Description);
				AssertEquals("Category", FilterCategories.Locations, officeOfExitFilter.Category);
			});
		}

		public void TestOfficeOfExitFilter()
		{
			var report1 = Factory.New<CusExitReport>();
			report1.CER_OfficeOfExit = "1500";
			var report2 = Factory.New<CusExitReport>();
			report2.CER_OfficeOfExit = "2000";

			var entryTypeFilter = (ModuleNkFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.OfficeOfExit];
			entryTypeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				entryTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				entryTypeFilter.Property = "1500";
				AssertMatch("1500", true, false);

				entryTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				entryTypeFilter.Property = "2000";
				AssertMatch("2000", true, false);
			});

			void AssertMatch(ZString message, bool match1, bool match2)
			{
				AssertEquals(message + "->report1", match1, report1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->report2", match2, report2.MatchesFilter(filter.Filter));
			}
		}

		public void TestJobNumberFilterProperties()
		{
			var jobNumberFilter = (ModuleTextFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.JobNumber];
			jobNumberFilter.IsActive = true;
			var comparisonOperatorList = jobNumberFilter.ComparisonOperator_List.GetAllCodes();

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Job Number", jobNumberFilter.Description);
				AssertEquals("Category", FilterCategories.NumbersAndReferences, jobNumberFilter.Category);
				AssertEquals("MaxLength", CusExitHeaderSchema.CXH_JobReference.MaxLength, jobNumberFilter.MaxLength);
				AssertCollectionNotContains("No IsBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank, comparisonOperatorList);
				AssertCollectionNotContains("No IsNotBlank", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank, comparisonOperatorList);
			});
		}

		public void TestJobNumberFilter()
		{
			var report1 = GetExitReportForTesting();
			report1.Header.CXH_JobReference = "DE172821";
			var report2 = GetExitReportForTesting();
			report2.Header.CXH_JobReference = "DE932878";
			var report3 = GetExitReportForTesting();
			report3.Header.CXH_JobReference = "AU718221";

			Factory.Save();

			var jobNumberFilter = (ModuleTextFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.JobNumber];
			jobNumberFilter.IsActive = true;

			CombineAssertions(() =>
			{
				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				jobNumberFilter.Property = "DE932878";
				AssertMatch("Exact", false, true, false);

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				jobNumberFilter.Property = "DE";
				AssertMatch("StartsWith", true, true, false);

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				jobNumberFilter.Property = "21";
				AssertMatch("Contains", true, false, true);

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				jobNumberFilter.Property = "DE932878";
				AssertMatch("NotEqual", true, false, true);

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				jobNumberFilter.Property = "DE";
				AssertMatch("NotStartsWith", false, false, true);

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				jobNumberFilter.Property = "17";
				AssertMatch("NotContain", false, true, true);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->header1", match1, report1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header2", match2, report2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->header3", match3, report3.MatchesFilter(filter.Filter));
			}
		}

		public void TestJobNumberFilter_MultipleNumbers()
		{
			var report1 = GetExitReportForTesting();
			report1.Header.CXH_JobReference = "E0172821";
			var report2 = GetExitReportForTesting();
			report2.Header.CXH_JobReference = "E0932878";
			var report3 = GetExitReportForTesting();
			report3.Header.CXH_JobReference = "B0718221";

			Factory.Save();

			BusinessObject[] filteredDecs = null;

			var jobNumberFilter = (ModuleTextFilter)filter[ExitControlReportFilterBusinessObject.FilterConstants.JobNumber];
			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			jobNumberFilter.IsActive = true;
			jobNumberFilter.Property = " E0172821 , E0932878 ";
			filteredDecs = Factory.Load(typeof(CusExitReport), filter.Filter);
			Assert("Should find 2 records but found " + filteredDecs.Length, filteredDecs.Length == 2);

			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			jobNumberFilter.Property = " E0172 , E0932 ";
			filteredDecs = Factory.Load(typeof(CusExitReport), filter.Filter);
			Assert("Should find 2 records but found " + filteredDecs.Length, filteredDecs.Length == 2);

			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			jobNumberFilter.Property = " 21 , 78 ";
			filteredDecs = Factory.Load(typeof(CusExitReport), filter.Filter);
			Assert("Should find 3 records but found " + filteredDecs.Length, filteredDecs.Length == 3);

			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			jobNumberFilter.Property = " EE , B ";
			filteredDecs = Factory.Load(typeof(CusExitReport), filter.Filter);
			Assert("Should find 1 record but found " + filteredDecs.Length, filteredDecs.Length == 1);
		}

		public void TestBranchFilterProperties()
		{
			var branchFilter = (ModuleGuidFilter)filter[ExitControlFilterBusinessObject.FilterConstants.Branch];
			branchFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Branch", branchFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, branchFilter.Category);
			});
		}

		public void TestBranchFilter()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var report1 = GetExitReportForTesting();
			report1.Header.CXH_GB_Branch = branch1.PK;

			var report2 = GetExitReportForTesting();
			report2.Header.CXH_GB_Branch = branch1.Company.Branches.AddNew().PK;

			Factory.Save();

			var branchFilter = (ModuleGuidFilter)filter[ExitControlFilterBusinessObject.FilterConstants.Branch];
			branchFilter.IsActive = true;

			branchFilter.Property = branch1.PK;
			AssertEquals(true, report1.MatchesFilter(filter.Filter));
			AssertEquals(false, report2.MatchesFilter(filter.Filter));
		}

		public void TestCarrierFilterProperties()
		{
			var carrierFilter = (ModuleGuidFilter)filter[ExitControlFilterBusinessObject.FilterConstants.Carrier];
			carrierFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Carrier", carrierFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, carrierFilter.Category);
			});
		}

		public void TestCarrierFilter()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var report1 = GetExitReportForTesting();
			report1.Header.CXH_OA_Carrier = carrier1.MainAddress.PK;

			var report2 = GetExitReportForTesting();
			report2.Header.CXH_OA_Carrier = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			Factory.Save();
			var carrierFilter = (ModuleGuidFilter)filter[ExitControlFilterBusinessObject.FilterConstants.Carrier];
			carrierFilter.IsActive = true;
			carrierFilter.Property = carrier1.PK;

			AssertEquals(true, report1.MatchesFilter(filter.Filter));
			AssertEquals(false, report2.MatchesFilter(filter.Filter));
		}

		public void TestExporterFilterProperties()
		{
			var exporterFilter = (ModuleGuidFilter)filter[ExitControlFilterBusinessObject.FilterConstants.Exporter];
			exporterFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Exporter", exporterFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, exporterFilter.Category);
			});
		}

		public void TestExporterFilter()
		{
			var exporter1 = Factory.NewWithValidTestData<OrgHeader>();
			var report1 = GetExitReportForTesting();
			report1.Header.CXH_OH_Exporter = exporter1.PK;

			var report2 = GetExitReportForTesting();
			report2.Header.CXH_OH_Exporter = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.Save();
			var exporterFilter = (ModuleGuidFilter)filter[ExitControlFilterBusinessObject.FilterConstants.Exporter];
			exporterFilter.IsActive = true;
			exporterFilter.Property = exporter1.PK;

			AssertEquals(true, report1.MatchesFilter(filter.Filter));
			AssertEquals(false, report2.MatchesFilter(filter.Filter));
		}

		public CusExitReport GetExitReportForTesting()
		{
			var header1 = Factory.NewWithValidTestData<CusExitHeader>();
			header1.CXH_JobReference = header1.PK.ToString().Substring(0, 35);
			var consignment1 = header1.CusExitConsignments.AddNew();

			var report1 = Factory.NewWithValidTestData<CusExitReport>();
			report1.CER_CXC_Consignment = consignment1.PK;
			report1.CER_OfficeOfExit = "DE001";
			report1.CER_CXH_Header = header1.PK;

			return report1;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ExitControlReportFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filter = new ExitControlReportFilterBusinessObject();
		}
		ExitControlReportFilterBusinessObject filter;
	}
}
