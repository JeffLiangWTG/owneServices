using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public abstract class TransactionHeaderCollectionForClaimTest : AccTransactionHeaderCollectionTest
	{
		protected AccQueryClaim QueryClaim
		{
			get { return fQueryClaim ?? (fQueryClaim = (AccQueryClaim)Factory.NewWithValidTestData(GetQueryClaimType())); }
			set { fQueryClaim = value; }
		}
		AccQueryClaim fQueryClaim;

		protected virtual Type GetQueryClaimType()
		{
			return typeof(ARAccQueryClaim);
		}

		protected virtual Type GetTransactionType()
		{
			return typeof(ARInvoice);
		}

		public void TestFiltering()
		{
			GlbCompany otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			GlbBranch otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, otherCompany.PK));
			ZQuery differentBranchOfTheSameCompanyFilter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			differentBranchOfTheSameCompanyFilter.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
			GlbBranch differentBranchOfTheSameCompany = Factory.LoadTop1<GlbBranch>(differentBranchOfTheSameCompanyFilter);

			AccQueryClaim queryClaim1 = (AccQueryClaim)Factory.NewWithValidTestData(GetQueryClaimType());

			AccTransactionHeader invoice1 = (AccTransactionHeader)Factory.NewWithValidTestData(GetTransactionType());
			AccTransactionHeader invoice2 = (AccTransactionHeader)Factory.NewWithValidTestData(GetTransactionType());
			AccTransactionHeader invoice3 = (AccTransactionHeader)Factory.NewWithValidTestData(GetTransactionType());
			invoice2.AH_GB = otherBranch.PK;
			invoice3.AH_GB = differentBranchOfTheSameCompany.PK;
			invoice1.AH_OH = invoice2.AH_OH = invoice3.AH_OH = queryClaim1.AY_OH_Debtor;

			Factory.Save();

			fQueryClaim = queryClaim1;
			TransactionHeaderCollection transactionCollection = (TransactionHeaderCollection)GetCollectionToTest();
			transactionCollection.Load();

			AssertEquals("Collection should contain two records.", 2, transactionCollection.Count);
			Assert("Collection should contain invoice1", transactionCollection.Contains(invoice1));
			Assert("Collection should contain invoice3", transactionCollection.Contains(invoice3));

			transactionCollection.Load(new ZQuery()); // to test RelationshipFilter alone

			AssertEquals("Collection should contain two records.", 2, transactionCollection.Count);
			Assert("Collection should contain invoice1", transactionCollection.Contains(invoice1));
			Assert("Collection should contain invoice3", transactionCollection.Contains(invoice3));
		}

		#region TestDefaultValues

		public void TestDefaultValues()
		{
			ZQuery branchesFilter = new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
			ZGuid branchPK = Factory.LoadTop1(typeof(GlbBranch), branchesFilter).PK;

			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			CheckAY_AHFiltering(ZArchitecture.Core.TransactionTypes.CreditNote, branchPK, org.PK);
			Assert("Invalid Invoice - Invalid Transaction Type", QueryClaim.AY_AHInfo.HasErrors());

			QueryClaim.AY_AH = ZGuid.NewZGuid();
			Assert("Invalid invoice number", QueryClaim.AY_AHInfo.HasErrors());

			QueryClaim.AY_AH = ZGuid.Empty;
			Factory.Save();

			CheckAY_AHFiltering(ZArchitecture.Core.TransactionTypes.Invoice, branchPK, org.PK);
			Assert("Invoice number should not have errors", !QueryClaim.AY_AHInfo.HasErrors());
		}

		void CheckAY_AHFiltering(ZString transactionType, ZGuid branch, ZGuid debtor)
		{
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = QueryClaim.Ledger;
			header.AH_TransactionType = transactionType;
			header.AH_GB = branch;
			header.AH_OH = debtor;

			QueryClaim.AY_GB = branch;
			QueryClaim.AY_OH_Debtor = debtor;
			QueryClaim.AY_AH = header.PK;
			QueryClaim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			Factory.Save();
			QueryClaim.Lookups.TransactionHeaders.Load();
			QueryClaim.Validation.ValidateAY_AH();
		}

		#endregion

		public void TestFilterBusinessObjectDefaults()
		{
			var claim = (AccQueryClaim)Factory.NewWithValidTestData(GetQueryClaimType());
			Action<string, bool> assertDefaultFilter = (filterName, isNotRemoveable) =>
			{
				string defaultFilterPropertyKey = filterName + ":Property";
				AssertEquals(string.Format("TransactionHeaders contains default filter for {0}", filterName), true, claim.Lookups.TransactionHeaders.FilterBusinessObjectDefaults.ContainsDefaultFor(defaultFilterPropertyKey));
				AssertEquals(string.Format("{0} default filter readonly state", filterName), isNotRemoveable, !claim.Lookups.TransactionHeaders.FilterBusinessObjectDefaults[defaultFilterPropertyKey].IsRemovable);
			};

			assertDefaultFilter("Transaction Type", true);
			assertDefaultFilter(claim is APAccQueryClaim ? "Creditor" : "Debtor", false);
			assertDefaultFilter("Branch", false);
		}
	}
}