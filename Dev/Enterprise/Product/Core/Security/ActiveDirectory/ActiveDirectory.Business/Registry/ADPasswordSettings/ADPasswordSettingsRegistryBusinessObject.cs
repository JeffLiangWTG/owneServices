using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory
{
	[XmlSerializerAssembly("Enterprise.Security.ActiveDirectory.XmlSerializers")]
	public class ADPasswordSettingsRegistryBusinessObject : RegistryBusinessObjectTemplate
	{
		public ADPasswordSettingsRegistryBusinessObject()
			: base()
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ADPasswordSettingsRegistryBusinessObject();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Dummy = reader.ReadElementStringAsZBool((NoResString)"Override");
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString("Override", Dummy.ToString());
		}

		public ZBool Dummy;
	}
}
