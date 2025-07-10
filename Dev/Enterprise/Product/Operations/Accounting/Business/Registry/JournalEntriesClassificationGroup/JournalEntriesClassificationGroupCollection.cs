using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JournalEntriesClassificationGroupCollection : RegistryBusinessObjectCollectionTemplate
	{
		public JournalEntriesClassificationGroupCollection()
			: base()
		{
		}

		public JournalEntriesClassificationGroupCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public new JournalEntriesClassificationGroup this[int x]
		{
			get { return (JournalEntriesClassificationGroup)base[x]; }
		}

		#region Implementation

		public new JournalEntriesClassificationGroup AddNew()
		{
			return (JournalEntriesClassificationGroup)base.AddNew();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new JournalEntriesClassificationGroup(CurrentFallbackLevel);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JournalEntriesClassificationGroupCollection(fallbackLevel);
		}

		#endregion
	}
}
