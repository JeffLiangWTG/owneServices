using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot("RegistryBusinessObjectCollection")]
	public class PersistedSqlConfigurationsCollection : RegistryBusinessObjectCollection
	{
		public PersistedSqlConfigurationsCollection()
		{
		}

		public new PersistedSqlConfiguration this[int i] => (PersistedSqlConfiguration)base[i];

		public new PersistedSqlConfiguration AddNew()
		{
			return (PersistedSqlConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PersistedSqlConfigurationsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PersistedSqlConfiguration();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is PersistedSqlConfigurationsCollection collection) || collection.Count != Count)
			{
				return false;
			}

			return GetHashCode() == collection.GetHashCode();
		}

		public override int GetHashCode()
		{
			return this.Cast<PersistedSqlConfiguration>().Aggregate(
				0,
				(current, item) => current ^ item.GetHashCode());
		}
	}
}
