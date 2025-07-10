using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EntityPrecedenceRuleItem : CodeDescriptionBool
	{
		public EntityPrecedenceRuleItem()
		{
		}

		public EntityPrecedenceRuleItem(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EntityPrecedenceRuleItem(fallbackLevel);
		}
	}
}
