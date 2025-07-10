using System;
using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(eHubGatewayRegistryDataType))]
	sealed class eHubGatewayRegistryDataTypeTest : RegistryDataTypeTestCase<eHubGatewayRegistryDataType>
	{
		public void TesteHubGatewayRegistryDataTypeValidation()
		{
			var eHubGatewayRegistryItem = new eHubGatewayRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached, "", isProduction: true);

			var dataType = (eHubGatewayRegistryDataType)eHubGatewayRegistryItem.DataType;
			var invalidListUrl = new[] { "www.google.com/", "http://www.google.com/", "http://www.google.com", "google.com/sth", "w_&#" };
			foreach (var url in invalidListUrl)
			{
				AssertExceptionThrown(typeof(RegistryValidationException), "It should be entered as the server name: {SERVER_NAME} in http://{SERVER_NAME}/Extra/ (In Example: www.google.com).",
					() => dataType.Validate(eHubGatewayRegistryItem, url, Guid.Empty, Guid.Empty, Guid.Empty));
			}

			AssertNoExceptionThrown(() => dataType.Validate(eHubGatewayRegistryItem, "www.google.com", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		protected override eHubGatewayRegistryDataType GetNewDataType()
		{
			return new eHubGatewayRegistryDataType(isProduction: true);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("www.google.com", Encoding.Unicode.GetBytes("www.google.com")),
				new ValidSampleAndBinaryValueInDB("google.com", Encoding.Unicode.GetBytes("google.com")),
				new ValidSampleAndBinaryValueInDB("127.0.0.8", Encoding.Unicode.GetBytes("127.0.0.8")),
				new ValidSampleAndBinaryValueInDB("valid.valid", Encoding.Unicode.GetBytes("valid.valid")),
				new ValidSampleAndBinaryValueInDB("localhost", Encoding.Unicode.GetBytes("localhost")),
				new ValidSampleAndBinaryValueInDB("machine-name-number", Encoding.Unicode.GetBytes("machine-name-number"))
			};
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}
	}
}
