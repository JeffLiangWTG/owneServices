using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[CodeProperty(Schema.Code), DescriptionProperty(Schema.Description)]
	public class AdditionalHouseBillOfLadingType : RegistryBusinessObject
	{
		public AdditionalHouseBillOfLadingType()
		{
		}

		public AdditionalHouseBillOfLadingType(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AdditionalHouseBillOfLadingType(fallbackLevel);
		}

		#region Enable

		public ZBool Enable
		{
			get { return enable; }
			set { SetNonPersistentPropertyValue<ZBool>(EnableInfo, ref enable, value); }
		}

		public ZPropertyInfo EnableInfo => GetZPropertyInfo(nameof(Enable));

		ZBool enable;

		#endregion

		#region Enable Messaging

		public ZBool EnableMessaging
		{
			get { return enableMessaging; }
			set { SetNonPersistentPropertyValue<ZBool>(EnableMessagingInfo, ref enableMessaging, value); }
		}

		public ZPropertyInfo EnableMessagingInfo => GetZPropertyInfo(nameof(EnableMessaging));

		ZBool enableMessaging;

		#endregion

		#region Write/Read XML

		protected override void WriteMoreElements(XmlWriter writer)
		{
			writer.WriteElementString(nameof(Enable), Enable.ToString());
			writer.WriteElementString(nameof(EnableMessaging), EnableMessaging.ToString());
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			Enable = new ZBool(reader.ReadElementString(nameof(Enable)));
			EnableMessaging = new ZBool(reader.ReadElementString(nameof(EnableMessaging)));
		}

		#endregion
	}
}
