using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped")]
	public class TestRigRegistryHeader : RegistryBusinessObjectTemplateWithChildCollection
	{
		public TestRigRegistryHeader()
		{
		}

		public TestRigRegistryHeader(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public TestRigRegistryCollection OptionsCollection
		{
			get
			{
				if (optionsCollection == null)
				{
					optionsCollection = new TestRigRegistryCollection(CurrentFallbackLevel, CurrentFactory);
					RegisterEditableChildObject(optionsCollection);
				}

				return optionsCollection;
			}
		}

		TestRigRegistryCollection optionsCollection;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TestRigRegistryHeader(fallbackLevel, factory)
			{
				optionsCollection = (TestRigRegistryCollection)OptionsCollection.Clone(fallbackLevel, factory)
			};
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			optionsCollection = (TestRigRegistryCollection)CollectionSerialiser.Deserialize(reader);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			CollectionSerialiser.Serialize(writer, OptionsCollection);
		}

		ZXmlSerializer CollectionSerialiser => collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(TestRigRegistryCollection)));
		ZXmlSerializer collectionSerialiser;

		protected override IEnumerable<RegistryBusinessObjectCollectionTemplate> ChildCollections => new[] { OptionsCollection };

		protected override void RunPreSaveValidationCore()
		{
			foreach (var options in OptionsCollection.Cast<TestRigRegistryOptions>())
			{
				options.Validation.ValidateAll();
			}

			base.RunPreSaveValidationCore();
		}
	}
}

