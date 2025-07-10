using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	public class TelephoneNumberCollection : Xsd.AutoTelephoneNumberCollection
	{
		protected Xsd.TelephoneNumber GetFirstTelephoneNumber(Xsd.TelephoneNumberNumberType type)
		{
			Xsd.TelephoneNumber result = null;

			foreach (Xsd.TelephoneNumber teleponeNumber in this)
			{
				if (teleponeNumber.NumberType == type)
				{
					result = teleponeNumber;
					break;
				}
			}

			return result;
		}
	}
}
