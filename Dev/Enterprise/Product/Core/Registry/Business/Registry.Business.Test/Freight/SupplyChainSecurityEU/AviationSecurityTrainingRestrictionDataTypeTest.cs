using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Freight.SupplyChainSecurityEU
{
	[TestedType(typeof(AviationSecurityTrainingRestrictionDataType))]
	public class AviationSecurityTrainingRestrictionDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AviationSecurityTrainingRestrictionDataType>
	{
		protected override AviationSecurityTrainingRestrictionDataType GetNewDataType()
		{
			return new AviationSecurityTrainingRestrictionDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var value1 = new AviationSecurityTrainingRestriction(true, true);
			var value2 = new AviationSecurityTrainingRestriction(true, false);
			var value3 = new AviationSecurityTrainingRestriction(false, false);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(value1, DataType.Serialise(value1)),
				new ValidSampleAndBinaryValueInDB(value2, DataType.Serialise(value2)),
				new ValidSampleAndBinaryValueInDB(value3, DataType.Serialise(value3))
			};
		}

		protected override string ExpectedEditorName => "AviationSecurityTrainingRestrictionRegistryItemEditor";
	}
}
