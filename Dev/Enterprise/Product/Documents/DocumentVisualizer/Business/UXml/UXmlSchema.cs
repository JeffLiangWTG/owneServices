using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class UXmlSchema
	{
		public IReadOnlyDictionary<Type, string> NaturalKeys
		{
			get { return naturalKeys ?? (naturalKeys = new ReadOnlyDictionary<Type, string>(GetNaturalKeys())); }
		}

		IReadOnlyDictionary<Type, string> naturalKeys;

		//TODO: extract from universal xsd
		//natural key will most likely have CandidateKeyAttribute and MandatoryAttribute
		Dictionary<Type, string> GetNaturalKeys()
		{
			return new Dictionary<Type, string>
			{
				{ typeof(OrganizationAddress), "AddressType" },
				{ typeof(TransportLeg), "LegType" }
			};
		}
	}
}