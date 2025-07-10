using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(eAdaptorNextOutboundConfigRegistryDataType))]
	sealed class eAdaptorNextOutboundConfigRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<eAdaptorNextOutboundConfigRegistryDataType>
	{
		protected override eAdaptorNextOutboundConfigRegistryDataType GetNewDataType()
		{
			return new eAdaptorNextOutboundConfigRegistryDataType(eAdaptorNextOutboundConfig.DefaultValue);
		}

		protected override string ExpectedEditorName
		{
			get { return "eAdaptorNextOutboundRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var config = eAdaptorNextOutboundConfig.DefaultValue;

			var config2 = eAdaptorNextOutboundConfig.DefaultValue;
			config2.ClientID = "asdasd";

			var serializer = new eAdaptorNextOutboundConfigRegistryDataType(eAdaptorNextOutboundConfig.DefaultValue);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(config, serializer.Serialise(config)),
				new ValidSampleAndBinaryValueInDB(config2, serializer.Serialise(config2))
			};
		}
	}
}
