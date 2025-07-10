using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RegistryServiceLevelCollection : CodeDescriptionBoolCollection
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RegistryServiceLevel();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new RegistryServiceLevelCollection();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public new RegistryServiceLevel AddNew()
		{
			return (RegistryServiceLevel)base.AddNew();
		}

		public new RegistryServiceLevel this[int i]
		{
			get { return (RegistryServiceLevel)base[i]; }
		}

		public RegistryServiceLevel Add(ZGuid guid, string code, MultilingualString description, bool value)
		{
			RegistryServiceLevel result = new RegistryServiceLevel(guid, code, description, value);
			base.Add(result);
			return result;
		}

		public RegistryServiceLevel FindByRefServiceLevelPK(ZGuid guid)
		{
			foreach (RegistryServiceLevel element in this)
			{
				if (element.RefServiceLevelPK == guid)
				{
					return element;
				}
			}

			return null;
		}

		public static RegistryServiceLevelCollection GetDefaultLevels()
		{
			return ObjectFactory.Get<IActiveServiceLevelProvider>().GetActiveServiceLevels(new BusinessObjectFactory());
		}
	}
}
