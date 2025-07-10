using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DataTransferSwitchRegistryBusinessObject : DataTransferRegistryBusinessObject
	{
		#region .ctors

		public DataTransferSwitchRegistryBusinessObject() : base() { }

		public DataTransferSwitchRegistryBusinessObject(BusinessObjectFactory factory) : base(factory) { }

		#endregion

		#region Schema

		public new abstract class Schema : DataTransferRegistryBusinessObject.Schema
		{
			public const string EnableInterface = "EnableInterface";
		}

		#endregion

		#region EnableInterface

		public ZBool EnableInterface
		{
			get { return enableInterface; }
			set { SetNonPersistentPropertyValue(EnableInterfaceInfo, ref enableInterface, value); }
		}

		ZBool enableInterface;

		public ZPropertyInfo EnableInterfaceInfo
		{
			get { return GetZPropertyInfo(Schema.EnableInterface); }
		}

		#endregion

		public override ZBool IsGoodToGo
		{
			get { return base.IsGoodToGo && EnableInterface; }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DataTransferSwitchRegistryBusinessObject();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EnableInterface, EnableInterface.ToString());
		}

		protected override void ReadElements(Enterprise.Registry.Business.XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			EnableInterface = new ZBool(reader.ReadElementStringAsZBool(Schema.EnableInterface));
		}
	}
}
