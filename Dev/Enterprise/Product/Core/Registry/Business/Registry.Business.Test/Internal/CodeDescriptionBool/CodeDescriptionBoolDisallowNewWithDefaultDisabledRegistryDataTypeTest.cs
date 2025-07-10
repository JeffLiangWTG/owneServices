using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolDisallowNewWithDefaultDisabledRegistryDataType))]
	sealed class CodeDescriptionBoolDisallowNewWithDefaultDisabledRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CodeDescriptionBoolDisallowNewWithDefaultDisabledRegistryDataType>
	{
		protected override CodeDescriptionBoolDisallowNewWithDefaultDisabledRegistryDataType GetNewDataType()
		{
			return new CodeDescriptionBoolDisallowNewWithDefaultDisabledRegistryDataType();
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
			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection();
			CodeDescriptionBoolDisallowNewWithDefaultDisabled element = collection.AddNew();
			element.Code = "ABC";
			element.Description = (NoResString)"XYZ";
			element.Bool = true;

			var collection2 = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection();
			CodeDescriptionBoolDisallowNewWithDefaultDisabled element2 = collection2.AddNew();
			element2.Code = "DEF";
			element2.Description = (NoResString)"UVW";
			element2.Bool = true;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new CodeDescriptionBoolDisallowNewWithDefaultDisabledRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new CodeDescriptionBoolDisallowNewWithDefaultDisabledRegistryDataType().Serialise(collection2))
			};
		}
	}
}
