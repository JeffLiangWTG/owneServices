using System.Collections.Specialized;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuMenuPivotBaseCollection : BusinessObjectCollection<StmMenuMenuPivotBase>, INotifyCollectionChanged
	{
		public StmMenuMenuPivotBaseCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			IsManagedForDataRefresh = true;
		}

		public StmMenuMenuPivotBaseCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			IsManagedForDataRefresh = true;
		}

		protected override BusinessObject AddNewCore()
		{
			var addedElement = (StmMenuMenuPivotBase)base.AddNewCore();
			NotifyCollectionChanged(NotifyCollectionChangedAction.Add, addedElement);
			return addedElement;
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			base.Remove(elementToRemove);
			NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, elementToRemove);
		}

		public override void Load()
		{
			base.Load();
			EditingMode = EditingMode;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public MenuEditingMode EditingMode
		{
			get { return editingMode; }
			set
			{
				editingMode = value;

				foreach (StmMenuMenuPivotBase item in this)
				{
					item.EditingMode = value;
				}
			}
		}
		MenuEditingMode editingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;

		void NotifyCollectionChanged(NotifyCollectionChangedAction action, object changedItem)
		{
			if (CollectionChanged != null)
			{
				CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, changedItem));
			}
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;
	}
}
