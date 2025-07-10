using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(CodeDescriptionBoolWithSelectionValidationRegistryDataType))]
	class CodeDescriptionBoolWithSelectionValidationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CodeDescriptionBoolWithSelectionValidationRegistryDataType>
	{
		protected override CodeDescriptionBoolWithSelectionValidationRegistryDataType GetNewDataType()
		{
			return new CodeDescriptionBoolWithSelectionValidationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return null; }
		}

		// Using custom EditorInfo.
		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CodeDescriptionBoolCollection collection = new CodeDescriptionBoolCollection();
			CodeDescriptionBool element = collection.AddNew();
			element.Code = "ABC";
			element.Description = (NoResString)"XYZ";
			element.Bool = true;

			byte[] byteArray = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,
				110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,
				114,0,97,0,121,0,79,0,102,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,
				66,0,111,0,111,0,108,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,
				58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,
				83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,
				115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,
				111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,67,0,
				111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,62,0,60,0,
				67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,
				77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,65,0,66,0,67,0,60,0,47,0,67,0,
				111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,88,0,89,0,90,0,60,0,
				47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,66,0,111,0,111,0,108,0,62,0,89,0,60,0,
				47,0,66,0,111,0,111,0,108,0,62,0,60,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,0,105,0,110,0,101,0,100,0,62,0,
				70,0,97,0,108,0,115,0,101,0,60,0,47,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,0,105,0,110,0,101,0,100,0,62,0,
				60,0,47,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,
				62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,
				116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArray)
			};
		}

		public void TestValidate_DefaultValueValidation()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("0", ResString._GetMultilingualString(12, "0", "Zero"));
			list.AddPair("1", ResString._GetMultilingualString(12, "1", "One"));

			RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter defaultValueGetter = (Guid companyPK, Guid branchPK, Guid departmentPK) =>
																							new CodeDescriptionBoolCollection(list, false);

			var registryItem = new CodeDescriptionBoolWithSelectionValidationRegistryItem("Name", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, new CodeDescriptionBoolRegistryEditorInfo(null), defaultValueGetter);
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection(list));

			OverrideWithNoChanges_ShouldThrowValidationException(registryItem);
			OverrideWithChanges_ShouldNotThrowValidationException(registryItem, list);
		}

		void OverrideWithNoChanges_ShouldThrowValidationException(CodeDescriptionBoolWithSelectionValidationRegistryItem registryItem)
		{
			var expectedErrorMessage = "Invalid Input. Please select at least one option to override.";

			var validate = new AnonymousMethod(() => DataType.ValidateBeforeRegistryFormSave(registryItem, registryItem.DefaultValue, Guid.Empty, Guid.Empty, Guid.Empty));
			var ex = AssertExceptionThrown<RegistryValidationException>(validate);
			AssertMultilineASCIIEquals(expectedErrorMessage, ex.Message);
		}

		void OverrideWithChanges_ShouldNotThrowValidationException(CodeDescriptionBoolWithSelectionValidationRegistryItem registryItem, CodeDescriptionPairList list)
		{
			var nonDefaultValue = new CodeDescriptionBoolCollection(list);
			foreach (CodeDescriptionBool item in nonDefaultValue)
			{
				item.Bool = true;
			}
			var validate = new AnonymousMethod(() => DataType.ValidateBeforeRegistryFormSave(registryItem, nonDefaultValue, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(validate);
		}
	}
}
