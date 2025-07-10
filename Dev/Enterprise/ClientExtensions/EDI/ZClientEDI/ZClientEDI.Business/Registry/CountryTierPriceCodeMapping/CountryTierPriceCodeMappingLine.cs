using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class CountryTierPriceCodeMappingLine : AutoCountryTierPriceCodeMappingLine
	{
		public CountryTierPriceCodeMappingLine()
		{
		}

		public CountryTierPriceCodeMappingLine(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new CountryTierPriceCodeMappingLine(fallbackLevel, factory);
			return clone;
		}

		[List("Countries")]
		public override ZString CountryCode
		{
			get { return base.CountryCode; }
			set { base.CountryCode = value; }
		}

		public virtual RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory ?? new BusinessObjectFactory()); }
		}

		public override void ValidateCountryCode()
		{
			base.ValidateCountryCode();
			MandatoryValidation.CheckEntered(CountryCodeInfo);
			ListValidation.ErrorIfInvalidCode(CountryCodeInfo);

			var parentCollection = GetParentCollection(this, typeof(CountryTierPriceCodeMappingLineCollection));
			if (parentCollection != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CountryCodeInfo, parentCollection);
			}
		}

		public override void ValidateCountryTierCode()
		{
			base.ValidateCountryTierCode();
			MandatoryValidation.CheckEntered(CountryTierCodeInfo);
		}
	}
}
