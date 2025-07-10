using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.DE.Business.XmlSerializers")]
	public class ExportStatusRequestRecipientRegistryCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ExportStatusRequestRecipientRegistryCollection()
			: base()
		{
		}

		public ExportStatusRequestRecipientRegistryCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public ExportStatusRequestRecipientRegistryCollection DefaultCollection
		{
			get
			{
				if (defaultCollection == null)
				{
					defaultCollection = new ExportStatusRequestRecipientRegistryCollection()
					{
						new ExportStatusRequestRecipientRegistry() { SystemCode = ExportStatusRequestRecipientRegistry.AtlasSystemCode, MessageRecipient = "DE001348" },
						new ExportStatusRequestRecipientRegistry() { SystemCode = ExportStatusRequestRecipientRegistry.AESSystemCode, MessageRecipient = "DE001342" }
					};
				}
				return defaultCollection;
			}
		}
		ExportStatusRequestRecipientRegistryCollection defaultCollection;

		public new ExportStatusRequestRecipientRegistry this[int i] => (ExportStatusRequestRecipientRegistry)Elements[i];

		public new ExportStatusRequestRecipientRegistry AddNew() => (ExportStatusRequestRecipientRegistry)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new ExportStatusRequestRecipientRegistry(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new ExportStatusRequestRecipientRegistryCollection(fallbackLevel, factory);

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
