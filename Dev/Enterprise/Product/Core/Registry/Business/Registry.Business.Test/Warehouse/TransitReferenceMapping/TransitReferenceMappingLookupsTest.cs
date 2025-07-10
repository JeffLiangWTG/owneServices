using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Testing
{
	sealed class TransitReferenceMappingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSourceReferenceCategoryList()
		{
			var parent = new TransitReferenceMapping();
			var lookups = new TransitReferenceMappingLookups(parent);

			AssertNotNull(lookups.SourceReferenceCategoryList);
			AssertEquals(true, lookups.SourceReferenceCategoryList.ContainsCode(TransitWarehouseReferenceCategories.Codes.AdditionalReference));
			AssertContainsExactElementsInAnyOrder(new string[] { TransitWarehouseReferenceCategories.Codes.AdditionalReference, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseReferenceCategories.Codes.PortReference },
				lookups.SourceReferenceCategoryList.GetAllCodes());
		}

		public void TestTargetReferenceCategoryList()
		{
			var parent = new TransitReferenceMapping();
			var lookups = new TransitReferenceMappingLookups(parent);

			AssertNotNull(lookups.TargetReferenceCategoryList);
			AssertEquals(true, lookups.TargetReferenceCategoryList.ContainsCode(TransitWarehouseReferenceCategories.Codes.AdditionalReference));
			AssertContainsExactElementsInAnyOrder(new string[] { TransitWarehouseReferenceCategories.Codes.AdditionalReference, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseReferenceCategories.Codes.PortReference },
				lookups.TargetReferenceCategoryList.GetAllCodes());
		}

		public void TestSourceReferenceTypeList()
		{
			var parent = new TransitReferenceMapping();
			var lookups = new TransitReferenceMappingLookups(parent);

			AssertNotNull(lookups.SourceReferenceTypeList);
			AssertEquals(0, lookups.SourceReferenceTypeList.Count);

			parent.SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			var addtionalReferenceKeyValuePairs = new List<KeyValuePair<string, string>>();
			foreach (CodeDescriptionPair item in lookups.SourceReferenceTypeList)
			{
				addtionalReferenceKeyValuePairs.Add(new KeyValuePair<string, string>(item.Code, item.Description));
			}
			AssertContainsExactElementsInAnyOrder(WarehouseDocketReferenceTypeRegistry.GetWarehouseAdditionalReferenceTypeKeyValuePairs(), addtionalReferenceKeyValuePairs);

			parent.SourceCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference;
			var expectedCustomReferencePairs = new CodeDescriptionPairList();
			expectedCustomReferencePairs.AddPair(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseCustomsReferenceTypes.Descriptions.CustomsNumber);
			expectedCustomReferencePairs.AddPair(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, TransitWarehouseCustomsReferenceTypes.Descriptions.CustomsReleaseNumber);
			AssertContainsExactElementsInAnyOrder(expectedCustomReferencePairs, lookups.SourceReferenceTypeList);

			parent.SourceCategory = TransitWarehouseReferenceCategories.Codes.PortReference;
			var expectedPortReferencePairs = new CodeDescriptionPairList();
			expectedPortReferencePairs.AddPair(TransitWarehousePortReferenceTypes.Codes.PortAuthority, TransitWarehousePortReferenceTypes.Descriptions.PortAuthority);
			AssertContainsExactElementsInAnyOrder(expectedPortReferencePairs, lookups.SourceReferenceTypeList);
		}

		public void TestTargetReferenceTypeList()
		{
			var parent = new TransitReferenceMapping();
			var lookups = new TransitReferenceMappingLookups(parent);

			AssertNotNull(lookups.TargetReferenceTypeList);
			AssertEquals(0, lookups.TargetReferenceTypeList.Count);

			parent.TargetCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			var addtionalReferenceKeyValuePairs = new List<KeyValuePair<string, string>>();
			foreach (CodeDescriptionPair item in lookups.TargetReferenceTypeList)
			{
				addtionalReferenceKeyValuePairs.Add(new KeyValuePair<string, string>(item.Code, item.Description));
			}
			AssertContainsExactElementsInAnyOrder(WarehouseDocketReferenceTypeRegistry.GetWarehouseAdditionalReferenceTypeKeyValuePairs(), addtionalReferenceKeyValuePairs);

			parent.TargetCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference;
			var expectedCustomReferencePairs = new CodeDescriptionPairList();
			expectedCustomReferencePairs.AddPair(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseCustomsReferenceTypes.Descriptions.CustomsNumber);
			expectedCustomReferencePairs.AddPair(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, TransitWarehouseCustomsReferenceTypes.Descriptions.CustomsReleaseNumber);
			AssertContainsExactElementsInAnyOrder(expectedCustomReferencePairs, lookups.TargetReferenceTypeList);

			parent.TargetCategory = TransitWarehouseReferenceCategories.Codes.PortReference;
			var expectedPortReferencePairs = new CodeDescriptionPairList();
			expectedPortReferencePairs.AddPair(TransitWarehousePortReferenceTypes.Codes.PortAuthority, TransitWarehousePortReferenceTypes.Descriptions.PortAuthority);
			AssertContainsExactElementsInAnyOrder(expectedPortReferencePairs, lookups.TargetReferenceTypeList);
		}
	}
}
