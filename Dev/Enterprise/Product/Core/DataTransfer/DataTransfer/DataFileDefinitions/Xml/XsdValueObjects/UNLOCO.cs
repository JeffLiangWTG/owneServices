using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoUNLOCO")]
	public class UNLOCO : Xsd.AutoUNLOCO
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return base.IsSpecified && !Value.IsEmpty; }
		}

		public static Xsd.UNLOCO FromPort(RefUNLOCO port)
		{
			Xsd.UNLOCO result = null;
			if (port != null && port.Country != null)
			{
				result = new Xsd.UNLOCO();
				result.Value = port.RL_Code;
				result.Country = port.Country.RN_DescMultilingual;
				result.City = port.RL_PortName;
			}
			return result;
		}

		public static Xsd.UNLOCO FromPortCode(BusinessObjectFactory factory, ZString portCode)
		{
			Xsd.UNLOCO result = null;

			RefUNLOCO port = (RefUNLOCO)factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, portCode);
			if (port != null)
			{
				result = Xsd.UNLOCO.FromPort(port);
			}
			else if (!portCode.IsEmpty)
			{
				result = new Xsd.UNLOCO();
				result.Value = portCode;
			}

			return result;
		}

		public static Xsd.UNLOCO FromIATACode(BusinessObjectFactory factory, ZString iATACode)
		{
			Xsd.UNLOCO result = null;
			if (!iATACode.IsEmpty)
			{
				ZQuery iATACodeFilter = new ZQuery(RefUNLOCOSchema.RL_IATA, SQLComparisonOperator.Equal, iATACode);
				iATACodeFilter.OrderBy = RefUNLOCOSchema.RL_Code.Name;
				RefUNLOCO port = (RefUNLOCO)factory.LoadTop1(typeof(RefUNLOCO), iATACodeFilter);
				if (port != null)
				{
					result = Xsd.UNLOCO.FromPort(port);
				}
			}
			return result;
		}
	}
}
