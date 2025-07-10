using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	sealed class NctsPreviousDocumentPhase4LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCusTempStorageRegHeaders()
		{
			var regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_CustomsOffice = "FR230020";
			regHeader.SRH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;

			var regHeader3 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader3.SRH_CustomsOffice = "FR230020";
			regHeader3.SRH_AppCode = PreviousDocumentCodeList.Codes._760;
			Factory.Save();

			var previousDocument = Factory.NewWithValidTestData<NctsPreviousDocument>();
			AssertCusTempStorageRegHeadersAdditionalFilter(previousDocument, "", "document has no parent");

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var movementHeader = nctsHeader.MovementHeader;
			var cargoDesc = movementHeader.GoodsItems.AddNew();
			previousDocument = cargoDesc.PreviousDocuments.AddNew();
			AssertCusTempStorageRegHeadersAdditionalFilter(previousDocument, "", "document has a parent with a movementHeader");

			AssertWithCustomsOffice(false, "", "", "", "NctsHeader has no customs Office.");
			AssertWithCustomsOffice(true, "DES", "value", "", "NctsHeader has no DEP customs office.");
			AssertWithCustomsOffice(true, "DEP", "", "", "The DEP customs office has no value.");
			AssertWithCustomsOffice(true, "DEP", "FR230020", "FR230020", "The DEP customs office has value FR230020.");

			CombineAssertions("Only IST Tempstorage should be retrieve.", () =>
			{
				AssertEquals(1, previousDocument.Lookups.ReferenceList.Count);
				AssertEquals(FRConstants.TemporaryStorage.AppCodeIST, ((CusTempStorageRegHeaderCollection)previousDocument.Lookups.ReferenceList).ElementAt(0).SRH_AppCode);
				AssertEquals("FR230020", ((CusTempStorageRegHeaderCollection)previousDocument.Lookups.ReferenceList).ElementAt(0).SRH_CustomsOffice);
			});
		}

		void AssertWithCustomsOffice(bool hasCustomsOffice, string code, string data, string value, string comment)
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var movementHeader = nctsHeader.MovementHeader;
			var cargoDesc = movementHeader.GoodsItems.AddNew();

			nctsHeader.CustomsOffices.RemoveAndDeleteAll();

			if (hasCustomsOffice)
			{
				var customsOffice = nctsHeader.CustomsOffices.AddNew();
				customsOffice.CY_Code = code;
				customsOffice.CY_Data = data;
			}

			var previousDocument = cargoDesc.PreviousDocuments.AddNew();
			AssertCusTempStorageRegHeadersAdditionalFilter(previousDocument, value, comment);
		}

		void AssertCusTempStorageRegHeadersAdditionalFilter(NctsPreviousDocument previousDocument, string value, string cause)
		{
			var filterBusinessObjectDefaults = ((CusTempStorageRegHeaderCollection)previousDocument.Lookups.ReferenceList).FilterBusinessObjectDefaults;
			var valueForComment = value.IsNullOrEmpty() ? "empty" : value;
			CombineAssertions($"filterBusinessObjectDefaults has 2 items and the value is {valueForComment} because: {cause}", () =>
			{
				AssertEquals("additional filter count", 2, ((CusTempStorageRegHeaderCollection)previousDocument.Lookups.ReferenceList).FilterBusinessObjectDefaults.Count);
				AssertEquals("first additionalFilter name should be equal status", "Status", filterBusinessObjectDefaults[Declaration.PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status + ":Property"].FilterName);
				AssertEquals("first additionalFilter value should be equal OPN", "OPN", filterBusinessObjectDefaults[Declaration.PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status + ":Property"].Value);

				AssertEquals("second additionalFilter name should be equal status", "Customs Office", filterBusinessObjectDefaults[Declaration.PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice + ":Property"].FilterName);
				AssertEquals($"second additionalFilter value should be {valueForComment}", value, filterBusinessObjectDefaults[Declaration.PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice + ":Property"].Value);
			});
		}
	}
}
