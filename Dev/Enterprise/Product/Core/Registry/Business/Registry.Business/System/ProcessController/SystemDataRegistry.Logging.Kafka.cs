using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	partial class SystemDataRegistry
	{
		#region SuppressResourceStringsCheckRegion

		public KafkaSecurityRegistryItem ProcessControllerKafkaSecurity =>
			GetItem("ProcessControllerKafkaSecurity",
				() => new KafkaSecurityRegistryItem("ProcessControllerKafkaSecurity",
					Categories.System_ProcessController_Logging_Kafka,
					ResString.GetMultilingualString("{77C6F970-A204-4F4A-A555-380CD9142E71}", "Kafka Security Settings"),
					ResString.GetMultilingualString("{7CBCE6F0-B044-41A9-9347-F5F546E208E5}", @"Kafka Security Settings.

SSL : The required certificate will need to be installed and accessible in the windows store.
SSL_PLAINTEXT : Please supply a username and password.
SASL_SSL : Please supply a username and password. The required certificate will need to be installed and accessible in the windows store.
PLAINTEXT : No other configuration necessary."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					new KafkaSecurity()
				));

		public StringRegistryItem KafkaTopic =>
			GetItem("KafkaTopic",
				() => new EndpointNameRegistryItem(
					"KafkaTopic",
					Categories.System_ProcessController_Logging_Kafka,
					ResString.GetMultilingualString("{C9B85C92-854D-4D43-8AE9-6B62300994DA}", "Kafka Topic"),
					ResString.GetMultilingualString("{E2BE7EA1-38EA-4BAC-87EE-A80EE7BBED88}", @"Enter Kafka Topic.

Use {0} to get the Active Directory Site Name."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					"topic-wisecloud-{0}-app-process-controller")
			);

		public CodeDescriptionPairListRegistryItem KafkaBrokers =>
			GetItem("KafkaBrokers",
				() => new CodeDescriptionPairListRegistryItem(
					"KafkaBrokers",
					Categories.System_ProcessController_Logging_Kafka,
					ResString.GetMultilingualString("{CB2E6219-2D1E-49D5-9E44-25B19E1FB6AA}", "Kafka Brokers URI"),
					ResString.GetMultilingualString("{EC193CB7-755E-4809-871F-02378D811517}", @"This is the list of hosts and ports that identifies Kafka brokers.

URI should not contain protocol. Example: host:1234"),
					255,
					RegistryStorageFlags.System,
					false,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					new ReadOnlyCodeDescriptionPairList())
				{
					EditorInfo = new CodeDescriptionPairListEditorInfo(
						true,
						false,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
						ResString.GetMultilingualString("{7998AC0C-EF96-46C8-90E7-3DE881B83BB2}", "URI")),
				}
			);

		public StringRegistryItem KafkaElasticsearchServiceUri =>
			GetItem("KafkaElasticsearchServiceUri", () => new StringRegistryItem(
				"KafkaElasticsearchServiceUri",
				Categories.System_ProcessController_Logging_Kafka,
				ResString.GetMultilingualString("{FE9A0F2A-F0FB-4752-B1D2-E4444F5DD595}", "Elasticsearch Service URL"),
				ResString.GetMultilingualString("{8E68DD12-A83F-4923-A056-93950BC06FA2}", "Enter Elasticsearch Service URL."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				string.Empty));

		public StringRegistryItem KafkaElasticsearchServerUserName =>
			GetItem("KafkaElasticsearchServerUserName", () => new StringRegistryItem(
				"KafkaElasticsearchServerUserName",
				Categories.System_ProcessController_Logging_Kafka,
				ResString.GetMultilingualString("{A43CA04B-3CD9-49AD-A804-30BAC9AFC6A8}", "Elasticsearch Server Username"),
				ResString.GetMultilingualString("{6C3B20FB-EB21-4C25-B561-E067C081E071}", "Enter Elasticsearch Server Username."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				string.Empty));

		public StringRegistryItem KafkaElasticsearchServerPassword =>
			GetItem("KafkaElasticsearchServerPassword", () => new StringRegistryItem(
				"KafkaElasticsearchServerPassword",
				Categories.System_ProcessController_Logging_Kafka,
				ResString.GetMultilingualString("{A506819B-8FEF-49EC-B2AA-B5E9AF20F744}", "Elasticsearch Server Password"),
				ResString.GetMultilingualString("{709E4F17-BF19-4ED0-97F9-839820133FA2}", "Enter Elasticsearch Server Password."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				string.Empty)
			{
				EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password),
			});

		public StringRegistryItem KafkaElasticsearchIndex =>
			GetItem("KafkaElasticsearchIndex", () => new EndpointNameRegistryItem(
				"KafkaElasticsearchIndex",
				Categories.System_ProcessController_Logging_Kafka,
				ResString.GetMultilingualString("{D870D494-9D1F-4EB9-AD25-81B6019175E6}", "Elasticsearch Index"),
				ResString.GetMultilingualString("{215C5AF9-3E68-424A-9912-1F5D26AEB3FD}", @"Enter Elasticsearch Index.

Use {0} to get the Active Directory Site Name."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				"wisecloud-{0}-app-process-controller"));

		public IntRegistryItem KafkaElasticsearchMaximumResultsByQuery =>
			GetItem("KafkaElasticsearchMaximumResultsByQuery", () =>
				new IntRegistryItem(
					"KafkaElasticsearchMaximumResultsByQuery",
					Categories.System_ProcessController_Logging_Kafka,
					ResString.GetMultilingualString("{27B0C697-5367-46E6-AB9D-88163F535EC0}", "Elasticsearch Maximum Results By Query"),
					ResString.GetMultilingualString("{E03A8718-8048-47B0-B462-AADA44DA65BB}", "Maximum logs to show per file."),
					new NumericRegistryEditorInfo(0),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
					300000,
					10000,
					300000)
			);

		#endregion
	}
}
