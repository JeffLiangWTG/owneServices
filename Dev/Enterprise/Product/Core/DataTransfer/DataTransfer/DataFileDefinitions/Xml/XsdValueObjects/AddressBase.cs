using System.Xml.Serialization;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoAddressBase")]
	public class AddressBase : Xsd.AutoAddressBase
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return base.IsSpecified && !AddressLine1.IsEmpty; }
		}

		public ZString GetPhoneNumber(Xsd.TelephoneNumberNumberType phoneNumberType)
		{
			foreach (Xsd.TelephoneNumber phoneNumber in TelephoneNumbers)
			{
				if (phoneNumber.NumberType == phoneNumberType)
				{
					return phoneNumber.Value;
				}
			}
			return ZString.Empty;
		}
	}
}
