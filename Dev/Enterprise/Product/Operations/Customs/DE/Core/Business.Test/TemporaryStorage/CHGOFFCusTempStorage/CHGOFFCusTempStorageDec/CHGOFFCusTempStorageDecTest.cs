using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGOFFCusTempStorageDec))]
	class CHGOFFCusTempStorageDecTest : CusTempStorageDecAbstractTest<CHGOFFCusTempStorageDec>
	{
		public void TestDefaults()
		{
			AssertEquals(TemporaryStorageDeclarationTypeList.Codes.ChangeDisposalEntitledTrader, GetCusTempStorageDecForTesting().STH_DeclarationType);
		}

		public void TestLookupsOverriddenType()
		{
			AssertType<CHGOFFCusTempStorageDecLookups>(GetCusTempStorageDecForTesting().Lookups);
		}

		public void TestValidationOverriddenType()
		{
			AssertType<CHGOFFCusTempStorageDecValidation>(GetCusTempStorageDecForTesting().Validation);
		}

		public void TestSTH_IdentificationIndicator()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			var storageLine1 = storageDec.CusTempStorageLines.AddNew();
			storageLine1.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.AWB;
			var storageLine2 = storageDec.CusTempStorageLines.AddNew();
			storageLine2.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.AWB;
			storageDec.STH_IdentificationIndicator = "QRS";
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

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObjectForTest(factory);

		protected override BusinessObject GetNewBusinessObject() => Factory.New<CHGOFFCusTempStorageDec>();

		protected override CHGOFFCusTempStorageDec GetCusTempStorageDecForTesting() => GetNewBusinessObjectForTest(Factory);

		CHGOFFCusTempStorageDec GetNewBusinessObjectForTest(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "CHGOFF";

			var storageJobHeader = factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			storageJobHeader.SJH_JobReference = "DECHGTST001";
			storageJobHeader.SJH_OH_Customer = customer.PK;

			return storageJobHeader.CHGOFFCusTempStorageDecs.AddNew();
		}

		#endregion

	}
}
