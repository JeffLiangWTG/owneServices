using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	partial class AsycudaBillEntryHeaderDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportAsycudaBillEntryHeaderData()
		{
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var billCountryEntryHeader1 = dataObjectTestHelper.SetupCountryBillEntryHeader("US", 1, "CLR", "Sender Reference1");
			billCountryEntryHeader1.EntryNumberCollection = new List<EntryNumber>();
			billCountryEntryHeader1.EntryNumberCollection.AddSafe(new EntryNumber()
			{
				Number = "EN0001",
				Type = new UniversalDataBuss.DataObjects.Universal.EntryType() { Code = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration },
				EntryStatus = new UniversalDataBuss.DataObjects.Universal.EntryStatus() { Code = AsycudaRegistrationStatuses.Codes.Registered },
				IssueDate = new ZDateTime(2017, 6, 23)
			});
			var readCountryBO = new AsycudaBillEntryHeaderDataObjectReader(billCountryEntryHeader1, Logger, Factory, bill, readerHelper).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var factory = new BusinessObjectFactory();
			var countryBO = factory.Load<AsycudaBill>(readCountryBO.PK);

			AssertNotNull(countryBO);
			AssertEquals("ABL_BillStatus", "", countryBO.ABL_BillStatus);
			AssertEquals("ABL_SenderReference", "", countryBO.ABL_SenderReference);
			AssertEquals("RegistrationNumber", "EN0001", countryBO.CusEntryNumber.CE_EntryNum);
			AssertEquals("RegistrationNumber Country", Core.Constants.CountryCodes.UnitedStates, countryBO.CusEntryNumber.CE_RN_NKCountryCode);
		}

		public void TestUpdateAsycudaBillEntryHeaderData()
		{
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "SNT";
			bill.ABL_SenderReference = "0000001";
			Factory.SaveForTesting();

			var billCountryEntryHeader1 = dataObjectTestHelper.SetupCountryBillEntryHeader("US", 1, "CLR", "Sender Reference1");
			var countryBO = new AsycudaBillEntryHeaderDataObjectReader(billCountryEntryHeader1, Logger, Factory, bill, readerHelper).ReadIntoBusinessObject();
			AssertNotNull(countryBO);
			AssertEquals("Country is updated", bill.PK, countryBO.PK);
			AssertEquals("ABL_BillStatus unchanged", "SNT", countryBO.ABL_BillStatus);
			AssertEquals("ABL_SenderReference unchanged", "0000001", countryBO.ABL_SenderReference);
		}

		public void TestUpdateEntryNumber()
		{
			var bill = header.Bills.AddNew();
			var entryNumber = bill.CustomsEntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "EN0002";
			entryNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			entryNumber.CE_RN_NKCountryCode = header.AMA_RN_NKCountry;
			AssertEquals("EN0002", bill.CusEntryNumber.CE_EntryNum);

			var billCountryEntryHeader1 = dataObjectTestHelper.SetupCountryBillEntryHeader("US", 1, "CLR", "Sender Reference1");
			billCountryEntryHeader1.EntryNumberCollection = new List<EntryNumber>();
			billCountryEntryHeader1.EntryNumberCollection.AddSafe(new EntryNumber()
			{
				Number = "EN0001",
				Type = new UniversalDataBuss.DataObjects.Universal.EntryType() { Code = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration },
				EntryStatus = new UniversalDataBuss.DataObjects.Universal.EntryStatus() { Code = AsycudaRegistrationStatuses.Codes.Registered },
				IssueDate = new ZDateTime(2017, 6, 23)
			});
			var readCountryBO = new AsycudaBillEntryHeaderDataObjectReader(billCountryEntryHeader1, Logger, Factory, bill, readerHelper).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var factory = new BusinessObjectFactory();
			var countryBO = factory.Load<AsycudaBill>(readCountryBO.PK);

			AssertNotNull(countryBO);
			AssertEquals("ABL_BillStatus", "", countryBO.ABL_BillStatus);
			AssertEquals("ABL_SenderReference", "", countryBO.ABL_SenderReference);
			AssertEquals("EN0001", countryBO.CusEntryNumber.CE_EntryNum);
			AssertEquals("RegistrationNumber Country", Core.Constants.CountryCodes.UnitedStates, countryBO.CusEntryNumber.CE_RN_NKCountryCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			dataObjectTestHelper = new AsycudaManifestDataObjectReaderTestHelper();
			header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			readerHelper = header.ApplicationBusinessProvider.GetAsycudaManifestDataObjectReaderHelper(header.AMA_RN_NKCountry);
		}
		AsycudaManifestDataObjectReaderHelper readerHelper;
		AsycudaManifestDataObjectReaderTestHelper dataObjectTestHelper;
		AsycudaManifestHeader header;
	}
}
