using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	abstract class MenuEditableBusinessObjectCollectionTestCase<T> : BusinessObjectCollectionTestCase where T : BusinessObjectCollection, IMenuEditable
	{
		protected new T Collection
		{
			get
			{
				return (T)base.Collection;
			}
		}

		public void TestEditingMode()
		{
			AssertEquals("EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, Collection.EditingMode);
			Collection.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("EditingMode", MenuEditingMode.AllowEditingOfClientSpecificOnly, Collection.EditingMode);
			BusinessObject child1 = Collection.AddNew();
			IMenuEditable child1MenuEditable = (IMenuEditable)child1;
			AssertEquals("child1.EditingMode", MenuEditingMode.AllowEditingOfClientSpecificOnly, child1MenuEditable.EditingMode);
			Collection.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("EditingMode", MenuEditingMode.AllowEditingOfSystemDefinedOnly, Collection.EditingMode);
			AssertEquals("child1.EditingMode", MenuEditingMode.AllowEditingOfSystemDefinedOnly, child1MenuEditable.EditingMode);
			BusinessObject child2 = Collection.AddNew();
			IMenuEditable child2MenuEditable = (IMenuEditable)child2;
			AssertEquals("child2.EditingMode", MenuEditingMode.AllowEditingOfSystemDefinedOnly, child2MenuEditable.EditingMode);
			Collection.RemoveAll();
			Collection.EditingMode = MenuEditingMode.AllowAll;
			Collection.Load();
			AssertEquals("Contains(child1)", true, Collection.Contains(child1));
			AssertEquals("Contains(child2)", true, Collection.Contains(child2));
			AssertEquals("child1.EditingMode", MenuEditingMode.AllowAll, child1MenuEditable.EditingMode);
			AssertEquals("child2.EditingMode", MenuEditingMode.AllowAll, child2MenuEditable.EditingMode);
		}
	}
}
