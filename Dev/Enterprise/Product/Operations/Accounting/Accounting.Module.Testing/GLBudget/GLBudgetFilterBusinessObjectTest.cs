using System;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GLBudgetFilterBusinessObject))]
	class GLBudgetFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public void TestYearFilterValidation()
		{
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Year"];

			filter.Property = "AAAA";
			filter.IsActive = true;

			filter.Validation.ValidateAll();
			AssertHasErrors("Format error expected", filter.PropertyInfo);

			filter.Property = "20000";
			filter.IsActive = true;

			filter.Validation.ValidateAll();
			AssertHasErrors("Format error expected", filter.PropertyInfo);

			filter.Property = "";
			filter.IsActive = true;

			filter.Validation.ValidateAll();
			AssertNoErrors("No format errors expected", filter.PropertyInfo);

			filter.Property = "2000";
			filter.IsActive = true;

			filter.Validation.ValidateAll();
			AssertNoErrors("No format errors expected", filter.PropertyInfo);
		}

		public void TestYearFilter()
		{
			Budget1.AU_Year = 2000;
			Budget2.AU_Year = 2001;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Year"];

			filter.Property = "AAAA";
			filter.IsActive = true;

			filter.Validation.ValidateAll();
			AssertHasErrors("Format error expected", filter.PropertyInfo);

			filter.Property = "20000";
			filter.IsActive = true;

			filter.Validation.ValidateAll();
			AssertHasErrors("Format error expected", filter.PropertyInfo);

			filter.Property = "2000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Budget1", FilterCollection.Contains(Budget1));
			Assert("Expecting collection not to contain Budget2", !FilterCollection.Contains(Budget2));

			filter.Property = "02001 ";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Budget1", !FilterCollection.Contains(Budget1));
			Assert("Expecting collection to contain Budget2", FilterCollection.Contains(Budget2));

			filter.Property = "2002";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Budget1", !FilterCollection.Contains(Budget1));
			Assert("Expecting collection not to contain Budget2", !FilterCollection.Contains(Budget2));
		}

		public void TestBranchManagementCodeFilter()
		{
			var codeCollection = new BranchManagementCodeDescriptionBoolCollection();
			codeCollection.Add("BRA", null, true);
			codeCollection.Add("BRB", null, true);
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeCollection);

			Branch1.GB_AccountingGroupCode = "BRA";
			Branch2.GB_AccountingGroupCode = "BRB";

			Budget1.AU_GB = Branch1.PK;
			Budget2.AU_GB = Branch2.PK;

			Factory.Save();

			var branchManagementCodeFilter = (ModuleTextFilter)FilterBO["Branch Management Code"];
			branchManagementCodeFilter.Property = "BRA";
			branchManagementCodeFilter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Collection should Contain Budget1", new[] { Budget1 }, FilterCollection);

			branchManagementCodeFilter.Property = "BRB";
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Collection should Contain Budget2", new[] { Budget2 }, FilterCollection);
		}

		public void TestBranchFilter()
		{
			Budget1.AU_GB = Branch1.PK;
			Budget2.AU_GB = Branch2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Branch"];

			filter.Property = Branch1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Budget1", FilterCollection.Contains(Budget1));
			Assert("Expecting collection not to contain Budget2", !FilterCollection.Contains(Budget2));

			filter.Property = Branch3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Budget1", !FilterCollection.Contains(Budget1));
			Assert("Expecting collection not to contain Budget2", !FilterCollection.Contains(Budget2));
		}

		public void TestDepartmentFilter()
		{
			Budget1.AU_GE = Department1.PK;
			Budget2.AU_GE = Department2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Department"];

			filter.Property = Department1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Budget1", FilterCollection.Contains(Budget1));
			Assert("Expecting collection not to contain Budget2", !FilterCollection.Contains(Budget2));

			filter.Property = Department3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Budget1", !FilterCollection.Contains(Budget1));
			Assert("Expecting collection not to contain Budget2", !FilterCollection.Contains(Budget2));
		}

		public void TestGLAccountFilter()
		{
			Budget1.AU_AG = Account1.PK;
			Budget2.AU_AG = Account2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["GL Account"];

			filter.Property = Account1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Budget1", FilterCollection.Contains(Budget1));
			Assert("Expecting collection not to contain Budget2", !FilterCollection.Contains(Budget2));

			filter.Property = Account3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Budget1", !FilterCollection.Contains(Budget1));
			Assert("Expecting collection not to contain Budget2", !FilterCollection.Contains(Budget2));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GLBudgetFilterBusinessObject();
		}

		GLBudget Budget1;
		GLBudget Budget2;

		GlbBranch Branch1;
		GlbBranch Branch2;
		GlbBranch Branch3;

		AccGLHeader Account1;
		AccGLHeader Account2;
		AccGLHeader Account3;

		GlbDepartment Department1;
		GlbDepartment Department2;
		GlbDepartment Department3;

		GLBudgetCollection FilterCollection;
		GLBudgetFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			Budget1 = Factory.NewWithValidTestData<GLBudget>();
			Budget2 = Factory.NewWithValidTestData<GLBudget>();

			Branch1 = Factory.NewWithValidTestData<GlbBranch>();
			Branch2 = Factory.NewWithValidTestData<GlbBranch>();
			Branch3 = Factory.NewWithValidTestData<GlbBranch>();

			Branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			Branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			Account1 = Factory.NewWithValidTestData<AccGLHeader>();
			Account2 = Factory.NewWithValidTestData<AccGLHeader>();
			Account3 = Factory.NewWithValidTestData<AccGLHeader>();

			Department1 = Factory.NewWithValidTestData<GlbDepartment>();
			Department2 = Factory.NewWithValidTestData<GlbDepartment>();
			Department3 = Factory.NewWithValidTestData<GlbDepartment>();

			FilterCollection = new GLBudgetCollection(Factory);
			FilterBO = (GLBudgetFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
