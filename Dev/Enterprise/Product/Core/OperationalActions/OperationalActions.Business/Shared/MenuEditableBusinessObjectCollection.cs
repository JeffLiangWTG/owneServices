using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;

namespace Enterprise.Services.OperationalActions.Business
{
	public abstract class MenuEditableBusinessObjectCollection<T> : BusinessObjectCollection<T>, IMenuEditable where T : BusinessObject, IMenuEditable
	{
		MenuEditingMode editingMode;

		protected MenuEditableBusinessObjectCollection(BusinessObjectFactory factory)
			: this(factory, null)
		{
		}

		protected MenuEditableBusinessObjectCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
		}

		public MenuEditingMode EditingMode
		{
			get { return editingMode; }
			set
			{
				editingMode = value;
				foreach (T child in this)
				{
					child.EditingMode = value;
				}
			}
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			SetUpChild((T)child);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			SetUpChild((T)child);
		}

		protected virtual void SetUpChild(T child)
		{
			child.EditingMode = EditingMode;
		}
	}
}
