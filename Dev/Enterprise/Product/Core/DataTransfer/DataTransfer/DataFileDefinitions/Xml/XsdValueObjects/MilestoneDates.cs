using System.Xml.Serialization;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoMilestoneDates")]
	public class MilestoneDates : Xsd.AutoMilestoneDates
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return base.IsSpecified && (Estimated.IsValid || Actual.IsValid); }
		}

		public static Xsd.MilestoneDates FromEstimatedAndActual(ZDateTime estimated, ZDateTime actual)
		{
			Xsd.MilestoneDates result = null;

			result = new Xsd.MilestoneDates();
			if (estimated.IsValid)
			{
				result.Estimated = estimated;
			}

			if (actual.IsValid)
			{
				result.Actual = actual;
			}
			return result;
		}
	}
}
