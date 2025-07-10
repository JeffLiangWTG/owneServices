using Enterprise.ZArchitecture.Environment.Testing;

namespace Enterprise.Registry.Business.Testing
{
	abstract class MilestoneEventUpdatesRegistryDataTypeTest<T> : NonPersistentBusinessObjectRegistryDataTypeTestCase<T>
			where T : IMilestoneEventUpdatesRegistryDataType
	{
		#region Overrides

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(GetValidSampleCollection(), GetValidSampleBinaryValue()) };
		}

		#endregion

		#region Implementation

		protected abstract MilestoneEventUpdatesCollection GetValidSampleCollection();

		protected abstract byte[] GetValidSampleBinaryValue();

		#endregion
	}
}
