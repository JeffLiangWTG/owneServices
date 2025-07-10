using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class SendOrganizationDataToCertCapture : RegistryBusinessObjectTemplate
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SendOrganizationDataToCertCapture();
		}

		ZBool enableSend;

		public ZBool EnableSend
		{
			get => enableSend;
			set => SetNonPersistentPropertyValue(EnableSendInfo, ref enableSend, value);
		}

		public ZPropertyInfo EnableSendInfo => GetZPropertyInfo(Schema.EnableSend);

		#region Schema

		public abstract class Schema
		{
			public const string EnableSend = "EnableSend";
		}

		#endregion

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EnableSend = reader.ReadElementStringAsZBool(Schema.EnableSend);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EnableSend, EnableSend.ToString());
		}
	}
}
