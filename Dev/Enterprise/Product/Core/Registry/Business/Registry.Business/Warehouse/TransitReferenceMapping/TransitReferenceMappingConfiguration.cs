using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class TransitReferenceMappingConfiguration : RegistryBusinessObjectTemplate
	{
		public TransitReferenceMappingConfiguration()
		{
		}

		public TransitReferenceMappingConfiguration(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TransitReferenceMappingConfiguration(fallbackLevel);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			TransitReferenceMappingConfiguration castedClone = (TransitReferenceMappingConfiguration)clone;
			if (TransitReferenceMappingCollection != null)
			{
				castedClone.transitReferenceMappingCollection = (TransitReferenceMappingCollection)TransitReferenceMappingCollection.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.TransitReferenceMappingCollection);
			}
		}

		#region TransitReferenceMappingCollection

		public TransitReferenceMappingCollection TransitReferenceMappingCollection
		{
			get
			{
				if (transitReferenceMappingCollection == null)
				{
					transitReferenceMappingCollection = new TransitReferenceMappingCollection();
					RegisterEditableChildObject(TransitReferenceMappingCollection);
				}
				return transitReferenceMappingCollection;
			}
		}
		TransitReferenceMappingCollection transitReferenceMappingCollection;

		ZXmlSerializer transitReferenceMappingCollectionSerialiser;
		ZXmlSerializer TransitReferenceMappingCollectionSerialiser
		{
			get
			{
				return transitReferenceMappingCollectionSerialiser ?? (transitReferenceMappingCollectionSerialiser = ZXmlSerializer.New(typeof(TransitReferenceMappingCollection)));
			}
		}

		#endregion
		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			TransitReferenceMappingCollectionSerialiser.Serialize(writer, TransitReferenceMappingCollection);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			transitReferenceMappingCollection = (TransitReferenceMappingCollection)TransitReferenceMappingCollectionSerialiser.Deserialize(reader);
			RegisterEditableChildObject(TransitReferenceMappingCollection);
		}
	}
}
