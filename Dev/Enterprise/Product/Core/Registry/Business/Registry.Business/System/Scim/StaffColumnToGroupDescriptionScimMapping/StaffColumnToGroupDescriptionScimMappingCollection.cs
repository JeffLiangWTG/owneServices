using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class StaffColumnToGroupDescriptionScimMappingCollection : RegistryBusinessObjectCollectionTemplate
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StaffColumnToGroupDescriptionScimMapping();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StaffColumnToGroupDescriptionScimMappingCollection();
		}

		public new StaffColumnToGroupDescriptionScimMapping this[int i]
		{
			get { return (StaffColumnToGroupDescriptionScimMapping)Elements[i]; }
		}

		public new StaffColumnToGroupDescriptionScimMapping AddNew()
		{
			return (StaffColumnToGroupDescriptionScimMapping)base.AddNew();
		}

		public bool HasGroup(IGlbGroup group)
		{
			return this.Cast<StaffColumnToGroupDescriptionScimMapping>().Any(i => i.GroupDescriptionMapping.EqualsIgnoringCase(group.GG_Desc));
		}
	}
}
