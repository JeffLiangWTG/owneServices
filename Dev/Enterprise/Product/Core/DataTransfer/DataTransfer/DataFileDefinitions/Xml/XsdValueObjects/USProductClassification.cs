using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoUSProductClassification")]
	public class USProductClassification : Xsd.AutoUSProductClassification
	{
		public USProductClassification()
		{
		}

		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return CountryOfOriginSpecified
					|| Manufacturer.IsSpecified
					|| ProductClaimSpecified
					|| SPISpecified
					|| AntiDumping.IsSpecified
					|| Countervailing.IsSpecified
					|| PermitsLicenses.IsSpecified
					|| PIRPRuling.IsSpecified
					|| NAFTANetCostSpecified
					|| OGAIndicatorsSpecified
					|| FDAs.IsSpecified
					|| FCCs.IsSpecified
					|| DOTs.IsSpecified
					|| PercentageOfActiveIngredientSpecified
					|| TSCAIndicatorSpecified
					|| ZoneStatusSpecified
					|| SoftwoodLumberSpecified
					|| LaceyActDetails.IsSpecified
					|| OverriddenTaxRateSpecified
					|| TaxCodeSpecified
					|| TaxRateTypeSpecified;
			}
			set { base.IsSpecified = value; }
		}
	}
}
