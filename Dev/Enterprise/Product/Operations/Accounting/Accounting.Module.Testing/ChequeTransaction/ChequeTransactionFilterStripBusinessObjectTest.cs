using CargoWise.Types;
using Enterprise.Accounting.Business.ChequeTransaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ChequeTransactionFilterStripBusinessObject))]
	public class ChequeTransactionFilterStripBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ChequeTransactionFilterStripBusinessObject();
		}

		protected ChequeTransactionHeaderCollection FilterCollection;
		protected ChequeTransactionFilterStripBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			FilterCollection = new ChequeTransactionHeaderCollection(Factory);
			FilterBO = GetNewFilterStripBusinessObject() as ChequeTransactionFilterStripBusinessObject;
		}

		#region Module Text Or Number Filter

		void AsserModuleTextOrNumberFilterCore<T>(string filterDescription, string propertyName, T value1, T value2)
		{
			var batch1 = Factory.NewWithValidTestData<ChequeTransactionHeader>();
			var batch2 = Factory.NewWithValidTestData<ChequeTransactionHeader>();

			batch1[propertyName] = value1;
			batch2[propertyName] = value2;

			var filter = FilterBO[filterDescription] as ModuleTextFilter;
			filter.IsActive = true;

			filter.Property = value1.ToString();
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Should Contain batch1", new[] { batch1 }, FilterCollection);
			AssertCollectionNotContains("Shouldn't contain batch2", new[] { batch2 }, FilterCollection);

			filter.Property = value2.ToString();
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Should Contain batch2", new[] { batch2 }, FilterCollection);
			AssertCollectionNotContains("Shouldn't contain batch1", new[] { batch1 }, FilterCollection);

			filter.Property = "Random Test Value";
			FilterCollection.Load(FilterBO.Filter);
			AssertCollectionNotContains("Shouldn't contain batch1 & batch2", new[] { batch1, batch2 }, FilterCollection);

			batch1[propertyName] = value1;
			batch2[propertyName] = value1;
			filter.Property = value1.ToString();
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Should Contain batch1 & batch2", new[] { batch1, batch2 }, FilterCollection);
		}

		public void TestFilterForTransactionNumber()
		{
			var filterDescription = "Transaction Number";
			AsserModuleTextOrNumberFilterCore(filterDescription, AccPaymentBatchSchema.APB_BatchNumber.Name, "000001", "000002");

			var filter = FilterBO[filterDescription] as ModuleNumberFilter;
			AssertEquals("Filter should have the correct max length", ModuleNumberFilter.MultiplyMaxLength(AccPaymentBatchSchema.APB_BatchNumber.MaxLength), filter.MaxLength);
		}

		public void TestFilterForTransactionType()
		{
			var filterDescription = "Transaction Type";
			AsserModuleTextOrNumberFilterCore(filterDescription, AccPaymentBatchSchema.APB_PaymentType.Name, "CET", "COC");

			var list = new CodeDescriptionPairList(OLookUpEditType.ChequeTransactionHeader);
			AssertNotNull(FilterBO.ChequeTransactionTypesList);
			AssertEquals(OLookUpEditType.ChequeTransactionHeader, FilterBO.ChequeTransactionTypesList.LookupEditType);
			AssertEquals(list.ElementsAsString, FilterBO.ChequeTransactionTypesList.ElementsAsString);
		}

		#endregion

		#region Module Date Filter

		void AsserModuleDateFilterCore(string filterDescription, string propertyName)
		{
			var value1 = new ZDateTime(2021, 5, 1, 12, 0, 0);
			var value2 = new ZDateTime(2021, 6, 1, 12, 0, 0);

			var batch1 = Factory.NewWithValidTestData<ChequeTransactionHeader>();
			var batch2 = Factory.NewWithValidTestData<ChequeTransactionHeader>();
			batch1[propertyName] = value1;
			batch2[propertyName] = value2;

			var filter = FilterBO[filterDescription] as ModuleDateFilter;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.IsActive = true;

			filter.Property1 = value1;
			filter.Property2 = value2.AddDays(-1);
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Should Contain batch1", new[] { batch1 }, FilterCollection);
			AssertCollectionNotContains("Shouldn't contain batch2", new[] { batch2 }, FilterCollection);

			filter.Property1 = value2;
			filter.Property2 = ZDateTime.Empty;
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Should Contain batch2", new[] { batch2 }, FilterCollection);
			AssertCollectionNotContains("Shouldn't contain batch1", new[] { batch1 }, FilterCollection);

			filter.Property1 = new ZDateTime(2023, 5, 1, 12, 0, 0);
			filter.Property2 = ZDateTime.Empty;
			FilterCollection.Load(FilterBO.Filter);
			AssertCollectionNotContains("Shouldn't contain batch1 & batch2", new[] { batch1, batch2 }, FilterCollection);

			filter.Property1 = new ZDateTime(2020, 5, 1, 12, 0, 0);
			filter.Property2 = ZDateTime.Empty;
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Should Contain batch1 & batch2", new[] { batch1, batch2 }, FilterCollection);
		}

		public void TestFilterForTransactionDate()
		{
			var filterDescription = "Transaction Date";
			var propertyName = AccPaymentBatchSchema.APB_PaymentDate.Name;
			AsserModuleDateFilterCore(filterDescription, propertyName);
		}

		#endregion

		#region Module Guid Filter

		void AsserModuleGuidFilterCore(string filterDescription, string propertyName, ZGuid value1, ZGuid value2, ZGuid value3)
		{
			var batch1 = Factory.NewWithValidTestData<ChequeTransactionHeader>();
			var batch2 = Factory.NewWithValidTestData<ChequeTransactionHeader>();
			batch1[propertyName] = value1;
			batch2[propertyName] = value2;

			var filter = FilterBO[filterDescription] as ModuleGuidFilter;
			filter.IsActive = true;

			filter.Property = value1;
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Should Contain batch1", new[] { batch1 }, FilterCollection);
			AssertCollectionNotContains("Shouldn't contain batch2", new[] { batch2 }, FilterCollection);

			filter.Property = value2;
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Should Contain batch2", new[] { batch2 }, FilterCollection);
			AssertCollectionNotContains("Shouldn't contain batch1", new[] { batch1 }, FilterCollection);

			filter.Property = value3;
			FilterCollection.Load(FilterBO.Filter);
			AssertCollectionNotContains("Shouldn't contain batch1 & batch2", new[] { batch1, batch2 }, FilterCollection);
		}

		public void TestFilterForDebtorOrCreditor()
		{
			var filterDescription = "Debtor/Creditor";
			var propertyName = AccPaymentBatchSchema.APB_OH_DebtorOrCreditor.Name;

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();

			AsserModuleGuidFilterCore(filterDescription, propertyName, orgHeader1.PK, orgHeader2.PK, orgHeader3.PK);

			AssertNotNull(FilterBO.OrganizationList);
			AssertEquals(typeof(OrgHeaderCollection), FilterBO.OrganizationList.GetType());
		}

		public void TestFilterForBankAccount()
		{
			var filterDescription = "Bank Account";
			var propertyName = AccPaymentBatchSchema.APB_AB.Name;

			var bankAccount1 = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount3 = Factory.NewWithValidTestData<AccBankAccount>();

			AsserModuleGuidFilterCore(filterDescription, propertyName, bankAccount1.PK, bankAccount2.PK, bankAccount3.PK);

			AssertNotNull(FilterBO.BankList);
			AssertEquals(typeof(AccBankAccountCollection), FilterBO.BankList.GetType());
		}

		public void TestFilterForCreatingBranch()
		{
			var filterDescription = "Creating Branch";
			var propertyName = AccPaymentBatchSchema.APB_GB.Name;

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();

			AsserModuleGuidFilterCore(filterDescription, propertyName, branch1.PK, branch2.PK, branch3.PK);

			AssertNotNull(FilterBO.BranchList);
			AssertEquals(typeof(GlbBranchCollection), FilterBO.BranchList.GetType());
		}

		#endregion
	}
}
