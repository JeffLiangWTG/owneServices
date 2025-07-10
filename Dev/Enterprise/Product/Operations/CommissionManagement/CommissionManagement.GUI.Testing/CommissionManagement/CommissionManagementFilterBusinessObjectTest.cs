using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.CommissionManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.CommissionManagement.GUI.CommissionManagementFilterBusinessObject;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(CommissionManagementFilterBusinessObject))]
	internal class CommissionManagementFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filter

		public void TestFilter_OnlyShowCommissionsForCurrentLoginCompanyRegistry()
		{
			var companyA = Factory.NewWithValidTestData<GlbCompany>();
			var companyB = Factory.NewWithValidTestData<GlbCompany>();
			var branchA = Factory.NewWithValidTestData<GlbBranch>();
			branchA.GB_GC = companyA.PK;
			var branchB = Factory.NewWithValidTestData<GlbBranch>();
			branchB.GB_GC = companyB.PK;

			var commissionHeaderA = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderA.CH0_GC = companyA.PK;
			commissionHeaderA.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderA.CH0_GroupingSourceID = ZGuid.NewZGuid();
			var commissionLineA1 = commissionHeaderA.Lines.AddNew();
			commissionLineA1.FillWithValidTestData();
			var commissionLineA2 = commissionHeaderA.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLineA2.FillWithValidTestData();

			var commissionHeaderB = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderB.CH0_GC = companyB.PK;
			commissionHeaderB.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderB.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionLineB1 = commissionHeaderB.Lines.AddNew();
			commissionLineB1.FillWithValidTestData();
			var commissionLineB2 = commissionHeaderB.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLineB2.FillWithValidTestData();

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branchA.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var filterBizObj = GetNewFilterStripBusinessObject();

				OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineA1, commissionLineA2 });

				OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineA1, commissionLineA2, commissionLineB1, commissionLineB2 });
			}
		}

		public void TestFilter_OnlyShowCurrentStaffWhenSecurityDisallowed()
		{
			var staff1Branch = Factory.NewWithValidTestData<GlbBranch>();
			var staff1Company = Factory.NewWithValidTestData<GlbCompany>();
			var staff1Organisation = Factory.NewWithValidTestData<OrgHeader>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			staff1.GS_GB_HomeBranch = staff1Branch.PK;
			staff1Company.GC_OH_OrgProxy = staff1Organisation.PK;
			staff1Branch.GB_GC = staff1Company.PK;

			var staff2Organisation = Factory.NewWithValidTestData<OrgHeader>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SCW";

			var commissionLine1A = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLine1A.CL0_GS_NKStaff = "ADL";
			commissionLine1A.CL0_OH_Party = staff1Organisation.PK;
			var commissionLine1B = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLine1B.CL0_GS_NKStaff = "ADL";
			commissionLine1B.CL0_OH_Party = staff1Organisation.PK;
			var commissionLine2A = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLine2A.CL0_GS_NKStaff = "SCW";
			commissionLine2A.CL0_OH_Party = staff2Organisation.PK;
			var commissionLine2B = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLine2B.CL0_GS_NKStaff = "SCW";
			commissionLine2B.CL0_OH_Party = staff2Organisation.PK;
			var commissionLineForOrg = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLineForOrg.CL0_GS_NKStaff = "";
			commissionLineForOrg.CL0_OH_Party = staff1Organisation.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), staff1Branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				var filterBizObj = GetNewFilterStripBusinessObject();

				Env.Security.CommissionManagerViewForAnyEntity.IsAllowed = false;
				Env.Security.CommissionManagerViewForAnyEntityInOrganisation.IsAllowed = false;
				AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine1A, commissionLine1B });

				Env.Security.CommissionManagerViewForAnyEntity.IsAllowed = false;
				Env.Security.CommissionManagerViewForAnyEntityInOrganisation.IsAllowed = true;
				AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine1A, commissionLine1B, commissionLineForOrg });

				Env.Security.CommissionManagerViewForAnyEntity.IsAllowed = true;
				Env.Security.CommissionManagerViewForAnyEntityInOrganisation.IsAllowed = true;
				AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine1A, commissionLine1B, commissionLine2A, commissionLine2B, commissionLineForOrg });
			}
		}

		public void TestFilter_DoNotShowUncommissionableLinesUnlessTheyHaveBeenApprovedOrPaid()
		{
			var header = Factory.NewWithValidTestData<AccCommissionHeader>();

			var headerLine = header.Lines.AddNew();
			headerLine.FillWithValidTestData();

			var commissionableLineGroup = header.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true)));
			var commissionableLineGroupLine = commissionableLineGroup.Lines.AddNew();
			commissionableLineGroupLine.FillWithValidTestData();

			var uncommissionableLineGroup = header.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, false)));
			var uncommissionableLineGroupLine = uncommissionableLineGroup.Lines.AddNew();
			uncommissionableLineGroupLine.FillWithValidTestData();

			var approvedUncommissionableLineGroupLine = uncommissionableLineGroup.Lines.AddNew();
			approvedUncommissionableLineGroupLine.CL0_ApprovedDateTimeUtc = new ZDateTime(2002, 2, 2);
			approvedUncommissionableLineGroupLine.FillWithValidTestData();

			var paidUncommissionableLineGroupLine = uncommissionableLineGroup.Lines.AddNew();
			paidUncommissionableLineGroupLine.CL0_PaidDateTimeUtc = new ZDateTime(2002, 2, 2);
			paidUncommissionableLineGroupLine.FillWithValidTestData();

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { headerLine, commissionableLineGroupLine, approvedUncommissionableLineGroupLine, paidUncommissionableLineGroupLine });
		}

		#endregion

		#region Module Filters

		#region CommissionHeader Filters

		public void TestCompanyFilter()
		{
			var companyA = Factory.NewWithValidTestData<GlbCompany>();
			var companyABranch = companyA.Branches.AddNew();
			companyABranch.GB_Code = "AAA";
			var companyB = Factory.NewWithValidTestData<GlbCompany>();
			var companyBBranch = companyB.Branches.AddNew();
			companyBBranch.GB_Code = "BBB";

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var jobRelatedInvoiceA = Factory.NewWithValidTestData<ARInvoice>();
			jobRelatedInvoiceA.AH_GC = companyA.PK;
			jobRelatedInvoiceA.AH_GB = companyABranch.PK;
			jobRelatedInvoiceA.AH_JH = job.PK;
			var jobRelatedInvoiceB = Factory.NewWithValidTestData<ARInvoice>();
			jobRelatedInvoiceB.AH_GC = companyB.PK;
			jobRelatedInvoiceB.AH_GB = companyBBranch.PK;
			jobRelatedInvoiceB.AH_JH = job.PK;

			var invoiceA = Factory.NewWithValidTestData<ARInvoice>();
			invoiceA.AH_GC = companyA.PK;
			invoiceA.AH_GB = companyABranch.PK;
			var invoiceB = Factory.NewWithValidTestData<ARCreditNote>();
			invoiceB.AH_GC = companyB.PK;
			invoiceB.AH_GB = companyBBranch.PK;

			var jobRelatedInvoiceACommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			jobRelatedInvoiceACommissionHeader.CH0_GC = companyA.PK;
			jobRelatedInvoiceACommissionHeader.CH0_AH_Source = jobRelatedInvoiceA.PK;
			jobRelatedInvoiceACommissionHeader.CH0_GroupingSourceTableCode = job.TablePrefix;
			jobRelatedInvoiceACommissionHeader.CH0_GroupingSourceID = job.PK;
			var jobRelatedInvoiceACommissionHeaderLineA = jobRelatedInvoiceACommissionHeader.Lines.AddNew();
			jobRelatedInvoiceACommissionHeaderLineA.FillWithValidTestData();
			var jobRelatedInvoiceACommissionHeaderLineB = jobRelatedInvoiceACommissionHeader.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			jobRelatedInvoiceACommissionHeaderLineB.FillWithValidTestData();

			var jobRelatedInvoiceBCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			jobRelatedInvoiceBCommissionHeader.CH0_GC = companyB.PK;
			jobRelatedInvoiceBCommissionHeader.CH0_AH_Source = jobRelatedInvoiceB.PK;
			jobRelatedInvoiceBCommissionHeader.CH0_GroupingSourceTableCode = job.TablePrefix;
			jobRelatedInvoiceBCommissionHeader.CH0_GroupingSourceID = job.PK;
			var jobRelatedInvoiceBCommissionHeaderLineA = jobRelatedInvoiceBCommissionHeader.Lines.AddNew();
			jobRelatedInvoiceBCommissionHeaderLineA.FillWithValidTestData();
			var jobRelatedInvoiceBCommissionHeaderLineB = jobRelatedInvoiceBCommissionHeader.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			jobRelatedInvoiceBCommissionHeaderLineB.FillWithValidTestData();

			var invoiceACommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			invoiceACommissionHeader.CH0_GC = companyA.PK;
			invoiceACommissionHeader.CH0_AH_Source = invoiceA.PK;
			invoiceACommissionHeader.CH0_GroupingSourceTableCode = invoiceA.TablePrefix;
			invoiceACommissionHeader.CH0_GroupingSourceID = invoiceA.PK;
			var invoiceACommissionHeaderLineA = invoiceACommissionHeader.Lines.AddNew();
			invoiceACommissionHeaderLineA.FillWithValidTestData();
			var invoiceACommissionHeaderLineB = invoiceACommissionHeader.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			invoiceACommissionHeaderLineB.FillWithValidTestData();

			var invoiceBCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			invoiceBCommissionHeader.CH0_GC = companyB.PK;
			invoiceBCommissionHeader.CH0_AH_Source = invoiceB.PK;
			invoiceBCommissionHeader.CH0_GroupingSourceTableCode = invoiceA.TablePrefix;
			invoiceBCommissionHeader.CH0_GroupingSourceID = invoiceA.PK;
			var invoiceBCommissionHeaderLineA = invoiceBCommissionHeader.Lines.AddNew();
			invoiceBCommissionHeaderLineA.FillWithValidTestData();
			var invoiceBCommissionHeaderLineB = invoiceBCommissionHeader.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			invoiceBCommissionHeaderLineB.FillWithValidTestData();

			Factory.Save();

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var filterBizObj = GetNewFilterStripBusinessObject();
			var companyFilter = (ModuleGuidFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.Company];
			AssertNotNull(companyFilter);
			AssertEquals("Company", companyFilter.MultilingualDescription);
			companyFilter.IsActive = true;

			companyFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			companyFilter.Property = companyA.PK;
			AssertContainsExactElementsInAnyOrder(filterBizObj,
				new[] { jobRelatedInvoiceACommissionHeaderLineA, jobRelatedInvoiceACommissionHeaderLineB, invoiceACommissionHeaderLineA, invoiceACommissionHeaderLineB });

			companyFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			companyFilter.Property = companyA.PK;
			AssertContainsExactElementsInAnyOrder(filterBizObj,
				new[] { jobRelatedInvoiceBCommissionHeaderLineA, jobRelatedInvoiceBCommissionHeaderLineB, invoiceBCommissionHeaderLineA, invoiceBCommissionHeaderLineB });
		}

		public void TestCompanyFilter_OnlyAvailableWhenShowOnlyCurrentCompanyRegistryIsFalse()
		{
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var filterBizObj = GetNewFilterStripBusinessObject();
			var companyFilter = (ModuleGuidFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.Company];
			AssertNotNull(companyFilter);

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			filterBizObj = GetNewFilterStripBusinessObject();
			companyFilter = (ModuleGuidFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.Company];
			AssertNull(companyFilter);
		}

		public void TestRecognitionDateFilter()
		{
			var testHelper = new TestObjectCreator(Factory);
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true).AddToFilter(AccChargeCodeSchema.AC_Code, "CCLR"));
			var aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			var job1ARInvoiceA = Factory.NewWithValidTestData<ARInvoice>();
			job1ARInvoiceA.AH_JH = job1.PK;

			var job1ARInvoiceB = Factory.NewWithValidTestData<ARInvoice>();
			job1ARInvoiceB.AH_JH = job1.PK;

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var invoiceReversal = Factory.NewWithValidTestData<ARCreditNote>();
			invoiceReversal.AH_TransactionBelongsToGroup = invoice.PK;

			var job1ARInvoiceACommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			job1ARInvoiceACommissionHeader.CH0_AH_Source = job1ARInvoiceA.PK;
			job1ARInvoiceACommissionHeader.CH0_GroupingSourceTableCode = job1.TablePrefix;
			job1ARInvoiceACommissionHeader.CH0_GroupingSourceID = job1.PK;
			job1ARInvoiceACommissionHeader.CH0_CommissionDate = new ZDate(2004, 1, 1);
			var job1ARInvoiceACommissionLineA = job1ARInvoiceACommissionHeader.Lines.AddNew();
			job1ARInvoiceACommissionLineA.FillWithValidTestData();
			var job1ARInvoiceACommissionLineB = job1ARInvoiceACommissionHeader.LineGroups.AddNew(chargeCode).Lines.AddNew();
			job1ARInvoiceACommissionHeader.LineGroups[0].CLG_CommissionDate = new ZDate(2004, 6, 30);
			job1ARInvoiceACommissionLineB.FillWithValidTestData();

			var job1ARInvoiceBCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			job1ARInvoiceBCommissionHeader.CH0_AH_Source = job1ARInvoiceB.PK;
			job1ARInvoiceBCommissionHeader.CH0_GroupingSourceTableCode = job1.TablePrefix;
			job1ARInvoiceBCommissionHeader.CH0_GroupingSourceID = job1.PK;
			job1ARInvoiceBCommissionHeader.CH0_CommissionDate = new ZDate(2005, 1, 1);
			var job1ARInvoiceBCommissionLineA = job1ARInvoiceBCommissionHeader.Lines.AddNew();
			job1ARInvoiceBCommissionLineA.FillWithValidTestData();
			var job1ARInvoiceBCommissionLineB = job1ARInvoiceBCommissionHeader.LineGroups.AddNew(chargeCode).Lines.AddNew();
			job1ARInvoiceBCommissionLineB.FillWithValidTestData();

			var invoiceCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			invoiceCommissionHeader.CH0_AH_Source = invoice.PK;
			invoiceCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			invoiceCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			invoiceCommissionHeader.CH0_CommissionDate = new ZDate(2010, 1, 1);
			var invoiceCommissionHeaderLineA = invoiceCommissionHeader.Lines.AddNew();
			invoiceCommissionHeaderLineA.FillWithValidTestData();
			var invoiceCommissionHeaderLineB = invoiceCommissionHeader.LineGroups.AddNew(chargeCode).Lines.AddNew();
			invoiceCommissionHeaderLineB.FillWithValidTestData();

			var reversalCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			reversalCommissionHeader.CH0_AH_Source = invoiceReversal.PK;
			reversalCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			reversalCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			reversalCommissionHeader.CH0_CommissionDate = new ZDate(2015, 1, 1);
			var reversalCommissionHeaderLineA = reversalCommissionHeader.Lines.AddNew();
			reversalCommissionHeaderLineA.FillWithValidTestData();
			var reversalCommissionHeaderLineB = reversalCommissionHeader.LineGroups.AddNew(chargeCode).Lines.AddNew();
			reversalCommissionHeader.LineGroups[0].CLG_CommissionDate = new ZDate(2015, 6, 30);
			reversalCommissionHeaderLineB.FillWithValidTestData();

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var recognitionDateFilter = (ModuleDateFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.RecognitionDate];
			AssertNotNull(recognitionDateFilter);
			AssertEquals("Recognition Date", recognitionDateFilter.MultilingualDescription);
			recognitionDateFilter.IsActive = true;

			recognitionDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			recognitionDateFilter.Property1 = new ZDateTime(2005, 1, 1);
			recognitionDateFilter.Property2 = new ZDateTime(2010, 1, 1);

			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { job1ARInvoiceBCommissionLineA, job1ARInvoiceBCommissionLineB, invoiceCommissionHeaderLineA, invoiceCommissionHeaderLineB });

			recognitionDateFilter.Property1 = new ZDateTime(2004, 1, 3);
			recognitionDateFilter.Property2 = new ZDateTime(2004, 12, 31);

			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { job1ARInvoiceACommissionLineB });

			recognitionDateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { job1ARInvoiceACommissionLineA, job1ARInvoiceACommissionLineB, job1ARInvoiceBCommissionLineA, job1ARInvoiceBCommissionLineB, invoiceCommissionHeaderLineA, invoiceCommissionHeaderLineB, reversalCommissionHeaderLineA, reversalCommissionHeaderLineB });

			recognitionDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertContainsExactElementsInAnyOrder(filterBizObj, Enumerable.Empty<AccCommissionLine>());

			recognitionDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			recognitionDateFilter.Property1 = new ZDateTime(2010, 1, 1);
			recognitionDateFilter.Property2 = new ZDateTime(2015, 6, 1);
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { invoiceCommissionHeaderLineA, invoiceCommissionHeaderLineB, reversalCommissionHeaderLineA });
		}

		public void TestCustomerFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var commissionHeader1 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader1.CH0_OH_Customer = org1.PK;
			commissionHeader1.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeader1.CH0_GroupingSourceID = ZGuid.NewZGuid();
			var commissionLine1A = commissionHeader1.Lines.AddNew();
			commissionLine1A.FillWithValidTestData();
			var commissionLine1B = commissionHeader1.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLine1B.FillWithValidTestData();

			var commissionHeader2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader2.CH0_OH_Customer = org2.PK;
			commissionHeader2.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeader2.CH0_GroupingSourceID = ZGuid.NewZGuid();
			var commissionLine2A = commissionHeader2.Lines.AddNew();
			commissionLine2A.FillWithValidTestData();
			var commissionLine2B = commissionHeader2.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLine2B.FillWithValidTestData();

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var customerFilter = (ModuleGuidFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.Customer];
			AssertNotNull(customerFilter);
			AssertEquals("Customer", customerFilter.MultilingualDescription);
			customerFilter.IsActive = true;

			customerFilter.Property = org1.PK;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine1A, commissionLine1B });

			customerFilter.Property = org2.PK;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine2A, commissionLine2B });
		}

		public void TestCommissionStreamFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var commissionHeader1 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader1.CH0_CommissionStream = "";
			commissionHeader1.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeader1.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionLine1A = commissionHeader1.Lines.AddNew();
			commissionLine1A.FillWithValidTestData();
			var commissionLine1B = commissionHeader1.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLine1B.FillWithValidTestData();

			var commissionHeader2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader2.CH0_CommissionStream = "AAA";
			commissionHeader2.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeader2.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionLine2A = commissionHeader2.Lines.AddNew();
			commissionLine2A.FillWithValidTestData();
			var commissionLine2B = commissionHeader2.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLine2B.FillWithValidTestData();

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var streamFilter = (ModuleTextFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.CommissionStream];
			AssertNotNull(streamFilter);
			AssertEquals("Commission Stream", streamFilter.MultilingualDescription);
			streamFilter.IsActive = true;

			streamFilter.Property = "";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine1A, commissionLine1B, commissionLine2A, commissionLine2B });

			streamFilter.Property = "AAA";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine2A, commissionLine2B });
		}

		public void TestProductFilter()
		{
			var commissionHeaderALL = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderALL.CH0_Product = OrgCommissionAgreementItemLookups.AllProductsCode;
			commissionHeaderALL.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderALL.CH0_GroupingSourceID = ZGuid.NewZGuid();
			var commissionLineALL1 = commissionHeaderALL.Lines.AddNew();
			commissionLineALL1.FillWithValidTestData();
			var commissionLineALL2 = commissionHeaderALL.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLineALL2.FillWithValidTestData();

			var commissionHeaderXXX = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderXXX.CH0_Product = "XXX";
			commissionHeaderXXX.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderXXX.CH0_GroupingSourceID = ZGuid.NewZGuid();
			var commissionLineXXX1 = commissionHeaderXXX.Lines.AddNew();
			commissionLineXXX1.FillWithValidTestData();
			var commissionLineXXX2 = commissionHeaderXXX.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLineXXX2.FillWithValidTestData();

			var commissionHeaderYYY = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderYYY.CH0_Product = "YYY";
			commissionHeaderYYY.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderYYY.CH0_GroupingSourceID = ZGuid.NewZGuid();
			var commissionLineYYY1 = commissionHeaderYYY.Lines.AddNew();
			commissionLineYYY1.FillWithValidTestData();
			var commissionLineYYY2 = commissionHeaderYYY.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLineYYY2.FillWithValidTestData();

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var productFilter = (ModuleTextFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.Product];
			AssertNotNull(productFilter);
			AssertEquals("Product", productFilter.MultilingualDescription);
			productFilter.IsActive = true;

			productFilter.Property = OrgCommissionAgreementItemLookups.AllProductsCode;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineALL1, commissionLineALL2 });

			productFilter.Property = "XXX";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineXXX1, commissionLineXXX2 });

			productFilter.Property = "YYY";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineYYY1, commissionLineYYY2 });
		}

		public void TestServiceFilter()
		{
			var commissionHeaderALL = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderALL.CH0_Service = OrgCommissionAgreementItemLookups.AllServicesCode;
			commissionHeaderALL.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderALL.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionLineALL1 = commissionHeaderALL.Lines.AddNew();
			commissionLineALL1.FillWithValidTestData();
			var commissionLineALL2 = commissionHeaderALL.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLineALL2.FillWithValidTestData();

			var commissionHeaderXXX = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderXXX.CH0_Service = "XXX";
			commissionHeaderXXX.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderXXX.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionLineXXX1 = commissionHeaderXXX.Lines.AddNew();
			commissionLineXXX1.FillWithValidTestData();
			var commissionLineXXX2 = commissionHeaderXXX.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLineXXX2.FillWithValidTestData();

			var commissionHeaderYYY = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderYYY.CH0_Service = "YYY";
			commissionHeaderYYY.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderYYY.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionLineYYY1 = commissionHeaderYYY.Lines.AddNew();
			commissionLineYYY1.FillWithValidTestData();
			var commissionLineYYY2 = commissionHeaderYYY.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLineYYY2.FillWithValidTestData();

			Factory.Save();

			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(true))
			{
				var filterBizObj = GetNewFilterStripBusinessObject();
				var serviceFilter = (ModuleTextFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.Service];
				AssertNotNull(serviceFilter);
				AssertEquals("Service", serviceFilter.MultilingualDescription);
				serviceFilter.IsActive = true;

				serviceFilter.Property = OrgCommissionAgreementItemLookups.AllServicesCode;
				AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineALL1, commissionLineALL2 });

				serviceFilter.Property = "XXX";
				AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineXXX1, commissionLineXXX2 });

				serviceFilter.Property = "YYY";
				AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineYYY1, commissionLineYYY2 });
			}
		}

		public void TestSubModuleFilter()
		{
			var commissionHeaderALL = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderALL.CH0_SubModule = OrgCommissionAgreementItemLookups.AllSubModulesCode;
			commissionHeaderALL.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderALL.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionLineALL1 = commissionHeaderALL.Lines.AddNew();
			commissionLineALL1.FillWithValidTestData();
			var commissionLineALL2 = commissionHeaderALL.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLineALL2.FillWithValidTestData();

			var commissionHeaderXXX = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderXXX.CH0_SubModule = "XXX";
			commissionHeaderXXX.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderXXX.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionLineXXX1 = commissionHeaderXXX.Lines.AddNew();
			commissionLineXXX1.FillWithValidTestData();
			var commissionLineXXX2 = commissionHeaderXXX.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLineXXX2.FillWithValidTestData();

			var commissionHeaderYYY = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderYYY.CH0_SubModule = "YYY";
			commissionHeaderYYY.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderYYY.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionLineYYY1 = commissionHeaderYYY.Lines.AddNew();
			commissionLineYYY1.FillWithValidTestData();
			var commissionLineYYY2 = commissionHeaderYYY.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLineYYY2.FillWithValidTestData();

			Factory.Save();

			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(true))
			{
				var filterBizObj = GetNewFilterStripBusinessObject();
				var subModuleFilter = (ModuleTextFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.SubModule];
				AssertNotNull(subModuleFilter);
				AssertEquals("Sub-Module", subModuleFilter.MultilingualDescription);
				subModuleFilter.IsActive = true;

				subModuleFilter.Property = OrgCommissionAgreementItemLookups.AllSubModulesCode;
				AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineALL1, commissionLineALL2 });

				subModuleFilter.Property = "XXX";
				AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineXXX1, commissionLineXXX2 });

				subModuleFilter.Property = "YYY";
				AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineYYY1, commissionLineYYY2 });
			}
		}

		public void TestTransactionNumberFilter()
		{
			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_TransactionNum = "00001000";
			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_TransactionNum = "00001001";
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_JobNum = "00001001";

			var invoiceCommissionHeader1 = Factory.NewWithValidTestData<AccCommissionHeader>();
			invoiceCommissionHeader1.CH0_GroupingSourceTableCode = invoice1.TablePrefix;
			invoiceCommissionHeader1.CH0_GroupingSourceID = invoice1.PK;
			var invoiceCommissionLine1A = invoiceCommissionHeader1.Lines.AddNew();
			invoiceCommissionLine1A.FillWithValidTestData();
			var invoiceCommissionLine1B = invoiceCommissionHeader1.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			invoiceCommissionLine1B.FillWithValidTestData();

			var invoiceCommissionHeader2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			invoiceCommissionHeader2.CH0_GroupingSourceTableCode = invoice2.TablePrefix;
			invoiceCommissionHeader2.CH0_GroupingSourceID = invoice2.PK;
			var invoiceCommissionLine2A = invoiceCommissionHeader2.Lines.AddNew();
			invoiceCommissionLine2A.FillWithValidTestData();
			var invoiceCommissionLine2B = invoiceCommissionHeader2.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			invoiceCommissionLine2B.FillWithValidTestData();

			var jobCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			jobCommissionHeader.CH0_GroupingSourceTableCode = job.TablePrefix;
			jobCommissionHeader.CH0_GroupingSourceID = job.PK;
			var jobCommissionLineA = jobCommissionHeader.Lines.AddNew();
			jobCommissionLineA.FillWithValidTestData();
			var jobCommissionLineB = jobCommissionHeader.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			jobCommissionLineB.FillWithValidTestData();

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var transactionNumberFilter = (ModuleTextFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.TransactionNumber];
			AssertNotNull(transactionNumberFilter);
			AssertEquals("Transaction Number", transactionNumberFilter.MultilingualDescription);
			transactionNumberFilter.IsActive = true;

			transactionNumberFilter.Property = "00001000";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { invoiceCommissionLine1A, invoiceCommissionLine1B });

			transactionNumberFilter.Property = "00001001";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { invoiceCommissionLine2A, invoiceCommissionLine2B });
		}

		public void TestJobNumberFilter()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_JobNum = "00001001";
			var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_JobNum = "00001002";
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_TransactionNum = "00001001";

			var jobCommissionHeader1 = Factory.NewWithValidTestData<AccCommissionHeader>();
			jobCommissionHeader1.CH0_GroupingSourceTableCode = job1.TablePrefix;
			jobCommissionHeader1.CH0_GroupingSourceID = job1.PK;
			jobCommissionHeader1.CH0_JobNumber = job1.JH_JobNum;
			var jobCommissionLine1A = jobCommissionHeader1.Lines.AddNew();
			jobCommissionLine1A.FillWithValidTestData();
			var jobCommissionLine1B = jobCommissionHeader1.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			jobCommissionLine1B.FillWithValidTestData();

			var jobCommissionHeader2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			jobCommissionHeader2.CH0_GroupingSourceTableCode = job2.TablePrefix;
			jobCommissionHeader2.CH0_GroupingSourceID = job2.PK;
			jobCommissionHeader2.CH0_JobNumber = job2.JH_JobNum;
			var jobCommissionLine2A = jobCommissionHeader2.Lines.AddNew();
			jobCommissionLine2A.FillWithValidTestData();
			var jobCommissionLine2B = jobCommissionHeader2.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			jobCommissionLine2B.FillWithValidTestData();

			var invoiceCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			invoiceCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			invoiceCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			var invoiceCommissionLineA = invoiceCommissionHeader.Lines.AddNew();
			invoiceCommissionLineA.FillWithValidTestData();
			var invoiceCommissionLineB = invoiceCommissionHeader.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			invoiceCommissionLineB.FillWithValidTestData();

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var jobNumberFilter = (ModuleTextFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.JobNumber];
			AssertNotNull(jobNumberFilter);
			AssertEquals("Job Number", jobNumberFilter.MultilingualDescription);
			jobNumberFilter.IsActive = true;

			jobNumberFilter.Property = "00001001";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { jobCommissionLine1A, jobCommissionLine1B });

			jobNumberFilter.Property = "00001002";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { jobCommissionLine2A, jobCommissionLine2B });
		}

		public void TestSnapshotDateFilter()
		{
			var commissionHeader1 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader1.CH0_SnapshotDateTime = new ZDateTime(2001, 1, 1);
			commissionHeader1.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeader1.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionLine1A = commissionHeader1.Lines.AddNew();
			commissionLine1A.FillWithValidTestData();
			var commissionLine1B = commissionHeader1.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLine1B.FillWithValidTestData();

			var commissionHeader2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader2.CH0_SnapshotDateTime = new ZDateTime(2002, 1, 1);
			commissionHeader2.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeader2.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionLine2A = commissionHeader2.Lines.AddNew();
			commissionLine2A.FillWithValidTestData();
			var commissionLine2B = commissionHeader2.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLine2B.FillWithValidTestData();

			var commissionHeader3 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader3.CH0_SnapshotDateTime = new ZDateTime(2003, 1, 1);
			commissionHeader3.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeader3.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionLine3A = commissionHeader3.Lines.AddNew();
			commissionLine3A.FillWithValidTestData();
			var commissionLine3B = commissionHeader3.LineGroups.AddNew(Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_IsCommissionable, true))).Lines.AddNew();
			commissionLine3B.FillWithValidTestData();

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var snapshotDateFilter = (ModuleDateFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.SnapshotDate];
			AssertNotNull(snapshotDateFilter);
			AssertEquals("Snapshot Date", snapshotDateFilter.MultilingualDescription);
			snapshotDateFilter.IsActive = true;

			snapshotDateFilter.Property1 = new ZDateTime(2001, 1, 1);
			snapshotDateFilter.Property2 = new ZDateTime(2002, 1, 1);
			snapshotDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine1A, commissionLine1B, commissionLine2A, commissionLine2B });
		}

		#endregion

		#region CommissionAgreement Filters

		public void TestAgreementIdFilter()
		{
			var opportunityA = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunityA.P8_OpportunityID = "O00001001";

			var agreementA1 = opportunityA.ApprovedCommissionAgreements.AddNew();
			agreementA1.CA0_Name = "#1";
			agreementA1.FillWithValidTestData();
			var agreementA1recipient = agreementA1.Recipients.AddNew();
			agreementA1recipient.FillWithValidTestData();
			var agreementA1recipientRate = agreementA1recipient.Rates.AddNew();

			var agreementA2 = opportunityA.ApprovedCommissionAgreements.AddNew();
			agreementA2.CA0_Name = "#2";
			agreementA2.FillWithValidTestData();
			var agreementA2recipient = agreementA2.Recipients.AddNew();
			agreementA2recipient.FillWithValidTestData();
			var agreementA2recipientRate = agreementA2recipient.Rates.AddNew();

			var opportunityB = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunityB.P8_OpportunityID = "O00001002";

			var agreementB1 = opportunityB.ApprovedCommissionAgreements.AddNew();
			agreementB1.CA0_Name = "#1";
			agreementB1.FillWithValidTestData();
			var agreementB1recipient = agreementB1.Recipients.AddNew();
			agreementB1recipient.FillWithValidTestData();
			var agreementB1recipientRate = agreementB1recipient.Rates.AddNew();

			var agreementB2 = opportunityB.ApprovedCommissionAgreements.AddNew();
			agreementB2.CA0_Name = "#2";
			agreementB2.FillWithValidTestData();
			var agreementB2recipient = agreementB2.Recipients.AddNew();
			agreementB2recipient.FillWithValidTestData();
			var agreementB2recipientRate = agreementB2recipient.Rates.AddNew();

			var commissionLineA1 = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLineA1.CL0_CAT = agreementA1recipientRate.PK;
			var commissionLineA2 = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLineA2.CL0_CAT = agreementA2recipientRate.PK;
			var commissionLineB1 = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLineB1.CL0_CAT = agreementB1recipientRate.PK;
			var commissionLineB2 = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLineB2.CL0_CAT = agreementB2recipientRate.PK;

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var agreementIdFilter = (ModuleTextFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.AgreementId];
			AssertNotNull(agreementIdFilter);
			AssertEquals("Agreement ID", agreementIdFilter.MultilingualDescription);
			agreementIdFilter.IsActive = true;
			agreementIdFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

			agreementIdFilter.Property = "O00001001";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineA1, commissionLineA2 });

			agreementIdFilter.Property = "O00001002";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineB1, commissionLineB2 });

			agreementIdFilter.Property = "O00001001#1";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineA1 });

			agreementIdFilter.Property = "O00001001#2";
			agreementIdFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLineA1, commissionLineB1, commissionLineB2 });
		}

		#endregion

		#region CommissionLine Filters

		public void TestEntityStaffFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SCW";
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var commissionLine1A = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLine1A.CL0_GS_NKStaff = "ADL";
			var commissionLine1B = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLine1B.CL0_GS_NKStaff = "ADL";
			var commissionLine2A = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLine2A.CL0_GS_NKStaff = "SCW";
			var commissionLine2B = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLine2B.CL0_GS_NKStaff = "SCW";
			var commissionLineForOrg = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLineForOrg.CL0_OH_Party = org.PK;

			Factory.Save();

			Env.Security.CommissionManagerViewForAnyEntity.IsAllowed = true;

			var filterBizObj = GetNewFilterStripBusinessObject();
			var entityStaffFilter = (ModuleNkFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.EntityStaff];
			AssertNotNull(entityStaffFilter);
			CombineAssertions(() =>
			{
				AssertEquals("MultilingualDescription", "Entity Staff", entityStaffFilter.MultilingualDescription);
				AssertEquals("Category", FilterCategories.Organisations, entityStaffFilter.Category);

				AssertEquals("Visibility", FilterVisibility.Visible, entityStaffFilter.Visibility);
				AssertEquals("HasComparisonOperator", true, entityStaffFilter.HasComparisonOperator);
				AssertEquals("DefaultProperty", ZString.Empty, entityStaffFilter.DefaultProperty);
			});

			entityStaffFilter.IsActive = true;

			entityStaffFilter.Property = "ADL";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine1A, commissionLine1B });

			entityStaffFilter.Property = "SCW";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine2A, commissionLine2B });
		}

		public void TestEntityStaffFilter_WithoutViewAnySecurityRight()
		{
			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			Env.Security.CommissionManagerViewForAnyEntity.IsAllowed = false;
			Env.Security.CommissionManagerViewForAnyEntityInOrganisation.IsAllowed = false;

			var filterBizObj = GetNewFilterStripBusinessObject();
			var entityStaffFilter = (ModuleNkFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.EntityStaff];
			AssertNotNull(entityStaffFilter);
			CombineAssertions(() =>
			{
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, entityStaffFilter.Visibility);
				AssertEquals("HasComparisonOperator", false, entityStaffFilter.HasComparisonOperator);
				AssertEquals("DefaultProperty", GlbStaff.CurrentUser.GS_Code, entityStaffFilter.DefaultProperty);
			});

			var expectedSecurityErrorMessage = string.Format(@"You do not have the appropriate security rights to view commissions for entities other than your current login ({0}).

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{1}", GlbStaff.CurrentUser.GS_Code, Env.Security.CommissionManagerViewForAnyEntity.DisplayTextPathToSecurityRight);

			entityStaffFilter.IsActive = true;
			entityStaffFilter.Property = otherStaff.GS_Code;
			AssertHasError(entityStaffFilter.PropertyInfo, expectedSecurityErrorMessage);

			entityStaffFilter.Property = "";
			AssertHasError(entityStaffFilter.PropertyInfo, expectedSecurityErrorMessage);

			entityStaffFilter.Property = GlbStaff.CurrentUser.GS_Code;
			AssertNoErrors(entityStaffFilter.PropertyInfo);

			expectedSecurityErrorMessage = string.Format(@"You do not have the appropriate security rights to view commissions for entities other than your current login ({0}).
As a result, you are not allowed to specify an 'Or' filter category for this filter.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{1}", GlbStaff.CurrentUser.GS_Code, Env.Security.CommissionManagerViewForAnyEntity.DisplayTextPathToSecurityRight);

			entityStaffFilter.OrCategory = FilterOrCategory.Aqua;
			entityStaffFilter.Validation.ValidateAll();
			AssertHasError(entityStaffFilter.PropertyInfo, expectedSecurityErrorMessage);

			entityStaffFilter.OrCategory = FilterOrCategory.None;
			entityStaffFilter.Validation.ValidateAll();
			AssertNoErrors(entityStaffFilter.PropertyInfo);

			entityStaffFilter.GroupOrCategory = FilterOrCategory.Olive;
			entityStaffFilter.Validation.ValidateAll();
			AssertHasError(entityStaffFilter.PropertyInfo, expectedSecurityErrorMessage);
		}

		public void TestEntityOrganisationFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";

			var commissionLine1A = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLine1A.CL0_OH_Party = org1.PK;
			var commissionLine1B = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLine1B.CL0_OH_Party = org1.PK;
			var commissionLine2A = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLine2A.CL0_OH_Party = org2.PK;
			var commissionLine2B = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLine2B.CL0_OH_Party = org2.PK;
			var commissionLineForStaff = Factory.NewWithValidTestData<AccCommissionLine>();
			commissionLineForStaff.CL0_GS_NKStaff = "ADL";

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var entityOrgFilter = (ModuleGuidFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.EntityOrganisation];
			AssertNotNull(entityOrgFilter);
			AssertEquals("Entity Organization", entityOrgFilter.MultilingualDescription);
			entityOrgFilter.IsActive = true;

			entityOrgFilter.Property = org1.PK;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine1A, commissionLine1B });

			entityOrgFilter.Property = org2.PK;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine2A, commissionLine2B });
		}

		public void TestEntityOrganisationFilter_WithoutViewAnySecurityRight()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var staffCompany = Factory.NewWithValidTestData<GlbCompany>();
			var staffBranch = Factory.NewWithValidTestData<GlbBranch>();
			var staffDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var staffOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			staffOrganisation.OH_Code = "TESTORG";
			staffCompany.GC_OH_OrgProxy = staffOrganisation.PK;
			staffBranch.GB_GC = staffCompany.PK;
			staff.GS_GB_HomeBranch = staffBranch.PK;

			var otherOrganisation = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), staffBranch.PK.ToGuid(), staffDepartment.PK.ToGuid()))
			{
				Env.Security.CommissionManagerViewForAnyEntity.IsAllowed = false;

				var filterBizObj = GetNewFilterStripBusinessObject();
				var entityOrgFilter = (ModuleGuidFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.EntityOrganisation];
				AssertNotNull(entityOrgFilter);
				CombineAssertions(() =>
				{
					AssertEquals("Visibility", FilterVisibility.AlwaysVisible, entityOrgFilter.Visibility);
					AssertEquals("HasComparisonOperator", false, entityOrgFilter.HasComparisonOperator);
					AssertEquals("DefaultProperty", staffOrganisation.PK, entityOrgFilter.DefaultProperty);
				});

				var expectedSecurityErrorMessage = string.Format(@"You do not have the appropriate security rights to view commissions for entities outside your current login proxy organisation ({0}).

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{1}", "TESTORG", Env.Security.CommissionManagerViewForAnyEntity.DisplayTextPathToSecurityRight);

				entityOrgFilter.IsActive = true;
				entityOrgFilter.Property = otherOrganisation.PK;
				AssertHasError(entityOrgFilter.PropertyInfo, expectedSecurityErrorMessage);

				entityOrgFilter.Property = ZGuid.Empty;
				AssertHasError(entityOrgFilter.PropertyInfo, expectedSecurityErrorMessage);

				entityOrgFilter.Property = staffOrganisation.PK;
				AssertNoErrors(entityOrgFilter.PropertyInfo);

				expectedSecurityErrorMessage = string.Format(@"You do not have the appropriate security rights to view commissions for entities outside your current login proxy organisation ({0}).
As a result, you are not allowed to specify an 'Or' filter category for this filter.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{1}", "TESTORG", Env.Security.CommissionManagerViewForAnyEntity.DisplayTextPathToSecurityRight);

				entityOrgFilter.OrCategory = FilterOrCategory.Aqua;
				entityOrgFilter.Validation.ValidateAll();
				AssertHasError(entityOrgFilter.PropertyInfo, expectedSecurityErrorMessage);

				entityOrgFilter.OrCategory = FilterOrCategory.None;
				entityOrgFilter.Validation.ValidateAll();
				AssertNoErrors(entityOrgFilter.PropertyInfo);

				entityOrgFilter.GroupOrCategory = FilterOrCategory.Olive;
				entityOrgFilter.Validation.ValidateAll();
				AssertHasError(entityOrgFilter.PropertyInfo, expectedSecurityErrorMessage);
			}
		}

		public void TestPaymentStatusFilter()
		{
			var unpaidCommissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
			var paidCommissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
			paidCommissionLine.CL0_PaidDateTimeUtc = new ZDate(2002, 1, 1);

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var paymentStatusFilter = (ModuleTextFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.PaymentStatus];
			AssertNotNull(paymentStatusFilter);
			AssertEquals("Payment Status", paymentStatusFilter.MultilingualDescription);
			AssertEquals(FilterCategories.StatusAndFlags, paymentStatusFilter.Category);

			paymentStatusFilter.IsActive = true;

			paymentStatusFilter.Property = AccCommissionLinePaymentStatusList.Codes.Paid;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { paidCommissionLine });

			paymentStatusFilter.Property = AccCommissionLinePaymentStatusList.Codes.Unpaid;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { unpaidCommissionLine });
		}

		public void TestCommissionStatusFilter()
		{
			var pendingCommissionLine = Factory.NewWithValidTestData<AccCommissionLine>();

			var approvedCommissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
			approvedCommissionLine.CL0_ApprovedDateTimeUtc = new ZDate(2000, 1, 1);

			var paidCommissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
			paidCommissionLine.CL0_PaidDateTimeUtc = new ZDate(2002, 1, 1);

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var commissionStatusFilter = (ModuleTextFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.CommissionStatus];
			AssertNotNull(commissionStatusFilter);
			AssertEquals("Commission Status", commissionStatusFilter.MultilingualDescription);
			AssertEquals(FilterCategories.StatusAndFlags, commissionStatusFilter.Category);

			AssertContainsExactElementsInAnyOrder(
				new[] { ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual },
				commissionStatusFilter.ComparisonOperator_List.Cast<ICodeDescription>().Select(x => x.Code));

			commissionStatusFilter.IsActive = true;

			commissionStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			commissionStatusFilter.Property = AccCommissionLineCommissionStatusList.Codes.Pending;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { pendingCommissionLine });

			commissionStatusFilter.Property = AccCommissionLineCommissionStatusList.Codes.Approved;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { approvedCommissionLine });

			commissionStatusFilter.Property = AccCommissionLineCommissionStatusList.Codes.Paid;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { paidCommissionLine });

			commissionStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			commissionStatusFilter.Property = AccCommissionLineCommissionStatusList.Codes.Pending;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { approvedCommissionLine, paidCommissionLine });

			commissionStatusFilter.Property = AccCommissionLineCommissionStatusList.Codes.Approved;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { pendingCommissionLine, paidCommissionLine });

			commissionStatusFilter.Property = AccCommissionLineCommissionStatusList.Codes.Paid;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { pendingCommissionLine, approvedCommissionLine });
		}

		public void TestApprovalRequestFilter()
		{
			var line1 = Factory.NewWithValidTestData<AccCommissionLine>();
			var line2 = Factory.NewWithValidTestData<AccCommissionLine>();
			var line3 = Factory.NewWithValidTestData<AccCommissionLine>();
			var line4 = Factory.NewWithValidTestData<AccCommissionLine>();

			var approvalRequestA = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			approvalRequestA.Items.AddNew().CRI_CL0 = line1.PK;
			approvalRequestA.Items.AddNew().CRI_CL0 = line2.PK;

			var approvalRequestB = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			approvalRequestB.Items.AddNew().CRI_CL0 = line3.PK;

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var approvalRequestFilter = (ModuleGuidFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.ApprovalRequest];
			AssertNotNull(approvalRequestFilter);
			AssertEquals("Approval Request", approvalRequestFilter.MultilingualDescription);

			approvalRequestFilter.IsActive = true;

			approvalRequestFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			approvalRequestFilter.Property = approvalRequestA.PK;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { line1, line2 });

			approvalRequestFilter.Property = approvalRequestB.PK;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { line3 });

			approvalRequestFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			approvalRequestFilter.Property = approvalRequestA.PK;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { line3, line4 });

			approvalRequestFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { line4 });

			approvalRequestFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { line1, line2, line3 });
		}

		public void TestExcludeCanceledFilter()
		{
			var commissionLine1 = Factory.NewWithValidTestData<AccCommissionLine>();
			var commissionLine2 = Factory.NewWithValidTestData<AccCommissionLine>();
			var canceledCommissionLine1 = Factory.NewWithValidTestData<AccCommissionLine>();
			var canceledCommissionLine2 = Factory.NewWithValidTestData<AccCommissionLine>();
			canceledCommissionLine1.CL0_CancelledDateTimeUtc = new ZDateTime(2001, 1, 1);
			canceledCommissionLine2.CL0_CancelledDateTimeUtc = new ZDateTime(2002, 2, 2);

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var excludeCanceledFilter = (ModuleFlagsFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.ExcludeCanceled];
			AssertNotNull(excludeCanceledFilter);
			AssertEquals("Exclude Canceled", excludeCanceledFilter.MultilingualDescription);
			excludeCanceledFilter.IsActive = true;

			excludeCanceledFilter.Property0 = true;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine1, commissionLine2 });

			excludeCanceledFilter.Property0 = false;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine1, commissionLine2, canceledCommissionLine1, canceledCommissionLine2 });
		}

		public void TestExcludeWithheldFilter()
		{
			var opportunityWithoutDraft = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var opportunityWithoutDraftRecipient = opportunityWithoutDraft.Recipients.AddNew();
			opportunityWithoutDraftRecipient.FillWithValidTestData();
			var opportunityWithoutDraftRate = opportunityWithoutDraftRecipient.Rates.AddNew();

			var opportunityWithDraft = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			opportunityWithDraft.CreateDraft();
			var opportunityWithDraftRecipient = opportunityWithDraft.Recipients.AddNew();
			opportunityWithDraftRecipient.FillWithValidTestData();
			var opportunityWithDraftRate = opportunityWithDraftRecipient.Rates.AddNew();

			var commissionHeaderForOpportunityWithoutDraft = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderForOpportunityWithoutDraft.CH0_CA0 = opportunityWithoutDraft.PK;
			commissionHeaderForOpportunityWithoutDraft.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderForOpportunityWithoutDraft.CH0_GroupingSourceID = ZGuid.NewZGuid();
			var opportunityWithoutDraftCommissionLine = commissionHeaderForOpportunityWithoutDraft.Lines.AddNew();
			opportunityWithoutDraftCommissionLine.CL0_CAT = opportunityWithoutDraftRate.PK;

			var commissionHeaderForOpportunityWithDraft = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderForOpportunityWithDraft.CH0_CA0 = opportunityWithDraft.PK;
			commissionHeaderForOpportunityWithDraft.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderForOpportunityWithDraft.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var opportunityWithDraftCommissionLine_Unpaid = commissionHeaderForOpportunityWithDraft.Lines.AddNew();
			opportunityWithDraftCommissionLine_Unpaid.CL0_CAT = opportunityWithDraftRate.PK;
			opportunityWithDraftCommissionLine_Unpaid.CL0_PaidDateTimeUtc = ZDateTime.Empty;

			var opportunityWithDraftCommissionLine_Paid = commissionHeaderForOpportunityWithDraft.Lines.AddNew();
			opportunityWithDraftCommissionLine_Paid.CL0_CAT = opportunityWithDraftRate.PK;
			opportunityWithDraftCommissionLine_Paid.CL0_PaidDateTimeUtc = new ZDateTime(2002, 2, 2);

			var commissionHeaderWithNoOpportunity = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderWithNoOpportunity.CH0_CA0 = ZGuid.Empty;
			commissionHeaderWithNoOpportunity.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderWithNoOpportunity.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionLineWithNoRate = commissionHeaderWithNoOpportunity.Lines.AddNew();
			commissionLineWithNoRate.CL0_CAT = ZGuid.Empty;

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var excludeWithheldFilter = (ModuleFlagsFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.ExcludeWithheld];
			AssertNotNull(excludeWithheldFilter);
			AssertEquals("Exclude Withheld", excludeWithheldFilter.MultilingualDescription);
			excludeWithheldFilter.IsActive = true;

			excludeWithheldFilter.Property0 = true;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { opportunityWithoutDraftCommissionLine, opportunityWithDraftCommissionLine_Paid, commissionLineWithNoRate });

			excludeWithheldFilter.Property0 = false;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { opportunityWithoutDraftCommissionLine, opportunityWithDraftCommissionLine_Unpaid, opportunityWithDraftCommissionLine_Paid, commissionLineWithNoRate });
		}

		public void TestExcludeWithheld_Queued()
		{
			var opportunityWithoutDraft = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var opportunityWithoutDraftRecipient = opportunityWithoutDraft.Recipients.AddNew();
			opportunityWithoutDraftRecipient.FillWithValidTestData();
			var opportunityWithoutDraftRate = opportunityWithoutDraftRecipient.Rates.AddNew();

			var commissionHeaderForOpportunityWithoutDraft = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderForOpportunityWithoutDraft.CH0_CA0 = opportunityWithoutDraft.PK;
			commissionHeaderForOpportunityWithoutDraft.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeaderForOpportunityWithoutDraft.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var opportunityWithoutDraftCommissionLine = commissionHeaderForOpportunityWithoutDraft.Lines.AddNew();
			opportunityWithoutDraftCommissionLine.CL0_CAT = opportunityWithoutDraftRate.PK;

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var excludeWithheldFilter = (ModuleFlagsFilter)filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.ExcludeWithheld];
			AssertNotNull(excludeWithheldFilter);
			AssertEquals("Exclude Withheld", excludeWithheldFilter.MultilingualDescription);
			excludeWithheldFilter.IsActive = true;

			excludeWithheldFilter.Property0 = true;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { opportunityWithoutDraftCommissionLine });

			opportunityWithoutDraft.SendToCalculationQueue(new ZDateTime(2002, 2, 2), true);

			Factory.Save();

			var elements = Factory.Load<ViewCommissionLine>(filterBizObj.Filter).Select(x => x.AccCommissionLine);
			AssertEquals(0, elements.Count());
		}

		#endregion

		#endregion

		#region Implementation

		void AssertContainsExactElementsInAnyOrder(FilterBusinessObject filterBizObj, IEnumerable<AccCommissionLine> expectedAccCommissionLines)
		{
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccCommissionLine>.PKOnlyComparer,
				expectedAccCommissionLines,
				Factory.Load<ViewCommissionLine>(filterBizObj.Filter).Select(x => x.AccCommissionLine));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CommissionManagementFilterBusinessObject();
		}

		#endregion
	}

	[TestedType(typeof(EntityStaffModuleFilter))]
	class EntityStaffModuleFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EntityStaffModuleFilter("moo", new DummyBusinessObjectCollection(Factory));
		}
	}

	[TestedType(typeof(EntityOrganisationModuleFilter))]
	class EntityOrganisationModuleFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EntityOrganisationModuleFilter("moo", new DummyBusinessObjectCollection(Factory));
		}
	}
}
