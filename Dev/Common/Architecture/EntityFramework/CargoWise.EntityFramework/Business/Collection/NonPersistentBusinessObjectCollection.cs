using System.Data;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Business.Internal;

namespace CargoWise.EntityFramework
{
	public abstract class NonPersistentBusinessObjectCollection<T> : BusinessObjectCollection<T>, INonPersistentBusinessObjectCollectionInternal where T : NonPersistentBusinessObject
	{
		protected NonPersistentBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected NonPersistentBusinessObjectCollection()
			: this(null)
		{
		}

		public override void Load(ZQuery filter)
		{
			ErrorReporter.ReportOnce("NonPersistentNoLoad" + GetType().FullName, "Cannot Load() on a NonPersistentBusinessObjectCollection");
		}

		internal override BusinessObject CreateNewBusinessObject()
		{
			BusinessObject newElement = CreateNonPersistentBusinessObject();
			if (newElement != null)
			{ HookupElementToCollection(newElement); }

			return newElement;
		}

		internal override void FireListResetEventInternal()
		{
			base.FireListResetEventInternal();
			//fix generic problems of the type 'a grid is backed by an NPBO, we add a bunch of elements to the NPBO but then don't re-sort afterwards'
			//(e.g. preview tab in Import Wizard, tasks grid of Containment Barrier Outcome)
			//I think most persistent collections are fine since they're loaded in sane ways (e.g. BusinessObjectCollection.Load)
			//prevent re-entrancy though (because when sorting finishes in BusinessObjectCollectionSorter we call FireListResetEvent)
			if (Sorter.IsSorted && !Sorter.IsSorting)
			{
				Sorter.Resort();
				OnAfterResort();
			}
		}

		protected abstract BusinessObject CreateNonPersistentBusinessObject();

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new NonPersistentBusinessObjectFindBoxListProvider(this); }
		}

		NonPersistentBusinessObject INonPersistentBusinessObjectCollectionInternal.GetItemToDeserialise(XElement element) => GetItemToDeserialise(element);

		protected virtual NonPersistentBusinessObject GetItemToDeserialise(XElement element)
		{
			return null;
		}

		void INonPersistentBusinessObjectCollectionInternal.InitialiseCollectionForDeserialisation() => InitialiseCollectionForDeserialisation();

		protected virtual void InitialiseCollectionForDeserialisation()
		{
		}

		#region DataTable

		DataTable INonPersistentBusinessObjectCollectionInternal.GetNonPersistentTable(NonPersistentBusinessObject element)
		{
			if (GetElementTypeFromCollectionType(GetType()).IsInstanceOfType(element))
			{
				return nonPersistentTable ?? (nonPersistentTable = element.CreateNewNonPersistentTable());
			}
			return null;
		}
		DataTable nonPersistentTable;

		#endregion
	}
}
