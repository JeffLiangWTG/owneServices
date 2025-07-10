using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.CodeDescriptionBoolWithSingleTrueRegistryItem;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolWithSingleTrueRegistryDataType))]
	sealed class CodeDescriptionBoolWithSingleTrueRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CodeDescriptionBoolWithSingleTrueRegistryDataType>
	{
		protected override CodeDescriptionBoolWithSingleTrueRegistryDataType GetNewDataType()
		{
			return new CodeDescriptionBoolWithSingleTrueRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return null; }
		}

		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new CodeDescriptionBoolWithSingleTrueCollection();
			var element1 = collection.AddNew();
			element1.Code = "ABC";
			element1.Description = (NoResString)"http://www.abc.com";
			element1.Bool = true;
			element1.SystemDefined = true;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection))
			};
		}
	}
}
