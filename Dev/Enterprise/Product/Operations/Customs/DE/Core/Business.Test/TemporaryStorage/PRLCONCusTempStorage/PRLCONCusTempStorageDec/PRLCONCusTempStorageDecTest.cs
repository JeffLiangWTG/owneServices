using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(PRLCONCusTempStorageDec))]
	public class PRLCONCusTempStorageDecTest : CusTempStorageDecAbstractTest<PRLCONCusTempStorageDec>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(TemporaryStorageDeclarationTypeList.Codes.PresentationLedgerConsolidation, GetCusTempStorageDecForTesting().STH_DeclarationType);
		}

		public void TestCusTempStorageLines()
		{
			AssertType<PRLCONCusTempStorageLineToConsolidateCollection>(GetCusTempStorageDecForTesting().CusTempStorageLines);
		}

		public void TestLookupsOverriddenType()
		{
			AssertType<PRLCONCusTempStorageDecLookups>(GetCusTempStorageDecForTesting().Lookups);
		}

		public void TestValidationOverriddenType()
		{
			AssertType<PRLCONCusTempStorageDecValidation>(GetCusTempStorageDecForTesting().Validation);
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObjectForTest(factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForTest(Factory);

		PRLCONCusTempStorageDec GetNewBusinessObjectForTest(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "CUSTEST";
			var presenter = factory.NewWithValidTestData<OrgAddress>();
			var representative = factory.NewWithValidTestData<OrgAddress>();

			var storageJobHeader = factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			storageJobHeader.SJH_JobReference = "DECUSPRL001";
			storageJobHeader.SJH_OH_Customer = customer.PK;
			storageJobHeader.SJH_OA_Presenter = presenter.PK;
			storageJobHeader.SJH_OA_Representative = representative.PK;

			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();

			return storageDec;
		}

		protected override PRLCONCusTempStorageDec GetCusTempStorageDecForTesting() => GetNewBusinessObjectForTest(Factory);

		#endregion
	}
}
