#nullable enable
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;

namespace Enterprise.Registry.Business
{
	public static class RatingFeatureHelper
	{
		public static class CarrierConnect
		{
			public static bool IsEnabledForRateSelection() => IsFeatureEnabled() && RatingDataRegistry.Instance.CargoWiseCarrierConnectForJobAutorating.Value;

			public static bool IsFeatureEnabled() => GetFeatureRule()?.Enabled ?? false;

			static CWCarrierConnectFeatureRule? GetFeatureRule()
			{
				CWCarrierConnectFeatureRule? cWCarrierConnectFeatureRule = null;

				var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect);
				if (featureData != null)
				{
					featureData?.TryDeserializeParameterAsJson(out cWCarrierConnectFeatureRule);
				}

				return cWCarrierConnectFeatureRule;
			}

			public sealed class CWCarrierConnectFeatureRule
			{
				// Note: this is a DTO class which is deserialized from JSON. Backwards compatibility is important!
				public bool Enabled { get; set; }
			}
		}

		public static class Urs
		{
			public static bool IsEnabled
				=> (GetFeatureRule()?.Enabled ?? false) || RatingDataRegistry.Instance.EnableUrsIntegration.Value;

			public static bool IsEnabledForLegacy
				=> ((GetFeatureRule()?.Enabled ?? false) || RatingDataRegistry.Instance.EnableUrsIntegration.Value)
					&& RatingDataRegistry.Instance.UseUrsForLegacyRateSelectorAndMMS.Value;

			public static string Url
				=> GetFeatureRule()?.Url ?? RatingDataRegistry.Instance.UniversalRatesServiceUrl.Value;

			static UrsFeatureRule? GetFeatureRule()
			{
				UrsFeatureRule? ursFeatureUrl = null;

				var featureData = ObjectFactory
					.Get<IFeatureControlManager>()
					.GetFeatureData(LicenceFeatureCodeList.Codes.UniversalRatesService);
				featureData?.TryDeserializeParameterAsJson(out ursFeatureUrl);

				return ursFeatureUrl;
			}

			/// <summary>
			/// Represents the URS rule stored in the feature control data.
			/// Any changes made to this must be backwards compatible as there may be existing data in the FCM database.
			/// </summary>
			public sealed class UrsFeatureRule
			{
				public bool Enabled { get; set; }

				public string? Url { get; set; }
			}
		}
	}
}
