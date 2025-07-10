namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class ImporterProviderTest : Customs.Business.Testing.DataProviderTestCase<ImporterProvider>
	{
		public void TestId()
		{
			SetUpTestData();
			AssertEquals("ConsigneeRegNo", "62318879", importerProvider.Id);

			bill.ABL_ConsigneeRegNoType = "CGT";
			AssertEquals("ConsigneeRegNo", "CGT62318879", importerProvider.Id);

			bill.ABL_ConsigneeRegNoType = string.Empty;
			AssertEquals("ConsigneeRegNo", "NR", importerProvider.Id);
		}

		public void TestName()
		{
			SetUpTestData();
			AssertEquals("Precondition: Id is not null or NR", "62318879", importerProvider.Id);
			AssertNull("ConsigneeName is null when Id is registered", importerProvider.Name);

			bill.ABL_ConsigneeRegNo = string.Empty;
			importerProvider = new ImporterProvider(bill);
			AssertEquals("Precondition: Id is Not Registered", "NR", importerProvider.Id);
			AssertEquals("ConsigneeName is not null when Id is not registered", "Consignee", importerProvider.Name);
		}

		public void TestAddress()
		{
			SetUpTestData();
			AssertEquals("Precondition: Id is not null or NR", "62318879", importerProvider.Id);
			AssertNull("ConsigneeAddress is null when Id is registered", importerProvider.Address);

			bill.ABL_ConsigneeRegNo = string.Empty;
			importerProvider = new ImporterProvider(bill);
			AssertEquals("Precondition: Id is Not Registered", "NR", importerProvider.Id);
			AssertNotNull("ConsigneeAddress is not null when Id is not empty", importerProvider.Address);
		}

		public void TestContact()
		{
			SetUpTestData();
			AssertNull(importerProvider.Contact);
		}

		public void TestContactDetails()
		{
			SetUpTestData();
			AssertNull(importerProvider.ContactDetails);
		}

		public void TestForceIncludeNameAndAddressDetailsInMessage()
		{
			SetUpTestData();
			bill.ABL_ConsigneeName = "Consignee";
			bill.ABL_ConsigneeRegNo = "";
			AssertEquals("Precondition: Id is NR", "NR", importerProvider.Id);
			Assert("When Id is NR", importerProvider.ForceIncludeNameAndAddressDetailsInMessage);

			bill.ABL_ConsigneeRegNo = "1234";
			bill.ABL_ConsigneeRegNoType = ImporterIdentificationTypes.Codes.EOR;
			AssertEquals("Precondition: Id is not null or NR", "1234", importerProvider.Id);
			Assert("When Id is not null or NR", !importerProvider.ForceIncludeNameAndAddressDetailsInMessage);
		}

		protected override ImporterProvider GetProvider()
		{
			SetUpTestData();
			return importerProvider;
		}

		void SetUpTestData()
		{
			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsOffice = "101";

			bill = header.Bills.AddNew();
			bill.ABL_ConsigneeRegNoType = "EOR";
			bill.ABL_ConsigneeRegNo = "62318879";
			bill.ABL_ConsigneeName = "Consignee";

			importerProvider = new ImporterProvider(bill);
		}

		AsycudaManifestHeader header; 
		AsycudaBill bill;
		ImporterProvider importerProvider;
	}
}
