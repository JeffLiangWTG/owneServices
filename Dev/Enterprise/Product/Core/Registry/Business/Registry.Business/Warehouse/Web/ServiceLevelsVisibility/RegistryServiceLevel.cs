using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class RegistryServiceLevel : CodeDescriptionBool
	{
		#region Schema

		abstract new class Schema : RegistryBusinessObject.Schema
		{
			public const string refServiceLevelPK = "refServiceLevelPK";
		}

		#endregion

		public RegistryServiceLevel() { }

		public RegistryServiceLevel(ZGuid refServiceLevelPK, ZString code, MultilingualString description, ZBool isPublished)
		{
			this.refServiceLevelPK = refServiceLevelPK;
			Code = code;
			Description = description;
			Bool = isPublished;
		}

		public ZGuid RefServiceLevelPK
		{
			get
			{
				return refServiceLevelPK;
			}
		}
		ZGuid refServiceLevelPK;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RegistryServiceLevel(refServiceLevelPK, Code, Description, Bool);
		}

		protected override bool CodeAndDescriptionReadOnly
		{
			get { return true; }
		}

		#region Xml Serialisation

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.refServiceLevelPK, RefServiceLevelPK.ToString());
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			refServiceLevelPK = new ZGuid(reader.ReadElementString(Schema.refServiceLevelPK));
		}

		#endregion
	}
}
