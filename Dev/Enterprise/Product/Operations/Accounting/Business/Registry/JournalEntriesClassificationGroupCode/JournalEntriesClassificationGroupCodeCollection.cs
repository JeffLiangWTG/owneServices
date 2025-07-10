using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JournalEntriesClassificationGroupCodeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public JournalEntriesClassificationGroupCodeCollection()
			: base()
		{
		}

		public JournalEntriesClassificationGroupCodeCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public new JournalEntriesClassificationGroupCode this[int x]
		{
			get { return (JournalEntriesClassificationGroupCode)base[x]; }
		}

		#region Implementation

		public new JournalEntriesClassificationGroupCode AddNew()
		{
			return (JournalEntriesClassificationGroupCode)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new JournalEntriesClassificationGroupCode(CurrentFallbackLevel);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JournalEntriesClassificationGroupCodeCollection(fallbackLevel);
		}

		#endregion
	}
}
