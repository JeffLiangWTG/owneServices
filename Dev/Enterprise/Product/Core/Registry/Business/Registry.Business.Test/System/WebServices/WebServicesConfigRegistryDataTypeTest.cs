using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebServicesConfigRegistryDataType))]
	sealed class WebServicesConfigRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<WebServicesConfigRegistryDataType>
	{
		public void TestDeserialised()
		{
			var webServicesConfigCollection = new WebServicesConfigCollection();
			var webServicesConfig = new WebServicesConfig
			{
				Name = "Forwarding",
				IsEnabled = false,
				IsAutoManaged = false,
				IsCustomURL = true,
				URL = "YR6TST.webprint.wisegrid.net",
				NumberOfServerClusters = 2,
			};

			webServicesConfigCollection.Add(webServicesConfig);

			var dataType = new WebServicesConfigRegistryDataType(new WebServicesConfigCollection());
			var deserialisedConfig = dataType.Deserialise(dataType.Serialise(webServicesConfigCollection));

			AssertEquals(webServicesConfig.Name, deserialisedConfig[0].Name);
			AssertEquals(webServicesConfig.IsEnabled, deserialisedConfig[0].IsEnabled);
			AssertEquals(webServicesConfig.IsAutoManaged, deserialisedConfig[0].IsAutoManaged);
			AssertEquals(webServicesConfig.IsCustomURL, deserialisedConfig[0].IsCustomURL);
			AssertEquals(webServicesConfig.URL, deserialisedConfig[0].URL);
			AssertEquals(webServicesConfig.NumberOfServerClusters, deserialisedConfig[0].NumberOfServerClusters);
		}

		#region Implementation

		protected override string ExpectedEditorName
		{
			get { return "WebServicesRegistryGridItemEditor"; }
		}

		protected override WebServicesConfigRegistryDataType GetNewDataType()
		{
			return new WebServicesConfigRegistryDataType(new WebServicesConfigCollection());
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var firstCollection = new WebServicesConfigCollection();
			var secondCollection = new WebServicesConfigCollection();
			var webServicesConfig = new WebServicesConfig
			{
				IsEnabled = false,
				IsAutoManaged = true,
				IsCustomURL = true,
				URL = "YR6TST.webprint.wisegrid.net",
				NumberOfServerClusters = 2,
			};
			secondCollection.Add(webServicesConfig);

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(firstCollection, GetNewDataType().Serialise(firstCollection)),
				new ValidSampleAndBinaryValueInDB(secondCollection, GetNewDataType().Serialise(secondCollection)),
			};
		}

		#endregion
	}
}
