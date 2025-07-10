using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(OrgCollectionCallsFilterBusinessObject))]
	public class OrgCollectionCallsFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrgCollectionCallsFilterBusinessObject();
		}

		#region Filters

		#region TestBranchFilter

		public void TestBranchFilter()
		{
			GlbBranch aAABranch = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch bBBBranch = Factory.NewWithValidTestData<GlbBranch>();

			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation.CompanyData.OB_GB_ControllingBranch = aAABranch.PK;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation2.CompanyData.OB_GB_ControllingBranch = bBBBranch.PK;

			Factory.Save();

			((ModuleGuidFilter)FilterBusinessObject["A/R Client Branch"]).Property = aAABranch.PK;
			((ModuleGuidFilter)FilterBusinessObject["A/R Client Branch"]).IsActive = true;

			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			AssertEquals("Filtered for AAA branch, Collection should contain 1 collection call", 1, collectionCallCollection.Count);
			Assert("Filtered for AAA branch, Collection should contain Collection Call with AAABranch", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));

			((ModuleGuidFilter)FilterBusinessObject["A/R Client Branch"]).IsActive = false;

			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with AAABranch", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call with BBBBranch", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));

			((ModuleGuidFilter)FilterBusinessObject["A/R Client Branch"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)FilterBusinessObject["A/R Client Branch"]).IsActive = true;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with AAABranch", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call with BBBBranch", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
		}

		#endregion

		#region TestDebtroCodeFilter

		public void TestDebtorCodeFilter()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			GlbCompany newGLBCompany = Factory.NewWithValidTestData<GlbCompany>();

			OrgHeader newOrganisation3 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation3.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation3.CompanyData.OB_GC = newGLBCompany.PK;

			Factory.Save();

			((ModuleGuidFilter)FilterBusinessObject["A/R Client Code"]).Property = newOrganisation.PK;
			((ModuleGuidFilter)FilterBusinessObject["A/R Client Code"]).IsActive = true;

			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			AssertEquals("Filtered for Debtor Code, Collection should contain 1 collection call", 1, collectionCallCollection.Count);
			Assert("Filtered for Debtor Code, Collection should contain Collection Call with NewOrganisation", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));

			((ModuleGuidFilter)FilterBusinessObject["A/R Client Code"]).IsActive = false;

			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with NewOrganisation", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call with NewOrganisation2", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should not contain Collection Call with NewOrganisation3", !collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));

			((ModuleGuidFilter)FilterBusinessObject["A/R Client Code"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)FilterBusinessObject["A/R Client Code"]).IsActive = true;

			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with NewOrganisation", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call with NewOrganisation2", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should not contain Collection Call with NewOrganisation3", !collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));
		}

		#endregion

		#region TestBranchManagementCodeFilter

		public void TestBranchManagementCodeFilter()
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

			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			organisation1.CompanyData.OB_IsDebtor = ZBool.True;
			organisation1.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			organisation1.CompanyData.OB_GB_ControllingBranch = branch1.PK;

			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2.CompanyData.OB_IsDebtor = ZBool.True;
			organisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			organisation2.CompanyData.OB_GB_ControllingBranch = branch2.PK;

			Factory.Save();

			var branchManagementCodeFilter = (ModuleTextFilter)FilterBusinessObject["Branch Management Code"];
			branchManagementCodeFilter.Property = "BRA";
			branchManagementCodeFilter.IsActive = true;
			var collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			AssertContainsExactElementsInAnyOrder("Collection should only contain organisation1", new[] { organisation1.CompanyData.PK }, collectionCallCollection.Select(x => x.PK));

			branchManagementCodeFilter.Property = "BRB";
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			AssertContainsExactElementsInAnyOrder("Collection should only contain organisation2", new[] { organisation2.CompanyData.PK }, collectionCallCollection.Select(x => x.PK));
		}

		#endregion

		public void TestSettlementGroupFilter()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation2.ARSettlementGroupPK = newOrganisation.PK;

			GlbCompany newGLBCompany = Factory.NewWithValidTestData<GlbCompany>();

			OrgHeader newOrganisation3 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation3.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation3.CompanyData.OB_GC = newGLBCompany.PK;
			newOrganisation3.ARSettlementGroupPK = newOrganisation.PK;

			Factory.Save();

			((ModuleGuidFilter)FilterBusinessObject["SettlementGroup"]).Property = newOrganisation.PK;
			((ModuleGuidFilter)FilterBusinessObject["SettlementGroup"]).IsActive = true;

			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			AssertEquals("Filtered for Debtor Code, Collection should contain 2 collection call", 2, collectionCallCollection.Count);
			Assert("Collection should contain Collection Call with NewOrganisation", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call with NewOrganisation2", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));

			((ModuleGuidFilter)FilterBusinessObject["SettlementGroup"]).Property = newOrganisation2.PK;
			((ModuleGuidFilter)FilterBusinessObject["SettlementGroup"]).IsActive = true;

			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			AssertEquals("Filtered for Debtor Code, Collection should contain 0 collection call", 0, collectionCallCollection.Count);
		}

		#region TestDebtorGroupFilter

		public void TestDebtorGroupFilter()
		{
			OrgDebtorGroup aAADebtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			OrgDebtorGroup bBBDebtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();

			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation.CompanyData.OB_OJ_ARDebtorGroup = aAADebtorGroup.PK;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation2.CompanyData.OB_OJ_ARDebtorGroup = bBBDebtorGroup.PK;

			Factory.Save();

			((ModuleGuidFilter)FilterBusinessObject["A/R Client Group"]).Property = aAADebtorGroup.PK;
			((ModuleGuidFilter)FilterBusinessObject["A/R Client Group"]).IsActive = true;

			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			AssertEquals("Filtered for AAA debtor group, Collection should contain 1 collection call", 1, collectionCallCollection.Count);
			Assert("Filtered for AAA debtor group, Collection should contain Collection Call with AAA debtor group", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));

			((ModuleGuidFilter)FilterBusinessObject["A/R Client Group"]).IsActive = false;

			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with AAA debtor group", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call with BBB debtor group", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));

			((ModuleGuidFilter)FilterBusinessObject["A/R Client Group"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)FilterBusinessObject["A/R Client Group"]).IsActive = true;

			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with AAA debtor group", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call with BBB debtor group", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
		}
		#endregion

		#region TestAccountsRelationshipFilter

		public void TestAccountsRelationshipFilter()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation.CompanyData.OB_ARCategory = Env.Registry.ReceivablesCategoryList[0].Code;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation2.CompanyData.OB_ARCategory = Env.Registry.ReceivablesCategoryList[1].Code;

			OrgHeader newOrganisation3 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation3.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation3.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			((ModuleTextFilter)FilterBusinessObject["Accounts Relationship"]).Property = Env.Registry.ReceivablesCategoryList[0].Code;
			((ModuleTextFilter)FilterBusinessObject["Accounts Relationship"]).IsActive = true;

			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with the first AR Category", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should not contain Collection Call with second AR Category", !collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should not contain Collection Call without AR Category specified", !collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));

			((ModuleTextFilter)FilterBusinessObject["Accounts Relationship"]).IsActive = false;

			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with first AR Category", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call with second AR Category", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call without AR Category specified", collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));

			((ModuleTextFilter)FilterBusinessObject["Accounts Relationship"]).Property = ZString.Empty;
			((ModuleTextFilter)FilterBusinessObject["Accounts Relationship"]).IsActive = true;

			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with first AR Category", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call with second AR Category", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call without AR Category specified", collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));
		}
		#endregion

		#region TestConsolidationCategoryFilter

		public void TestConsolidationCategoryFilter()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation.CompanyData.OB_ARConsolidatedAccountingCategory = Enterprise.MasterFiles.Business.AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value[0].Code;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation2.CompanyData.OB_ARConsolidatedAccountingCategory = Enterprise.MasterFiles.Business.AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value[1].Code;

			OrgHeader newOrganisation3 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation3.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation3.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			((ModuleTextFilter)FilterBusinessObject["Consolidation Category"]).Property = Enterprise.MasterFiles.Business.AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value[0].Code;
			((ModuleTextFilter)FilterBusinessObject["Consolidation Category"]).IsActive = true;

			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with the first AR Consolidated Category", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should not contain Collection Call with second AR Consolidated Category", !collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should not contain Collection Call without AR Consolidated Category specified", !collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));

			((ModuleTextFilter)FilterBusinessObject["Consolidation Category"]).Property = ZString.Empty;
			((ModuleTextFilter)FilterBusinessObject["Consolidation Category"]).IsActive = false;

			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with first AR Consolidated Category", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call with second AR Consolidated Category", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call without AR Consolidated Category specified", collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));
		}

		#endregion

		#region TestCreditRatingFilter

		public void TestCreditRatingFilter()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation.CompanyData.OB_ARCreditRating = Env.Registry.ARCreditRatingList[0].Code;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation2.CompanyData.OB_ARConsolidatedAccountingCategory = Env.Registry.ARCreditRatingList[1].Code;

			OrgHeader newOrganisation3 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation3.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation3.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			((ModuleTextFilter)FilterBusinessObject["Credit Rating"]).Property = Env.Registry.ARCreditRatingList[0].Code;
			((ModuleTextFilter)FilterBusinessObject["Credit Rating"]).IsActive = true;

			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with the first AR Credit Rating", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should not contain Collection Call with second AR Credit Rating", !collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should not contain Collection Call without AR Credit Rating specified", !collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));

			((ModuleTextFilter)FilterBusinessObject["Credit Rating"]).IsActive = false;

			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with first AR Credit Rating", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call with second AR Credit Rating", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call without AR Credit Rating specified", collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));

			((ModuleTextFilter)FilterBusinessObject["Credit Rating"]).Property = ZString.Empty;
			((ModuleTextFilter)FilterBusinessObject["Credit Rating"]).IsActive = true;

			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();

			Assert("Collection should contain Collection Call with first AR Credit Rating", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call with second AR Credit Rating", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call without AR Credit Rating specified", collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));
		}

		#endregion

		#region TestCallDateFilter

		public void TestCallDateFilter()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader newOrganisation3 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation3.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation3.CompanyData.OB_IsDebtor = ZBool.True;

			OrgCollectionNote collecitonNote = newOrganisation.CollectionNotes.AddNew();
			collecitonNote.PN_SystemCreateTimeUtc = new ZDateTime(2004, 2, 3);

			OrgCollectionNote collecitonNote2 = newOrganisation.CollectionNotes.AddNew();
			collecitonNote2.PN_SystemCreateTimeUtc = new ZDateTime(2004, 2, 15);

			OrgCollectionNote collecitonNote3 = newOrganisation2.CollectionNotes.AddNew();
			collecitonNote3.PN_SystemCreateTimeUtc = new ZDateTime(2004, 2, 2);

			Factory.Save();

			((ModuleDateFilter)FilterBusinessObject["Call Date"]).Property1 = new ZDateTime(2004, 2, 3);
			((ModuleDateFilter)FilterBusinessObject["Call Date"]).Property2 = new ZDateTime(2004, 2, 15);
			((ModuleDateFilter)FilterBusinessObject["Call Date"]).IsActive = true;
			((ModuleDateFilter)FilterBusinessObject["Call Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should contain Collection Call of NewOrganisation", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should not contain Collection Call of NewOrganisation2", !collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should not contain Collection Call of NewOrganisation3", !collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));

			((ModuleDateFilter)FilterBusinessObject["Call Date"]).Property1 = new ZDateTime(2004, 2, 2);
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should contain Collection Call of NewOrganisation", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation2", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should not contain Collection Call of NewOrganisation3", !collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));

			((ModuleDateFilter)FilterBusinessObject["Call Date"]).Property1 = new ZDateTime(2003, 2, 2);
			((ModuleDateFilter)FilterBusinessObject["Call Date"]).Property2 = new ZDateTime(2003, 2, 15);
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should not contain Collection Call of NewOrganisation", !collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should not contain Collection Call of NewOrganisation2", !collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should not contain Collection Call of NewOrganisation3", !collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));

			((ModuleDateFilter)FilterBusinessObject["Call Date"]).Property1 = ZDateTime.Empty;
			((ModuleDateFilter)FilterBusinessObject["Call Date"]).Property2 = ZDateTime.Empty;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should contain Collection Call of NewOrganisation", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation2", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation3", collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));
		}

		#endregion

		#region TestFollowUpDateFilter

		public void TestFollowUpDateFilter()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader newOrganisation3 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation3.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation3.CompanyData.OB_IsDebtor = ZBool.True;

			OrgCollectionNote collecitonNote = newOrganisation.CollectionNotes.AddNew();
			collecitonNote.PN_CallBackDate = new ZDateTime(2004, 2, 3);

			OrgCollectionNote collecitonNote2 = newOrganisation.CollectionNotes.AddNew();
			collecitonNote2.PN_CallBackDate = new ZDateTime(2004, 2, 15);

			OrgCollectionNote collecitonNote3 = newOrganisation2.CollectionNotes.AddNew();
			collecitonNote3.PN_CallBackDate = new ZDateTime(2004, 2, 2);

			Factory.Save();

			((ModuleDateFilter)FilterBusinessObject["Follow Up Date"]).Property1 = new ZDateTime(2004, 2, 3);
			((ModuleDateFilter)FilterBusinessObject["Follow Up Date"]).Property2 = new ZDateTime(2004, 2, 15);
			((ModuleDateFilter)FilterBusinessObject["Follow Up Date"]).IsActive = true;
			((ModuleDateFilter)FilterBusinessObject["Follow Up Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should contain Collection Call of NewOrganisation", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should not contain Collection Call of NewOrganisation2", !collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should not contain Collection Call of NewOrganisation3", !collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));

			((ModuleDateFilter)FilterBusinessObject["Follow Up Date"]).Property1 = new ZDateTime(2004, 2, 2);
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should contain Collection Call of NewOrganisation", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation2", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should not contain Collection Call of NewOrganisation3", !collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));

			((ModuleDateFilter)FilterBusinessObject["Follow Up Date"]).Property1 = new ZDateTime(2003, 2, 2);
			((ModuleDateFilter)FilterBusinessObject["Follow Up Date"]).Property2 = new ZDateTime(2003, 2, 15);
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should not contain Collection Call of NewOrganisation", !collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should not contain Collection Call of NewOrganisation2", !collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should not contain Collection Call of NewOrganisation3", !collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));

			((ModuleDateFilter)FilterBusinessObject["Follow Up Date"]).Property1 = ZDateTime.Empty;
			((ModuleDateFilter)FilterBusinessObject["Follow Up Date"]).Property2 = ZDateTime.Empty;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should contain Collection Call of NewOrganisation", collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation2", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation3", collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));
		}

		#endregion

		#region TestCallStatusFilter

		public void TestCallStatusFilter()
		{
			OrgHeader newOrganisation1 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation1.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader newOrganisation3 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation3.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			newOrganisation3.CompanyData.OB_IsDebtor = ZBool.True;

			OrgCollectionNote collectionNote1 = newOrganisation1.CollectionNotes.AddNew();
			collectionNote1.PN_Status = "CLS";

			OrgCollectionNote collectionNote2 = newOrganisation2.CollectionNotes.AddNew();
			collectionNote2.PN_Status = "CLS";
			collectionNote2 = newOrganisation2.CollectionNotes.AddNew();
			collectionNote2.PN_Status = "WRK";

			Factory.Save();

			((ModuleTextFilter)FilterBusinessObject["Call Status"]).Property = "WRK";
			FilterBusinessObject["Call Status"].IsActive = false;

			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should contain Collection Call of newOrganisation1", collectionCallCollection.Contains(newOrganisation1.CompanyData.PK));
			Assert("Collection should contain Collection Call of newOrganisation2", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call of newOrganisation3", collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));

			FilterBusinessObject["Call Status"].IsActive = true;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should not contain Collection Call of newOrganisation1", !collectionCallCollection.Contains(newOrganisation1.CompanyData.PK));
			Assert("Collection should contain Collection Call of newOrganisation2", collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert("Collection should not contain Collection Call of newOrganisation3", !collectionCallCollection.Contains(newOrganisation3.CompanyData.PK));
		}

		#endregion

		#region TestSalesRepresentativeFilter

		public void TestSalesRepresentativeFilter()
		{
			PrepareForStaffAssignmentTest();

			((ModuleNkFilter)FilterBusinessObject["Sales Representative"]).Property = NewStaff.GS_Code;
			((ModuleNkFilter)FilterBusinessObject["Sales Representative"]).IsActive = true;
			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			AssertEquals("Collection should contain 1 collection call", 1, collectionCallCollection.Count);
			Assert("Collection should contain Collection Call of NewOrganisation", collectionCallCollection.Contains(NewOrganisation.CompanyData.PK));

			((ModuleNkFilter)FilterBusinessObject["Sales Representative"]).Property = NewStaff2.GS_Code;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			AssertEquals("Collection should contain 1 collection call", 1, collectionCallCollection.Count);
			Assert("Collection should contain Collection Call of NewOrganisation2", collectionCallCollection.Contains(NewOrganisation2.CompanyData.PK));

			((ModuleNkFilter)FilterBusinessObject["Sales Representative"]).IsActive = false;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should contain Collection Call of NewOrganisation", collectionCallCollection.Contains(NewOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation2", collectionCallCollection.Contains(NewOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation3", collectionCallCollection.Contains(NewOrganisation3.CompanyData.PK));

			((ModuleNkFilter)FilterBusinessObject["Sales Representative"]).IsActive = true;
			((ModuleNkFilter)FilterBusinessObject["Sales Representative"]).Property = NewStaff3.GS_Code;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			AssertEquals("Collection should be empty", 0, collectionCallCollection.Count);

			((ModuleNkFilter)FilterBusinessObject["Sales Representative"]).Property = ZString.Empty;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should contain Collection Call of NewOrganisation", collectionCallCollection.Contains(NewOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation2", collectionCallCollection.Contains(NewOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation3", collectionCallCollection.Contains(NewOrganisation3.CompanyData.PK));
		}

		#endregion

		#region TestCustomerServiceRepresentativeFilter

		public void TestCustomerServiceRepresentativeFilter()
		{
			PrepareForStaffAssignmentTest();

			((ModuleNkFilter)FilterBusinessObject["Customer Service Representative"]).Property = NewStaff.GS_Code;
			((ModuleNkFilter)FilterBusinessObject["Customer Service Representative"]).IsActive = true;
			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			AssertEquals("Collection should contain 1 collection call", 1, collectionCallCollection.Count);
			Assert("Collection should contain Collection Call of NewOrganisation3", collectionCallCollection.Contains(NewOrganisation3.CompanyData.PK));

			((ModuleNkFilter)FilterBusinessObject["Customer Service Representative"]).Property = NewStaff3.GS_Code;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			AssertEquals("Collection should contain 1 collection call", 1, collectionCallCollection.Count);
			Assert("Collection should contain Collection Call of NewOrganisation2", collectionCallCollection.Contains(NewOrganisation2.CompanyData.PK));

			((ModuleNkFilter)FilterBusinessObject["Customer Service Representative"]).IsActive = false;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should contain Collection Call of NewOrganisation", collectionCallCollection.Contains(NewOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation2", collectionCallCollection.Contains(NewOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation3", collectionCallCollection.Contains(NewOrganisation3.CompanyData.PK));

			((ModuleNkFilter)FilterBusinessObject["Customer Service Representative"]).IsActive = true;
			((ModuleNkFilter)FilterBusinessObject["Customer Service Representative"]).Property = NewStaff2.GS_Code;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			AssertEquals("Collection should be empty", 0, collectionCallCollection.Count);

			((ModuleNkFilter)FilterBusinessObject["Customer Service Representative"]).Property = ZString.Empty;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should contain Collection Call of NewOrganisation", collectionCallCollection.Contains(NewOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation2", collectionCallCollection.Contains(NewOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation3", collectionCallCollection.Contains(NewOrganisation3.CompanyData.PK));
		}

		#endregion

		#region TestCreditControllerFilter

		public void TestCreditControllerFilter()
		{
			PrepareForStaffAssignmentTest();

			((ModuleNkFilter)FilterBusinessObject["Credit Controller"]).Property = NewStaff.GS_Code;
			((ModuleNkFilter)FilterBusinessObject["Credit Controller"]).IsActive = true;
			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			AssertEquals("Collection should contain 1 collection call", 1, collectionCallCollection.Count);
			Assert("Collection should contain Collection Call of NewOrganisation2", collectionCallCollection.Contains(NewOrganisation2.CompanyData.PK));

			((ModuleNkFilter)FilterBusinessObject["Credit Controller"]).Property = NewStaff4.GS_Code;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			AssertEquals("Collection should contain 1 collection call", 1, collectionCallCollection.Count);
			Assert("Collection should contain Collection Call of NewOrganisation3", collectionCallCollection.Contains(NewOrganisation3.CompanyData.PK));

			((ModuleNkFilter)FilterBusinessObject["Credit Controller"]).IsActive = false;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should contain Collection Call of NewOrganisation", collectionCallCollection.Contains(NewOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation2", collectionCallCollection.Contains(NewOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation3", collectionCallCollection.Contains(NewOrganisation3.CompanyData.PK));

			((ModuleNkFilter)FilterBusinessObject["Credit Controller"]).IsActive = true;
			((ModuleNkFilter)FilterBusinessObject["Credit Controller"]).Property = NewStaff3.GS_Code;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			AssertEquals("Collection should be empty", 0, collectionCallCollection.Count);

			((ModuleNkFilter)FilterBusinessObject["Credit Controller"]).Property = ZString.Empty;
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load();
			Assert("Collection should contain Collection Call of NewOrganisation", collectionCallCollection.Contains(NewOrganisation.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation2", collectionCallCollection.Contains(NewOrganisation2.CompanyData.PK));
			Assert("Collection should contain Collection Call of NewOrganisation3", collectionCallCollection.Contains(NewOrganisation3.CompanyData.PK));
		}

		#endregion

		#endregion

		#region Transaction Post Date Filters

		[TestDate(2009, 7, 13)]
		public void TestTransactionsNotPostedFilter()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			ARInvoice invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_OH = newOrganisation.PK;
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_PostDate = ZDateTime.Now.AddDays(-5); // NOTE - this class has the 'TestDate' attribute set to '13/07/2009'.

			ARInvoice invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_OH = newOrganisation2.PK;
			invoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice2.AH_PostDate = ZDateTime.Now.AddDays(5); // NOTE - this class has the 'TestDate' attribute set to '13/07/2009'.

			Factory.Save();

			ModuleDateFilter dateFilter = ((ModuleDateFilter)FilterBusinessObject["No Transactions for Post Date"]);
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Now.AddDays(-7);
			dateFilter.Property2 = ZDateTime.Now.AddDays(-3);
			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load(FilterBusinessObject.Filter);
			Assert(collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert(!collectionCallCollection.Contains(newOrganisation.CompanyData.PK));

			dateFilter.Property1 = ZDateTime.Now.AddDays(3);
			dateFilter.Property2 = ZDateTime.Now.AddDays(7);
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load(FilterBusinessObject.Filter);
			Assert(collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert(!collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
		}

		[TestDate(2009, 7, 13)]
		public void TestTransactionsPostedFilter()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			ARInvoice invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_OH = newOrganisation.PK;
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_PostDate = ZDateTime.Now.AddDays(-5); // NOTE - this class has the 'TestDate' attribute set to '13/07/2009'.

			ARInvoice invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_OH = newOrganisation2.PK;
			invoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice2.AH_PostDate = ZDateTime.Now.AddDays(5); // NOTE - this class has the 'TestDate' attribute set to '13/07/2009'.

			Factory.Save();

			ModuleDateFilter dateFilter = ((ModuleDateFilter)FilterBusinessObject["Has Transactions for Post Date"]);
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Now.AddDays(-7);
			dateFilter.Property2 = ZDateTime.Now.AddDays(-3);
			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load(FilterBusinessObject.Filter);
			Assert(collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert(!collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));

			dateFilter.Property1 = ZDateTime.Now.AddDays(3);
			dateFilter.Property2 = ZDateTime.Now.AddDays(7);
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load(FilterBusinessObject.Filter);
			Assert(collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			Assert(!collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
		}

		[TestDate(2009, 7, 13)]
		public void TestPostDateFiltersOnlySearchForARandAPTransactions()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			ARInvoice invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_OH = newOrganisation2.PK;
			invoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice2.AH_PostDate = ZDateTime.Now.AddDays(-5); // NOTE - this class has the 'TestDate' attribute set to '13/07/2009'.

			GLJournal journal = Factory.New<GLJournal>();
			journal.PostPeriod = periodHelper.CurrentPeriodInt;

			Factory.Save();

			ModuleDateFilter dateFilter = ((ModuleDateFilter)FilterBusinessObject["Has Transactions for Post Date"]);
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Now.AddDays(-7);
			dateFilter.Property2 = periodHelper.CurrentPeriod.AM_EndDate.AddMinutes(1);

			OrgCollectionCallCollection collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load(FilterBusinessObject.Filter);
			Assert("GL Journal fits in date range, but AH_Ledger is not AR/AP, so null AH_OH shouldn't be taken into account",
				!collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert(collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
			// NOTE: Prior to the 'AH_Ledger IN ('AR', 'AP') clause being added to these queries, the results would include null 'AH_OH' values, 
			// which would evaluate to 'null' in the query, so no results were returned.

			dateFilter.IsActive = false;
			dateFilter = ((ModuleDateFilter)FilterBusinessObject["No Transactions for Post Date"]);
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Now.AddDays(-7);
			dateFilter.Property2 = periodHelper.CurrentPeriod.AM_EndDate.AddMinutes(1);
			collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load(FilterBusinessObject.Filter);
			Assert("GL Journal fits in date range, but AH_Ledger is not AR/AP, so null AH_OH shouldn't be taken into account",
				collectionCallCollection.Contains(newOrganisation.CompanyData.PK));
			Assert(!collectionCallCollection.Contains(newOrganisation2.CompanyData.PK));
		}

		#endregion

		#region Max Days and/or Amount Overdue Filters

		public void TestMaxDaysAndAmountOverdueFilter()
		{
			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			organisation1.CompanyData.OB_IsDebtor = ZBool.True;
			organisation1.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2.CompanyData.OB_IsDebtor = ZBool.True;
			organisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			var organisation3 = Factory.NewWithValidTestData<OrgHeader>();
			organisation3.CompanyData.OB_IsDebtor = ZBool.True;
			organisation3.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			var currentDate = ZDateTime.Today;

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001100", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m, organisation1, TestObjectCreator.CC1.PK);
			invoice1.AH_DueDate = currentDate.AddDays(-10);
			invoice1.SubmittedFromInvoicingForm = true;

			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001200", TestObjectCreator.AUD, 1m, 300m, 0m, 300m, 0m, organisation2, TestObjectCreator.CC1.PK);
			invoice2.AH_DueDate = currentDate.AddDays(-5);
			invoice2.SubmittedFromInvoicingForm = true;

			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001200", TestObjectCreator.AUD, 1m, 400m, 0m, 400m, 0m, organisation3, TestObjectCreator.CC1.PK);
			invoice3.AH_DueDate = currentDate.AddDays(2);
			invoice3.SubmittedFromInvoicingForm = true;

			Factory.Save();

			var filter = ((DaysAndAmountOverdueModuleFilter)FilterBusinessObject["Max Days and/or Amount Overdue"]);
			filter.IsActive = true;
			filter.DaysOverdue = 0;
			filter.AmountOverdue = 100;
			filter.AndOrDecider = "AND";

			var collectionCallCollection = new OrgCollectionCallCollection(Factory, FilterBusinessObject.Filter);
			collectionCallCollection.Load(FilterBusinessObject.Filter);
			AssertContainsExactElementsInAnyOrder("should contain organisation1 and organisation2", new[] { organisation1.PK, organisation2.PK }, collectionCallCollection.Cast<OrgCollectionCall>().Select(x => x.CC_OH));

			filter.DaysOverdue = 6 + CalculateDateCorrection(currentDate);
			filter.AmountOverdue = 100;
			collectionCallCollection.Load(FilterBusinessObject.Filter);
			AssertContainsExactElementsInAnyOrder("should contain only organisation1", new[] { organisation1.PK }, collectionCallCollection.Cast<OrgCollectionCall>().Select(x => x.CC_OH));

			filter.DaysOverdue = 6 + CalculateDateCorrection(currentDate);
			filter.AmountOverdue = 300;
			collectionCallCollection.Load(FilterBusinessObject.Filter);
			AssertEquals("No match found", 0, collectionCallCollection.Count);

			filter.AndOrDecider = "OR";
			collectionCallCollection.Load(FilterBusinessObject.Filter);
			AssertContainsExactElementsInAnyOrder("should contain organisation1 and organisation2", new[] { organisation1.PK, organisation2.PK }, collectionCallCollection.Cast<OrgCollectionCall>().Select(x => x.CC_OH));

			filter.DaysOverdue = 15;
			filter.AmountOverdue = 300;
			collectionCallCollection.Load(FilterBusinessObject.Filter);
			AssertContainsExactElementsInAnyOrder("should contain only organisation2", new[] { organisation2.PK }, collectionCallCollection.Cast<OrgCollectionCall>().Select(x => x.CC_OH));
		}

		ZInt CalculateDateCorrection(ZDateTime currentDate)
		{
			var noOfDaysToIncrease = 0;
			using (var command = Db.Connection.Command("SELECT GETDATE()"))
			{
				var sQLDate = ((DateTime)command.ExecuteScalar()).Date;
				noOfDaysToIncrease = (sQLDate - currentDate).Days;
			}
			return noOfDaysToIncrease;
		}

		#endregion

		#region Lookups

		#region TestStaffRolesList

		public void TestStaffRolesList()
		{
			Assert("StaffRolesList should contain QueryDeciderNoSelection Code", FilterBusinessObject.StaffRolesList.ContainsCode(OrgCollectionCallsFilterBusinessObject.QueryDeciderNoSelectionCode));
			Assert("StaffRolesList should contain SalesRep Code", FilterBusinessObject.StaffRolesList.ContainsCode(StaffAssignmentRoles.Codes.SalesRep));
			Assert("StaffRolesList should contain CustomerServiceRep Code", FilterBusinessObject.StaffRolesList.ContainsCode(StaffAssignmentRoles.Codes.CustomerServiceRep));
			Assert("StaffRolesList should contain CreditController Code", FilterBusinessObject.StaffRolesList.ContainsCode(StaffAssignmentRoles.Codes.CreditController));
		}

		#endregion

		#region TestOB_ARCategory_List

		public void TestOB_ARCategory_List()
		{
			Assert("OB_ARCategory_List should be the same as Env.Registry.ReceivablesCategoryList", Compare2CodeDescriptionPairListsAreSame(Env.Registry.ReceivablesCategoryList, FilterBusinessObject.OB_ARCategory_List));
		}

		#endregion

		#region TestOB_ARCreditRating_List

		public void TestOB_ARCreditRating_List()
		{
			Assert("OB_ARCreditRating_List should be the same as Env.Registry.ARCreditRatingList", Compare2CodeDescriptionPairListsAreSame(Env.Registry.ARCreditRatingList, FilterBusinessObject.OB_ARCreditRating_List));
		}

		#endregion

		#region TestOB_ARConsolidatedAccountingCategory_List

		public void TestOB_ARConsolidatedAccountingCategory_List()
		{
			Assert("OB_ARConsolidatedAccountingCategory_List should be the same as ConsolidatedAccountingCategoryList",
				Compare2CodeDescriptionPairListsAreSame(AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value.ToReadOnlyCodeDescriptionPairList(), FilterBusinessObject.OB_ARConsolidatedAccountingCategory_List));
		}

		#endregion

		#region TestOrgHeaders

		public void TestOrgHeaders()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader newOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation2.CompanyData.OB_IsDebtor = ZBool.False;
			newOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			GlbCompany newGLBCompany = Factory.NewWithValidTestData<GlbCompany>();

			OrgHeader newOrganisation3 = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation3.CompanyData.OB_IsDebtor = ZBool.True;
			newOrganisation3.CompanyData.OB_GC = newGLBCompany.PK;

			Factory.Save();

			FilterBusinessObject.OrgHeaders.Load();
			Assert("OrgHeaders collection should contain NewOrganisation", FilterBusinessObject.OrgHeaders.Contains(newOrganisation));
			Assert("OrgHeaders collection should not contain NewOrganisation2", !FilterBusinessObject.OrgHeaders.Contains(newOrganisation2));
			Assert("OrgHeaders collection should not contain NewOrganisation3", !FilterBusinessObject.OrgHeaders.Contains(newOrganisation3));
		}

		#endregion

		#region TestCallStatusList

		public void TestCallStatusList()
		{
			AssertNotNull(FilterBusinessObject.CallStatusList);
			Assert(FilterBusinessObject.CallStatusList.Count > 0);
		}

		#endregion

		#endregion

		#region Implementation

		OrgHeader NewOrganisation;
		OrgHeader NewOrganisation2;
		OrgHeader NewOrganisation3;
		GlbStaff NewStaff;
		GlbStaff NewStaff2;
		GlbStaff NewStaff3;
		GlbStaff NewStaff4;

		void PrepareForStaffAssignmentTest()
		{
			NewOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			NewOrganisation.CompanyData.OB_IsDebtor = ZBool.True;
			NewOrganisation.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			NewOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
			NewOrganisation2.CompanyData.OB_IsDebtor = ZBool.True;
			NewOrganisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			NewOrganisation3 = Factory.NewWithValidTestData<OrgHeader>();
			NewOrganisation3.CompanyData.OB_IsDebtor = ZBool.True;
			NewOrganisation3.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			NewStaff = Factory.NewWithValidTestData<GlbStaff>();
			OrgStaffAssignments orgStaffAssignment = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment.O8_GS_NKPersonResponsible = NewStaff.GS_Code;
			orgStaffAssignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignment.O8_OH = NewOrganisation.PK;

			OrgStaffAssignments orgStaffAssignment2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment2.O8_GS_NKPersonResponsible = NewStaff.GS_Code;
			orgStaffAssignment2.O8_Role = StaffAssignmentRoles.Codes.CreditController;
			orgStaffAssignment2.O8_OH = NewOrganisation2.PK;

			OrgStaffAssignments orgStaffAssignment4 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment4.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment4.O8_GS_NKPersonResponsible = NewStaff.GS_Code;
			orgStaffAssignment4.O8_Role = StaffAssignmentRoles.Codes.CustomerServiceRep;
			orgStaffAssignment4.O8_OH = NewOrganisation3.PK;

			NewStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			OrgStaffAssignments orgStaffAssignment3 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment3.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment3.O8_GS_NKPersonResponsible = NewStaff2.GS_Code;
			orgStaffAssignment3.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignment3.O8_OH = NewOrganisation2.PK;

			NewStaff3 = Factory.NewWithValidTestData<GlbStaff>();
			OrgStaffAssignments orgStaffAssignment5 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment5.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment5.O8_GS_NKPersonResponsible = NewStaff3.GS_Code;
			orgStaffAssignment5.O8_Role = StaffAssignmentRoles.Codes.CustomerServiceRep;
			orgStaffAssignment5.O8_OH = NewOrganisation2.PK;

			NewStaff4 = Factory.NewWithValidTestData<GlbStaff>();
			OrgStaffAssignments orgStaffAssignment6 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment6.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment6.O8_GS_NKPersonResponsible = NewStaff4.GS_Code;
			orgStaffAssignment6.O8_Role = StaffAssignmentRoles.Codes.CreditController;
			orgStaffAssignment6.O8_OH = NewOrganisation3.PK;

			Factory.Save();
		}

		bool Compare2CodeDescriptionPairListsAreSame(ReadOnlyCodeDescriptionPairList list1, ReadOnlyCodeDescriptionPairList list2)
		{
			return list1.Equals(list2);
		}

		OrgCollectionCallsFilterBusinessObject fFilterBusinessObject;
		OrgCollectionCallsFilterBusinessObject FilterBusinessObject
		{
			get
			{
				if (fFilterBusinessObject == null)
				{
					fFilterBusinessObject = new OrgCollectionCallsFilterBusinessObject();
				}
				return fFilterBusinessObject;
			}
		}

		#endregion
	}
}
