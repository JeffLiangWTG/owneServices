using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RefDocTypeEntryCollection : RegistryBusinessObjectCollection
	{
		public RefDocTypeEntryCollection()
		{
		}

		public RefDocTypeEntryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new RefDocTypeEntry this[int i]
		{
			get { return (RefDocTypeEntry)base[i]; }
		}

		public new RefDocTypeEntry AddNew()
		{
			return (RefDocTypeEntry)base.AddNew();
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RefDocTypeEntry(CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RefDocTypeEntryCollection(factory);
		}

		public override bool Equals(object obj)
		{
			var collection = obj as RefDocTypeEntryCollection;
			if (collection == null || collection.Count != this.Count)
			{
				return false;
			}

			for (int i = 0; i < collection.Count; i++)
			{
				if (collection[i].RefDocTypePK != this[i].RefDocTypePK)
				{
					return false;
				}
			}

			return true;
		}

		public override int GetHashCode()
		{
			var hash = 0;
			foreach (RefDocTypeEntry item in this)
			{
				hash = hash ^ item.RefDocTypePK.GetHashCode();
			}
			return hash;
		}

		#endregion
	}
}
