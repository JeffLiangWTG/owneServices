using System;
using System.Collections;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine
{
	public class ReportCollection : NonPersistentBusinessObjectCollection<NonPersistentBusinessObject>, IDeliverableCollection
	{
		public ReportCollection()
			: this(null) // Was: new BusinessObjectFactory() { NameForDebugging = "Report Collection" })
		{
		}

		public ReportCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new IDeliverable this[int index]
		{
			get { return (IDeliverable)Elements[index]; }
		}

		public new IDeliverable AddNew()
		{
			return (Report)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new Report(new DocumentPack(), null);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#region IList Methods

		public void Add(IDeliverable deliverableItem)
		{
			if (!(deliverableItem is BusinessObject))
			{
				throw new ApplicationException("Any object that implements IDeliverable must be a BusinessObject");
			}
			else
			{
				base.Add((BusinessObject)deliverableItem);
			}
		}

		public void Insert(int position, BusinessObject @object)
		{
			((IList)this).Insert(position, @object);
		}

		public int IndexOf(BusinessObject @object)
		{
			return Elements.IndexOf(@object);
		}

		#endregion
	}
}
