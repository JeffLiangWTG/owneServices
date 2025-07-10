using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	partial class SystemDataRegistry
	{
		#region SuppressResourceStringsCheckRegion

		public StringRegistryItem ElasticsearchServiceUri =>
			GetItem("ElasticsearchServiceUri", () => new StringRegistryItem(
				"ElasticsearchServiceUri",
				Categories.System_ProcessController_Logging_ElasticSearch,
				ResString.GetMultilingualString("{BC30BB53-EBCC-4322-992D-6954FFAB1060}", "Elasticsearch Service URL"),
				ResString.GetMultilingualString("{E1DD3647-AE0F-4A2D-87B9-3C0CF0789092}", "Enter Elasticsearch Service URL."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				string.Empty));

		public StringRegistryItem ElasticsearchServerUserName =>
			GetItem("ElasticsearchServerUserName", () => new StringRegistryItem(
				"ElasticsearchServerUserName",
				Categories.System_ProcessController_Logging_ElasticSearch,
				ResString.GetMultilingualString("{C0DF22AF-330F-476C-BFAC-F28E53141BDF}", "Elasticsearch Server Username"),
				ResString.GetMultilingualString("{8FC39823-CEB0-4681-9E75-EE6C2F3CDF25}", "Enter Elasticsearch Server Username."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				string.Empty));

		public StringRegistryItem ElasticsearchServerPassword =>
			GetItem("ElasticsearchServerPassword", () => new StringRegistryItem(
				"ElasticsearchServerPassword",
				Categories.System_ProcessController_Logging_ElasticSearch,
				ResString.GetMultilingualString("{CD5D2317-20C1-46C8-B2B0-75111402CFBF}", "Elasticsearch Server Password"),
				ResString.GetMultilingualString("{9B09070B-E3C7-4A9D-85F4-52A374AD24E1}", "Enter Elasticsearch Server Password."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				string.Empty)
			{
				EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password),
			});

		public StringRegistryItem ElasticsearchIndex =>
			GetItem("ElasticsearchIndex", () => new EndpointNameRegistryItem(
				"ElasticsearchIndex",
				Categories.System_ProcessController_Logging_ElasticSearch,
				ResString.GetMultilingualString("{F845A2A7-16BB-4E0E-AF4C-908A21382FA5}", "Elasticsearch Index"),
				ResString.GetMultilingualString("{6A330C46-39C5-444C-BF69-D65F747632D7}", @"Enter Elasticsearch Index.

Use {0} to get the Active Directory Site Name."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				"wisecloud-{0}-app-process-controller"));

		public IntRegistryItem ElasticsearchMaximumResultsByQuery =>
			GetItem("ElasticsearchMaximumResultsByQuery", () => new IntRegistryItem(
				"ElasticsearchMaximumResultsByQuery",
				Categories.System_ProcessController_Logging_ElasticSearch,
				ResString.GetMultilingualString("{41949EE5-833A-4A08-B6D6-3BC1CC188491}", "Elasticsearch Maximum Results By Query"),
				ResString.GetMultilingualString("{2CD21FFA-8B5E-4C39-9F73-1910B7C7B11C}", "Maximum logs to show per file."),
				new NumericRegistryEditorInfo(0),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				300000,
				10000,
				300000));

		#endregion
	}
}
