using System.IO;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Billing.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class WiseCloudDiscount : AutoWiseCloudDiscount
	{
		public static WiseCloudDiscount NewFromXml(string xml)
		{
			WiseCloudDiscount result;
			if (string.IsNullOrEmpty(xml))
			{
				result = new WiseCloudDiscount();
			}
			else
			{
				var serializer = ZXmlSerializer.New(typeof(WiseCloudDiscount));
				using (var reader = new StringReader(xml))
				{
					result = (WiseCloudDiscount)serializer.Deserialize(reader);
				}
			}

			return result;
		}

		public override void ValidateExpiryMonthsFromAgreedGoLive()
		{
			base.ValidateExpiryMonthsFromAgreedGoLive();
			MandatoryValidation.CheckNotNegative(ExpiryMonthsFromAgreedGoLiveInfo);
		}
	}
}

