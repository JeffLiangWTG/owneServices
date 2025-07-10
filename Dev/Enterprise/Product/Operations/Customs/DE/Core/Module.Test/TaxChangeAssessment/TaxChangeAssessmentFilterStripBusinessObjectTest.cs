using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(TaxChangeAssessmentFilterStripBusinessObject))]
	class TaxChangeAssessmentFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCreatedTimeFilterProperties()
		{
			filter.QueryObjectType = typeof(TaxChangeAssessment);
			var createdTimeFilter = (ModuleDateFilter)filter[FilterDescriptions.CreatedTime];
			CombineAssertions(() =>
			{
				AssertEquals("AlwaysVisible", FilterVisibility.AlwaysVisible, createdTimeFilter.Visibility);
				AssertEquals("Default Value", ModuleDateFilter.DateRangeSearchTexts.Last3Mths, createdTimeFilter.PropertySearch);
			});
		}

		public void TestTypeFilter()
		{
			var taxChangeAssessment = Factory.New<TaxChangeAssessment>();
			taxChangeAssessment.CreateStmNote("Type1", TaxChangeAssessment.Schema.TaxChangeAssessmentType);
			var taxChangeAssessment2 = Factory.New<TaxChangeAssessment>();
			taxChangeAssessment2.CreateStmNote("Type2", TaxChangeAssessment.Schema.TaxChangeAssessmentType);
			Factory.Save();

			var typeFilter = (ModuleTextFilter)filter[TaxChangeAssessmentFilterStripBusinessObject.Schema.Type];
			typeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			typeFilter.Property = "Type1";
			typeFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("taxChangeAssessment", true, taxChangeAssessment.MatchesFilter(filter.Filter));
				AssertEquals("taxChangeAssessment2", false, taxChangeAssessment2.MatchesFilter(filter.Filter));
			});
		}

		public void TestTypeFilterProperties()
		{
			var typeFilter = (ModuleTextFilter)filter[TaxChangeAssessmentFilterStripBusinessObject.Schema.Type];
			CombineAssertions(() =>
			{
				AssertEquals("Description", "Type", typeFilter.Description);
				AssertEquals("Category", FilterCategories.ModesAndTypes, typeFilter.Category);
			});
		}

		public void TestReferenceNumberFilter()
		{
			var taxChangeAssessment = Factory.New<TaxChangeAssessment>();
			CreateCusEntryNumber(taxChangeAssessment, "DEMRN123");
			var taxChangeAssessment2 = Factory.New<TaxChangeAssessment>();
			CreateCusEntryNumber(taxChangeAssessment, "DEMRN456");
			Factory.Save();

			var referenceNumberFilter = (ModuleTextFilter)filter[TaxChangeAssessmentFilterStripBusinessObject.Schema.ReferenceNumber];
			referenceNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			referenceNumberFilter.Property = "DEMRN123";
			referenceNumberFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("taxChangeAssessment", true, taxChangeAssessment.MatchesFilter(filter.Filter));
				AssertEquals("taxChangeAssessment2", false, taxChangeAssessment2.MatchesFilter(filter.Filter));
			});
		}

		public void TestReferenceNumberFilterProperties()
		{
			var referenceNumberFilter = (ModuleTextFilter)filter[TaxChangeAssessmentFilterStripBusinessObject.Schema.ReferenceNumber];
			CombineAssertions(() =>
			{
				AssertEquals("Description", "Reference Number", referenceNumberFilter.Description);
				AssertEquals("Category", FilterCategories.NumbersAndReferences, referenceNumberFilter.Category);
				AssertEquals("MaxLength", CusEntryNumSchema.CE_EntryNum.MaxLength, referenceNumberFilter.MaxLength);
				AssertEquals("SupportsBlankComparisonOperators", false, referenceNumberFilter.SupportsBlankComparisonOperators);
			});
		}

		public void TestLRNFilter()
		{
			var taxChangeAssessment = Factory.New<TaxChangeAssessment>();
			taxChangeAssessment.CreateStmNote("B00000001", TaxChangeAssessment.Schema.LocalReferenceNumber);
			var taxChangeAssessment2 = Factory.New<TaxChangeAssessment>();
			taxChangeAssessment2.CreateStmNote("B00000002", TaxChangeAssessment.Schema.LocalReferenceNumber);
			Factory.Save();

			var lrnFilter = (ModuleTextFilter)filter[TaxChangeAssessmentFilterStripBusinessObject.Schema.LRN];
			lrnFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			lrnFilter.Property = "B00000001";
			lrnFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("taxChangeAssessment", true, taxChangeAssessment.MatchesFilter(filter.Filter));
				AssertEquals("taxChangeAssessment2", false, taxChangeAssessment2.MatchesFilter(filter.Filter));
			});
		}

		public void TestLRNFilterProperties()
		{
			var lrnFilter = (ModuleTextFilter)filter[TaxChangeAssessmentFilterStripBusinessObject.Schema.LRN];
			CombineAssertions(() =>
			{
				AssertEquals("Description", "LRN", lrnFilter.Description);
				AssertEquals("Category", FilterCategories.NumbersAndReferences, lrnFilter.Category);
			});
		}

		public void TestIssueDateFilter()
		{
			var taxChangeAssessment = Factory.New<TaxChangeAssessment>();
			CreateCusEntryNumber(taxChangeAssessment, "DEMRN123", issueDate: new DateTime(2020, 02, 15));
			var taxChangeAssessment2 = Factory.New<TaxChangeAssessment>();
			CreateCusEntryNumber(taxChangeAssessment, "DEMRN456", issueDate: new DateTime(2021, 02, 15));
			Factory.Save();

			var issueDateFilter = (ModuleDateFilter)filter[TaxChangeAssessmentFilterStripBusinessObject.Schema.IssueDate];
			issueDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			issueDateFilter.Property1 = new ZDate(2020, 02, 01);
			issueDateFilter.Property2 = new ZDate(2020, 02, 28);
			issueDateFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("taxChangeAssessment", true, taxChangeAssessment.MatchesFilter(filter.Filter));
				AssertEquals("taxChangeAssessment2", false, taxChangeAssessment2.MatchesFilter(filter.Filter));
			});
		}

		public void TestIssueDateProperties()
		{
			var issueDateFilter = (ModuleDateFilter)filter[TaxChangeAssessmentFilterStripBusinessObject.Schema.IssueDate];
			CombineAssertions(() =>
			{
				AssertEquals("Description", "Issue Date", issueDateFilter.Description);
				AssertEquals("Category", FilterCategories.Dates, issueDateFilter.Category);
			});
		}

		public void TestMaturityDateFilter()
		{
			var taxChangeAssessment = Factory.New<TaxChangeAssessment>();
			CreateCusEntryNumber(taxChangeAssessment, "DEMRN123", new DateTime(2020, 02, 15));
			var taxChangeAssessment2 = Factory.New<TaxChangeAssessment>();
			CreateCusEntryNumber(taxChangeAssessment, "DEMRN456", new DateTime(2021, 02, 15));
			Factory.Save();

			var maturityDateFilter = (ModuleDateFilter)filter[TaxChangeAssessmentFilterStripBusinessObject.Schema.MaturityDate];
			maturityDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			maturityDateFilter.Property1 = new ZDate(2020, 02, 01);
			maturityDateFilter.Property2 = new ZDate(2020, 02, 28);
			maturityDateFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("taxChangeAssessment", true, taxChangeAssessment.MatchesFilter(filter.Filter));
				AssertEquals("taxChangeAssessment2", false, taxChangeAssessment2.MatchesFilter(filter.Filter));
			});
		}

		public void TestMaturityDateFilterProperties()
		{
			var maturityDateFilter = (ModuleDateFilter)filter[TaxChangeAssessmentFilterStripBusinessObject.Schema.MaturityDate];
			CombineAssertions(() =>
			{
				AssertEquals("Description", "Maturity Date", maturityDateFilter.Description);
				AssertEquals("Category", FilterCategories.Dates, maturityDateFilter.Category);
			});
		}

		public void TestBranchFilter()
		{
			var branch = Factory.New<GlbBranch>();
			GlbCompany.CurrentCompany.Branches.Add(branch);
			var taxChangeAssessment = Factory.New<TaxChangeAssessment>();
			var taxChangeAssessment2 = Factory.New<TaxChangeAssessment>();
			taxChangeAssessment2.EM_GB = branch.PK;
			Factory.Save();

			var branchFilter = (ModuleGuidFilter)filter[TaxChangeAssessmentFilterStripBusinessObject.Schema.Branch];
			branchFilter.Property = branch.PK;
			branchFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("taxChangeAssessment", false, taxChangeAssessment.MatchesFilter(filter.Filter));
				AssertEquals("taxChangeAssessment2", true, taxChangeAssessment2.MatchesFilter(filter.Filter));
			});
		}

		public void TestBranchFilterProperties()
		{
			var branchFilter = (ModuleGuidFilter)filter[TaxChangeAssessmentFilterStripBusinessObject.Schema.Branch];
			CombineAssertions(() =>
			{
				AssertEquals("Description", "Branch", branchFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, branchFilter.Category);
				AssertEquals("SupportsBlankComparisonOperators", false, branchFilter.SupportsBlankComparisonOperators);
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new TaxChangeAssessmentFilterStripBusinessObject();

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var exclusions = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			exclusions.Add(TableFilter(StmNoteSchema.Constants.TableName, TaxChangeAssessmentFilterStripBusinessObject.Schema.Type));
			exclusions.Add(TableFilter(StmNoteSchema.Constants.TableName, TaxChangeAssessmentFilterStripBusinessObject.Schema.LRN));
			exclusions.Add(TableFilter(AutoEDIMessage.Schema.TableName, TaxChangeAssessmentFilterStripBusinessObject.Schema.Type));
			exclusions.Add(TableFilter(AutoEDIMessage.Schema.TableName, TaxChangeAssessmentFilterStripBusinessObject.Schema.LRN));
			return exclusions;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var exclusions = base.GetFiltersExcludedFromSubgroupCheck();
			exclusions.Add(TableFilter(StmNoteSchema.Constants.TableName, TaxChangeAssessmentFilterStripBusinessObject.Schema.Type));
			exclusions.Add(TableFilter(StmNoteSchema.Constants.TableName, TaxChangeAssessmentFilterStripBusinessObject.Schema.LRN));
			exclusions.Add(TableFilter(AutoEDIMessage.Schema.TableName, TaxChangeAssessmentFilterStripBusinessObject.Schema.Type));
			exclusions.Add(TableFilter(AutoEDIMessage.Schema.TableName, TaxChangeAssessmentFilterStripBusinessObject.Schema.LRN));
			return exclusions;
		}

		protected override void SetUp()
		{
			base.SetUp();

			filter = new TaxChangeAssessmentFilterStripBusinessObject();
		}
		TaxChangeAssessmentFilterStripBusinessObject filter;

		CusEntryNumber CreateCusEntryNumber(TaxChangeAssessment taxChangeAssessment, string entryNum, DateTime expiryDate = default, DateTime issueDate = default)
		{
			var cusEntryNumber = CusEntryNumber.New(taxChangeAssessment, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNumber.CE_EntryNum = entryNum;
			cusEntryNumber.CE_ExpiryDate = expiryDate;
			cusEntryNumber.CE_IssueDate = issueDate;
			return cusEntryNumber;
		}
	}
}
