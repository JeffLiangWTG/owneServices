using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.eHub
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ScavengingSettingCollection : RegistryBusinessObjectCollectionTemplate<ScavengingSetting>
	{
		public ScavengingSettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public ScavengingSettingCollection() : base(null, null)
		{
		}

		public ScavengingSetting Get(string taskName)
		{
			return base.Elements.Cast<ScavengingSetting>().FirstOrDefault(e => e.TaskName == taskName);
		}

		public bool HasMultipleTaskWithSameName(string taskName)
		{
			return base.Elements.Cast<ScavengingSetting>().Count(e => e.TaskName == taskName) > 1;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ScavengingSetting(CurrentFallbackLevel, CurrentFactory, this);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ScavengingSettingCollection(fallbackLevel, factory);
		}
	}
}
