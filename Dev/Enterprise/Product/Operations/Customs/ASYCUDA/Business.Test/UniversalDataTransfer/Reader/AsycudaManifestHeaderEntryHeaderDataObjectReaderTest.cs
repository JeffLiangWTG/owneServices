using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	partial class AsycudaManifestHeaderEntryHeaderDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportAsycudaManifestHeaderEntryHeaderData()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var helper = new ZZDataTestHelper(Factory.BOFactory);
			Factory.SaveForTesting();
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var readerHelper = header.ApplicationBusinessProvider.GetAsycudaManifestDataObjectReaderHelper(header.AMA_RN_NKCountry);
			var headerEntryHeader1 = help.SetupCountryHeaderEntryHeader("US", 1);
			headerEntryHeader1.EntryNumberCollection = new List<EntryNumber>();
			headerEntryHeader1.EntryNumberCollection.AddSafe(new EntryNumber()
			{
				Number = "EN0001",
				Type = new UniversalDataBuss.DataObjects.Universal.EntryType() { Code = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration },
				EntryStatus = new UniversalDataBuss.DataObjects.Universal.EntryStatus() { Code = AsycudaRegistrationStatuses.Codes.Registered },
				IssueDate = new ZDateTime(2017, 6, 23)
			});
			var readBO = new AsycudaManifestHeaderEntryHeaderDataObjectReader(headerEntryHeader1, Logger, Factory, header, readerHelper).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var factory = new BusinessObjectFactory();
			var headerBO = factory.Load<AsycudaManifestHeader>(readBO.PK);

			AssertNotNull(headerBO);
			AssertEquals("Header.AMA_RN_NKCountry", "US", headerBO.AMA_RN_NKCountry);
			AssertEquals("RegistrationNumber", "EN0001", headerBO.RegistrationNumber);
			AssertEquals("RegistrationStatus", AsycudaRegistrationStatuses.Codes.Registered, headerBO.RegistrationStatus);
			AssertEquals("RegistrationDate", new ZDateTime(2017, 6, 23), headerBO.RegistrationDate);
		}

		public void TestUpdatingEntryNumber()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var helper = new ZZDataTestHelper(Factory.BOFactory);
			Factory.SaveForTesting();
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var readerHelper = header.ApplicationBusinessProvider.GetAsycudaManifestDataObjectReaderHelper(header.AMA_RN_NKCountry);
			header.RegistrationNumber = "EN0002";

			var headerEntryHeader1 = help.SetupCountryHeaderEntryHeader("US", 1);
			headerEntryHeader1.EntryNumberCollection = new List<EntryNumber>();
			headerEntryHeader1.EntryNumberCollection.AddSafe(new EntryNumber()
			{
				Number = "EN0001",
				Type = new UniversalDataBuss.DataObjects.Universal.EntryType() { Code = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration },
				EntryStatus = new UniversalDataBuss.DataObjects.Universal.EntryStatus() { Code = AsycudaRegistrationStatuses.Codes.Registered },
				IssueDate = new ZDateTime(2017, 6, 23)
			});
			var readHeaderBO = new AsycudaManifestHeaderEntryHeaderDataObjectReader(headerEntryHeader1, Logger, Factory, header, readerHelper).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var factory = new BusinessObjectFactory();
			var headerBO = factory.Load<AsycudaManifestHeader>(readHeaderBO.PK);

			AssertNotNull(headerBO);
			AssertEquals("Header.AMA_RN_NKCountry", "US", headerBO.AMA_RN_NKCountry);
			AssertEquals("RegistrationNumber", "EN0001", headerBO.RegistrationNumber);
			AssertEquals("RegistrationStatus", AsycudaRegistrationStatuses.Codes.Registered, headerBO.RegistrationStatus);
			AssertEquals("RegistrationDate", new ZDateTime(2017, 6, 23), headerBO.RegistrationDate);
		}
		public void TestUpdateAsycudaManifestHeaderEntryHeaderData()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var helper = new ZZDataTestHelper(Factory.BOFactory);
			Factory.SaveForTesting();
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var readerHelper = header.ApplicationBusinessProvider.GetAsycudaManifestDataObjectReaderHelper(header.AMA_RN_NKCountry);
			var countryPK = header.PK;
			Factory.SaveForTesting();
			var headerEntryHeader1 = help.SetupCountryHeaderEntryHeader("US", 1);
			var headerBO = new AsycudaManifestHeaderEntryHeaderDataObjectReader(headerEntryHeader1, Logger, Factory, header, readerHelper).ReadIntoBusinessObject();
			AssertNotNull(headerBO);
			AssertEquals("country is updated", countryPK, headerBO.PK);
			AssertEquals("Header.AMA_RN_NKCountry", "US", headerBO.AMA_RN_NKCountry);
		}
	}
}
