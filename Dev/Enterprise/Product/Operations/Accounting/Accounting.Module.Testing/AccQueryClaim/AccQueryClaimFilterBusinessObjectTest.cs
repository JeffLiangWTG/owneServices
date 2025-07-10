using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class AccQueryClaimFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		protected abstract Invoice CreateNewInvoice(BusinessObjectFactory factory);
		protected abstract Invoice CreateNewInvoiceOfOtherLedger(BusinessObjectFactory factory);
		protected abstract AccQueryClaim CreateNewAccQueryClaim(BusinessObjectFactory factory);

		protected AccQueryClaimFilterBusinessObject TestFilterBizO
		{
			get
			{
				if (TestFilterBizO_internal == null)
				{
					TestFilterBizO_internal = (AccQueryClaimFilterBusinessObject)GetNewFilterStripBusinessObject();
				}
				return TestFilterBizO_internal;
			}
		}
		AccQueryClaimFilterBusinessObject TestFilterBizO_internal;

		class AccQueryClaimComparer : IComparer<AccQueryClaim>
		{
			public int Compare(AccQueryClaim x, AccQueryClaim y)
			{
				return x.PK.CompareTo(y.PK);
			}

			public static AccQueryClaimComparer GetComparer()
			{
				return Comparer_internal ?? (Comparer_internal = new AccQueryClaimComparer());
			}
			static AccQueryClaimComparer Comparer_internal;
		}

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

			var claim1 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim1.AY_GB = branch1.PK;
			var claim2 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim2.AY_GB = branch2.PK;

			Factory.Save();

			var branchManagementCodeFilter = (ModuleTextFilter)TestFilterBizO["Branch Management Code"];
			branchManagementCodeFilter.Property = "BRA";
			branchManagementCodeFilter.IsActive = true;
			AccQueryClaim[] accQueryClaims = Factory.Load<APAccQueryClaim>(TestFilterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Collection should Contain claim1", new[] { claim1 }, accQueryClaims);

			branchManagementCodeFilter.Property = "BRB";
			accQueryClaims = Factory.Load<APAccQueryClaim>(TestFilterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Collection should Contain claim2", new[] { claim2 }, accQueryClaims);
		}

		#endregion

		public void TestInvoiceNumberFilter()
		{
			Invoice invoice1 = CreateNewInvoice(Factory);
			invoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			AccQueryClaim claim1 = CreateNewAccQueryClaim(Factory);
			claim1.AY_AH = invoice1.PK;

			Invoice invoice3 = CreateNewInvoice(Factory);
			invoice3.AH_OH = TestObjectCreator.AALSHI.PK;
			AccQueryClaim claim3 = CreateNewAccQueryClaim(Factory);
			claim3.AY_AH = invoice3.PK;

			invoice1.AH_TransactionNum = "00001000"; //need the same numbers as AR Transaction Number fountain generates.
			invoice3.AH_TransactionNum = "00001001";

			Factory.Save();

			Invoice invoice2 = CreateNewInvoiceOfOtherLedger(Factory);
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			AccQueryClaim claim2 = CreateNewAccQueryClaim(Factory);
			claim2.AY_AH = invoice2.PK;

			invoice2.AH_TransactionNum = invoice1.AH_TransactionNum; //after AR Transaction Number fountain generates invoice1.AH_TransactionNum.
			Factory.Save();

			ModuleTextFilter invoiceNumberFilter = ((ModuleTextFilter)TestFilterBizO["Invoice Number"]);
			invoiceNumberFilter.Property = invoice1.AH_TransactionNum;
			invoiceNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			invoiceNumberFilter.IsActive = true;

			AccQueryClaim[] accQueryClaims = Factory.Load<APAccQueryClaim>(TestFilterBizO.Filter);

			AssertEquals("There should be 1 transaction in the collection", 1, accQueryClaims.Length);
			AssertEquals("Collection should contain claim1 ", claim1.PK, accQueryClaims[0].PK);

			invoiceNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			invoiceNumberFilter.Property = "100";

			accQueryClaims = Factory.Load<APAccQueryClaim>(TestFilterBizO.Filter);
			AssertEquals("There should be 2 transactions in the collection", 2, accQueryClaims.Length);
			Array.Sort(accQueryClaims, AccQueryClaimComparer.GetComparer());
			Assert("Collection should contain claim1 ", Array.BinarySearch(accQueryClaims, claim1, AccQueryClaimComparer.GetComparer()) >= 0);
			Assert("Collection should contain claim3 ", Array.BinarySearch(accQueryClaims, claim3, AccQueryClaimComparer.GetComparer()) >= 0);
		}
	}
}
