using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JournalEntriesNumberCustomisationCollection : TransactionNumberSequenceCustomisationCollection
	{
		public JournalEntriesNumberCustomisationCollection()
			: base()
		{
		}

		public JournalEntriesNumberCustomisationCollection(JournalEntriesNumberCustomisationSetting parent, FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
			this.Parent = parent;
		}

		public JournalEntriesNumberCustomisationSetting Parent { get; set; }

		public new JournalEntriesNumberCustomisation this[int x]
		{
			get { return (JournalEntriesNumberCustomisation)base[x]; }
		}

		public new JournalEntriesNumberCustomisation this[string elementName]
		{
			get
			{
				foreach (JournalEntriesNumberCustomisation element in this)
				{
					if (element.ElementName == elementName)
					{
						return element;
					}
				}

				return null;
			}
		}

		#region Implementation

		public new JournalEntriesNumberCustomisation AddNew()
		{
			return (JournalEntriesNumberCustomisation)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JournalEntriesNumberCustomisationCollection(Parent, fallbackLevel);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new JournalEntriesNumberCustomisation(CurrentFallbackLevel);
		}

		#endregion
	}
}
