using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OutboundSendLimitsRegistryDataType))]
	sealed class OutboundSendLimitsRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<OutboundSendLimitsRegistryDataType>
	{
		protected override string ExpectedEditorName => "OutboundSendLimitsRegistryItemEditor";
		protected override OutboundSendLimitsRegistryDataType GetNewDataType()
		{
			return new OutboundSendLimitsRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var result = new OutboundSendLimitsRule();
			result.ReadOnly = false;
			result.SendCountLimit = new CargoWise.Types.ZShort(3);

			var result2 = new OutboundSendLimitsRule();
			result2.ReadOnly = false;
			result2.SendCountLimit = new CargoWise.Types.ZShort(2);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(result, new OutboundSendLimitsRegistryDataType().Serialise(result)),
				new ValidSampleAndBinaryValueInDB(result2, new OutboundSendLimitsRegistryDataType().Serialise(result2))
			};
		}
	}
}
