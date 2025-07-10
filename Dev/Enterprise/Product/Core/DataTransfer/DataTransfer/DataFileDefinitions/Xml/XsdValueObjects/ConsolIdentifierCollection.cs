using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	public class ConsolIdentifierCollection : Xsd.AutoConsolIdentifierCollection
	{
		public Xsd.ConsolIdentifier FindFirst(Xsd.ConsolIdentifierType typeOfIdentifier)
		{
			foreach (Xsd.ConsolIdentifier identifier in this)
			{
				if (identifier.ConsolIdentifierType == typeOfIdentifier)
				{
					return identifier;
				}
			}
			return null;
		}

		public Xsd.ConsolIdentifierCollection Find(Xsd.ConsolIdentifierType typeOfIdentifier)
		{
			Xsd.ConsolIdentifierCollection result = new ConsolIdentifierCollection();
			foreach (Xsd.ConsolIdentifier identifier in this)
			{
				if (identifier.ConsolIdentifierType == typeOfIdentifier)
				{
					result.Add(identifier);
				}
			}
			return result;
		}
	}
}
