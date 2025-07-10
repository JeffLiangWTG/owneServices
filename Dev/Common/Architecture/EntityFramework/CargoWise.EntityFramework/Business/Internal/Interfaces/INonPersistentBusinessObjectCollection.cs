using System.Data;
using System.Xml.Linq;

namespace CargoWise.EntityFramework
{
	internal interface INonPersistentBusinessObjectCollectionInternal : INonPersistentBusinessObjectCollection
	{
		void InitialiseCollectionForDeserialisation();
		NonPersistentBusinessObject GetItemToDeserialise(XElement element);
		DataTable GetNonPersistentTable(NonPersistentBusinessObject element);
	}

	public interface INonPersistentBusinessObjectCollection : IBusinessObjectCollection
	{
		void RemoveAndDeleteAll();
	}
}
