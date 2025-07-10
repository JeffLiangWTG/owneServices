using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuTemplatePivotBaseCollection : StmMenuTemplatePivotCollection, INotifyCollectionChanged
	{
		public StmMenuTemplatePivotBaseCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			IsManagedForDataRefresh = true;
		}

		public StmMenuTemplatePivotBaseCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			IsManagedForDataRefresh = true;
		}

		public new StmMenuTemplatePivotBase this[int index]
		{
			get { return (StmMenuTemplatePivotBase)Elements[index]; }
		}

		public new StmMenuTemplatePivotBase AddNew()
		{
			var newPivot = (StmMenuTemplatePivotBase)base.AddNew();
			newPivot.DocManagerFilter = DocManagerFilter;
			return newPivot;
		}

		protected override BusinessObject AddNewCore()
		{
			var addedElement = (StmMenuTemplatePivotBase)base.AddNewCore();
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
				foreach (StmMenuTemplatePivotBase item in this)
				{
					item.EditingMode = value;
				}
			}
		}
		MenuEditingMode editingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;

		#region DocManagerFilter

		public ZString DocManagerFilter
		{
			get { return docManagerFilter; }
			set
			{
				docManagerFilter = value;
				foreach (StmMenuTemplatePivotBase item in this)
				{
					item.DocManagerFilter = value;
				}
			}
		}
		ZString docManagerFilter;

		#endregion

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
