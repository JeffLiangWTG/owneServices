using System.Text;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ServerUsernamePasswordConfigurationRegistryDataType))]
	sealed class ServerUsernamePasswordConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ServerUsernamePasswordConfigurationRegistryDataType>
	{
		#region Implementation

		protected override ServerUsernamePasswordConfigurationRegistryDataType GetNewDataType()
		{
			return new ServerUsernamePasswordConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ServerUsernamePasswordConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			ServerUsernamePasswordConfiguration config = new ServerUsernamePasswordConfiguration();
			config.UserName = "geoffuser";
			config.Password = "geoffpass";
			config.ConfirmPassword = "geoffpass";

			string xml =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				@"<ServerUsernamePasswordConfiguration>
					<UserName>geoffuser</UserName>
					<Password>geoffpass</Password>
				</ServerUsernamePasswordConfiguration>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(config, Encoding.Unicode.GetBytes(xml))
			};
		}

		#endregion
	}
}
