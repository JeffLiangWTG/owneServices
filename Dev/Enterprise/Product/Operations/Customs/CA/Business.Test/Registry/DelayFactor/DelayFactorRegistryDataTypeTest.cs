using Enterprise.Customs.CA.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(DelayFactorRegistryDataType))]
	sealed class DelayFactorRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DelayFactorRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "DelayFactorRegistryItemEditor"; }
		}

		protected override DelayFactorRegistryDataType GetNewDataType()
		{
			return new DelayFactorRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			DelayFactorRegistryBusinessObject sample = new DelayFactorRegistryBusinessObject();
			sample.HVSDelayInterval = 4;
			sample.HVSDelayIntervalType = DelayIntervalTypeCodes.Codes.DAR;
			sample.CONDelayInterval = 2;
			sample.CONDelayIntervalType = DelayIntervalTypeCodes.Codes.DAY;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample, new DelayFactorRegistryDataType().Serialise(sample))
			};
		}
	}
}
