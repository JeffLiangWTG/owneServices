using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FTPDestinationOverrideInfoRegistryDataType))]
	sealed class FTPDestinationOverrideRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<FTPDestinationOverrideInfoRegistryDataType>
	{
		protected override FTPDestinationOverrideInfoRegistryDataType GetNewDataType()
		{
			return new FTPDestinationOverrideInfoRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var first = new FTPDestinationOverrideInfo();
			var second = new FTPDestinationOverrideInfo("FtpAddressTest", "UserNameTest", "PasswordTest");
			return new[]
			{
				new ValidSampleAndBinaryValueInDB(first, DataType.Serialise(first)),
				new ValidSampleAndBinaryValueInDB(second, DataType.Serialise(second)),
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "FTPDestinationOverrideRegistryItemEditor"; }
		}
	}
}
