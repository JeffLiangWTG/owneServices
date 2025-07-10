using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CommunicationStatus : CodeDescriptionBool
	{
		#region Schema

		protected new abstract class Schema : CodeDescriptionBool.Schema
		{
			public const string Closed = "Closed";
		}

		#endregion

		public CommunicationStatus()
		{
		}

		ZBool closed;
		public virtual ZBool Closed
		{
			get { return closed; }
			set { SetNonPersistentPropertyValue(ClosedInfo, ref closed, value); }
		}

		public ZPropertyInfo ClosedInfo
		{
			get { return GetZPropertyInfo(Schema.Closed); }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CommunicationStatus();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((CommunicationStatus)clone).Closed = Closed;
		}

		#region XML Serialisation

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.Closed, Closed.ToString());
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			Closed = new ZBool(reader.ReadElementString(Schema.Closed));
		}

		#endregion
	}
}
