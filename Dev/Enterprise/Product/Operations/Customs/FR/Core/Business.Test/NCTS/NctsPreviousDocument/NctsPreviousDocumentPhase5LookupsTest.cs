using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	sealed class NctsPreviousDocumentPhase5LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReferenceList()
		{
			var previousDocument = GetPreviousDocumentForTest();

			CombineAssertions(() =>
			{
				previousDocument.CSI_Code = ZString.Empty;
				AssertNotNull("ReferenceList is not null if the previous document code is not N337", previousDocument.Lookups.ReferenceList);
				AssertType<CodeDescriptionPairList>("ReferenceList is of type CodeDescriptionPairList if previous document code is not N337", previousDocument.Lookups.ReferenceList);

				previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
				var referenceList = previousDocument.Lookups.ReferenceList;
				AssertNotNull("ReferenceList is not null if the previous document code is N337", referenceList);
				AssertType<CusTempStorageRegHeaderCollection>("ReferenceList is of type CusTempStorageRegHeaderCollection if previous document code is N337", referenceList);
			});
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_CustomsOffice = "FR230020";
			header1.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;

			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_CustomsOffice = "FR230021";
			header2.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;

			var referenceList = previousDocument.Lookups.ReferenceList as CusTempStorageRegHeaderCollection;
			CombineAssertions("Verify loaded BOs in ReferenceList", () =>
			{
				AssertEquals("ReferenceList should contain 2 headers.", 2, referenceList.Count);
				AssertEquals("ReferenceList first header.", header1, referenceList[0]);
				AssertEquals("ReferenceList second header.", header2, referenceList[1]);
			});
		}

		public void TestReferenceListFilters()
		{
			var previousDocument = GetPreviousDocumentForTest();
			var filters = GetTempStorageFilters(previousDocument);

			CombineAssertions("N337 Filters", () =>
			{
				AssertEquals("Count", 5, filters.Count);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.TsdNumber, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status, value: "OPN", isRemovable: false);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PackingType, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PreviousReferenceNumber, value: string.Empty, isRemovable: true);
			});
		}

		public void TestReferenceListFilters_IfAny_CustomsOfficeExist()
		{
			var previousDocument = GetPreviousDocumentForTest("FR230020");
			var filters = GetTempStorageFilters(previousDocument);

			CombineAssertions("N337 filters when a customs office is present.", () =>
			{
				AssertEquals("Count", 5, filters.Count);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.TsdNumber, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status, value: "OPN", isRemovable: false);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, value: "FR230020", isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PackingType, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PreviousReferenceNumber, value: string.Empty, isRemovable: true);
			});
		}

		public void TestReferenceListFilters_IfAnyCSI_ReferenceNumberExist()
		{
			var previousDocument = GetPreviousDocumentForTest("FR230020");
			previousDocument.CSI_ReferenceNumber = "123456";
			var filters = GetTempStorageFilters(previousDocument);

			CombineAssertions("N337 filters when a CSI reference number is present.", () =>
			{
				AssertEquals("Count", 5, filters.Count);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.TsdNumber, value: "123456", isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status, value: "OPN", isRemovable: false);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, value: "FR230020", isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PackingType, value: string.Empty, isRemovable: true);
				AssertFilterBODefault(filters, PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.PreviousReferenceNumber, value: string.Empty, isRemovable: true);
			});
		}

		void AssertFilterBODefault(FilterBusinessObjectDefaults filters, string propertyName, string value, bool isRemovable)
		{
			var filterKey = new StringBuilder().Append(propertyName).Append(FilterBusinessObjectDefault.FilterPropertyDelimiter).Append("Property").ToString();
			var filter = filters[filterKey];
			AssertEquals($"{propertyName} filter value", filter.Value, value);
			AssertEquals($"{propertyName} Filter removable", isRemovable, filter.IsRemovable);
		}

		NctsPreviousDocument GetPreviousDocumentForTest(ZString customsOfficeCode = default)
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			if (!customsOfficeCode.IsEmpty)
			{
				var movementHeader = nctsHeader.MovementHeader;
				movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
				var customsOffice = movementHeader.CustomsOfficesForDeparture.AddNew();
				customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				customsOffice.CY_Data = customsOfficeCode;
			}

			var bill = nctsHeader.Bills.AddNew();
			var goodItem = bill.GoodsItems.AddNew();
			previousDocument = goodItem.PreviousDocuments.AddNew();
			return previousDocument;
		}

		FilterBusinessObjectDefaults GetTempStorageFilters(NctsPreviousDocument previousDocument)
		{
			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			var referenceList = previousDocument.Lookups.ReferenceList;
			var cusTempStorageHeaders = referenceList as CusTempStorageRegHeaderCollection;
			return cusTempStorageHeaders.FilterBusinessObjectDefaults;
		}

		NctsPreviousDocument previousDocument;
		NctsHeader nctsHeader;
	}
}
