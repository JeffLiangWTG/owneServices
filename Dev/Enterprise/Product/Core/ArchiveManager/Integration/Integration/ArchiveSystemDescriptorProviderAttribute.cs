using System;
using System.Xml.Serialization;
using CargoWise.Definitions;

namespace Enterprise.ArchiveManager.Integration
{
	[XmlSerializerAssembly("Enterprise.ArchiveManager.Integration.XmlSerializers")]
	[Serializable]
	public class ArchiveSystemDescriptorProviderAttribute : AssemblyMetaDataAttributeWithType
	{
		public ArchiveSystemDescriptorProviderAttribute(Type concreteIArchiveSystemDescriptorType)
			: base(concreteIArchiveSystemDescriptorType)
		{ }

		public ArchiveSystemDescriptorProviderAttribute()
		{ }
	}
}
