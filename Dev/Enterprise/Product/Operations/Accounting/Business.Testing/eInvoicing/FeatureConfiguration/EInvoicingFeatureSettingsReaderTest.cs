using System.Threading;
using System.Threading.Tasks;
using CargoWise.FeatureControl.Abstractions;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EInvoicing.FeatureConfiguration.Testing
{
	public class EInvoicingFeatureSettingsReaderTest : TestCase
	{
		public void TestNoDataInCountryBlock()
		{
			var featureControlManager = GetFeatureControlManagerMock();

			var reader = new EInvoicingFeatureSettingsReader("Country1", featureControlManager);
			AssertNotNull(reader.Value);
			AssertNotNull(reader.Value.Features);
			AssertNotNull(reader.Value.Transport);
			AssertNull(reader.Value.Transport.Delivery);
			AssertNull(reader.Value.Transport.Destination);
			AssertNull(reader.Value.Transport.MessageType);
			AssertEquals(false, reader.HasFeature("Feature1"));
		}

		public void TestStandardSettingsType()
		{
			var featureControlManager = GetFeatureControlManagerMock();

			var reader = new EInvoicingFeatureSettingsReader("Country2", featureControlManager);
			AssertNotNull(reader.Value);
			AssertNotNull(reader.Value.Features);
			AssertEquals(true, reader.Value.Features.Contains("Feature1"));
			AssertEquals(false, reader.Value.Features.Contains("Feature3"));
			AssertEquals(true, reader.HasFeature("Feature1"));
			AssertEquals(false, reader.HasFeature("Feature3"));
			AssertNotNull(reader.Value.Transport);
			AssertEquals("XTT", reader.Value.Transport.Delivery);
			AssertEquals("SOME_ENDPOINT_NAME", reader.Value.Transport.Destination);
			AssertEquals("GEN", reader.Value.Transport.MessageType);
		}

		public void TestCustomSettings()
		{
			var featureControlManager = GetFeatureControlManagerMock();

			var reader = new EInvoicingFeatureSettingsReader("Country3", featureControlManager);
			Assert(reader.TryGetCustomProperty("ServiceConnectorKey", out string key));
			AssertEquals("ABC12345", key);
			Assert(reader.TryGetCustomProperty("ApiGatewayOverride.Host", out string host));
			AssertEquals("test.example.com", host);
			Assert(reader.TryGetCustomProperty("EmptyObject", out object emptyObject));
			Assert(!reader.TryGetCustomProperty("MissingProperty", out string missingProp));
			AssertEquals(default(string), missingProp);

			Assert(reader.TryGetCustomProperty("$", out CustomSettings customSettings));
			AssertEquals("ABC12345", customSettings.ServiceConnectorKey);
			AssertEquals("test.example.com", customSettings.ApiGatewayOverride.Host);

			Assert(!reader.TryGetCustomProperty("ServiceConnectorKey", out int typeMismatchedValue1));
			var defaultInstance = new ApiGatewayOverride();
			Assert(reader.TryGetCustomProperty("$", out ApiGatewayOverride mismatchedType));
			AssertEquals(defaultInstance.Host, mismatchedType.Host);

			// Country1 does not even have the CountrySpecific block in its JSON
			reader = new EInvoicingFeatureSettingsReader("Country1", featureControlManager);
			Assert(!reader.TryGetCustomProperty("ServiceConnectorKey", out string doesNotExist));
			AssertEquals(default(string), doesNotExist);
		}

		public void TestDefaultValueIfCountryNotFound()
		{
			var featureControlManager = GetFeatureControlManagerMock();

			var reader = new EInvoicingFeatureSettingsReader("DoesNotExist", featureControlManager);
			AssertNotNull(reader.Value);
			AssertNotNull(reader.Value.Features);
			AssertEquals(false, reader.HasFeature("Feature1"));
			AssertNotNull(reader.Value.Transport);
			AssertNull(reader.Value.Transport.Delivery);
			AssertNull(reader.Value.Transport.Destination);
			AssertNull(reader.Value.Transport.MessageType);
		}

		public void TestReinitializesOnlyIfValueChanged()
		{
			var featureData1 = new Mock<IFeatureData>();
			var featureData2 = new Mock<IFeatureData>();
			featureData1.SetupGet(x => x.Parameter).Returns("{\"Country\": {\"Features\": [\"Feature1\"]}}");
			featureData2.SetupGet(x => x.Parameter).Returns("{\"Country\": {\"Features\": [\"Feature2\"]}}");

			var returnSecondFeatureData = false;
			Task<IFeatureData> GetFeatureData(string featureCode, CancellationToken cancellationToken)
			{
				return Task.FromResult(!returnSecondFeatureData ? featureData1.Object : featureData2.Object);
			}

			var featureControlManagerMock = new Mock<IFeatureControlManager>();
			featureControlManagerMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingEInvoicingConfiguration, CancellationToken.None)).Returns(GetFeatureData);
			var readerMock = new Mock<EInvoicingFeatureSettingsReader>("Country", featureControlManagerMock.Object) { CallBase = true };
			var reader = readerMock.Object;

			readerMock.Protected().Verify("Initialize", Times.Exactly(1));
			readerMock.Protected().Verify("HasFeatureDataChanged", Times.Exactly(0));
			AssertEquals(true, reader.HasFeature("Feature1"));
			AssertEquals(false, reader.HasFeature("Feature2"));
			readerMock.Protected().Verify("Initialize", Times.Exactly(1));
			readerMock.Protected().Verify("HasFeatureDataChanged", Times.Exactly(2));

			returnSecondFeatureData = true;
			AssertEquals(false, reader.HasFeature("Feature1"));
			AssertEquals(true, reader.HasFeature("Feature2"));
			readerMock.Protected().Verify("Initialize", Times.Exactly(2));
			readerMock.Protected().Verify("HasFeatureDataChanged", Times.Exactly(4));

			AssertNotNull(reader.Value.Features);
			AssertNotNull(reader.Value.Transport);
			readerMock.Protected().Verify("Initialize", Times.Exactly(2));
			readerMock.Protected().Verify("HasFeatureDataChanged", Times.Exactly(6));
		}

		IFeatureControlManager GetFeatureControlManagerMock()
		{
			string featureDataParameter = @"
			{
				""Country1"": {
				},
				""Country2"": {
					""Features"": [""Feature1"", ""Feature2""],
					""Transport"": {
						""Delivery"": ""XTT"",
						""Destination"": ""SOME_ENDPOINT_NAME"",
						""MessageType"": ""GEN""
					}
				},
				""Country3"": {
					""Features"": [""Feature3"", ""Feature4""],
					""CountrySpecific"": {
						""ServiceConnectorKey"": ""ABC12345"",
						""ApiGatewayOverride"": {
							""Host"": ""test.example.com""
						},
						""EmptyObject"": {}
					}
				},
			}";

			var featureDataMock = new Mock<IFeatureData>();
			featureDataMock.SetupGet(x => x.Parameter).Returns(featureDataParameter);

			var featureControlManagerMock = new Mock<IFeatureControlManager>();
			featureControlManagerMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingEInvoicingConfiguration, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			return featureControlManagerMock.Object;
		}

		class CustomSettings
		{
			public string ServiceConnectorKey { get; set; }
			public ApiGatewayOverride ApiGatewayOverride { get; set; }
		}

		// This exists only so that we have nested settings for this example
		class ApiGatewayOverride
		{
			public string Host { get; set; }
		}
	}
}
