using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(AzureSignalRConnectionStringDataType))]
	sealed class AzureSignalRConnectionStringDataTypeTest : StringRegistryDataTypeTest
	{
		protected override StringRegistryDataType GetNewDataType()
		{
			return new AzureSignalRConnectionStringDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			string valid1 = "Endpoint=https://validurl.com;AccessKey=somevalidkey;";
			string valid2 = "endpoint=https://validurl.com;accesskey=somevalidkey;";
			string valid3 = "Endpoint=https://validurl.com;AccessKey=somevalidkey;version=1.0";
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(valid1, Encoding.Unicode.GetBytes(valid1)),
				new ValidSampleAndBinaryValueInDB(valid2, Encoding.Unicode.GetBytes(valid2)),
				new ValidSampleAndBinaryValueInDB(valid3, Encoding.Unicode.GetBytes(valid3)),
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] {
				"some invalid string",
				"Endpoint=some invalid endpoint;AccessKey=somevalidkey;",
				"Endpoint=https://validurl.com;AccessKey= ;",
				"Endpoint=http://invalidurl.com;AccessKey=somevalidkey;",
				"Endpoint:https://validurl.com;AccessKey:somevalidkey;",
			};
		}
	}
}
