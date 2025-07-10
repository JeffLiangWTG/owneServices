using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(PaymentBatchFilterStripBusinessObject))]
	public class PaymentBatchFilterStripBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new PaymentBatchFilterStripBusinessObject();
		}

		protected AccPaymentBatchCollection FilterCollection;
		protected PaymentBatchFilterStripBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			FilterCollection = new AccPaymentBatchCollection(Factory);
			FilterBO = GetNewFilterStripBusinessObject() as PaymentBatchFilterStripBusinessObject;
		}

		#region Module Text Or Number Filter

		void AsserModuleTextOrNumberFilterCore<T>(string filterDescription, string propertyName, T value1, T value2)
		{
			var batch1 = Factory.NewWithValidTestData<AccPaymentBatch>();
			var batch2 = Factory.NewWithValidTestData<AccPaymentBatch>();

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

		public void TestFilterForBatchNumber()
		{
			var filterDescription = "Payment Batch Number";
			AsserModuleTextOrNumberFilterCore(filterDescription, AccPaymentBatchSchema.APB_BatchNumber.Name, "000001", "000002");

			var filter = FilterBO[filterDescription] as ModuleNumberFilter;
			AssertEquals("Filter should have the correct max length", ModuleNumberFilter.MultiplyMaxLength(AccPaymentBatchSchema.APB_BatchNumber.MaxLength), filter.MaxLength);
		}

		public void TestFilterForChequeOrReference()
		{
			var filterDescription = "Cheque or Reference";
			AsserModuleTextOrNumberFilterCore(filterDescription, AccPaymentBatchSchema.APB_ChequeOrReference.Name, "000001", "000002");

			var filter = FilterBO[filterDescription] as ModuleNumberFilter;
			AssertEquals("Filter should have the correct max length", ModuleNumberFilter.MultiplyMaxLength(AccPaymentBatchSchema.APB_ChequeOrReference.MaxLength), filter.MaxLength);
		}

		public void TestFilterForPaymentType()
		{
			var filterDescription = "Payment Type";
			AsserModuleTextOrNumberFilterCore(filterDescription, AccPaymentBatchSchema.APB_PaymentType.Name, "CSH", "CCD");

			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.PaymentMethod);
			AssertNotNull(FilterBO.PaymentTypeList);
			AssertEquals(OLookUpEditType.PaymentMethod, FilterBO.PaymentTypeList.LookupEditType);
			AssertEquals(list.ElementsAsString, FilterBO.PaymentTypeList.ElementsAsString);
		}

		public void TestFilterForBatchStatus()
		{
			var filterDescription = "Batch Status";
			AsserModuleTextOrNumberFilterCore(filterDescription, AccPaymentBatchSchema.APB_Status.Name, AccPaymentBatchStatus.Working, AccPaymentBatchStatus.Completed);

			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.PaymentBatchStatus);
			AssertNotNull(FilterBO.BatchStatusList);
			AssertEquals(OLookUpEditType.PaymentBatchStatus, FilterBO.BatchStatusList.LookupEditType);
			AssertEquals(list.ElementsAsString, FilterBO.BatchStatusList.ElementsAsString);
		}

		#endregion

		#region Module Date Filter

		void AsserModuleDateFilterCore(string filterDescription, string propertyName)
		{
			var value1 = new ZDateTime(2021, 5, 1, 12, 0, 0);
			var value2 = new ZDateTime(2021, 6, 1, 12, 0, 0);

			var batch1 = Factory.NewWithValidTestData<AccPaymentBatch>();
			var batch2 = Factory.NewWithValidTestData<AccPaymentBatch>();
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

		public void TestFilterForPaymentDate()
		{
			var filterDescription = "Payment Date";
			var propertyName = AccPaymentBatchSchema.APB_PaymentDate.Name;
			AsserModuleDateFilterCore(filterDescription, propertyName);
		}

		public void TestFilterForPostDate()
		{
			var filterDescription = "Post Date";
			var propertyName = AccPaymentBatchSchema.APB_PostDate.Name;
			AsserModuleDateFilterCore(filterDescription, propertyName);
		}

		#endregion

		#region Module Guid Filter

		void AsserModuleGuidFilterCore(string filterDescription, string propertyName, ZGuid value1, ZGuid value2, ZGuid value3)
		{
			var batch1 = Factory.NewWithValidTestData<AccPaymentBatch>();
			var batch2 = Factory.NewWithValidTestData<AccPaymentBatch>();
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

		public void TestFilterForCheckBook()
		{
			var filterDescription = "Cheque Book";
			var propertyName = AccPaymentBatchSchema.APB_AK.Name;

			var chequeBook1 = Factory.NewWithValidTestData<AccChequeBook>();
			var chequeBook2 = Factory.NewWithValidTestData<AccChequeBook>();
			var chequeBook3 = Factory.NewWithValidTestData<AccChequeBook>();

			AsserModuleGuidFilterCore(filterDescription, propertyName, chequeBook1.PK, chequeBook2.PK, chequeBook3.PK);

			AssertNotNull(FilterBO.CheckBookList);
			AssertEquals(typeof(AccChequeBookCollection), FilterBO.CheckBookList.GetType());
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
