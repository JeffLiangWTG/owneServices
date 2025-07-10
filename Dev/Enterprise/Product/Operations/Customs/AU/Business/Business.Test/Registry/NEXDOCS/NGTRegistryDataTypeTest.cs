using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(NGTRegistryDataType))]
	sealed class NGTRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<NGTRegistryDataType>
	{
		protected override string ExpectedEditorName => "NGTRegistryItemEditor";

		protected override NGTRegistryDataType GetNewDataType()
		{
			return new NGTRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var ngt1 = new NGT
			{
				Password = "1"
			};
			var ngt2 = new NGT
			{
				Password = "2"
			};

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(ngt1, new NGTRegistryDataType().Serialise(ngt1)),
				new ValidSampleAndBinaryValueInDB(ngt2, new NGTRegistryDataType().Serialise(ngt2))
			};
		}
	}
}
