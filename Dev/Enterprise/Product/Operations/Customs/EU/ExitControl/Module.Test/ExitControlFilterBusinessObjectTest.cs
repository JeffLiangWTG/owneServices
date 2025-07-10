using System;
using System.Collections.Generic;
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

namespace Enterprise.Customs.EU.ExitControl.Module.Testing
{
	[TestedType(typeof(ExitControlFilterBusinessObject))]
	sealed class ExitControlFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestReferenceNumberFilterProperties()
		{
			var messageStatusFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.RegistrationNumberExt];
			messageStatusFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Registration Number (ext.)", messageStatusFilter.Description);
				AssertEquals("Category", FilterCategories.NumbersAndReferences, messageStatusFilter.Category);
			});
		}

		public void TestReferenceNumberFilter()
		{
			CusExitHeader SetUpHeader(string number)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				cons.CXC_ReferenceNumber = number;
				return header;
			}
			var header1 = SetUpHeader("AH3");
			var header2 = SetUpHeader("ADD");
			var header3 = SetUpHeader("OTH");

			Factory.Save();

			var moduleFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.RegistrationNumberExt];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				moduleFilter.Property = "ADD";
				AssertExitHeadersMatchFilter("Exact", (header1, false), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("Contains", (header1, true), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				moduleFilter.Property = "AH3";
				AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, true), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("NotStartsWith", (header1, false), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("NotContain", (header1, false), (header2, true), (header3, false));
			});
		}

		public void TestMessageStatusFilterProperties()
		{
			var messageStatusFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.MessageStatus];
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
			CusExitHeader SetUpHeader(string status)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				var report = header.CusExitReports.AddNew();
				report.CER_CXC_Consignment = cons.PK;
				report.CER_MessageStatus = status;
				return header;
			}
			var header1 = SetUpHeader("ACC");
			var header2 = SetUpHeader("ACC");
			var header3 = SetUpHeader("FAL");

			Factory.Save();

			var moduleFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.MessageStatus];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				moduleFilter.Property = "ACC";
				AssertExitHeadersMatchFilter("Exact", (header1, true), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("Contains", (header1, true), (header2, true), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				moduleFilter.Property = "ACC";
				AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				moduleFilter.Property = "F";
				AssertExitHeadersMatchFilter("NotStartsWith", (header1, true), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("NotContain", (header1, false), (header2, false), (header3, false));
			});
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
			CusExitHeader SetUpHeader(string reference)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				var report = header.CusExitReports.AddNew();
				report.CER_CXC_Consignment = cons.PK;
				cons.CXC_MovementReference = reference;
				return header;
			}
			var header1 = SetUpHeader("DE172821");
			var header2 = SetUpHeader("DE932878");
			var header3 = SetUpHeader("AU718221");

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
				AssertEquals(message + "->report1", match1, header1.MatchesFilter(filter.Filter));
				AssertEquals(message + "->report2", match2, header2.MatchesFilter(filter.Filter));
				AssertEquals(message + "->report3", match3, header3.MatchesFilter(filter.Filter));
			}
		}

		public void TestJobNumberFilterProperties()
		{
			var jobNumberFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.JobNumber];
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
			var header1 = Factory.New<CusExitHeader>();
			header1.CXH_JobReference = "DE172821";
			var header2 = Factory.New<CusExitHeader>();
			header2.CXH_JobReference = "DE932878";
			var header3 = Factory.New<CusExitHeader>();
			header3.CXH_JobReference = "AU718221";

			var jobNumberFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.JobNumber];
			jobNumberFilter.IsActive = true;

			CombineAssertions(() =>
			{
				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				jobNumberFilter.Property = "DE932878";
				AssertExitHeadersMatchFilter("Exact", (header1, false), (header2, true), (header3, false));

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				jobNumberFilter.Property = "DE";
				AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, true), (header3, false));

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				jobNumberFilter.Property = "21";
				AssertExitHeadersMatchFilter("Contains", (header1, true), (header2, false), (header3, true));

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				jobNumberFilter.Property = "DE932878";
				AssertExitHeadersMatchFilter("NotEqual", (header1, true), (header2, false), (header3, true));

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				jobNumberFilter.Property = "DE";
				AssertExitHeadersMatchFilter("NotStartsWith", (header1, false), (header2, false), (header3, true));

				jobNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				jobNumberFilter.Property = "17";
				AssertExitHeadersMatchFilter("NotContain", (header1, false), (header2, true), (header3, true));
			});
		}

		public void TestJobNumberFilter_MultipleNumbers()
		{
			var header1 = Factory.New<CusExitHeader>();
			header1.CXH_JobReference = "E0172821";
			var header2 = Factory.New<CusExitHeader>();
			header2.CXH_JobReference = "E0932878";
			var header3 = Factory.New<CusExitHeader>();
			header3.CXH_JobReference = "B0718221";

			Factory.Save();

			BusinessObject[] filteredDecs = null;

			CombineAssertions(() =>
			{
				var jobNumberFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.JobNumber];
				jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				jobNumberFilter.IsActive = true;
				jobNumberFilter.Property = " E0172821 , E0932878 ";
				filteredDecs = Factory.Load(typeof(CusExitHeader), filter.Filter);
				Assert("Should find 2 records but found " + filteredDecs.Length, filteredDecs.Length == 2);

				jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				jobNumberFilter.Property = " E0172 , E0932 ";
				filteredDecs = Factory.Load(typeof(CusExitHeader), filter.Filter);
				Assert("Should find 2 records but found " + filteredDecs.Length, filteredDecs.Length == 2);

				jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				jobNumberFilter.Property = " 21 , 78 ";
				filteredDecs = Factory.Load(typeof(CusExitHeader), filter.Filter);
				Assert("Should find 3 records but found " + filteredDecs.Length, filteredDecs.Length == 3);

				jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				jobNumberFilter.Property = " EE , B ";
				filteredDecs = Factory.Load(typeof(CusExitHeader), filter.Filter);
				Assert("Should find 1 record but found " + filteredDecs.Length, filteredDecs.Length == 1);
			});
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
			AssertOrganisationFilter(ExitControlFilterBusinessObject.FilterConstants.Carrier, header => header.CXH_OA_CarrierInfo);
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
			AssertOrganisationFilter(ExitControlFilterBusinessObject.FilterConstants.Exporter, header => header.CXH_OH_ExporterInfo, true);
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
			var company = GlbCompany.CurrentCompany;

			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			branch1.GB_BranchName = "Test Branch 1";

			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "BR2";
			branch2.GB_RL_NKHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			branch2.GB_BranchName = "Test Branch 2";

			company.Factory.Save();

			var header1 = GetNewCusExitHeader();
			header1.CXH_GB_Branch = branch1.PK;
			var header2 = GetNewCusExitHeader();
			header2.CXH_GB_Branch = branch2.PK;

			Factory.Save();

			var branchFilter = (ModuleGuidFilter)filter[ExitControlFilterBusinessObject.FilterConstants.Branch];
			branchFilter.IsActive = true;

			branchFilter.Property = branch1.PK;
			AssertEquals(true, header1.MatchesFilter(filter.Filter));
			AssertEquals(false, header2.MatchesFilter(filter.Filter));

			branchFilter.Property = branch2.PK;
			AssertEquals(false, header1.MatchesFilter(filter.Filter));
			AssertEquals(true, header2.MatchesFilter(filter.Filter));
		}

		public void TestStatusFilter()
		{
			CusExitHeader SetUpHeader(string status)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				var report = header.CusExitReports.AddNew();
				report.CER_CXC_Consignment = cons.PK;
				report.CER_Status = status;
				return header;
			}
			var header1 = SetUpHeader("ADD");
			var header2 = SetUpHeader("ADD");
			var header3 = SetUpHeader("RED");

			Factory.Save();

			var moduleFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.Status];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				moduleFilter.Property = "ADD";
				AssertExitHeadersMatchFilter("Exact", (header1, true), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				moduleFilter.Property = "D";
				AssertExitHeadersMatchFilter("Contains", (header1, true), (header2, true), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				moduleFilter.Property = "ADD";
				AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				moduleFilter.Property = "R";
				AssertExitHeadersMatchFilter("NotStartsWith", (header1, true), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				moduleFilter.Property = "D";
				AssertExitHeadersMatchFilter("NotContain", (header1, false), (header2, false), (header3, false));
			});
		}

		public void TestDiscrepanciesFilter()
		{
			CusExitHeader SetUpHeader(string behavior)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				var report = header.CusExitReports.AddNew();
				report.CER_CXC_Consignment = cons.PK;
				report.CER_Behavior = behavior;
				return header;
			}
			var header1 = SetUpHeader("DIS");
			var header2 = SetUpHeader("STD");

			Factory.Save();

			var moduleFilter = (ModuleFlagsFilter)filter[ExitControlFilterBusinessObject.FilterConstants.Discrepancies];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.Property0 = true;
				AssertExitHeadersMatchFilter("True", (header1, true), (header2, false));

				moduleFilter.Property0 = false;
				AssertExitHeadersMatchFilter("False", (header1, true), (header2, true));
			});
		}

		public void TestContainerNumberFilter()
		{
			CusExitHeader SetUpHeader(string number, bool isEquipment)
			{
				var header = Factory.New<CusExitHeader>();
				var container = header.CusExitContainers.AddNew();
				container.CXN_ContainerNumber = number;
				container.CXN_IsEquipment = isEquipment;
				return header;
			}
			var header1 = SetUpHeader("AH3", false);
			var header2 = SetUpHeader("AH3", true);
			var header3 = SetUpHeader("OTH", false);

			Factory.Save();

			var moduleFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.ContainerNumber];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				moduleFilter.Property = "AH3";
				AssertExitHeadersMatchFilter("Exact", (header1, true), (header2, false), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, false), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("Contains", (header1, true), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				moduleFilter.Property = "AH3";
				AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("NotStartsWith", (header1, false), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("NotContain", (header1, false), (header2, false), (header3, false));
			});
		}

		public void TestSealNumberFilter()
		{
			CusExitHeader SetUpHeader(string number)
			{
				var header = Factory.New<CusExitHeader>();
				var container = header.CusExitContainers.AddNew();
				var seal = container.AllSealNumbers.AddNew();
				seal.BK_SealNumber = number;
				return header;
			}
			var header1 = SetUpHeader("AH3");
			var header2 = SetUpHeader("ADD");
			var header3 = SetUpHeader("OTH");

			Factory.Save();

			var moduleFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.SealNumber];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				moduleFilter.Property = "ADD";
				AssertExitHeadersMatchFilter("Exact", (header1, false), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("Contains", (header1, true), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				moduleFilter.Property = "AH3";
				AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, true), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("NotStartsWith", (header1, false), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("NotContain", (header1, false), (header2, true), (header3, false));
			});
		}

		public void TestMovementReferenceNumberFilter()
		{
			CusExitHeader SetUpHeader(string number)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				cons.CXC_MovementReference = number;
				return header;
			}
			var header1 = SetUpHeader("AH3");
			var header2 = SetUpHeader("ADD");
			var header3 = SetUpHeader("OTH");

			Factory.Save();

			var moduleFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.MovementReferenceNumber];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				moduleFilter.Property = "ADD";
				AssertExitHeadersMatchFilter("Exact", (header1, false), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("Contains", (header1, true), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				moduleFilter.Property = "AH3";
				AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, true), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("NotStartsWith", (header1, false), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("NotContain", (header1, false), (header2, true), (header3, false));
			});
		}

		public void TestReferenceNumberUCRFilter()
		{
			CusExitHeader SetUpHeader(string number, ZString itemNumber)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				cons.CXC_UniqueConsignmentReference = number;
				if (!itemNumber.IsEmpty)
				{
					var consItem = cons.CusExitConsignmentItems.AddNew();
					consItem.CCI_LineNumber = 1;
					consItem.CCI_UniqueConsignmentReference = itemNumber;
				}
				return header;
			}
			var header1 = SetUpHeader("AH3", ZString.Empty);
			var header2 = SetUpHeader("ADD", ZString.Empty);
			var header3 = SetUpHeader("OTH", ZString.Empty);
			var header4 = SetUpHeader("DD2", "AH3");

			Factory.Save();

			var moduleFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.ReferenceNumberUCR];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				moduleFilter.Property = "ADD";
				AssertExitHeadersMatchFilter("Exact", (header1, false), (header2, true), (header3, false), (header4, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, true), (header3, false), (header4, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("Contains", (header1, true), (header2, false), (header3, true), (header4, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				moduleFilter.Property = "AH3";
				AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, true), (header3, true), (header4, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("NotStartsWith", (header1, false), (header2, false), (header3, true), (header4, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("NotContain", (header1, false), (header2, true), (header3, false), (header4, true));
			});
		}

		public void TestLocationFilter()
		{
			CusExitHeader SetUpHeader(string location)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				var report = header.CusExitReports.AddNew();
				report.CER_CXC_Consignment = cons.PK;
				report.CER_Location = location;
				return header;
			}
			var header1 = SetUpHeader("AH3");
			var header2 = SetUpHeader("ADD");
			var header3 = SetUpHeader("OTH");

			Factory.Save();

			var moduleFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.Location];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				moduleFilter.Property = "ADD";
				AssertExitHeadersMatchFilter("Exact", (header1, false), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("Contains", (header1, true), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				moduleFilter.Property = "AH3";
				AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, true), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("NotStartsWith", (header1, false), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("NotContain", (header1, false), (header2, true), (header3, false));
			});
		}

		public void TestOfficeOfExitFilter()
		{
			CusExitHeader SetUpHeader(string officeOfExit)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				var report = header.CusExitReports.AddNew();
				report.CER_CXC_Consignment = cons.PK;
				report.CER_OfficeOfExit = officeOfExit;
				return header;
			}
			var header1 = SetUpHeader("AH3");
			var header2 = SetUpHeader("ADD");
			var header3 = SetUpHeader("OTH");

			Factory.Save();

			var moduleFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.OfficeOfExit];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				moduleFilter.Property = "ADD";
				AssertExitHeadersMatchFilter("Exact", (header1, false), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("Contains", (header1, true), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				moduleFilter.Property = "AH3";
				AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, true), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("NotStartsWith", (header1, false), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("NotContain", (header1, false), (header2, true), (header3, false));
			});
		}

		public void TestBrokerFilter()
		{
			CusExitHeader SetUpHeader(string agent)
			{
				var header = Factory.New<CusExitHeader>();
				header.CXH_GS_NKCustomsAgent = agent;
				return header;
			}
			var header1 = SetUpHeader("AH3");
			var header2 = SetUpHeader("ADD");
			var header3 = SetUpHeader("OTH");

			Factory.Save();

			var moduleFilter = (ModuleNkFilter)filter[ExitControlFilterBusinessObject.FilterConstants.Broker];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
				moduleFilter.Property = "ADD";
				AssertExitHeadersMatchFilter("Exact", (header1, false), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.NotEqual;
				moduleFilter.Property = "AH3";
				AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, true), (header3, true));
			});
		}

		public void TestModeOfTransportFilter()
		{
			CusExitHeader SetUpHeader(string transport)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				var report = header.CusExitReports.AddNew();
				report.CER_CXC_Consignment = cons.PK;
				report.CER_TransportMode = transport;
				return header;
			}
			var header1 = SetUpHeader("AIR");
			var header2 = SetUpHeader("SEA");
			var header3 = SetUpHeader("ROA");

			Factory.Save();

			var moduleFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.ModeOfTransport];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				moduleFilter.Property = "SEA";
				AssertExitHeadersMatchFilter("Exact", (header1, false), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, false), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				moduleFilter.Property = "R";
				AssertExitHeadersMatchFilter("Contains", (header1, true), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				moduleFilter.Property = "AIR";
				AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, true), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("NotStartsWith", (header1, false), (header2, true), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				moduleFilter.Property = "R";
				AssertExitHeadersMatchFilter("NotContain", (header1, false), (header2, true), (header3, false));
			});
		}

		public void TestTransportIDFilter()
		{
			CusExitHeader SetUpHeader(string transport)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				var report = header.CusExitReports.AddNew();
				report.CER_CXC_Consignment = cons.PK;
				report.CER_TransportID = transport;
				return header;
			}
			var header1 = SetUpHeader("AH3");
			var header2 = SetUpHeader("ADD");
			var header3 = SetUpHeader("OTH");

			Factory.Save();

			var moduleFilter = (ModuleTextFilter)filter[ExitControlFilterBusinessObject.FilterConstants.TransportID];
			moduleFilter.IsActive = true;

			CombineAssertions(() =>
			{
				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				moduleFilter.Property = "ADD";
				AssertExitHeadersMatchFilter("Exact", (header1, false), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, true), (header3, false));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("Contains", (header1, true), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				moduleFilter.Property = "AH3";
				AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, true), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				moduleFilter.Property = "A";
				AssertExitHeadersMatchFilter("NotStartsWith", (header1, false), (header2, false), (header3, true));

				moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				moduleFilter.Property = "H";
				AssertExitHeadersMatchFilter("NotContain", (header1, false), (header2, true), (header3, false));
			});
		}

		public void TestArrivalDateFilter()
		{
			CusExitHeader SetUpHeader(ZDateTimeOffset date)
			{
				var header = Factory.New<CusExitHeader>();
				var cons = header.CusExitConsignments.AddNew();
				var report = header.CusExitReports.AddNew();
				report.CER_CXC_Consignment = cons.PK;
				report.CER_DateTime = date;
				return header;
			}
			var header1 = SetUpHeader(new ZDateTimeOffset(2022, 1, 1, 10, 0, 0));
			var header2 = SetUpHeader(new ZDateTimeOffset(2022, 1, 1, 10, 0, 0));
			var header3 = SetUpHeader(new ZDateTimeOffset(2021, 2, 16, 15, 30, 0));

			Factory.Save();

			var moduleFilter = (ModuleDateFilter)filter[ExitControlFilterBusinessObject.FilterConstants.ArrivalDate];
			moduleFilter.IsActive = true;
			moduleFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			CombineAssertions(() =>
			{
				moduleFilter.Property1 = new ZDateTime(2022, 1, 1, 10, 0, 0);
				moduleFilter.Property2 = new ZDateTime(2022, 1, 1, 10, 0, 0);
				AssertExitHeadersMatchFilter("Date in 2022", (header1, true), (header2, true), (header3, false));

				moduleFilter.Property1 = new ZDateTime(2021, 2, 16, 15, 30, 0);
				moduleFilter.Property2 = new ZDateTime(2021, 2, 16, 15, 30, 0);
				AssertExitHeadersMatchFilter("Date in 2021", (header1, false), (header2, false), (header3, true));

				moduleFilter.Property1 = new ZDateTime(2020, 5, 26, 18, 15, 0);
				moduleFilter.Property2 = new ZDateTime(2020, 5, 26, 18, 15, 0);
				AssertExitHeadersMatchFilter("Date in 2020", (header1, false), (header2, false), (header3, false));
			});
		}

		void AssertExitHeadersMatchFilter(ZString message, params (CusExitHeader header, bool expected)[] headerMatches)
		{
			var headerIndex = 1;

			foreach (var (header, expected) in headerMatches)
			{
				AssertEquals($"{message}->header{headerIndex++}", expected, header.MatchesFilter(filter.Filter));
			}
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, ExitControlFilterBusinessObject.FilterConstants.Carrier));

			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, ExitControlFilterBusinessObject.FilterConstants.Carrier));

			result.Add(TableFilter(CusExitConsignmentSchema.Constants.TableName, ExitControlFilterBusinessObject.FilterConstants.ReferenceNumberUCR));
			result.Add(TableFilter(CusExitConsignmentItemSchema.Constants.TableName, ExitControlFilterBusinessObject.FilterConstants.ReferenceNumberUCR));

			result.Add(TableFilter(CusExitConsignmentSchema.Constants.TableName, ExitControlFilterBusinessObject.FilterConstants.EntryConsignment));

			result.Add(TableFilter(CusExitContainerSchema.Constants.TableName, ExitControlFilterBusinessObject.FilterConstants.ContainerNumber));
			result.Add(TableFilter(CusExitContainerSchema.Constants.TableName, ExitControlFilterBusinessObject.FilterConstants.SealNumber));
			return result;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheck();
			result.Add(TableFilter(CusExitConsignmentSchema.Constants.TableName, ExitControlFilterBusinessObject.FilterConstants.ReferenceNumberUCR));
			result.Add(TableFilter(CusExitConsignmentItemSchema.Constants.TableName, ExitControlFilterBusinessObject.FilterConstants.ReferenceNumberUCR));

			result.Add(TableFilter(CusExitConsignmentSchema.Constants.TableName, ExitControlFilterBusinessObject.FilterConstants.EntryConsignment));

			result.Add(TableFilter(CusExitContainerSchema.Constants.TableName, ExitControlFilterBusinessObject.FilterConstants.ContainerNumber));
			result.Add(TableFilter(CusExitContainerSchema.Constants.TableName, ExitControlFilterBusinessObject.FilterConstants.SealNumber));
			result.Add(TableFilter(CusSealSchema.Constants.TableName, ExitControlFilterBusinessObject.FilterConstants.SealNumber));
			return result;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ExitControlFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filter = new ExitControlFilterBusinessObject();
		}
		ExitControlFilterBusinessObject filter;

		void AssertOrganisationFilter(string filterName, Func<CusExitHeader, ZPropertyInfo> addressGetter, bool isOrgHeader = false)
		{
			var (header1, org1) = GetNewCusExitHeader("DE172821", addressGetter, isOrgHeader);
			var (header2, org2) = GetNewCusExitHeader("DE932878", addressGetter, isOrgHeader);
			Factory.Save();

			var addressFilter = (ModuleGuidFilter)filter[filterName];
			addressFilter.IsActive = true;

			CombineAssertions(() =>
			{
				addressFilter.Property = org1.PK;
				AssertExitHeadersMatchFilter("DE172821", (header1, true), (header2, false));

				addressFilter.Property = org2.PK;
				AssertExitHeadersMatchFilter("DE932878", (header1, false), (header2, true));

				var filterStrip2 = filter.FilterStrips.AddNew();
				filterStrip2.FilterDescription = filterName;
				var addressFilter2 = (ModuleGuidFilter)filter[filterName + " (1)"];
				addressFilter2.IsActive = true;
				addressFilter2.Property = org1.PK;
				AssertExitHeadersMatchFilter("DE172821 AND DE932878", (header1, false), (header2, false));

				addressFilter.OrCategory = FilterOrCategory.Blue;
				addressFilter2.OrCategory = FilterOrCategory.Blue;
				AssertExitHeadersMatchFilter("DE172821 OR DE932878", (header1, true), (header2, true));
			});
		}

		CusExitHeader GetNewCusExitHeader()
		{
			var header = Factory.New<CusExitHeader>();
			header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
			return header;
		}

		(CusExitHeader, OrgHeader) GetNewCusExitHeader(string orgCode, Func<CusExitHeader, ZPropertyInfo> addressGetter, bool isOrgHeader)
		{
			var header = GetNewCusExitHeader();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			var addressProperty = addressGetter(header);
			addressProperty.Value = isOrgHeader ? orgHeader.PK : orgAddress.PK;

			return (header, orgHeader);
		}
	}
}
