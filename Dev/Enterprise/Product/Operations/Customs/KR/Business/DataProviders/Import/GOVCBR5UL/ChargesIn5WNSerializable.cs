using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlRoot("ChargesIn5WN")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ChargesIn5WNSerializable : IChargesIn5WN
	{
		public short VersionNumber { get; set; }
		public string VersionDescription { get; set; }
		public ChargesSerializable RefundAmounts { get; set; }

		ZShort IChargesIn5WN.VersionNumber => VersionNumber;
		ZString IChargesIn5WN.VersionDescription => VersionDescription;
		ICharges IChargesIn5WN.RefundAmounts => RefundAmounts;
	}
}
