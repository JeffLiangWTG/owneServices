using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Registry.Business.Testing
{
	sealed class TransitReferenceMappingValidationTest : TestCaseWithFactory
	{
		const string selectionErr = "Enter a valid selection.";
		const string duplicateString = "Duplicated reference mappings.";

		public void TestSourceCategory()
		{
			var mapping = new TransitReferenceMapping();
			AssertNoErrors(mapping.SourceCategoryInfo);

			mapping.SourceCategory = "XXX";
			AssertHasError(mapping.SourceCategoryInfo, selectionErr);

			mapping.SourceCategory = "";
			AssertHasError(mapping.SourceCategoryInfo, "Please enter a Source Category.");

			mapping.SourceCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference;
			AssertNoErrors(mapping.SourceCategoryInfo);

			AssertDuplicate(mapping.SourceCategoryInfo.Name, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseReferenceCategories.Codes.PortReference);
		}

		public void TestSourceType()
		{
			var mapping = new TransitReferenceMapping();
			AssertNoErrors(mapping.SourceTypeInfo);

			mapping.SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			mapping.SourceType = "";
			AssertHasError(mapping.SourceTypeInfo, "Please enter a Source Type.");

			mapping.SourceType = WarehouseAdditionalReferenceTypes.Codes.Other;
			AssertNoErrors(mapping.SourceTypeInfo);

			mapping.SourceType = "XXX";
			AssertNoErrors(mapping.SourceTypeInfo);

			AssertDuplicate(mapping.SourceTypeInfo.Name, WarehouseAdditionalReferenceTypes.Codes.Other, WarehouseAdditionalReferenceTypes.Codes.AssignedPicker);
		}

		public void TestSourceType_Conflicts()
		{
			var coll = new TransitReferenceMappingCollection();
			coll.RemoveAll();
			var mapping1 = coll.AddNew();
			var mapping2 = coll.AddNew();

			mapping1.SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			mapping1.SourceType = WarehouseAdditionalReferenceTypes.Codes.Other;
			mapping1.Direction = TransitWarehouseConsignmentDirections.Codes.Import;
			mapping2.SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			mapping2.SourceType = WarehouseAdditionalReferenceTypes.Codes.Other;
			mapping2.Direction = TransitWarehouseConsignmentDirections.Codes.Export;
			AssertNoErrors(mapping2.SourceTypeInfo);

			mapping2.Direction = TransitWarehouseConsignmentDirections.Codes.Import;
			AssertHasError(mapping2.SourceTypeInfo, "Other conflicting Source Type exist.");
		}

		public void TestDirection_Conflicts()
		{
			var coll = new TransitReferenceMappingCollection();
			coll.RemoveAll();
			var mapping1 = coll.AddNew();
			var mapping2 = coll.AddNew();
			var mapping3 = coll.AddNew();

			mapping1.SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			mapping1.SourceType = WarehouseAdditionalReferenceTypes.Codes.Other;
			mapping1.Direction = TransitWarehouseConsignmentDirections.Codes.Import;
			mapping2.SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			mapping2.SourceType = WarehouseAdditionalReferenceTypes.Codes.Other;
			mapping2.Direction = string.Empty;
			mapping3.SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			mapping3.SourceType = WarehouseAdditionalReferenceTypes.Codes.AssignedPicker;
			mapping3.Direction = TransitWarehouseConsignmentDirections.Codes.Import;
			AssertNoErrors(mapping3.DirectionInfo);

			mapping3.SourceType = WarehouseAdditionalReferenceTypes.Codes.Other;
			AssertHasError(mapping3.DirectionInfo, "Other conflicting Direction exist.");
		}

		public void TestSourceType_EqualToTargetType()
		{
			var mapping = new TransitReferenceMapping();
			AssertNoErrors(mapping.SourceTypeInfo);

			mapping.SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			mapping.TargetCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;

			mapping.TargetType = WarehouseAdditionalReferenceTypes.Codes.Other;
			mapping.SourceType = WarehouseAdditionalReferenceTypes.Codes.Other;
			AssertHasError(mapping.SourceTypeInfo, "Source Type cannot equal to Target Type.");

			mapping.SourceType = WarehouseAdditionalReferenceTypes.Codes.AssignedPicker;
			AssertNoErrors(mapping.SourceTypeInfo);
		}

		public void TestTargetCategory()
		{
			var mapping = new TransitReferenceMapping();
			AssertNoErrors(mapping.TargetCategoryInfo);

			mapping.TargetCategory = "XXX";
			AssertHasError(mapping.TargetCategoryInfo, selectionErr);

			mapping.TargetCategory = "";
			AssertHasError(mapping.TargetCategoryInfo, "Please enter a Target Category.");

			mapping.TargetCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference;
			AssertNoErrors(mapping.TargetCategoryInfo);

			AssertDuplicate(mapping.TargetCategoryInfo.Name, TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseReferenceCategories.Codes.PortReference);
		}

		public void TestTargetType()
		{
			var mapping = new TransitReferenceMapping();
			AssertNoErrors(mapping.TargetTypeInfo);

			mapping.TargetCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			mapping.TargetType = "XXX";
			AssertHasError(mapping.TargetTypeInfo, selectionErr);

			mapping.TargetType = "";
			AssertHasError(mapping.TargetTypeInfo, "Please enter a Target Type.");

			mapping.TargetType = WarehouseAdditionalReferenceTypes.Codes.Other;
			AssertNoErrors(mapping.TargetTypeInfo);

			AssertDuplicate(mapping.TargetTypeInfo.Name, WarehouseAdditionalReferenceTypes.Codes.Other, WarehouseAdditionalReferenceTypes.Codes.AssignedPicker);
		}

		public void TestTargetType_EqualToSourceType()
		{
			var mapping = new TransitReferenceMapping();
			AssertNoErrors(mapping.SourceTypeInfo);

			mapping.SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			mapping.TargetCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;

			mapping.SourceType = WarehouseAdditionalReferenceTypes.Codes.Other;
			mapping.TargetType = WarehouseAdditionalReferenceTypes.Codes.Other;
			AssertHasError(mapping.TargetTypeInfo, "Target Type cannot equal to Source Type.");

			mapping.TargetType = WarehouseAdditionalReferenceTypes.Codes.AssignedPicker;
			AssertNoErrors(mapping.TargetTypeInfo);
		}

		void AssertDuplicate(string assertPropertyName, string validChangedValueNotAllForMethodA, string validChangedValueNotAllForMethodB)
		{
			var coll = new TransitReferenceMappingCollection();
			coll.RemoveAll();
			var mapping1 = coll.AddNew();
			var mapping2 = coll.AddNew();

			mapping1.SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			mapping1.SourceType = WarehouseAdditionalReferenceTypes.Codes.Other;
			mapping1.TargetCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference;
			mapping1.TargetType = TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber;

			mapping2.SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference;
			mapping2.SourceType = WarehouseAdditionalReferenceTypes.Codes.Other;
			mapping2.TargetCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference;
			mapping2.TargetType = TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber;

			coll.RunPreSaveValidation();
			AssertHasRowError(mapping1, duplicateString);
			AssertHasRowError(mapping2, duplicateString);

			mapping1.FindPropertyInfo(assertPropertyName).SetValueFromString(validChangedValueNotAllForMethodA);
			mapping2.FindPropertyInfo(assertPropertyName).SetValueFromString(validChangedValueNotAllForMethodB);
			coll.RunPreSaveValidation();
			AssertNoRowError(mapping1, duplicateString);
			AssertNoRowError(mapping2, duplicateString);
		}
	}
}
