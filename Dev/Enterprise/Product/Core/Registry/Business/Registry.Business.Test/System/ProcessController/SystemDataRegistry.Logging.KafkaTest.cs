using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	sealed partial class SystemDataRegistryTest
	{
		public void TestProcessControllerKafkaSecurity()
		{
			TestGenericRegistryItem(
				ItemSet.ProcessControllerKafkaSecurity,
				"ProcessControllerKafkaSecurity",
				"System/Process Controller/Logging/Kafka",
				"Kafka Security Settings",
				"Kafka Security Settings.\r\n\r\nSSL : The required certificate will need to be installed and accessible in the windows store.\r\n" +
				"SSL_PLAINTEXT : Please supply a username and password.\r\n" +
				"SASL_SSL : Please supply a username and password. The required certificate will need to be installed and accessible in the windows store.\r\n" +
				"PLAINTEXT : No other configuration necessary.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController);
		}

		public void TestKafkaTopic()
		{
			TestGenericRegistryItem(
				ItemSet.KafkaTopic,
				"KafkaTopic",
				"System/Process Controller/Logging/Kafka",
				"Kafka Topic",
				"Enter Kafka Topic.\r\n\r\nUse {0} to get the Active Directory Site Name.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				"topic-wisecloud-{0}-app-process-controller");
		}

		public void TestKafkaBrokers()
		{
			TestRegistryItem(
				ItemSet.KafkaBrokers,
				"KafkaBrokers",
				SystemDataRegistry.Categories.System_ProcessController_Logging_Kafka,
				"Kafka Brokers URI",
				"This is the list of hosts and ports that identifies Kafka brokers.\r\n\r\n" +
				"URI should not contain protocol. Example: host:1234",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				255,
				0);
			var editorInfo = (CodeDescriptionPairListEditorInfo)ItemSet.KafkaBrokers.EditorInfo;
			AssertEquals(nameof(editorInfo.ShowCodeColumn), true, editorInfo.ShowCodeColumn);
			AssertEquals(nameof(editorInfo.ShowDescriptionColumn), false, editorInfo.ShowDescriptionColumn);
			AssertEquals(nameof(editorInfo.CodeFieldCasing), CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, editorInfo.CodeFieldCasing);
			AssertEquals(nameof(editorInfo.CodeColumnCaption), "URI", editorInfo.CodeColumnCaption);
		}

		public void TestKafkaElasticsearchIndex()
		{
			TestGenericRegistryItem(
				ItemSet.KafkaElasticsearchIndex,
				"KafkaElasticsearchIndex",
				"System/Process Controller/Logging/Kafka",
				"Elasticsearch Index",
				"Enter Elasticsearch Index.\r\n\r\nUse {0} to get the Active Directory Site Name.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				"wisecloud-{0}-app-process-controller");
		}
	}
}
