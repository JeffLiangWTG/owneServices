using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RequireReasonForCLRDataType))]
	sealed class RequireReasonForCLRDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<RequireReasonForCLRDataType>
	{
		protected override string ExpectedEditorName => "RequireReasonForCLRRegistryItemEditor";

		protected override RequireReasonForCLRDataType GetNewDataType() => new RequireReasonForCLRDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var itemCollection1 = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "123", Title = "321", IsMandatory = true },
				new RequireReasonForCLRItem { Code = "abc", Title = "456", ClearingReason = "789", IsMandatory = false }
			};

			var wrapper1 = new RequireReasonForCLRWrapper(itemCollection1)
			{
				RequireReasonForCLR = true
			};

			var itemCollection2 = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "666", Title = "321" }
			};

			var wrapper2 = new RequireReasonForCLRWrapper(itemCollection2)
			{
				RequireReasonForCLR = false
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(wrapper1, new RequireReasonForCLRDataType().Serialise(wrapper1)),
				new ValidSampleAndBinaryValueInDB(wrapper2, new RequireReasonForCLRDataType().Serialise(wrapper2))
			};
		}

		public void TestValidateBeforeRegistryFormSaveCore()
		{
			var itemCollection = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "C1", Title = "Title1", ClearingReason = "Description1" },
				new RequireReasonForCLRItem { Code = "C2", Title = "Title2", ClearingReason = "Description2" }
			};
			var proposedValue = new RequireReasonForCLRWrapper(itemCollection)
			{
				RequireReasonForCLR = true
			};

			var registryItem = new RequireReasonForCLRRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, new RequireReasonForCLRWrapper());
			var validate = new AnonymousMethod(() => DataType.ValidateBeforeRegistryFormSave(registryItem, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(validate);

			itemCollection.RemoveAll();

			validate = () => DataType.ValidateBeforeRegistryFormSave(registryItem, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty);
			var ex = AssertExceptionThrown<RegistryValidationException>(validate);
			AssertMultilineASCIIEquals("Please add at least 1 item in the list when this registry is overridden to 'Yes'.", ex.Message);

			proposedValue.RequireReasonForCLR = false;
			validate = () => DataType.ValidateBeforeRegistryFormSave(registryItem, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertNoExceptionThrown(validate);
		}
	}
}
