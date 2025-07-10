using System.IO;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Billing.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class SingleCountryDiscount : AutoSingleCountryDiscount
	{
		public static SingleCountryDiscount NewFromXml(string xml)
		{
			SingleCountryDiscount result;
			if (string.IsNullOrEmpty(xml))
			{
				result = new SingleCountryDiscount();
			}
			else
			{
				var serializer = ZXmlSerializer.New(typeof(SingleCountryDiscount));
				using (var reader = new StringReader(xml))
				{
					result = (SingleCountryDiscount)serializer.Deserialize(reader);
				}
			}
			return result;
		}

		[List("Countries")]
		public override ZString Country { get => base.Country; set => base.Country = value; }

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(new BusinessObjectFactory() { RefreshEnabled = false }); }
		}

		public override void ValidateCountry()
		{
			base.ValidateCountry();
			MandatoryValidation.CheckEntered(CountryInfo);
			ListValidation.ErrorIfInvalidCode(CountryInfo);
		}
	}
}

