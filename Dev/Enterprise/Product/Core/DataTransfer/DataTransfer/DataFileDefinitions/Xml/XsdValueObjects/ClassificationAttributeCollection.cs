using System.Collections.Generic;
using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoClassificationAttributeCollection")]
	public class ClassificationAttributeCollection : Xsd.AutoClassificationAttributeCollection
	{
		public ClassificationAttribute[] GetAttributesByType(Xsd.ClassificationAttributeType type)
		{
			var result = new List<ClassificationAttribute>();
			foreach (Xsd.ClassificationAttribute attribute in this)
			{
				if (attribute.Type == type)
				{
					result.Add(attribute);
				}
			}
			return result.ToArray();
		}
	}
}
