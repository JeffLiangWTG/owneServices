using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(EmailDestinationOverrideDataType))]
	sealed class EmailDestinationOverrideDataTypeTestCase : RegistryDataTypeTestCase<EmailDestinationOverrideDataType>
	{
		protected override EmailDestinationOverrideDataType GetNewDataType()
		{
			return new EmailDestinationOverrideDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(null, Encoding.Unicode.GetBytes("*** NULL ***")),
				new ValidSampleAndBinaryValueInDB("bob@builders.com", Encoding.Unicode.GetBytes("bob@builders.com")),
				new ValidSampleAndBinaryValueInDB("robin@rocketship.com", Encoding.Unicode.GetBytes("robin@rocketship.com")),
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { "1234A", "QWERTY", "blah@." };
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}
	}
}
