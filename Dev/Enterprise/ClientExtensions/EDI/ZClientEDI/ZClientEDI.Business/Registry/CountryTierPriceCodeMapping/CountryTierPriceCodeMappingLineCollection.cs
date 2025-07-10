using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class CountryTierPriceCodeMappingLineCollection : RegistryBusinessObjectCollectionTemplate<CountryTierPriceCodeMappingLine>
	{
		public CountryTierPriceCodeMappingLineCollection() : this(null, null)
		{
		}

		public CountryTierPriceCodeMappingLineCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public CountryTierPriceCodeMappingLine AddNew(string countryCode, string countryTierCode)
		{
			var result = AddNew();
			result.CountryCode = countryCode;
			result.CountryTierCode = countryTierCode;
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CountryTierPriceCodeMappingLine(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CountryTierPriceCodeMappingLineCollection(fallbackLevel, factory);
		}
	}
}
