using System;
using System.Text.RegularExpressions;
using Enterprise.Integration;

namespace Enterprise.Registry.Business.Testing
{
	sealed partial class SystemDataRegistryTest
	{
		public void TestElasticsearchServiceUri()
		{
			TestGenericRegistryItem(
				ItemSet.ElasticsearchServiceUri,
				"ElasticsearchServiceUri",
				"System/Process Controller/Logging/Elasticsearch",
				"Elasticsearch Service URL",
				"Enter Elasticsearch Service URL.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				string.Empty);
		}

		public void TestElasticsearchServerUsername()
		{
			TestGenericRegistryItem(
				ItemSet.ElasticsearchServerUserName,
				"ElasticsearchServerUserName",
				"System/Process Controller/Logging/Elasticsearch",
				"Elasticsearch Server Username",
				"Enter Elasticsearch Server Username.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				string.Empty);
		}

		public void TestElasticsearchServerPassword()
		{
			TestGenericRegistryItem(
				ItemSet.ElasticsearchServerPassword,
				"ElasticsearchServerPassword",
				"System/Process Controller/Logging/Elasticsearch",
				"Elasticsearch Server Password",
				"Enter Elasticsearch Server Password.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				string.Empty);
		}

		public void TestElasticsearchIndex()
		{
			TestGenericRegistryItem(
				ItemSet.ElasticsearchIndex,
				"ElasticsearchIndex",
				"System/Process Controller/Logging/Elasticsearch",
				"Elasticsearch Index",
				"Enter Elasticsearch Index.\r\n\r\nUse {0} to get the Active Directory Site Name.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				"wisecloud-{0}-app-process-controller");
		}

		public void TestElasticsearchIndexWithSiteName()
		{
			var tempIndex = "test-{0}";
			SystemDataRegistry.Instance.ElasticsearchIndex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempIndex);

			var siteName = Regex.Match(EndpointNameRegistryItem.GetHostName(), @"^[\w]{0,3}").Value;
			AssertEquals("Indexes should be equals", SystemDataRegistry.Instance.ElasticsearchIndex.Value, string.Format(tempIndex, siteName));
		}

		public void TestElasticsearchMaximumResultsByQuery()
		{
			TestRegistryItem(
				ItemSet.ElasticsearchMaximumResultsByQuery,
				"ElasticsearchMaximumResultsByQuery",
				"System/Process Controller/Logging/Elasticsearch",
				"Elasticsearch Maximum Results By Query",
				"Maximum logs to show per file.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				300000,
				10000,
				300000);
		}
	}
}
