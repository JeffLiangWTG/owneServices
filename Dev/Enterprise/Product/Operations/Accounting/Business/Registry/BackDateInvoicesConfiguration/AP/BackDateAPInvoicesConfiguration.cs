using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class BackDateAPInvoicesConfiguration : RegistryBusinessObjectTemplate
	{
		public BackDateAPInvoicesConfiguration()
		{
		}

		public BackDateAPInvoicesConfiguration(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			if (PostDateConfigurationCollection.Count == 0)
			{
				PostDateConfigurationCollection.AddNew();
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BackDateAPInvoicesConfiguration(fallbackLevel);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			BackDateAPInvoicesConfiguration castedClone = (BackDateAPInvoicesConfiguration)clone;
			if (PostDateConfigurationCollection != null)
			{
				castedClone.postDateConfigurationCollection = (PostDateConfigurationCollection)PostDateConfigurationCollection.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.PostDateConfigurationCollection);
			}
		}

		#region Bound Properties

		#region PostDateConfigurationCollection

		public PostDateConfigurationCollection PostDateConfigurationCollection
		{
			get
			{
				if (postDateConfigurationCollection == null)
				{
					postDateConfigurationCollection = new PostDateConfigurationCollection();
					RegisterEditableChildObject(PostDateConfigurationCollection);
				}
				return postDateConfigurationCollection;
			}
		}
		PostDateConfigurationCollection postDateConfigurationCollection;

		ZXmlSerializer fPostDateConfigurationCollectionSerialiser;
		ZXmlSerializer PostDateConfigurationCollectionSerialiser
		{
			get
			{
				return fPostDateConfigurationCollectionSerialiser ?? (fPostDateConfigurationCollectionSerialiser = ZXmlSerializer.New(typeof(PostDateConfigurationCollection)));
			}
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			PostDateConfigurationCollectionSerialiser.Serialize(writer, PostDateConfigurationCollection);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			postDateConfigurationCollection = (PostDateConfigurationCollection)PostDateConfigurationCollectionSerialiser.Deserialize(reader);
			RegisterEditableChildObject(PostDateConfigurationCollection);
		}

		#endregion
	}
}