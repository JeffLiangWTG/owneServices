using System.Linq;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Newtonsoft.Json.Linq;

namespace Enterprise.Accounting.Business.EInvoicing.FeatureConfiguration
{
	/// <summary>
	/// Related content can be found in the wiki at:
	/// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/14784/Feature-Control-Rule-ACCEINVCF
	/// If you are implementing country-specific settings, please document them on the page above or on any of its subpages.
	/// </summary>
	public class EInvoicingFeatureSettingsReader
	{
		readonly IFeatureControlManager featureControlManager;
		readonly string countryCode;
		IFeatureData lastUsedFeatureData;
		EInvoicingFeatureSettings featureSettingsInstance;

		public EInvoicingFeatureSettings Value
		{
			get
			{
				if (HasFeatureDataChanged())
				{
					Initialize();
				}
				return featureSettingsInstance;
			}
		}

		public EInvoicingFeatureSettingsReader(string countryCode, IFeatureControlManager featureControlManager)
		{
			this.featureControlManager = featureControlManager;
			this.countryCode = countryCode;

			Initialize();
		}

		public bool HasFeature(string featureName) => Value.Features.Contains(featureName);

		public bool TryGetCustomProperty<T>(string name, out T value)
		{
			var prop = Value.CountrySpecific?.SelectToken(name, errorWhenNoMatch: false);
			if (prop != null)
			{
				try
				{
					value = prop.ToObject<T>();
					return true;
				}
				catch
				{
					value = default;
					return false;
				}
			}

			value = default;
			return false;
		}

		protected virtual void Initialize()
		{
			var featureData = featureControlManager.GetFeatureData(LicenceFeatureCodeList.Codes.AccountingEInvoicingConfiguration);
			if (featureData != null && !string.IsNullOrWhiteSpace(featureData.Parameter))
			{
				var allCountries = JObject.Parse(featureData.Parameter);
				var singleCountry = allCountries.SelectToken(countryCode, errorWhenNoMatch: false);
				if (singleCountry != null && singleCountry.Children().Any())
				{
					featureSettingsInstance = singleCountry.ToObject<EInvoicingFeatureSettings>();
				}
				lastUsedFeatureData = featureData;
			}

			featureSettingsInstance ??= new EInvoicingFeatureSettings();
		}

		protected virtual bool HasFeatureDataChanged()
		{
			var featureData = featureControlManager.GetFeatureData(LicenceFeatureCodeList.Codes.AccountingEInvoicingConfiguration);
			return !ReferenceEquals(featureData, lastUsedFeatureData);
		}
	}
}
