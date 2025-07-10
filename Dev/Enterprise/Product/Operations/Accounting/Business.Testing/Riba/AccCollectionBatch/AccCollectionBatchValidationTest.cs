namespace Enterprise.Accounting.Business.Riba.Testing
{
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Core;

	internal class AccCollectionBatchValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckACB_CollectionFileFormat()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var batch = Factory.NewWithValidTestData<AccCollectionBatch>();
				batch.ACB_RX_NKCurrency = "EUR";
				batch.ACB_CollectionFileFormat = ZString.Empty;
				AssertHasError(batch.ACB_CollectionFileFormatInfo, "Please enter a value.");
				batch.ACB_CollectionFileFormat = "ABC";
				AssertHasError(batch.ACB_CollectionFileFormatInfo, "Enter a valid selection.");
				batch.ACB_CollectionFileFormat = "RIB";
				AssertNoErrors(batch.ACB_CollectionFileFormatInfo);
				batch.ACB_CollectionFileFormat = "DEF";
				AssertNoErrors(batch.ACB_CollectionFileFormatInfo);
				batch.ACB_CollectionFileFormat = "SEP";
				AssertNoErrors(batch.ACB_CollectionFileFormatInfo);
			}
		}

		public void TestCheckACB_AB()
		{
			AccBankAccount bank1 = Factory.NewWithValidTestData<AccBankAccount>();
			bank1.AB_RX_NKAccountCurrency = "AUD";
			AccBankAccount bank2 = Factory.NewWithValidTestData<AccBankAccount>();
			bank2.AB_RX_NKAccountCurrency = "USD";
			var batch = Factory.New<AccCollectionBatch>();
			batch.ACB_RX_NKCurrency = "USD";
			batch.ACB_AB = ZGuid.Empty;
			AssertHasError(batch.ACB_ABInfo, "Please enter a Bank Account.");
			batch.ACB_AB = bank1.PK;
			AssertHasError(batch.ACB_ABInfo, "Bank account must have same currency as the batch.");
			batch.ACB_AB = bank2.PK;
			AssertNoErrors(batch.ACB_ABInfo);
		}

		public void TestCheckACB_TotalAmount()
		{
			var batch = Factory.New<AccCollectionBatch>();
			batch.IsCancelled = false;
			batch.ACB_TotalAmount = 100m;
			AssertNoErrors(batch.ACB_TotalAmountInfo);
			batch.ACB_TotalAmount = -10m;
			AssertHasError(batch.ACB_TotalAmountInfo, "Batch total amount can not be negative.");
			batch.ACB_TotalAmount = 0m;
			AssertHasError(batch.ACB_TotalAmountInfo, "Batch total amount can not be 0. If you would like to cancel this batch, please run 'Cancel Batch' in the main menu.");
			batch.IsCancelled = true;
			batch.Validation.ValidateACB_TotalAmount();
			AssertNoErrors(batch.ACB_TotalAmountInfo);
		}

		public void TestCheckACB_TypeEmpty()
		{
			var batch = Factory.New<AccCollectionBatch>();
			batch.ACB_Type = ZString.Empty;
			AssertHasError(batch.ACB_TypeInfo, "Please enter a Batch Type.");
		}

		public void TestCheckACB_TypeNotPresentInRegistry()
		{
			var batch = Factory.New<AccCollectionBatch>();
			var item = AccountingMasterFilesRegistry.Instance.CollectionBatchTypes;
			var lookUpList = new CodeDescriptionBoolCollection();
			lookUpList.Add("ST1", (NoResString)"Batch 1", true);
			lookUpList.Add("ST2", (NoResString)"Batch 2", false);
			lookUpList.Add("ST3", (NoResString)"Batch 3", true);
			item.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, lookUpList);
			batch.ACB_Type = "ST4";
			AssertHasError(batch.ACB_TypeInfo, "Enter a valid Batch Type.");
		}
	}
}