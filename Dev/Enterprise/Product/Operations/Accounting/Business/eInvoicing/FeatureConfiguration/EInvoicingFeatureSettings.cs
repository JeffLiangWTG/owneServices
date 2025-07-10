using System;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.Accounting.Business.EInvoicing.FeatureConfiguration
{
	/// <summary>
	/// Related content can be found in the wiki at:
	/// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/14784/Feature-Control-Rule-ACCEINVCF
	/// If you are implementing country-specific settings, please document them on the page above or on any of its subpages.
	/// </summary>
	public class EInvoicingFeatureSettings
	{
		public ImmutableHashSet<string> Features { get; }

		public TransportMode Transport { get; }

		public JObject CountrySpecific { get; }

		[JsonConstructor]
		public EInvoicingFeatureSettings(ImmutableHashSet<string> features, TransportMode transport, JObject countrySpecific)
		{
			Features = (features != null)
				? ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, features.ToArray())
				: ImmutableHashSet.Create<string>();
			Transport = transport ?? new TransportMode(null, null, null);
			CountrySpecific = countrySpecific ?? new JObject();
		}

		public EInvoicingFeatureSettings()
			: this(null, null, null)
		{
		}
	}

	public class TransportMode
	{
		public string Delivery { get; }
		public string MessageType { get; }
		public string Destination { get; }

		[JsonConstructor]
		public TransportMode(string delivery, string messageType, string destination)
		{
			Delivery = delivery;
			MessageType = messageType;
			Destination = destination;
		}
	}
}
