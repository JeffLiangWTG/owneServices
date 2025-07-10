using System.Text;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Registry.Testing
{
	[TestedType(typeof(EmailStringRegistryDataType))]
	class EmailStringRegistryDataTypeTestCase : RegistryDataTypeTestCase<EmailStringRegistryDataType>
	{
		protected override EmailStringRegistryDataType GetNewDataType()
		{
			return new EmailStringRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(null, Encoding.Unicode.GetBytes("*** NULL ***")),
				new ValidSampleAndBinaryValueInDB("bob@builders.com", Encoding.Unicode.GetBytes("bob@builders.com")),
				new ValidSampleAndBinaryValueInDB("robin@rocketship.com", Encoding.Unicode.GetBytes("robin@rocketship.com")),
				new ValidSampleAndBinaryValueInDB(string.Empty, Encoding.Unicode.GetBytes(string.Empty)),
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
