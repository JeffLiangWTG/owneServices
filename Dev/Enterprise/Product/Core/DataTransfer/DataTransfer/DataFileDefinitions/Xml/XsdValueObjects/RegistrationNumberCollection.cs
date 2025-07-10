using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	public class RegistrationNumberCollection : Xsd.AutoRegistrationNumberCollection
	{
		public Xsd.RegistrationNumber FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes regNoType)
		{
			return FindOrCreate(regNoType, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public Xsd.RegistrationNumber FindOrCreate(Xsd.RegistrationNumberTypes regNoType, ZString countryCode)
		{
			Xsd.RegistrationNumber result = FindRegistrationNumber(regNoType, countryCode);
			if (result == null)
			{
				result = AddNew();
				result.NumberType = regNoType;
				result.CountryOfRegistration = countryCode;
			}
			return result;
		}

		public Xsd.RegistrationNumber FindRegistrationNumber(Xsd.RegistrationNumberTypes regNoType, ZString countryCode)
		{
			foreach (Xsd.RegistrationNumber regNo in this)
			{
				if (regNo.NumberType == regNoType && regNo.CountryOfRegistration == countryCode)
				{
					return regNo;
				}
			}
			return null;
		}
	}
}
