using System.Xml.Serialization;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoDimensionValue")]
	public class DimensionValue : Xsd.AutoDimensionValue
	{
		public static DimensionValue FromAmountAndUnit(INumericZType amount, ZString unit)
		{
			Xsd.DimensionValue result = new Xsd.DimensionValue();
			result.Value = new ZDecimal(amount);
			if (!unit.IsEmpty)
			{
				result.DimensionType = unit;
			}

			return result;
		}
	}
}
