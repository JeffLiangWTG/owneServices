using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPRLCusTempStorageDec))]
	public class CUSPRLCusTempStorageDecTest : CusTempStorageDecAbstractTest<CUSPRLCusTempStorageDec>
	{
		public void TestDefaults()
		{
			var storageDec = GetCusTempStorageDecForTesting();
			CombineAssertions(() =>
			{
				AssertEquals("STH_DeclarationType", TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger, storageDec.STH_DeclarationType);
				AssertEquals("STH_IdentificationIndicator", TemporaryStorageIdentificationIndicatorList.Codes.REG, storageDec.STH_IdentificationIndicator);
			});
		}

		public void TestATNoReadOnly()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
			CombineAssertions(() =>
			{
				storageDec.CusEntryNumber.CE_IssueDate = ZDateTime.Empty;
				AssertEquals("not readonly", false, storageDec.ReferenceNumberInfo.ReadOnly);
				storageDec.CusEntryNumber.CE_IssueDate = ZDateTime.Now;
				AssertEquals("readonly", true, storageDec.ReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestFetchStrategyGet()
		{
			AssertType<CusTempStorageDecFetchStrategy>(GetCusTempStorageDecForTesting().FetchStrategy);
		}

		public void TestLoadOrCreate()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
			var storageDecLoaded = CUSPRLCusTempStorageDec.LoadOrCreate(storageHeader);
			var storageDecCreated = CUSPRLCusTempStorageDec.LoadOrCreate(Factory.New<CusTempStorageJobHeader>());

			AssertNotNull(storageDec);
			AssertNotNull(storageDecLoaded);
			AssertNotNull(storageDecCreated);
			AssertEquals(storageDec, storageDecLoaded);
			AssertNotEquals(storageDec, storageDecCreated);
		}

		public void TestValidationOverriddenType()
		{
			AssertType<CUSPRLCusTempStorageDecValidation>(GetCusTempStorageDecForTesting().Validation);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObjectForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObjectForTest(factory);

		CUSPRLCusTempStorageDec GetNewBusinessObjectForTest(BusinessObjectFactory factory)
		{
			var storageJobHeader = factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageJobHeader);
			return storageDec;
		}

		protected override CUSPRLCusTempStorageDec GetCusTempStorageDecForTesting() => GetNewBusinessObjectForTest(Factory);

		#endregion
	}
}
