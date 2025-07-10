using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Business.ModuleFilterWithListAndComparisonOperators<CargoWise.Types.ZGuid>;

namespace Enterprise.Accounting.Module.Testing
{
	public class AccountingFilterStripCreatorTestImplementationTest<FilteredBOType> :
			Assertion,
			IAccountingFilterStripTestImplementation<FilteredBOType>
			where FilteredBOType : BusinessObject
	{
		public void SetTestDataSupplier(IAccountingFilterStripTestDataSupplier<FilteredBOType> testDataSupplier)
		{
			TestDataSupplier = testDataSupplier;
		}

		public void TestControlForAmountFilters()
		{
			using (ZChildForm form = new ZChildForm())
			{
				ZFilterStripControl filterControl = (ZFilterStripControl)TestModule.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				ZFilterStrip filterStrip = (ZFilterStrip)filterControl.Controls["FilterStripsPanel"].Controls[0];
				filterStrip.CurrentDataItem.FilterDescription = "Job Cost Amount"; //Any Amount Filter with JobManagementAmountFilterControl
				bool isControlExist = false;
				foreach (Control control in filterStrip.Controls)
				{
					if (control.GetType() == typeof(JobManagementAmountFilterControl))
					{
						isControlExist = true;
						break;
					}
				}
				Assert("JobManagementAmountFilterControl must be used for Amount filters.", isControlExist);
			}
		}

		#region TestAddJobManagementFiltersOnlyIfSecurityIsAllowed

		public void TestAddJobManagementFiltersOnlyIfSecurityIsAllowed()
		{
			SecurityCheckpoint jobManagementSecurity;
			ZForm lastShownForm;
			var businessObjectForFilterCollection = TestDataSupplier.GetNewBusinessObjectForFilterCollection();
			var jobParent = TestDataSupplier.GetJobParent(businessObjectForFilterCollection);
			if (TestDataSupplier.ControllerIDForBillingIfDifferFromModuleController != null)
			{
				Factory.Save();
				lastShownForm = (ZForm)ZControllerFactory.Create(TestDataSupplier.ControllerIDForBillingIfDifferFromModuleController).ShowViewForm((BusinessObject)jobParent);
			}
			else
			{
				Factory.Save();
				IFilterGridModuleInternalsForTesting testModuleInternals = TestModule;
				testModuleInternals.ShowViewForm(businessObjectForFilterCollection);
				lastShownForm = (ZForm)testModuleInternals.LastController.LastShownForm;
			}
			using (lastShownForm)
			{
				var plugin = lastShownForm.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				if (plugin == null)
				{
					using (var customForm = (ZForm)TestDataSupplier.GetCustomFormWithJobInvoicing())
					{
						AssertNotNull("Module form doesn't have JobInvoicing plugin. GetCustomFormWithJobInvoicing must be overriden to get right form to check plugin security checkpoint.",
							customForm);
						plugin = customForm.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
						AssertNotNull("A form in GetCustomFormWithJobInvoicing must have JobInvoicing plugin.", plugin);
					}
				}
				jobManagementSecurity = plugin.SecurityCheckpoint;
				var invSupporter = ((IJobInvoicingPlugIn)jobParent).InvoicingSupporter;
				AssertEquals($"Security checkpoint for JobInvoicing plugin on form {lastShownForm.GetType()} and {invSupporter.GetType()}.JobInvoicingSecurity must be the same.", invSupporter.JobInvoicingSecurity, jobManagementSecurity);
			}

			jobManagementSecurity.IsAllowed = false;
			FilterBO.LoadLayout(null);
			AssertJobManagementFilters(false);

			jobManagementSecurity.IsAllowed = true;
			FilterBO.LoadLayout(null);
			AssertJobManagementFilters(true);
		}

		void AssertJobManagementFilters(bool mustBeAdded)
		{
			var filterList = new List<string>(new[]
			{
				"Job Close",
				"Job Open",
				"Job Open or Close",
				"Job Revenue Recognition Date",
				"Job Branch",
				"Job Department",
				"Job Operation Staff",
				"Job Sales Staff",
				"Job Accrual Amount",
				"Job Cost Amount",
				"Job Branch Management Code"
			});

			if (TestDataSupplier.IsRevenueFiltersAdded)
			{
				filterList.AddRange(new[]
				{
					"Job Revenue Amount",
					"Job Margin %",
					"Job Profit Amount",
					"Job WIP Amount",
					"Job WIP Amount (Excluding Deferred Charges)",
					"Job WIP Amount (Deferred Charges Only)"
				});
			}

			CombineAssertions(delegate()
			{
				foreach (string filter in filterList)
				{
					if (mustBeAdded)
					{
						AssertNotNull(string.Format("'{0}' filter must be added.", filter), FilterBO[filter]);
					}
					else
					{
						AssertNull(string.Format("'{0}' filter must not be added.", filter), FilterBO[filter]);
					}
				}
			});
		}

		#endregion

		#region Web Tests

		public void TestIsPublishedOnWeb()
		{
			var filtersNotPublished = new List<string>();
			filtersNotPublished.Add("AP Invoice #");
			filtersNotPublished.Add("Charges with Debtor");
			filtersNotPublished.Add("Charges with Creditor");
			filtersNotPublished.Add("AR Transaction #");
			filtersNotPublished.Add("Job Branch");
			filtersNotPublished.Add("Job Department");
			filtersNotPublished.Add("Job Operation Staff");
			filtersNotPublished.Add("Job Sales Staff");
			filtersNotPublished.Add("Job Open or Close");
			filtersNotPublished.Add("Job Open");
			filtersNotPublished.Add("Job Close");
			filtersNotPublished.Add("Job Revenue Recognition Date");
			filtersNotPublished.Add("Job Profit Amount");
			filtersNotPublished.Add("Job Revenue Amount");
			filtersNotPublished.Add("Job Cost Amount");
			filtersNotPublished.Add("Job WIP Amount");
			filtersNotPublished.Add("Job WIP Amount (Excluding Deferred Charges)");
			filtersNotPublished.Add("Job WIP Amount (Deferred Charges Only)");
			filtersNotPublished.Add("Job Accrual Amount");
			filtersNotPublished.Add("Job Margin %");

			foreach (string filterDescription in filtersNotPublished)
			{
				Assert(!FilterBO[filterDescription].IsPublishedOnWeb);
			}
		}

		#endregion

		#region Numbers And References Filters

		public void TestLocalJobReferenceFilter()
		{
			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Job Local Reference"];

			filter.Property = Job1.JH_JobLocalReference;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);

			filter.Property = Job2.JH_JobLocalReference;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
		}

		#region Test Filter Max Length

		public void TestLocalJobReferenceFilter_FilterMaxLength()
		{
			AssertEquals("MaxLength of Job Local Reference should be set correctly.", JobHeaderSchema.JH_JobLocalReference.MaxLength, FilterBO["Job Local Reference"].MaxLength);
		}

		#endregion

		#endregion

		#region Organisation Filters

		public void TestJobBranchManagementCodeFilter()
		{
			var codeCollection = new BranchManagementCodeDescriptionBoolCollection();
			codeCollection.Add("BRA", null, true);
			codeCollection.Add("BRB", null, true);
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeCollection);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_AccountingGroupCode = "BRA";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_AccountingGroupCode = "BRB";

			Job1.JH_GB = branch1.PK;
			Job2.JH_GB = branch2.PK;
			InactiveJob1.JH_GB = branch1.PK;
			InactiveJob2.JH_GB = branch2.PK;

			Factory.Save();

			var branchManagementCodeFilter = (ModuleTextFilter)FilterBO["Job Branch Management Code"];
			branchManagementCodeFilter.Property = "BRA";
			branchManagementCodeFilter.IsActive = true;
			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			branchManagementCodeFilter.Property = "BRB";
			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);

			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
		}

		public void TestBranchFilter()
		{
			var branch1 = TestObjectCreator.CreateBranch("BR1", "Branch 1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BR2", "Branch 2", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("BR3", "Branch 3", GlbCompany.CurrentCompany);

			Job1.JH_GB = branch1.PK;
			Job2.JH_GB = branch2.PK;
			InactiveJob1.JH_GB = branch1.PK;
			InactiveJob2.JH_GB = branch2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Job Branch"];
			filter.IsActive = true;
			Assert(filter.HasComparisonOperator);

			AssertEquals("Precondition: default operator is equals", SQLComparisonOperator.Equal, filter.SqlComparisonOperator);
			filter.Property = branch1.PK;
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to BR1 contains only Job1", true, FilterCollection, Job1);
			AssertCollection("Equal to BR1 contains only Job1", false, FilterCollection, Job2);
			AssertCollection("Equal to BR1 contains only Job1", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to BR1 contains only Job1 and job header InactiveJob1", true, FilterCollection, Job1);
			AssertCollection("Equal to BR1 contains only Job1 and job header InactiveJob1", false, FilterCollection, Job2);
			AssertCollection("Equal to BR1 contains only Job1 and job header InactiveJob1", false, FilterCollection, Job3);
			AssertCollection("Equal to BR1 contains only Job1 and job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Equal to BR1 contains only Job1 and job header InactiveJob1", false, FilterCollection, InactiveJob2);
			AssertCollection("Equal to BR1 contains only Job1 and job header InactiveJob1", false, FilterCollection, InactiveJob3);

			filter.Property = branch3.PK;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to BR3 contains no Jobs", false, FilterCollection, Job1);
			AssertCollection("Equal to BR3 contains no Jobs", false, FilterCollection, Job2);
			AssertCollection("Equal to BR3 contains no Jobs", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to BR3 contains no Jobs", false, FilterCollection, Job1);
			AssertCollection("Equal to BR3 contains no Jobs", false, FilterCollection, Job2);
			AssertCollection("Equal to BR3 contains no Jobs", false, FilterCollection, Job3);
			AssertCollection("Equal to BR3 contains no Jobs", false, FilterCollection, InactiveJob1);
			AssertCollection("Equal to BR3 contains no Jobs", false, FilterCollection, InactiveJob2);
			AssertCollection("Equal to BR3 contains no Jobs", false, FilterCollection, InactiveJob3);

			filter.Property = branch1.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Not Equal to BR1 contains all but Job1", false, FilterCollection, Job1);
			AssertCollection("Not Equal to BR1 contains all but Job1", true, FilterCollection, Job2);
			AssertCollection("Not Equal to BR1 contains all but Job1", true, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Not Equal to BR1 contains Job2, Job3, not contains parent table InactiveJob2, InactiveJob3", false, FilterCollection, Job1);
			AssertCollection("Not Equal to BR1 contains Job2, Job3, not contains parent table InactiveJob2, InactiveJob3", true, FilterCollection, Job2);
			AssertCollection("Not Equal to BR1 contains Job2, Job3, not contains parent table InactiveJob2, InactiveJob3", true, FilterCollection, Job3);
			AssertCollection("Not Equal to BR1 contains Job2, Job3, not contains parent table InactiveJob2, InactiveJob3", false, FilterCollection, InactiveJob1);
			AssertCollection("Not Equal to BR1 contains Job2, Job3, not contains parent table InactiveJob2, InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Not Equal to BR1 contains Job2, Job3, not contains parent table InactiveJob2, InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);

			Assert("Job Branch Filter supports Filters Match operator", filter.SupportsFiltersMatchComparisonOperator);
			filter.ComparisonOperator = ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Code", branch2.GB_Code);
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job1);
			AssertCollection("Matched Filters uses Selected Filters", true, FilterCollection, Job2);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job1);
			AssertCollection("Matched Filters uses Selected Filters", true, FilterCollection, Job2);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job3);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, InactiveJob1);
			AssertCollection("Matched Filters uses Selected Filters", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, InactiveJob3);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.Validation.ValidateComparisonOperator();
			Assert(filter.ComparisonOperatorInfo.HasErrors());

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.Validation.ValidateComparisonOperator();
			Assert(!filter.ComparisonOperatorInfo.HasErrors());
			Assert(!filter.ComparisonOperatorInfo.HasWarnings());
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Is Not Blank contains all Jobs", true, FilterCollection, Job1);
			AssertCollection("Is Not Blank contains all Jobs", true, FilterCollection, Job2);
			AssertCollection("Is Not Blank contains all Jobs", true, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Is Not Blank contains all active Job", true, FilterCollection, Job1);
			AssertCollection("Is Not Blank contains all active Job", true, FilterCollection, Job2);
			AssertCollection("Is Not Blank contains all active Job", true, FilterCollection, Job3);
			AssertCollection("Is Not Blank contains all inactive Job headers", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Is Not Blank contains all inactive Job headers", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Is Not Blank contains all inactive Job headers", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
		}

		public void TestBranchFilterWithEmptyCompanyOrgProxy()
		{
			var branch1 = TestObjectCreator.CreateBranch("BR1", "Branch 1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BR2", "Branch 2", GlbCompany.CurrentCompany);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;

			Job1.JH_GB = branch1.PK;
			Job2.JH_GB = branch2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Job Branch"];
			filter.IsActive = true;
			filter.Property = branch1.PK;
			AssertNoExceptionThrown(() => LoadCollection(FilterCollection, FilterBO.Filter));
		}

		public void TestBranchFilterWithEmptyBranchOrgProxy()
		{
			var branch1 = TestObjectCreator.CreateBranch("BR1", "Branch 1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BR2", "Branch 2", GlbCompany.CurrentCompany);
			branch2.GB_OH_OrgProxy = ZGuid.Empty;

			Job1.JH_GB = branch1.PK;
			Job2.JH_GB = branch2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Job Branch"];
			filter.IsActive = true;
			filter.Property = branch1.PK;
			AssertNoExceptionThrown(() => LoadCollection(FilterCollection, FilterBO.Filter));
		}

		public void TestDepartmentFilter()
		{
			var dept1 = TestObjectCreator.CreateDepartment("DP1", "Department 1");
			var dept2 = TestObjectCreator.CreateDepartment("DP2", "Department 2");
			var dept3 = TestObjectCreator.CreateDepartment("DP3", "Department 3");

			Job1.JH_GE = dept1.PK;
			Job2.JH_GE = dept2.PK;
			InactiveJob1.JH_GE = dept1.PK;
			InactiveJob2.JH_GE = dept2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Job Department"];

			filter.IsActive = true;
			Assert(filter.HasComparisonOperator);

			AssertEquals("Precondition: default operator is equals", SQLComparisonOperator.Equal, filter.SqlComparisonOperator);
			filter.Property = dept1.PK;
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to DP1 contains only Job1", true, FilterCollection, Job1);
			AssertCollection("Equal to DP1 contains only Job1", false, FilterCollection, Job2);
			AssertCollection("Equal to DP1 contains only Job1", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to DP1 contains only Job1 and job header InactiveJob1", true, FilterCollection, Job1);
			AssertCollection("Equal to DP1 contains only Job1 and job header InactiveJob1", false, FilterCollection, Job2);
			AssertCollection("Equal to DP1 contains only Job1 and job header InactiveJob1", false, FilterCollection, Job3);
			AssertCollection("Equal to DP1 contains only Job1 and job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Equal to DP1 contains only Job1 and job header InactiveJob1", false, FilterCollection, InactiveJob2);
			AssertCollection("Equal to DP1 contains only Job1 and job header InactiveJob1", false, FilterCollection, InactiveJob3);

			filter.Property = dept3.PK;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to DP3 contains no Jobs", false, FilterCollection, Job1);
			AssertCollection("Equal to DP3 contains no Jobs", false, FilterCollection, Job2);
			AssertCollection("Equal to DP3 contains no Jobs", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to DP3 contains no Jobs", false, FilterCollection, Job1);
			AssertCollection("Equal to DP3 contains no Jobs", false, FilterCollection, Job2);
			AssertCollection("Equal to DP3 contains no Jobs", false, FilterCollection, Job3);
			AssertCollection("Equal to DP3 contains no Jobs", false, FilterCollection, InactiveJob1);
			AssertCollection("Equal to DP3 contains no Jobs", false, FilterCollection, InactiveJob2);
			AssertCollection("Equal to DP3 contains no Jobs", false, FilterCollection, InactiveJob3);

			filter.Property = dept1.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Not Equal to DP1 contains all but Job1", false, FilterCollection, Job1);
			AssertCollection("Not Equal to DP1 contains all but Job1", true, FilterCollection, Job2);
			AssertCollection("Not Equal to DP1 contains all but Job1", true, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Not Equal to DP1 contains Job2, Job3, job header InactiveJob2, InactiveJob3", false, FilterCollection, Job1);
			AssertCollection("Not Equal to DP1 contains Job2, Job3, job header InactiveJob2, InactiveJob3", true, FilterCollection, Job2);
			AssertCollection("Not Equal to DP1 contains Job2, Job3, job header InactiveJob2, InactiveJob3", true, FilterCollection, Job3);
			AssertCollection("Not Equal to DP1 contains Job2, Job3, job header InactiveJob2, InactiveJob3", false, FilterCollection, InactiveJob1);
			AssertCollection("Not Equal to DP1 contains Job2, Job3, job header InactiveJob2, InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Not Equal to DP1 contains Job2, Job3, job header InactiveJob2, InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);

			Assert("Job Branch Filter supports Filters Match operator", filter.SupportsFiltersMatchComparisonOperator);
			filter.ComparisonOperator = ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Code", dept2.GE_Code);
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job1);
			AssertCollection("Matched Filters uses Selected Filters", true, FilterCollection, Job2);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job1);
			AssertCollection("Matched Filters uses Selected Filters", true, FilterCollection, Job2);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job3);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, InactiveJob1);
			AssertCollection("Matched Filters uses Selected Filters", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, InactiveJob3);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.Validation.ValidateComparisonOperator();
			Assert(filter.ComparisonOperatorInfo.HasErrors());

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.Validation.ValidateComparisonOperator();
			Assert(!filter.ComparisonOperatorInfo.HasErrors());
			Assert(!filter.ComparisonOperatorInfo.HasWarnings());
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Is Not Blank contains all Jobs", true, FilterCollection, Job1);
			AssertCollection("Is Not Blank contains all Jobs", true, FilterCollection, Job2);
			AssertCollection("Is Not Blank contains all Jobs", true, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Is Not Blank contains all Jobs", true, FilterCollection, Job1);
			AssertCollection("Is Not Blank contains all Jobs", true, FilterCollection, Job2);
			AssertCollection("Is Not Blank contains all Jobs", true, FilterCollection, Job3);
			AssertCollection("Is Not Blank contains all inactive job headers", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Is Not Blank contains all inactive job headers", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Is Not Blank contains all inactive job headers", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
		}

		public void TestOperationStaffFilter()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();

			Job1.JH_GS_NKRepOps = staff1.GS_Code;
			Job2.JH_GS_NKRepOps = staff2.GS_Code;
			Job3.JH_GS_NKRepOps = ZString.Empty;
			Job4.JH_GS_NKRepOps = ZString.Empty;

			InactiveJob1.JH_GS_NKRepOps = staff1.GS_Code;
			InactiveJob2.JH_GS_NKRepOps = staff2.GS_Code;
			InactiveJob3.JH_GS_NKRepOps = ZString.Empty;
			InactiveJob4.JH_GS_NKRepOps = ZString.Empty;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterBO["Job Operation Staff"];

			filter.IsActive = true;
			Assert(filter.HasComparisonOperator);

			AssertEquals("Precondition: default operator is equals", SQLComparisonOperator.Equal, filter.SqlComparisonOperator);
			filter.Property = staff1.GS_Code;
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to staff1's user code contains only Job1", true, FilterCollection, Job1);
			AssertCollection("Equal to staff1's user code contains only Job1", false, FilterCollection, Job2);
			AssertCollection("Equal to staff1's user code contains only Job1", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to staff1's user code contains only Job1 and job header InactiveJob1", true, FilterCollection, Job1);
			AssertCollection("Equal to staff1's user code contains only Job1 and job header InactiveJob1", false, FilterCollection, Job2);
			AssertCollection("Equal to staff1's user code contains only Job1 and job header InactiveJob1", false, FilterCollection, Job3);
			AssertCollection("Equal to staff1's user code contains only Job1 and job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Equal to staff1's user code contains only Job1 and job header InactiveJob1", false, FilterCollection, InactiveJob2);
			AssertCollection("Equal to staff1's user code contains only Job1 and job header InactiveJob1", false, FilterCollection, InactiveJob3);

			filter.Property = staff3.GS_Code;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, Job1);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, Job2);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, Job1);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, Job2);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, Job3);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, InactiveJob1);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, InactiveJob2);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, InactiveJob3);

			filter.Property = staff1.GS_Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Not Equal to staff1's code contains all but Job1", false, FilterCollection, Job1);
			AssertCollection("Not Equal to staff1's code contains all but Job1", true, FilterCollection, Job2);
			AssertCollection("Not Equal to staff1's code contains all but Job1", true, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Not Equal to staff1's code contains Job2, Job3, job header InactiveJob2, InactiveJob3", false, FilterCollection, Job1);
			AssertCollection("Not Equal to staff1's code contains Job2, Job3, job header InactiveJob2, InactiveJob3", true, FilterCollection, Job2);
			AssertCollection("Not Equal to staff1's code contains Job2, Job3, job header InactiveJob2, InactiveJob3", true, FilterCollection, Job3);
			AssertCollection("Not Equal to staff1's code contains Job2, Job3, job header InactiveJob2, InactiveJob3", false, FilterCollection, InactiveJob1);
			AssertCollection("Not Equal to staff1's code contains Job2, Job3, job header InactiveJob2, InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Not Equal to staff1's code contains Job2, Job3, job header InactiveJob2, InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);

			Assert("Job Operation Staff Filter supports Filters Match operator", filter.SupportsFiltersMatchComparisonOperator);
			filter.ComparisonOperator = ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Code", staff2.GS_Code);
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job1);
			AssertCollection("Matched Filters uses Selected Filters", true, FilterCollection, Job2);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job1);
			AssertCollection("Matched Filters uses Selected Filters", true, FilterCollection, Job2);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job3);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, InactiveJob1);
			AssertCollection("Matched Filters uses Selected Filters", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, InactiveJob3);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertEquals("Precondition: Job3's Operation Staff is blank", ZString.Empty, Job3.JH_GS_NKRepOps);
			AssertEquals("Precondition: Job4's Operation Staff is blank", ZString.Empty, Job4.JH_GS_NKRepOps);
			AssertCollection("Is Blank contains Jobs 3 & 4", false, FilterCollection, Job1);
			AssertCollection("Is Blank contains Jobs 3 & 4", false, FilterCollection, Job2);
			AssertCollection("Is Blank contains Jobs 3 & 4", true, FilterCollection, Job3);
			AssertCollection("Is Blank contains Jobs 3 & 4", true, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", false, FilterCollection, Job1);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", false, FilterCollection, Job2);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", true, FilterCollection, Job3);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", true, FilterCollection, Job4);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", false, FilterCollection, InactiveJob1);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", false, FilterCollection, InactiveJob2);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", true, FilterCollection, Job1);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", true, FilterCollection, Job2);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", false, FilterCollection, Job3);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", false, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", true, FilterCollection, Job1);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", true, FilterCollection, Job2);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", false, FilterCollection, Job3);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", false, FilterCollection, Job4);
			AssertCollection("Is Not Blank contains all Job headers where the staff code is not blank", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Is Not Blank contains all Job headers where the staff code is not blank", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", false, FilterCollection, InactiveJob3);
		}

		public void TestSalesStaffFilter()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_Code = ZString.Empty;

			Job1.JH_GS_NKRepSales = staff1.GS_Code;
			Job2.JH_GS_NKRepSales = staff2.GS_Code;
			Job4.JH_GS_NKRepSales = staff4.GS_Code;
			InactiveJob1.JH_GS_NKRepSales = staff1.GS_Code;
			InactiveJob2.JH_GS_NKRepSales = staff2.GS_Code;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterBO["Job Sales Staff"];

			filter.IsActive = true;
			Assert(filter.HasComparisonOperator);

			AssertEquals("Precondition: default operator is equals", SQLComparisonOperator.Equal, filter.SqlComparisonOperator);
			filter.Property = staff1.GS_Code;
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to staff1's user code contains only Job1", true, FilterCollection, Job1);
			AssertCollection("Equal to staff1's user code contains only Job1", false, FilterCollection, Job2);
			AssertCollection("Equal to staff1's user code contains only Job1", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to staff1's user code contains only Job1 and job header InactiveJob1", true, FilterCollection, Job1);
			AssertCollection("Equal to staff1's user code contains only Job1 and job header InactiveJob1", false, FilterCollection, Job2);
			AssertCollection("Equal to staff1's user code contains only Job1 and job header InactiveJob1", false, FilterCollection, Job3);
			AssertCollection("Equal to staff1's user code contains only Job1 and job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Equal to staff1's user code contains only Job1 and job header InactiveJob1", false, FilterCollection, InactiveJob2);
			AssertCollection("Equal to staff1's user code contains only Job1 and job header InactiveJob1", false, FilterCollection, InactiveJob3);

			filter.Property = staff3.GS_Code;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, Job1);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, Job2);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, Job1);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, Job2);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, Job3);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, InactiveJob1);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, InactiveJob2);
			AssertCollection("Equal to staff3's user code contains no Jobs", false, FilterCollection, InactiveJob3);

			filter.Property = staff1.GS_Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Not Equal to staff1's code contains all but Job1", false, FilterCollection, Job1);
			AssertCollection("Not Equal to staff1's code contains all but Job1", true, FilterCollection, Job2);
			AssertCollection("Not Equal to staff1's code contains all but Job1", true, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Not Equal to staff1's code contains Job2, Job3, job header InactiveJob2, InactiveJob3", false, FilterCollection, Job1);
			AssertCollection("Not Equal to staff1's code contains Job2, Job3, job header InactiveJob2, InactiveJob3", true, FilterCollection, Job2);
			AssertCollection("Not Equal to staff1's code contains Job2, Job3, job header InactiveJob2, InactiveJob3", true, FilterCollection, Job3);
			AssertCollection("Not Equal to staff1's code contains Job2, Job3, job header InactiveJob2, InactiveJob3", false, FilterCollection, InactiveJob1);
			AssertCollection("Not Equal to staff1's code contains Job2, Job3, job header InactiveJob2, InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Not Equal to staff1's code contains Job2, Job3, job header InactiveJob2, InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);

			Assert("Job Operation Staff Filter supports Filters Match operator", filter.SupportsFiltersMatchComparisonOperator);
			filter.ComparisonOperator = ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Code", staff2.GS_Code);
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job1);
			AssertCollection("Matched Filters uses Selected Filters", true, FilterCollection, Job2);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job1);
			AssertCollection("Matched Filters uses Selected Filters", true, FilterCollection, Job2);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, Job3);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, InactiveJob1);
			AssertCollection("Matched Filters uses Selected Filters", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Matched Filters uses Selected Filters", false, FilterCollection, InactiveJob3);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Is Blank contains Jobs 3 & 4", false, FilterCollection, Job1);
			AssertCollection("Is Blank contains Jobs 3 & 4", false, FilterCollection, Job2);
			AssertCollection("Is Blank contains Jobs 3 & 4", true, FilterCollection, Job3);
			AssertCollection("Is Blank contains Jobs 3 & 4", true, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", false, FilterCollection, Job1);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", false, FilterCollection, Job2);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", true, FilterCollection, Job3);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", true, FilterCollection, Job4);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", false, FilterCollection, InactiveJob1);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", false, FilterCollection, InactiveJob2);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Is Blank contains Jobs 3 & 4, job header InactiveJob 3 & 4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", true, FilterCollection, Job1);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", true, FilterCollection, Job2);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", true, FilterCollection, Job1);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", true, FilterCollection, Job2);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", false, FilterCollection, Job3);
			AssertCollection("Is Not Blank contains all Job headers where the staff code is not blank", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Is Not Blank contains all Job headers where the staff code is not blank", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Is Not Blank contains all Jobs where the staff code is not blank", false, FilterCollection, InactiveJob3);
		}

		public void TestTaxBranchFilter()
		{
			var branch1 = TestObjectCreator.CreateBranch("BR1", "Branch 1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BR2", "Branch 2", GlbCompany.CurrentCompany);

			Job1.JH_GB_TaxBranch = branch1.PK;
			Job2.JH_GB_TaxBranch = branch2.PK;
			Job3.JH_GB_TaxBranch = ZGuid.Empty;
			InactiveJob1.JH_GB_TaxBranch = branch1.PK;
			InactiveJob2.JH_GB_TaxBranch = branch2.PK;
			InactiveJob3.JH_GB_TaxBranch = ZGuid.Empty;
			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("IsTaxBranchApplicable: true, Tax Branch filter will exist", true, AccountingMasterFilesUtils.IsTaxBranchApplicable);
			FilterBO.LoadModuleFilters();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Job Tax Branch"];
			filter.IsActive = true;
			AssertEquals(true, filter.HasComparisonOperator);
			AssertEquals("Precondition: default operator is equals", SQLComparisonOperator.Equal, filter.SqlComparisonOperator);

			filter.Property = branch1.PK;
			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Jobs whose tax branch equal to branch1: Job1", true, FilterCollection, Job1);
			AssertCollection("Jobs whose tax branch equal to branch1: Job1, not contain Job2", false, FilterCollection, Job2);
			AssertCollection("Jobs whose tax branch equal to branch1: Job1, not contain Job3", false, FilterCollection, Job3);

			filter.Property = branch1.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Jobs whose tax branch not equal to branch1: Job2, not contain Job1", false, FilterCollection, Job1);
			AssertCollection("Jobs whose tax branch not equal to branch1: Job2", true, FilterCollection, Job2);
			AssertCollection("Jobs whose tax branch not equal to branch1: Job2, not contain Job3", false, FilterCollection, Job3);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Jobs whose tax branch is blank: Job3, not contain Job1", false, FilterCollection, Job1);
			AssertCollection("Jobs whose tax branch is blank: Job3, not contain Job2", false, FilterCollection, Job2);
			AssertCollection("Jobs whose tax branch is blank: Job3", true, FilterCollection, Job3);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Jobs whose tax branch is not blank: Job1 Job2, contain Job1", true, FilterCollection, Job1);
			AssertCollection("Jobs whose tax branch is not blank: Job1 Job2, contain Job2", true, FilterCollection, Job2);
			AssertCollection("Jobs whose tax branch is not blank: Job1 Job2, not contain Job3", false, FilterCollection, Job3);

			AssertEquals("Tax Branch filter supports Filters Match operator", true, filter.SupportsFiltersMatchComparisonOperator);
			filter.ComparisonOperator = ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Code", branch2.GB_Code);
			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Jobs whose tax branch's Code equal to branch2's Code: Job2, not contain Job1", false, FilterCollection, Job1);
			AssertCollection("Jobs whose tax branch's Code equal to branch2's Code: Job2", true, FilterCollection, Job2);
			AssertCollection("Jobs whose tax branch's Code equal to branch2's Code: Job2, not contain Job3", false, FilterCollection, Job3);
		}

		public void TestTaxBranchFilterExist()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("IsTaxBranchApplicable: false", !AccountingMasterFilesUtils.IsTaxBranchApplicable);
			using (var testModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(TestDataSupplier.FilterStripModuleID))
			{
				AssertNull("When currentCompany's GC_IsGSTRegistered is false, Tax Branch filter not exist", testModule.FilterBusinessObject["Job Tax Branch"]);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert("IsTaxBranchApplicable: false", !AccountingMasterFilesUtils.IsTaxBranchApplicable);
			using (var testModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(TestDataSupplier.FilterStripModuleID))
			{
				AssertNull("When registry EnableTaxBranchReporting is false, Tax Branch filter not exist", testModule.FilterBusinessObject["Job Tax Branch"]);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("IsTaxBranchApplicable: true", AccountingMasterFilesUtils.IsTaxBranchApplicable);
			using (var testModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(TestDataSupplier.FilterStripModuleID))
			{
				AssertNotNull("IsTaxBranchApplicable: true, Tax Branch filter exist", testModule.FilterBusinessObject["Job Tax Branch"]);
			}
		}

		#endregion

		#region Date Filters

		public void TestJobOpenDateFilter()
		{
			var date1 = new ZDateTime(2000, 1, 1, 11, 0, 0);
			var date2 = new ZDateTime(2000, 1, 1, 12, 0, 0);

			Job1.JH_A_JOP = date1;
			Job2.JH_A_JOP = date2;
			InactiveJob1.JH_A_JOP = date1;
			InactiveJob2.JH_A_JOP = date2;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO["Job Open"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = date2;
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = date1;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = date1;
			filter.Property2 = date2;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = date1;
			filter.Property2 = date1;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = date2;
			filter.Property2 = date1;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
		}

		public void TestJobCloseDateFilter()
		{
			var date1 = new ZDateTime(2000, 1, 1, 11, 0, 0);
			var date2 = new ZDateTime(2000, 1, 1, 12, 0, 0);

			Job1.JH_A_JCL = date1;
			Job2.JH_A_JCL = date2;
			InactiveJob1.JH_A_JCL = date1;
			InactiveJob2.JH_A_JCL = date2;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO["Job Close"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = date2;
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = date1;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = date1;
			filter.Property2 = date2;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain jbo header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain jbo header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = date1;
			filter.Property2 = date1;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = date2;
			filter.Property2 = date1;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
		}

		public void TestJobRevenueRecognitionDateFilter()
		{
			var date1 = new ZDateTime(2000, 1, 1, 11, 0, 0);
			var date2 = new ZDateTime(2000, 1, 1, 12, 0, 0);

			JobChargeRevRecognition revRecog1 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			JobChargeRevRecognition revRecog2 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			var revRecog3 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			var revRecog4 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			revRecog1.D3_RecognitionDate = date1;
			revRecog2.D3_RecognitionDate = date2;
			revRecog3.D3_RecognitionDate = date1;
			revRecog4.D3_RecognitionDate = date2;
			revRecog1.D3_JH = Job1.PK;
			revRecog2.D3_JH = Job2.PK;
			revRecog3.D3_JH = InactiveJob1.PK;
			revRecog4.D3_JH = InactiveJob2.PK;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO["Job Revenue Recognition Date"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = date2;
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = date1;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = date1;
			filter.Property2 = date2;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = date1;
			filter.Property2 = date1;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = date2;
			filter.Property2 = date1;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			var nonCurrentCompanyBranch = TestObjectCreator.NonCurrentCompanyBranch;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), nonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Job1.JH_GC = nonCurrentCompanyBranch.Company.PK;
				Job1.JH_GB = nonCurrentCompanyBranch.PK;

				InactiveJob1.JH_GC = nonCurrentCompanyBranch.Company.PK;
				InactiveJob1.JH_GB = nonCurrentCompanyBranch.PK;
				Factory.Save();
			}

			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Expecting collection should contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection should not contain Job1", false, FilterCollection, Job1);

			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Expecting collection should contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection should not contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
		}

		public void TestJobOpenOrCloseDateFilter()
		{
			var job1_JOP = new ZDateTime(2000, 1, 1, 11, 0, 0);
			var job1_JCL = new ZDateTime(2000, 1, 1, 12, 0, 0);
			var job2_JOP = new ZDateTime(2000, 1, 2, 11, 0, 0);
			var job2_JCL = new ZDateTime(2000, 1, 2, 12, 0, 0);
			var job3_JOP = new ZDateTime(2000, 1, 3, 11, 0, 0);
			var job3_JCL = new ZDateTime(2000, 1, 3, 12, 0, 0);

			Job1.JH_A_JOP = job1_JOP;
			Job1.JH_A_JCL = job1_JCL;
			Job2.JH_A_JOP = job2_JOP;
			Job2.JH_A_JCL = job2_JCL;
			Job3.JH_A_JOP = job3_JOP;
			Job3.JH_A_JCL = job3_JCL;
			InactiveJob1.JH_A_JOP = job1_JOP;
			InactiveJob1.JH_A_JCL = job1_JCL;
			InactiveJob2.JH_A_JOP = job2_JOP;
			InactiveJob2.JH_A_JCL = job2_JCL;
			InactiveJob3.JH_A_JOP = job3_JOP;
			InactiveJob3.JH_A_JCL = job3_JCL;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO["Job Open or Close"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = job3_JOP;
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain job header InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = job1_JCL;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = job1_JOP;
			filter.Property2 = job3_JCL;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain job header InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = job2_JOP;
			filter.Property2 = job2_JOP;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = job3_JCL;
			filter.Property2 = job1_JOP;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
		}

		#endregion

		#region Amount Filter Tests

		public void TestJobManagementFiltersWithMaxAmounts()
		{
			if (!TestDataSupplier.IsRevenueFiltersAdded)
			{
				Assert("The test in not applicable in the filter in not added", true);
				return;
			}
			var registryValue = new MaximumAllowedTransactionAmount()
			{
				MaximumAllowedHeaderAmount = 1200000000000000M,
				MaximumAllowedLineAmount = 1200000000000000M
			};

			using (AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			using (AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var maxAmount = 922337203685477m;
				var accrual1 = TestObjectCreator.CreateAccrualLineAndCharge(Job1.PK, maxAmount, TestObjectCreator.FRT.AC_Code);
				var accrual2 = TestObjectCreator.CreateAccrualLineAndCharge(Job1.PK, maxAmount, TestObjectCreator.FRT.AC_Code);
				var wip1 = TestObjectCreator.CreateWIPLineAndCharge(Job1.PK, maxAmount, TestObjectCreator.FRT.AC_Code);
				var wip2 = TestObjectCreator.CreateWIPLineAndCharge(Job1.PK, maxAmount, TestObjectCreator.FRT.AC_Code);
				var rev1 = TestObjectCreator.CreateRevenueLineAndCharge(Job1.PK, maxAmount, TestObjectCreator.FRT.AC_Code);
				var rev2 = TestObjectCreator.CreateRevenueLineAndCharge(Job1.PK, maxAmount, TestObjectCreator.FRT.AC_Code);
				var cost1 = TestObjectCreator.CreateCostLineAndCharge(Job1.PK, maxAmount, TestObjectCreator.FRT.AC_Code);
				var cost2 = TestObjectCreator.CreateCostLineAndCharge(Job1.PK, maxAmount, TestObjectCreator.FRT.AC_Code);

				Factory.Save();

				foreach (var filterName in new[] { "Job Profit Amount", "Job Margin %", "Job Revenue Amount", "Job Cost Amount",
				"Job Accrual Amount", "Job WIP Amount", "Job WIP Amount (Excluding Deferred Charges)", "Job WIP Amount (Deferred Charges Only)" })
				{
					var filter = (JobManagementAmountFilter)FilterBO[filterName];
					filter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
					filter.Property = -100;
					filter.IsActive = true;
					LoadCollection(FilterCollection, FilterBO.Filter);
					AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				}

				var loadedJob = Factory.Load<JobManagement>(Job1.PK);
				AssertEquals(1844674407370954m, loadedJob.TotalRevenue);
				AssertEquals(-1844674407370954m, loadedJob.TotalCost);
				AssertEquals(1844674407370954m, loadedJob.TotalWIP);
				AssertEquals(-1844674407370954m, loadedJob.TotalAccrual);
				AssertEquals(0m, loadedJob.TotalLineAmount);
			}
		}

		public void TestJobProfitFilter()
		{
			if (!TestDataSupplier.IsRevenueFiltersAdded)
			{
				Assert("The test in not applicable in the filter in not added", true);
				return;
			}

			BaseLineSetup(Job1, false, true, true, true);
			BaseLineSetup(Job2, true, false, true, true);
			BaseLineSetup(Job3, true, true, false, true);
			BaseLineSetup(Job4, true, true, true, false);

			SetupRevLine(Job5, 2000m);
			SetupRevLine(Job6, 3000m);

			Factory.Save();

			JobManagementAmountFilter filter = (JobManagementAmountFilter)FilterBO["Job Profit Amount"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = -480m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain job header InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.LessThan;
			filter.Property = -234m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain job header InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain job header InactiveJob4", false, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
			filter.Property = -141m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain job header InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain job header InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
			filter.Property = 2500m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection to contain Job6", true, FilterCollection, Job6);

			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection to contain Job6", true, FilterCollection, Job6);
			AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);
			AssertCollection("Expecting collection not to contain job header InactiveJob6", false, FilterCollection, InactiveJob6);

			var nonCurrentCompanyBranch = TestObjectCreator.NonCurrentCompanyBranch;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), nonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Job6.JH_GC = nonCurrentCompanyBranch.Company.PK;
				Job6.JH_GB = nonCurrentCompanyBranch.PK;

				InactiveJob6.JH_GC = nonCurrentCompanyBranch.Company.PK;
				InactiveJob6.JH_GB = nonCurrentCompanyBranch.PK;
				Factory.Save();
			}

			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);

			LoadCollection(FilterCollection, FilterBO.Filter);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);
			AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);
			AssertCollection("Expecting collection not to contain InactiveJob6", false, FilterCollection, InactiveJob6);
		}

		public void TestJobMarginPercentFilter()
		{
			if (!TestDataSupplier.IsRevenueFiltersAdded)
			{
				Assert("The test in not applicable in the filter in not added", true);
				return;
			}

			BaseLineSetup(Job1, false, true, true, true);
			BaseLineSetup(Job2, true, false, true, true);
			BaseLineSetup(Job3, true, true, false, true);
			BaseLineSetup(Job4, false, true, false, true);
			BaseLineSetup(Job5, true, false, false, false);

			JobCharge charge1 = CreateNewCharge();
			charge1.JR_JH = Job3.PK;
			charge1.JR_LocalSellAmt = 150m;
			charge1.JR_OSSellAmt = 150m;
			charge1.JR_LocalCostAmt = -100m;
			charge1.JR_OSCostAmt = -100m;

			Factory.Save();

			JobManagementAmountFilter filter = (JobManagementAmountFilter)FilterBO["Job Margin %"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 259.46m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain job header InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.LessThan;
			filter.Property = 400.00m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain job header InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain job header InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection to contain job header InactiveJob5", !IsFilterStripForParentTable, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
			filter.Property = 259.46m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain job header InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 100m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain job header InactiveJob5", false, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = -100m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain job header InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);
		}

		public void TestJobRevenueAmountFilter()
		{
			if (!TestDataSupplier.IsRevenueFiltersAdded)
			{
				Assert("The test in not applicable in the filter in not added", true);
				return;
			}

			BaseLineSetup(Job1, false, true, true, true);
			BaseLineSetup(Job2, true, false, true, true);
			BaseLineSetup(Job3, true, true, false, true);
			BaseLineSetup(Job4, true, true, true, false);

			AccTransactionLines rev1 = Factory.New<AccTransactionLines>();
			SetLine(Job2, rev1, TransactionLineTypes.Revenue, 5m);
			JobCharge charge1 = CreateNewCharge();
			charge1.JR_JH = Job2.PK;
			charge1.JR_AL_ARLine = rev1.PK;
			charge1.SetAmountsFromLinkedLinesForTests();
			rev1.AL_AH = Factory.NewWithValidTestData<ARInvoice>().PK;

			rev1 = Factory.New<AccTransactionLines>();
			SetLine(Job3, rev1, TransactionLineTypes.Revenue, -5m);
			charge1 = CreateNewCharge();
			charge1.JR_JH = Job3.PK;
			charge1.JR_AL_ARLine = rev1.PK;
			charge1.SetAmountsFromLinkedLinesForTests();
			rev1.AL_AH = Factory.NewWithValidTestData<ARInvoice>().PK;

			rev1 = Factory.New<AccTransactionLines>();
			SetLine(Job4, rev1, TransactionLineTypes.Revenue, 25m);
			charge1 = CreateNewCharge();
			charge1.JR_JH = Job4.PK;
			charge1.JR_AL_ARLine = rev1.PK;
			charge1.SetAmountsFromLinkedLinesForTests();
			rev1.AL_AH = Factory.NewWithValidTestData<ARInvoice>().PK;

			Factory.Save();

			JobManagementAmountFilter filter = (JobManagementAmountFilter)FilterBO["Job Revenue Amount"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 0m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 160m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain job header InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.LessThan;
			filter.Property = 161m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain job header InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
			filter.Property = 159m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain job header InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain job header InactiveJob4", false, FilterCollection, InactiveJob4);
		}

		public void TestJobWIPAmountFilter()
		{
			if (!TestDataSupplier.IsRevenueFiltersAdded)
			{
				Assert("The test in not applicable in the filter in not added", true);
				return;
			}

			BaseLineSetup(Job1, false, true, true, true);
			BaseLineSetup(Job2, true, false, true, true);
			BaseLineSetup(Job3, true, true, false, true);
			BaseLineSetup(Job4, true, true, true, false);

			WIP wIP1 = Factory.New<WIP>();
			JobCharge charge = CreateNewCharge();
			charge.JR_JH = Job1.PK;
			charge.JR_AL_ARLine = wIP1.PK;
			wIP1.AL_OSAmount = wIP1.AL_LineAmount = -10m;
			wIP1.AL_JH = Job1.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);

			wIP1 = Factory.New<WIP>();
			charge = CreateNewCharge();
			charge.JR_JH = Job2.PK;
			charge.JR_AL_ARLine = wIP1.PK;
			wIP1.AL_OSAmount = wIP1.AL_LineAmount = 135m;
			wIP1.AL_JH = Job2.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);

			wIP1 = Factory.New<WIP>();
			charge = CreateNewCharge();
			charge.JR_JH = Job4.PK;
			charge.JR_AL_ARLine = wIP1.PK;
			wIP1.AL_OSAmount = wIP1.AL_LineAmount = 15m;
			wIP1.AL_JH = Job4.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);

			charge = CreateNewCharge();
			charge.JR_JH = Job5.PK;
			charge.JR_LocalSellAmt = 75m;
			charge.JR_OSSellAmt = 75m;

			Factory.Save();

			JobManagementAmountFilter filter = (JobManagementAmountFilter)FilterBO["Job WIP Amount"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 0m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection to contain InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain job header InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection to contain InactiveJob5", !IsFilterStripForParentTable, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = -320m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain job header InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.LessThan;
			filter.Property = -199m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain job header InactiveJob4", false, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
			filter.Property = -201m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain job header InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain job header InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 75m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection to contain job header InactiveJob5", false, FilterCollection, InactiveJob5);
		}

		public void TestJobWIPAmountExcludingDeferredChargesFilter()
		{
			if (!TestDataSupplier.IsRevenueFiltersAdded)
			{
				Assert("The test in not applicable in the filter in not added", true);
				return;
			}

			BaseLineSetup(Job1, false, true, true, true);
			BaseLineSetup(Job2, true, false, true, true);
			BaseLineSetup(Job3, true, true, false, true);
			BaseLineSetup(Job4, true, true, true, false);

			WIP wIP1 = Factory.New<WIP>();
			JobCharge charge = CreateNewCharge();
			charge.JR_JH = Job1.PK;
			charge.JR_AL_ARLine = wIP1.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			wIP1.AL_OSAmount = wIP1.AL_LineAmount = -10m;
			wIP1.AL_JH = Job1.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);

			wIP1 = Factory.New<WIP>();
			charge = CreateNewCharge();
			charge.JR_JH = Job2.PK;
			charge.JR_AL_ARLine = wIP1.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			wIP1.AL_OSAmount = wIP1.AL_LineAmount = 135m;
			wIP1.AL_JH = Job2.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);

			wIP1 = Factory.New<WIP>();
			charge = CreateNewCharge();
			charge.JR_JH = Job4.PK;
			charge.JR_AL_ARLine = wIP1.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice_Batching;
			wIP1.AL_OSAmount = wIP1.AL_LineAmount = 15m;
			wIP1.AL_JH = Job4.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);

			charge = CreateNewCharge();
			charge.JR_JH = Job5.PK;
			charge.JR_LocalSellAmt = 75m;
			charge.JR_OSSellAmt = 75m;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice_Batching;

			Factory.Save();

			JobManagementAmountFilter filter = (JobManagementAmountFilter)FilterBO["Job WIP Amount (Excluding Deferred Charges)"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 0m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
			AssertCollection("Expecting collection to contain InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain job header InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection to contain job header InactiveJob5", !IsFilterStripForParentTable, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = -320m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.LessThan;
			filter.Property = -199m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
			filter.Property = -201m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain job header InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain job header InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = -185m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain job header InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain job header InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain job header InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);
		}

		public void TestJobWIPAmountDeferredChargesOnlyFilter()
		{
			if (!TestDataSupplier.IsRevenueFiltersAdded)
			{
				Assert("The test in not applicable in the filter in not added", true);
				return;
			}

			BaseLineSetup(Job1, false, true, true, true);
			BaseLineSetup(Job2, true, false, true, true);
			BaseLineSetup(Job3, true, true, false, true);
			BaseLineSetup(Job4, true, true, true, false);

			WIP wIP1 = Factory.New<WIP>();
			JobCharge charge = CreateNewCharge();
			charge.JR_JH = Job1.PK;
			charge.JR_AL_ARLine = wIP1.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			wIP1.AL_OSAmount = wIP1.AL_LineAmount = -10m;
			wIP1.AL_JH = Job1.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);

			wIP1 = Factory.New<WIP>();
			charge = CreateNewCharge();
			charge.JR_JH = Job2.PK;
			charge.JR_AL_ARLine = wIP1.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			wIP1.AL_OSAmount = wIP1.AL_LineAmount = 135m;
			wIP1.AL_JH = Job2.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);

			wIP1 = Factory.New<WIP>();
			charge = CreateNewCharge();
			charge.JR_JH = Job4.PK;
			charge.JR_AL_ARLine = wIP1.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice_Batching;
			wIP1.AL_OSAmount = wIP1.AL_LineAmount = 15m;
			wIP1.AL_JH = Job4.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);

			charge = CreateNewCharge();
			charge.JR_JH = Job5.PK;
			charge.JR_LocalSellAmt = 75m;
			charge.JR_OSSellAmt = 75m;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice_Batching;

			Factory.Save();

			JobManagementAmountFilter filter = (JobManagementAmountFilter)FilterBO["Job WIP Amount (Deferred Charges Only)"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 0m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection to contain InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain job header InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection to contain InactiveJob5", !IsFilterStripForParentTable, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 10m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain job header InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.LessThan;
			filter.Property = -14m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain job header InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain job header InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
			filter.Property = 9m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain job header InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain job header InactiveJob5", false, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 75m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain job header InactiveJob5", false, FilterCollection, InactiveJob5);
		}

		public void TestJobCostAmountFilter()
		{
			BaseLineSetup(Job1, false, true, true, true);
			BaseLineSetup(Job2, true, false, true, true);
			BaseLineSetup(Job3, true, true, false, true);
			BaseLineSetup(Job4, true, true, true, false);

			AccTransactionLines cost1 = Factory.New<AccTransactionLines>();
			SetLine(Job1, cost1, TransactionLineTypes.Cost, 5m);
			JobCharge charge1 = CreateNewCharge();
			charge1.JR_JH = Job1.PK;
			charge1.JR_AL_APLine = cost1.PK;
			charge1.SetAmountsFromLinkedLinesForTests();
			cost1.AL_AH = Factory.NewWithValidTestData<APInvoice>().PK;

			cost1 = Factory.New<AccTransactionLines>();
			SetLine(Job3, cost1, TransactionLineTypes.Cost, -5m);
			charge1 = CreateNewCharge();
			charge1.JR_JH = Job3.PK;
			charge1.JR_AL_APLine = cost1.PK;
			charge1.SetAmountsFromLinkedLinesForTests();
			cost1.AL_AH = Factory.NewWithValidTestData<APInvoice>().PK;

			cost1 = Factory.New<AccTransactionLines>();
			SetLine(Job4, cost1, TransactionLineTypes.Cost, 385m);
			charge1 = CreateNewCharge();
			charge1.JR_JH = Job4.PK;
			charge1.JR_AL_APLine = cost1.PK;
			charge1.SetAmountsFromLinkedLinesForTests();
			cost1.AL_AH = Factory.NewWithValidTestData<APInvoice>().PK;

			Factory.Save();

			JobManagementAmountFilter filter = (JobManagementAmountFilter)FilterBO["Job Cost Amount"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 0m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 200m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain job header InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
			filter.Property = 199m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain job header InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain job header InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.LessThan;
			filter.Property = 201m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain job header InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.LessThan;
			filter.Property = -100m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain job header InactiveJob4", false, FilterCollection, InactiveJob4);
		}

		public void TestJobAccrualAmountFilter()
		{
			BaseLineSetup(Job1, false, true, true, true);
			BaseLineSetup(Job2, true, false, true, true);
			BaseLineSetup(Job3, true, true, false, true);
			BaseLineSetup(Job4, true, true, true, false);

			Accrual accrual1 = Factory.New<Accrual>();
			JobCharge charge = CreateNewCharge();
			charge.JR_JH = Job1.PK;
			charge.JR_AL_APLine = accrual1.PK;
			accrual1.AL_OSAmount = accrual1.AL_LineAmount = -10m;
			accrual1.AL_JH = Job1.PK;
			accrual1.AL_GB = GlbBranch.CurrentBranch.PK;
			accrual1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual1);

			accrual1 = Factory.New<Accrual>();
			charge = CreateNewCharge();
			charge.JR_JH = Job2.PK;
			charge.JR_AL_APLine = accrual1.PK;
			accrual1.AL_OSAmount = accrual1.AL_LineAmount = 135m;
			accrual1.AL_JH = Job2.PK;
			accrual1.AL_GB = GlbBranch.CurrentBranch.PK;
			accrual1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual1);

			accrual1 = Factory.New<Accrual>();
			charge = CreateNewCharge();
			charge.JR_JH = Job3.PK;
			charge.JR_AL_APLine = accrual1.PK;
			accrual1.AL_OSAmount = accrual1.AL_LineAmount = 15m;
			accrual1.AL_JH = Job3.PK;
			accrual1.AL_GB = GlbBranch.CurrentBranch.PK;
			accrual1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual1);

			charge = CreateNewCharge();
			charge.JR_JH = Job5.PK;
			charge.JR_LocalCostAmt = 123m;
			charge.JR_OSCostAmt = 123m;

			Factory.Save();

			JobManagementAmountFilter filter = (JobManagementAmountFilter)FilterBO["Job Accrual Amount"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 0m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection to contain InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain job header InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection to contain InactiveJob5", !IsFilterStripForParentTable, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 225m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain job header InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);

			filter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
			filter.Property = 104m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain job header InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain job header InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.LessThan;
			filter.Property = 81m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain job header InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = 123m;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain job header InactiveJob5", false, FilterCollection, InactiveJob5);
		}

		public void TestAmountFiltersWhenJobsDoesntHaveTransactionLines()
		{
			BaseLineSetup(Job3, true, true, true, true);
			BaseLineSetup(Job4, true, true, true, false);
			BaseLineSetup(Job5, false, true, false, false);

			Factory.Save();

			if (TestDataSupplier.IsRevenueFiltersAdded)
			{
				JobManagementAmountFilter profitFilter = (JobManagementAmountFilter)FilterBO["Job Profit Amount"];

				profitFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				profitFilter.Property = 0m;
				profitFilter.IsActive = true;

				LoadCollection(FilterCollection, FilterBO.Filter);

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

				LoadCollection(FilterCollection, FilterBO.Filter);

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection to contain InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection to contain InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection to contain InactiveJob5", !IsFilterStripForParentTable, FilterCollection, InactiveJob5);

				profitFilter.IsActive = false;

				JobManagementAmountFilter marginFilter = (JobManagementAmountFilter)FilterBO["Job Margin %"];

				marginFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				marginFilter.Property = 0m;
				marginFilter.IsActive = true;

				LoadCollection(FilterCollection, FilterBO.Filter);

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

				LoadCollection(FilterCollection, FilterBO.Filter);

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection to contain InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection to contain InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection to contain InactiveJob5", !IsFilterStripForParentTable, FilterCollection, InactiveJob5);

				marginFilter.IsActive = false;
			}

			JobManagementAmountFilter accrualFilter = (JobManagementAmountFilter)FilterBO["Job Accrual Amount"];

			accrualFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			accrualFilter.Property = 0m;
			accrualFilter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain job header InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection to contain job header InactiveJob5", !IsFilterStripForParentTable, FilterCollection, InactiveJob5);

			if (TestDataSupplier.IsRevenueFiltersAdded)
			{
				JobManagementAmountFilter revenueFilter = (JobManagementAmountFilter)FilterBO["Job Revenue Amount"];

				revenueFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				revenueFilter.Property = 0m;
				revenueFilter.IsActive = true;

				LoadCollection(FilterCollection, FilterBO.Filter);

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);

				LoadCollection(FilterCollection, FilterBO.Filter);

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
				AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection to contain InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection to contain InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection to contain job header InactiveJob5", !IsFilterStripForParentTable, FilterCollection, InactiveJob5);
			}

			JobManagementAmountFilter costFilter = (JobManagementAmountFilter)FilterBO["Job Cost Amount"];

			costFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			costFilter.Property = 0m;
			costFilter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection to contain InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection to contain InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection to contain InactiveJob5", !IsFilterStripForParentTable, FilterCollection, InactiveJob5);

			if (TestDataSupplier.IsRevenueFiltersAdded)
			{
				JobManagementAmountFilter wipFilter = (JobManagementAmountFilter)FilterBO["Job WIP Amount"];

				wipFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				wipFilter.Property = 0m;
				wipFilter.IsActive = true;

				LoadCollection(FilterCollection, FilterBO.Filter);

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

				LoadCollection(FilterCollection, FilterBO.Filter);

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection to contain InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection to contain InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection to contain InactiveJob5", !IsFilterStripForParentTable, FilterCollection, InactiveJob5);
			}

			JobManagementAmountFilter lessThanFilter = (JobManagementAmountFilter)FilterBO["Job Cost Amount"];

			lessThanFilter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
			lessThanFilter.Property = 0m;
			lessThanFilter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", !TestDataSupplier.IsRevenueFiltersAdded, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection not to contain Job4", !TestDataSupplier.IsRevenueFiltersAdded, FilterCollection, Job4);
			AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
			AssertCollection("Expecting collection not to contain job header InactiveJob5", false, FilterCollection, InactiveJob5);

			if (TestDataSupplier.IsRevenueFiltersAdded)
			{
				JobManagementAmountFilter greaterThanFilter = (JobManagementAmountFilter)FilterBO["Job WIP Amount"];

				greaterThanFilter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
				greaterThanFilter.Property = 0m;
				greaterThanFilter.IsActive = true;

				LoadCollection(FilterCollection, FilterBO.Filter);

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);

				LoadCollection(FilterCollection, FilterBO.Filter);

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);
			}
		}

		public void TestHasAccrualWIPFilter()
		{
			BaseLineSetup(Job1, true, true, true, true);  // Has REV, CST, WIP , ACR
			BaseLineSetup(Job2, true, true, true, false); // Has REV, CST, WIP	
			BaseLineSetup(Job3, true, true, false, true); // Has REV, CST,	 , ACR	
			BaseLineSetup(Job4, true, true, false, false);  // Has REV, CST,     ,
			BaseLineSetup(Job5, false, false, true, true);  // Has	  ,    , WIP , ACR
			BaseLineSetup(Job6, false, false, false, false);// Has Nothing

			Factory.Save();

			Assert(FilterBO != null);

			if (FilterBO["Jobs with outstanding Accruals"] != null)
			{
				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding Accruals");

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);

				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding Accruals");

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);
				AssertCollection("Expecting collection not to contain job header InactiveJob1", false, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection not to contain job header InactiveJob3", false, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection not to contain job header InactiveJob5", false, FilterCollection, InactiveJob5);
				AssertCollection("Expecting collection not to contain InactiveJob6", false, FilterCollection, InactiveJob6);
			}

			if (FilterBO["Jobs without any outstanding Accrual"] != null)
			{
				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs without any outstanding Accrual");

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection to contain Job6", true, FilterCollection, Job6);

				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs without any outstanding Accrual");

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection to contain Job6", true, FilterCollection, Job6);
				AssertCollection("Expecting collection to contain InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection to contain InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection to contain job header InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection to contain InactiveJob5", !IsFilterStripForParentTable, FilterCollection, InactiveJob5);
				AssertCollection("Expecting collection to contain job header InactiveJob6", !IsFilterStripForParentTable, FilterCollection, InactiveJob6);
			}

			if (FilterBO["Jobs with outstanding WIPs"] != null)
			{
				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding WIPs");

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);

				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding WIPs");

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);
				AssertCollection("Expecting collection not to contain job header InactiveJob1", false, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection not to contain job header InactiveJob2", false, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection not to contain job header InactiveJob5", false, FilterCollection, InactiveJob5);
				AssertCollection("Expecting collection not to contain InactiveJob6", false, FilterCollection, InactiveJob6);
			}

			if (FilterBO["Jobs without any outstanding WIP"] != null)
			{
				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs without any outstanding WIP");

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
				AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection to contain Job6", true, FilterCollection, Job6);

				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs without any outstanding WIP");

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
				AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection to contain Job6", true, FilterCollection, Job6);
				AssertCollection("Expecting collection not to contain InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection not to contain InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection not to contain job header InactiveJob3", !IsFilterStripForParentTable, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection not to contain job header InactiveJob4", !IsFilterStripForParentTable, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection not to contain InactiveJob5", !IsFilterStripForParentTable, FilterCollection, InactiveJob5);
				AssertCollection("Expecting collection not to contain job header InactiveJob6", !IsFilterStripForParentTable, FilterCollection, InactiveJob6);
			}
		}

		public void TestAmountFiltersTogether()
		{
			BaseLineSetup(Job1, false, true, true, true);
			BaseLineSetup(Job2, true, false, true, true);
			BaseLineSetup(Job3, true, true, false, true);
			BaseLineSetup(Job4, true, true, true, false);

			Factory.Save();

			JobManagementAmountFilter accrualFilter = (JobManagementAmountFilter)FilterBO["Job Accrual Amount"];

			accrualFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			accrualFilter.Property = 0m;
			accrualFilter.IsActive = true;

			JobManagementAmountFilter costFilter = (JobManagementAmountFilter)FilterBO["Job Cost Amount"];

			costFilter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
			costFilter.Property = 199m;
			costFilter.IsActive = true;

			if (TestDataSupplier.IsRevenueFiltersAdded)
			{
				JobManagementAmountFilter revenueFilter = (JobManagementAmountFilter)FilterBO["Job Revenue Amount"];

				revenueFilter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
				revenueFilter.Property = 140m;
				revenueFilter.IsActive = true;

				JobManagementAmountFilter wipFilter = (JobManagementAmountFilter)FilterBO["Job WIP Amount"];

				wipFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				wipFilter.Property = -185m;
				wipFilter.IsActive = true;

				JobManagementAmountFilter profitFilter = (JobManagementAmountFilter)FilterBO["Job Profit Amount"];

				profitFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				profitFilter.Property = -235m;
				profitFilter.IsActive = true;

				JobManagementAmountFilter marginFilter = (JobManagementAmountFilter)FilterBO["Job Margin %"];

				marginFilter.SqlComparisonOperator = SQLComparisonOperator.GreaterThan;
				marginFilter.Property = 400m;
				marginFilter.IsActive = true;
			}

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
			AssertCollection("Expecting collection to contain Job4", true, FilterCollection, Job4);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
			AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
			AssertCollection("Expecting collection not to contain job header InactiveJob4", false, FilterCollection, InactiveJob4);
		}

		public void TestHasAccrualWIPFilterTogether()
		{
			BaseLineSetup(Job1, true, true, true, true);  // Has REV, CST, WIP , ACR
			BaseLineSetup(Job2, true, true, true, false); // Has REV, CST, WIP	
			BaseLineSetup(Job3, true, true, false, true); // Has REV, CST,	 , ACR	
			BaseLineSetup(Job4, true, true, false, false);  // Has REV, CST,     ,
			BaseLineSetup(Job5, false, false, true, true);  // Has	  ,    , WIP , ACR
			BaseLineSetup(Job6, false, false, false, false);// Has Nothing

			Factory.Save();

			Assert(FilterBO != null);

			if (FilterBO["Jobs with outstanding Accruals"] != null && FilterBO["Jobs with outstanding WIPs"] != null)
			{
				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding Accruals", "Jobs with outstanding WIPs");

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);

				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding Accruals", "Jobs with outstanding WIPs");

				AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection to contain Job5", true, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);
				AssertCollection("Expecting collection not to contain job header InactiveJob1", false, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection not to contain job header InactiveJob5", false, FilterCollection, InactiveJob5);
				AssertCollection("Expecting collection not to contain InactiveJob6", false, FilterCollection, InactiveJob6);
			}

			if (FilterBO["Jobs with outstanding Accruals"] != null && FilterBO["Jobs without any outstanding Accrual"] != null)
			{
				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding Accruals", "Jobs without any outstanding Accrual");

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);

				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding Accruals", "Jobs without any outstanding Accrual");

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);
				AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);
				AssertCollection("Expecting collection not to contain InactiveJob6", false, FilterCollection, InactiveJob6);
			}

			if (FilterBO["Jobs with outstanding Accruals"] != null && FilterBO["Jobs without any outstanding WIP"] != null)
			{
				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding Accruals", "Jobs without any outstanding WIP");

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);

				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding Accruals", "Jobs without any outstanding WIP");

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection to contain Job3", true, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);
				AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection not to contain job header InactiveJob3", false, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);
				AssertCollection("Expecting collection not to contain InactiveJob6", false, FilterCollection, InactiveJob6);
			}

			if (FilterBO["Jobs with outstanding WIPs"] != null && FilterBO["Jobs without any outstanding WIP"] != null)
			{
				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding WIPs", "Jobs without any outstanding WIP");

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);

				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding WIPs", "Jobs without any outstanding WIP");

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);
				AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);
				AssertCollection("Expecting collection not to contain InactiveJob6", false, FilterCollection, InactiveJob6);
			}

			if (FilterBO["Jobs with outstanding WIPs"] != null && FilterBO["Jobs without any outstanding Accrual"] != null)
			{
				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding WIPs", "Jobs without any outstanding Accrual");

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);

				ApplyFlagsFilterAndLoadCollection(FilterCollection, "Jobs with outstanding WIPs", "Jobs without any outstanding Accrual");

				AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
				AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
				AssertCollection("Expecting collection not to contain Job3", false, FilterCollection, Job3);
				AssertCollection("Expecting collection not to contain Job4", false, FilterCollection, Job4);
				AssertCollection("Expecting collection not to contain Job5", false, FilterCollection, Job5);
				AssertCollection("Expecting collection not to contain Job6", false, FilterCollection, Job6);
				AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
				AssertCollection("Expecting collection not to contain job header InactiveJob2", false, FilterCollection, InactiveJob2);
				AssertCollection("Expecting collection not to contain InactiveJob3", false, FilterCollection, InactiveJob3);
				AssertCollection("Expecting collection not to contain InactiveJob4", false, FilterCollection, InactiveJob4);
				AssertCollection("Expecting collection not to contain InactiveJob5", false, FilterCollection, InactiveJob5);
				AssertCollection("Expecting collection not to contain InactiveJob6", false, FilterCollection, InactiveJob6);
			}
		}

		#endregion

		#region Billing Filters

		public void TestChargesWithDebtorFilter()
		{
			var debtor1 = Factory.NewWithValidTestData<OrgHeader>();
			var debtor2 = Factory.NewWithValidTestData<OrgHeader>();
			var debtor3 = Factory.NewWithValidTestData<OrgHeader>();

			debtor1.OH_IsDebtor = true;
			debtor2.OH_IsDebtor = true;
			debtor3.OH_IsDebtor = true;

			debtor1.OH_Code = "DEBTOR1";
			debtor2.OH_Code = "DEBTOR2";
			debtor3.OH_Code = "DEBTOR3";

			BaseLineSetup(Job1, true, true, true, true, debtor1);
			BaseLineSetup(Job2, true, true, true, true, debtor2);

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterBO["Charges with Debtor"];

			filter.Property = debtor1.PK;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			filter.Property = debtor3.PK;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			filter.Property = debtor1.PK;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);

			filter.Property = ZGuid.Empty;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain InactiveJob1", true, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain InactiveJob2", true, FilterCollection, InactiveJob2);

			Assert("Charges With Debtor Filter supports Filters Match operator", filter.SupportsFiltersMatchComparisonOperator);
			filter.ComparisonOperator = ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.ModuleFilters.Where(x => x.Visibility == FilterVisibility.AlwaysApplied).ForEach(x => x.Visibility = FilterVisibility.Visible);
			filter.SelectedFilters.AddTextFilterStrip("Code", debtor1.OH_Code);
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
		}

		public void TestChargesWithCreditorFilter()
		{
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			var creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			var creditor3 = Factory.NewWithValidTestData<OrgHeader>();

			creditor1.OH_IsCreditor = true;
			creditor2.OH_IsCreditor = true;
			creditor3.OH_IsCreditor = true;

			creditor1.OH_Code = "CREDITOR1";
			creditor2.OH_Code = "CREDITOR2";
			creditor3.OH_Code = "CREDITOR3";

			BaseLineSetup(Job1, true, true, true, true, null, creditor1);
			BaseLineSetup(Job2, true, true, true, true, null, creditor2);

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterBO["Charges with Creditor"];

			filter.Property = creditor1.PK;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			filter.Property = creditor3.PK;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			filter.Property = creditor1.PK;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain job header InactiveJob2", !IsFilterStripForParentTable, FilterCollection, InactiveJob2);

			filter.Property = ZGuid.Empty;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain InactiveJob1", true, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain InactiveJob2", true, FilterCollection, InactiveJob2);

			Assert("Charges With Creditor Filter supports Filters Match operator", filter.SupportsFiltersMatchComparisonOperator);
			filter.ComparisonOperator = ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.ModuleFilters.Where(x => x.Visibility == FilterVisibility.AlwaysApplied).ForEach(x => x.Visibility = FilterVisibility.Visible);
			var creditor1Filter = filter.SelectedFilters.AddTextFilterStrip("Code", creditor1.OH_Code);
			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain job header InactiveJob1", !IsFilterStripForParentTable, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);
		}

		public void TestSupplierCostReferenceFilter()
		{
			AssertNotNull("FilterStripBusinessObject", FilterBO["Supplier Cost Reference"]);

			BaseLineSetup(Job1, false, false, false, true);
			BaseLineSetup(Job2, false, false, false, true);

			Job job1 = Factory.Load<Job>(Job1.PK);
			job1.Charges.Load();
			job1.Charges[0].JR_CostReference = "ABC";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Supplier Cost Reference"];

			filter.Property = "ABC";
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain job header InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			filter.Property = "XYZ";
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection not to contain Job1", false, FilterCollection, Job1);
			AssertCollection("Expecting collection not to contain Job2", false, FilterCollection, Job2);
			AssertCollection("Expecting collection not to contain InactiveJob1", false, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection not to contain InactiveJob2", false, FilterCollection, InactiveJob2);

			filter.Property = ZString.Empty;
			filter.IsActive = true;

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);

			LoadCollection(FilterCollection, FilterBO.Filter);

			AssertCollection("Expecting collection to contain Job1", true, FilterCollection, Job1);
			AssertCollection("Expecting collection to contain Job2", true, FilterCollection, Job2);
			AssertCollection("Expecting collection to contain InactiveJob1", true, FilterCollection, InactiveJob1);
			AssertCollection("Expecting collection to contain InactiveJob2", true, FilterCollection, InactiveJob2);
		}

		#endregion

		#region Implementation

		JobCharge CreateNewCharge()
		{
			JobCharge charge = Factory.New<JobCharge>();
			charge.JR_AC = TestObjCreator.CC1.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			return charge;
		}

		protected void SetupRevLine(JobHeader jobToSave, ZDecimal amount)
		{
			JobCharge charge1 = CreateNewCharge();
			charge1.JR_JH = jobToSave.PK;
			AccTransactionLines rev1 = Factory.New<AccTransactionLines>();
			SetLine(jobToSave, rev1, TransactionLineTypes.Revenue, amount);
			charge1.JR_AL_ARLine = rev1.PK;
			charge1.SetAmountsFromLinkedLinesForTests();
			rev1.AL_AH = Factory.NewWithValidTestData<ARInvoice>().PK;
		}

		protected void BaseLineSetup(JobHeader jobToSave, ZBool saveRevenue, ZBool saveCost, ZBool saveWIP, ZBool saveAccrual, OrgHeader debtor = null, OrgHeader creditor = null)
		{
			if (saveRevenue || saveCost)
			{
				JobCharge charge11 = CreateNewCharge();
				charge11.JR_JH = jobToSave.PK;
				charge11.JR_OH_SellAccount = debtor?.PK ?? ZGuid.Empty;
				charge11.JR_OH_CostAccount = creditor?.PK ?? ZGuid.Empty;
				JobCharge charge12 = CreateNewCharge();
				charge12.JR_JH = jobToSave.PK;

				charge12.JR_OH_SellAccount = debtor?.PK ?? ZGuid.Empty;
				charge12.JR_OH_CostAccount = creditor?.PK ?? ZGuid.Empty;

				if (saveRevenue)
				{
					AccTransactionLines rev1 = Factory.New<AccTransactionLines>();
					SetLine(jobToSave, rev1, TransactionLineTypes.Revenue, 120m);
					charge11.JR_AL_ARLine = rev1.PK;
					charge11.SetAmountsFromLinkedLinesForTests();
					rev1.AL_AH = Factory.NewWithValidTestData<ARInvoice>().PK;
					rev1.AL_AG = TestObjCreator.GLHeader1.PK;

					AccTransactionLines rev2 = Factory.New<AccTransactionLines>();
					SetLine(jobToSave, rev2, TransactionLineTypes.Revenue, 35m);
					charge12.JR_AL_ARLine = rev2.PK;
					charge12.SetAmountsFromLinkedLinesForTests();
					rev2.AL_AH = Factory.NewWithValidTestData<ARInvoice>().PK;
					rev2.AL_AG = TestObjCreator.GLHeader1.PK;
				}

				if (saveCost)
				{
					AccTransactionLines cost1 = Factory.New<AccTransactionLines>();
					SetLine(jobToSave, cost1, TransactionLineTypes.Cost, -140m);
					charge11.JR_AL_APLine = cost1.PK;
					charge11.SetAmountsFromLinkedLinesForTests();
					cost1.AL_AH = Factory.NewWithValidTestData<APInvoice>().PK;

					AccTransactionLines cost2 = Factory.New<AccTransactionLines>();
					SetLine(jobToSave, cost2, TransactionLineTypes.Cost, -65m);
					charge12.JR_AL_APLine = cost2.PK;
					charge12.SetAmountsFromLinkedLinesForTests();
					cost2.AL_AH = Factory.NewWithValidTestData<APInvoice>().PK;
				}
			}

			if (saveWIP || saveAccrual)
			{
				JobCharge charge21 = CreateNewCharge();
				charge21.JR_JH = jobToSave.PK;
				charge21.JR_OH_SellAccount = debtor?.PK ?? ZGuid.Empty;
				charge21.JR_OH_CostAccount = creditor?.PK ?? ZGuid.Empty;
				JobCharge charge22 = CreateNewCharge();
				charge22.JR_JH = jobToSave.PK;
				charge22.JR_OH_SellAccount = debtor?.PK ?? ZGuid.Empty;
				charge22.JR_OH_CostAccount = creditor?.PK ?? ZGuid.Empty;

				if (saveWIP)
				{
					WIP wIP1 = Factory.New<WIP>();
					SetLine(jobToSave, wIP1, TransactionLineTypes.WIP, 135m);
					charge21.JR_AL_ARLine = wIP1.PK;
					wIP1.AL_AC = charge21.JR_AC;
					TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);

					WIP wIP2 = Factory.New<WIP>();
					SetLine(jobToSave, wIP2, TransactionLineTypes.WIP, 50m);
					charge22.JR_AL_ARLine = wIP2.PK;
					wIP2.AL_AC = charge22.JR_AC;
					TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP2);
				}
				else
				{
					charge21.JR_OSSellAmt = 0m;
					charge22.JR_OSSellAmt = 0m;
				}

				if (saveAccrual)
				{
					Accrual accrual1 = Factory.New<Accrual>();
					SetLine(jobToSave, accrual1, TransactionLineTypes.Accrual, 35m);
					charge21.JR_AL_APLine = accrual1.PK;
					accrual1.AL_AC = charge21.JR_AC;
					TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual1);

					Accrual accrual2 = Factory.New<Accrual>();
					SetLine(jobToSave, accrual2, TransactionLineTypes.Accrual, 55m);
					charge22.JR_AL_APLine = accrual2.PK;
					accrual2.AL_AC = charge22.JR_AC;
					TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual2);

					if (!saveWIP)
					{
						charge21.JR_OSSellAmt = 0m;
						charge22.JR_OSSellAmt = 0m;
					}
				}
				else
				{
					charge21.JR_OSCostAmt = 0m;
					charge22.JR_OSCostAmt = 0m;
				}
			}
		}

		protected void SetLine(JobHeader jobToSave, AccTransactionLines line, ZString lineType, ZDecimal amount)
		{
			line.AL_JH = jobToSave.PK;
			line.AL_LineType = lineType;
			line.AL_OSAmount = line.AL_LineAmount = amount;
			line.AL_ExchangeRate = 1;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_AG = TestObjCreator.GLHeader1.PK;
		}

		void AssertCollection(string message, bool doesContain, IBusinessObjectCollection collection, JobHeader job)
		{
			AssertEquals(message, doesContain, collection.Contains(Job_FilteredBusinessObjectLink[job]));
		}

		void ApplyFlagsFilterAndLoadCollection(IBusinessObjectCollection collection, params string[] filterNames)
		{
			foreach (var filterName in filterNames)
			{
				var filter = (ModuleFlagsFilter)FilterBO[filterName];
				if (filter != null)
				{
					filter.Property0 = true;
					filter.IsActive = true;
				}
			}

			LoadCollection(FilterCollection, FilterBO.Filter);

			foreach (var filterName in filterNames)
			{
				var filter = (ModuleFlagsFilter)FilterBO[filterName];
				if (filter != null)
				{
					filter.IsActive = false;
				}
			}
		}

		void LoadCollection(IBusinessObjectCollection collection, ZQuery additionalFilter)
		{
			IActiveBusinessObjectCollection activeCollection = collection as IActiveBusinessObjectCollection;
			if (activeCollection != null)
			{
				activeCollection.AdditionalFilter = additionalFilter;
			}
			else
			{
				((BusinessObjectCollection)collection).Load(additionalFilter);
			}
		}

		JobHeader Job1;
		JobHeader Job2;
		JobHeader Job3;
		JobHeader Job4;
		JobHeader Job5;
		JobHeader Job6;
		JobHeader InactiveJob1;
		JobHeader InactiveJob2;
		JobHeader InactiveJob3;
		JobHeader InactiveJob4;
		JobHeader InactiveJob5;
		JobHeader InactiveJob6;

		Dictionary<JobHeader, FilteredBOType> Job_FilteredBusinessObjectLink;

		IBusinessObjectCollection FilterCollection;
		FilterStripBusinessObject FilterBO;

		IAccountingFilterStripTestDataSupplier<FilteredBOType> TestDataSupplier;

		BusinessObjectFactory Factory
		{
			get { return TestDataSupplier.Factory; }
		}

		ZFilterGridModule TestModule;

		TestObjectCreator TestObjCreator;

		bool IsFilterStripForParentTable
		{
			get
			{
				return !(typeof(FilteredBOType).IsAssignableFrom(typeof(JobManagement)));
			}
		}

		public void SetUp()
		{
			TestObjCreator = new TestObjectCreator(Factory);

			TestModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(TestDataSupplier.FilterStripModuleID);

			Job_FilteredBusinessObjectLink = new Dictionary<JobHeader, FilteredBOType>();

			JobHeader createJob(FilteredBOType businessObjectForFilterCollection)
			{
				var job = TestDataSupplier.GetJobForBusinessObjectFromFilterCollection(businessObjectForFilterCollection);
				Job_FilteredBusinessObjectLink.Add(job, businessObjectForFilterCollection);
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				// JobHeader is not valid with empty JH_Status.
				job.JH_Status = JobHeaderStatus.Working.Code;
				return job;
			}

			var sixBizOs = TestDataSupplier.GetSixNewBusinessObjectsForFilterCollection();
			Job1 = createJob(sixBizOs.filteredBO1);
			Job2 = createJob(sixBizOs.filteredBO2);
			Job3 = createJob(sixBizOs.filteredBO3);
			Job4 = createJob(sixBizOs.filteredBO4);
			Job5 = createJob(sixBizOs.filteredBO5);
			Job6 = createJob(sixBizOs.filteredBO6);
			Factory.Save();

			sixBizOs = TestDataSupplier.GetSixNewBusinessObjectsForFilterCollection();
			InactiveJob1 = createJob(sixBizOs.filteredBO1);
			Factory.Save();
			InactiveJob1.MarkAsInactive();
			InactiveJob2 = createJob(sixBizOs.filteredBO2);
			Factory.Save();
			InactiveJob2.MarkAsInactive();
			InactiveJob3 = createJob(sixBizOs.filteredBO3);
			Factory.Save();
			InactiveJob3.MarkAsInactive();
			InactiveJob4 = createJob(sixBizOs.filteredBO4);
			Factory.Save();
			InactiveJob4.MarkAsInactive();
			InactiveJob5 = createJob(sixBizOs.filteredBO5);
			Factory.Save();
			InactiveJob5.MarkAsInactive();
			InactiveJob6 = createJob(sixBizOs.filteredBO6);
			Factory.Save();
			InactiveJob6.MarkAsInactive();

			FilterCollection = TestModule.GridCollection;
			FilterBO = TestModule.FilterBusinessObject;

			if (typeof(JobHeader).IsAssignableFrom(typeof(FilteredBOType)))
			{
				var filter = (ModuleTextFilter)FilterBO["Active Status"];
				filter.IsActive = true;
				filter.Property = "All";
			}

			// Need to set this Registry Item to prevent reversing WIPs for the Enterprise.Freight.Agency.Business.ContainerDetention#
			Enterprise.Registry.Business.LinerAgencyDataRegistry.Instance.CreateWIPAccrualsForDetentionCharges.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
		}

		public void TearDown()
		{
			Job1.Dispose();
			Job2.Dispose();
			Job3.Dispose();
			Job4.Dispose();
			Job5.Dispose();
			Job6.Dispose();
			InactiveJob1.Dispose();
			InactiveJob2.Dispose();
			InactiveJob3.Dispose();
			InactiveJob4.Dispose();
			InactiveJob5.Dispose();
			InactiveJob6.Dispose();
			TestModule.Dispose();
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion

	}
}
