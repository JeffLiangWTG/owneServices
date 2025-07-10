using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGSPOCusTempStorageDec))]
	class CHGSPOCusTempStorageDecTest : CusTempStorageDecAbstractTest<CHGSPOCusTempStorageDec>
	{
		public void TestDefaults()
		{
			AssertEquals(TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm, GetCusTempStorageDecForTesting().STH_DeclarationType);
		}

		public void TestCusTempStorageLinesType()
		{
			AssertType<CusTempStorageLineCollection<CHGSPOCusTempStorageLine, CHGSPOCusTempStorageDec>>(GetCusTempStorageDecForTesting().CusTempStorageLines);
		}

		public void TestLookupsType()
		{
			AssertType<CHGSPOCusTempStorageDecLookups>(GetCusTempStorageDecForTesting().Lookups);
		}

		public void TestValidationType()
		{
			AssertType<CHGSPOCusTempStorageDecValidation>(GetCusTempStorageDecForTesting().Validation);
		}

		public void TestGetReferenceNo()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			storageDec.STH_OwnerReferenceNumber = "ATB150002930220195875";
			CombineAssertions(() =>
			{
				AssertEquals("Formatted ATB No.", "AT/B/15/000293/02/2019/5875", storageDec.ReferenceNumber);
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				AssertEquals("AWB No.", "ATB150002930220195875", storageDec.ReferenceNumber);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObjectForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObjectForTest(factory);

		protected override CHGSPOCusTempStorageDec GetCusTempStorageDecForTesting() => GetNewBusinessObjectForTest(Factory);

		CHGSPOCusTempStorageDec GetNewBusinessObjectForTest(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "CHGSPO";
			var presenter = factory.NewWithValidTestData<OrgAddress>();
			var representative = factory.NewWithValidTestData<OrgAddress>();

			var storageJobHeader = factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			storageJobHeader.SJH_JobReference = "REFERENCE1";
			storageJobHeader.SJH_OH_Customer = customer.PK;
			storageJobHeader.SJH_OA_Presenter = presenter.PK;
			storageJobHeader.SJH_OA_Representative = representative.PK;

			var storageDec = storageJobHeader.CHGSPOCusTempStorageDecs.AddNew();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm;
			storageDec.STH_SJH = storageJobHeader.PK;

			return storageDec;
		}
	}
}
