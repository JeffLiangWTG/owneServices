using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class IntercompanyEventConfiguration : RegistryBusinessObjectTemplate
	{
		public IntercompanyEventConfiguration()
		{
		}

		public IntercompanyEventConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public IntercompanyEventSettingCollection IntercompanyEventSettingCollection
		{
			get
			{
				if (intercompanyEventSettingCollection == null)
				{
					intercompanyEventSettingCollection = new IntercompanyEventSettingCollection();
					RegisterEditableChildObject(intercompanyEventSettingCollection);
				}

				return intercompanyEventSettingCollection;
			}
		}
		IntercompanyEventSettingCollection intercompanyEventSettingCollection;

		[ResourceStringData("IntercompanyEventConfiguration|EnableEventConfiguration", Caption = "Enable Import Event Configuration")]
		public ZBool EnableEventConfiguration
		{
			get { return enableEventConfiguration; }
			set
			{
				SetNonPersistentPropertyValue(EnableEventConfigurationInfo, ref enableEventConfiguration, value);
			}
		}

		public ZPropertyInfo EnableEventConfigurationInfo
		{
			get { return GetZPropertyInfo(nameof(EnableEventConfiguration)); }
		}

		ZBool enableEventConfiguration;

		ZXmlSerializer IntercompanyEventSettingCollectionSerializer => intercompanyEventSettingCollectionSerializer ?? (intercompanyEventSettingCollectionSerializer = ZXmlSerializer.New(typeof(IntercompanyEventSettingCollection)));
		ZXmlSerializer intercompanyEventSettingCollectionSerializer;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IntercompanyEventConfiguration(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			IntercompanyEventConfiguration castedClone = (IntercompanyEventConfiguration)clone;
			if (intercompanyEventSettingCollection != null)
			{
				castedClone.enableEventConfiguration = enableEventConfiguration;
				castedClone.intercompanyEventSettingCollection = (IntercompanyEventSettingCollection)IntercompanyEventSettingCollection.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.IntercompanyEventSettingCollection);
			}
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(nameof(EnableEventConfiguration), EnableEventConfiguration.ToString());
			IntercompanyEventSettingCollectionSerializer.Serialize(writer, IntercompanyEventSettingCollection);
		}
		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EnableEventConfiguration = new ZBool(reader.ReadElementString(nameof(EnableEventConfiguration)));
			intercompanyEventSettingCollection = (IntercompanyEventSettingCollection)IntercompanyEventSettingCollectionSerializer.Deserialize(reader);
			RegisterEditableChildObject(intercompanyEventSettingCollection);
		}

		#endregion
	}
}
