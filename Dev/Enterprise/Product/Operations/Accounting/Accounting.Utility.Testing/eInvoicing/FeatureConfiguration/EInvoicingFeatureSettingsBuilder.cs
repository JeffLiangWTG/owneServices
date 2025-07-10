using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Accounting.Business.EInvoicing.FeatureConfiguration;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.Accounting.Utility.Testing.eInvoicing
{
	public sealed class EInvoicingFeatureSettingsBuilder
	{
		public string CountryCode { get; }

		public IEnumerable<string> Features { get; set; } = Enumerable.Empty<string>();

		public string TransportDelivery { get; set; }
		public string TransportMessageType { get; set; }
		public string TransportDestination { get; set; }

		public JObject CountrySpecificObject { get; set; }

		public EInvoicingFeatureSettingsBuilder(string countryCode)
		{
			CountryCode = countryCode;
		}

		public EInvoicingFeatureSettingsBuilder WithFeatures(IEnumerable<string> features)
		{
			Features = features;
			return this;
		}

		public EInvoicingFeatureSettingsBuilder WithTransportDelivery(string delivery)
		{
			TransportDelivery = delivery;
			return this;
		}
		public EInvoicingFeatureSettingsBuilder WithTransportMessageType(string messageType)
		{
			TransportMessageType = messageType;
			return this;
		}
		public EInvoicingFeatureSettingsBuilder WithTransportDestination(string destination)
		{
			TransportDestination = destination;
			return this;
		}

		public EInvoicingFeatureSettingsBuilder WithCountrySpecificObject(JObject o)
		{
			CountrySpecificObject = o;
			return this;
		}

		public EInvoicingFeatureSettings Build()
			=> new EInvoicingFeatureSettings(
				Features.ToImmutableHashSet(),
				new TransportMode(TransportDelivery, TransportMessageType, TransportDestination),
				CountrySpecificObject
			);
	}

	public static class EInvoicingFeatureSettingsBuilderExtensions
	{
		public static string BuildJson(this EInvoicingFeatureSettingsBuilder builder)
			=> new[] { builder }.BuildJson();

		public static string BuildJson(this IEnumerable<EInvoicingFeatureSettingsBuilder> builders)
		{
			var parameterAsJson = "{";
			foreach (var builder in builders)
			{
				var settings = builder.Build();
				var settingsJson = JsonConvert.SerializeObject(settings);
				parameterAsJson += "\"" + builder.CountryCode + "\":" + settingsJson + ",";
			}
			parameterAsJson += "}";
			return parameterAsJson;
		}

		public static Mock<IFeatureData> BuildAsMock(this EInvoicingFeatureSettingsBuilder builder)
			=> new[] { builder }.BuildAllAsMock();

		public static Mock<IFeatureData> BuildAllAsMock(this IEnumerable<EInvoicingFeatureSettingsBuilder> builders)
		{
			var parameterAsJson = builders.BuildJson();
			var featureDataMock = new Mock<IFeatureData>();
			featureDataMock.SetupGet(x => x.Parameter).Returns(parameterAsJson);
			return featureDataMock;
		}

		public static IDisposable BuildAndRegisterFeatureControlMock(this EInvoicingFeatureSettingsBuilder builder)
			=> new[] { builder }.BuildAllAndRegisterFeatureControlMock();

		public static IDisposable BuildAllAndRegisterFeatureControlMock(this IEnumerable<EInvoicingFeatureSettingsBuilder> builders)
		{
			var featureDataMock = builders.BuildAllAsMock();

			var mock = new Mock<IFeatureControlManager>();
			mock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.AccountingEInvoicingConfiguration, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			return ObjectFactory.Substitute(mock.Object);
		}
	}
}
