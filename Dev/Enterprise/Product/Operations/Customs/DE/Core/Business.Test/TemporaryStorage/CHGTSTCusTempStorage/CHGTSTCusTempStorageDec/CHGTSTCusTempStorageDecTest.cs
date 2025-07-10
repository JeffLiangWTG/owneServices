using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGTSTCusTempStorageDec))]
	class CHGTSTCusTempStorageDecTest : CusTempStorageDecAbstractTest<CHGTSTCusTempStorageDec>
	{
		public void TestDefaults()
		{
			AssertEquals(TemporaryStorageDeclarationTypeList.Codes.ChangeCustodyInformation, GetCusTempStorageDecForTesting().STH_DeclarationType);
		}

		public void TestSTH_IdentificationIndicator()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			var storageLine1 = storageDec.CusTempStorageLines.AddNew();
			storageLine1.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.AWB;
			var storageLine2 = storageDec.CusTempStorageLines.AddNew();
			storageLine2.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.AWB;
			storageDec.STH_IdentificationIndicator = "XYZ";
			AssertEquals(OwnerReferenceTypeList.Codes.AWB, storageLine1.TSL_OwnerReferenceType);
			AssertEquals(OwnerReferenceTypeList.Codes.AWB, storageLine2.TSL_OwnerReferenceType);
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			AssertEquals(OwnerReferenceTypeList.Codes.REG, storageLine1.TSL_OwnerReferenceType);
			AssertEquals(OwnerReferenceTypeList.Codes.REG, storageLine2.TSL_OwnerReferenceType);
			var storageLine3 = storageDec.CusTempStorageLines.AddNew();
			storageLine3.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.ULD;
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			AssertEquals(ZString.Empty, storageLine1.TSL_OwnerReferenceType);
			AssertEquals(ZString.Empty, storageLine2.TSL_OwnerReferenceType);
			AssertEquals(OwnerReferenceTypeList.Codes.ULD, storageLine3.TSL_OwnerReferenceType);
		}

		public void TestFetchStrategyGet()
		{
			AssertType<CusTempStorageDecFetchStrategy>(GetCusTempStorageDecForTesting().FetchStrategy);
		}

		public void TestSTH_IdentificationIndicatorNotDefaulted()
		{
			AssertEquals(true, GetCusTempStorageDecForTesting().STH_IdentificationIndicator.IsEmpty);
		}

		public void TestLookupsOverriddenType()
		{
			AssertType<CHGTSTCusTempStorageDecLookups>(GetCusTempStorageDecForTesting().Lookups);
		}

		public void TestValidationOverriddenType()
		{
			AssertType<CHGTSTCusTempStorageDecValidation>(GetCusTempStorageDecForTesting().Validation);
		}

		public void TestNewCustodianBranchMaxlength()
		{
			AssertEquals(4, GetCusTempStorageDecForTesting().NewCustodianBranchInfo.MaxLength);
		}

		public void TestGetATNo()
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

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObjectForTest(factory);

		protected override BusinessObject GetNewBusinessObject() => Factory.New<CHGTSTCusTempStorageDec>();

		protected override CHGTSTCusTempStorageDec GetCusTempStorageDecForTesting() => GetNewBusinessObjectForTest(Factory);

		CHGTSTCusTempStorageDec GetNewBusinessObjectForTest(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "CHGTST";

			var storageJobHeader = factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			storageJobHeader.SJH_JobReference = "DECHGTST001";
			storageJobHeader.SJH_OH_Customer = customer.PK;

			return storageJobHeader.CHGTSTCusTempStorageDecs.AddNew();
		}
	}
}
