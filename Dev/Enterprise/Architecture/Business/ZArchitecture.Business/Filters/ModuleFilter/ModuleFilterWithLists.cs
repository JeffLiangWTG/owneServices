using System;
using System.Collections;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	[SuppressWeaklyTypedCollectionMessage]
	public delegate IList GetList();

	[SuppressWeaklyTypedCollectionMessage]
	public delegate IList GetListUsingCurrentModuleFilter(ModuleFilterWithList currentModuleFilter);

	public abstract class ModuleFilterWithList : ModuleFilter
	{
		#region Construction

		protected ModuleFilterWithList(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleFilterWithList(ZString description, SchemaColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public ModuleFilterWithList(ZString description, SchemaColumn filterColumn, ComparisonOptions options)
			: base(description, filterColumn, options)
		{
		}

		public ModuleFilterWithList(ZString description, Delegate queryDelegate)
			: base(description, queryDelegate)
		{
		}

		public ModuleFilterWithList(ZString description, SchemaColumn filterColumn, IList list)
			: this(description, filterColumn)
		{
			EnsureListIsNotNull(list);
			fList = list;
		}

		public ModuleFilterWithList(ZString description, Delegate queryDelegate, IList list)
			: this(description, queryDelegate)
		{
			EnsureListIsNotNull(list);
			fList = list;
		}

		public ModuleFilterWithList(ZString description, SchemaColumn filterColumn, GetList listDelegate)
			: base(description, filterColumn)
		{
			EnsureListDelegateIsNotNull(listDelegate);
			ListDelegate = listDelegate;
		}

		public ModuleFilterWithList(ZString description, Delegate queryDelegate, GetList listDelegate)
			: base(description, queryDelegate)
		{
			EnsureListDelegateIsNotNull(listDelegate);
			ListDelegate = listDelegate;
		}

		public ModuleFilterWithList(ZString description, SchemaColumn filterColumn, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate)
			: base(description, filterColumn)
		{
			EnsureListDelegateIsNotNull(listUsingCurrentModuleFilterDelegate);
			ListUsingCurrentModuleFilterDelegate = listUsingCurrentModuleFilterDelegate;
		}

		public ModuleFilterWithList(ZString description, Delegate queryDelegate, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate)
			: base(description, queryDelegate)
		{
			EnsureListDelegateIsNotNull(listUsingCurrentModuleFilterDelegate);
			ListUsingCurrentModuleFilterDelegate = listUsingCurrentModuleFilterDelegate;
		}

		#endregion

		#region List

		[SuppressWeaklyTypedCollectionMessage]
		public IList List
		{
			get
			{
				IList result = null;

				if (fList != null)
				{
					result = fList;
				}
				else if (ListDelegate != null)
				{
					result = ListDelegate.Invoke();
				}
				else if (ListUsingCurrentModuleFilterDelegate != null)
				{
					result = ListUsingCurrentModuleFilterDelegate.Invoke(this);
				}

				return result;
			}
		}

		readonly IList fList;
		readonly GetList ListDelegate;
		readonly GetListUsingCurrentModuleFilter ListUsingCurrentModuleFilterDelegate;

		#endregion

		public bool ShowDescription
		{
			get { return showDescription; }
			set { showDescription = value; }
		}
		bool showDescription = true;
	}

	public abstract class ModuleFilterWithLists : ModuleFilterWithSubDescriptions
	{
		#region Construction

		protected ModuleFilterWithLists(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleFilterWithLists(ZString description, SchemaColumn filterColumn1, IList list1, SchemaColumn filterColumn2, IList list2)
			: base(description)
		{
			EnsureFilterColumnIsNotNull(filterColumn1, filterColumn2);
			EnsureListIsNotNull(list1, list2);

			FilterColumn1 = filterColumn1;
			FilterColumn2 = filterColumn2;
			fList1 = list1;
			fList2 = list2;
		}

		public ModuleFilterWithLists(ZString description, Delegate queryDelegate, IList list1, IList list2)
			: base(description, queryDelegate)
		{
			EnsureListIsNotNull(list1, list2);

			fList1 = list1;
			fList2 = list2;
		}

		public ModuleFilterWithLists(ZString description, SchemaColumn filterColumn1, GetList list1Delegate, SchemaColumn filterColumn2, GetList list2Delegate)
			: base(description)
		{
			EnsureFilterColumnIsNotNull(filterColumn1, filterColumn2);
			EnsureListDelegateIsNotNull(list1Delegate, list2Delegate);

			FilterColumn1 = filterColumn1;
			FilterColumn2 = filterColumn2;
			List1Delegate = list1Delegate;
			List2Delegate = list2Delegate;
		}

		public ModuleFilterWithLists(ZString description, Delegate queryDelegate, GetList list1Delegate, GetList list2Delegate)
			: base(description, queryDelegate)
		{
			EnsureListDelegateIsNotNull(list1Delegate, list2Delegate);

			List1Delegate = list1Delegate;
			List2Delegate = list2Delegate;
		}

		public readonly SchemaColumn FilterColumn1;
		public readonly SchemaColumn FilterColumn2;

		#endregion

		#region Lists

		[SuppressWeaklyTypedCollectionMessage]
		public IList List1
		{
			get
			{
				IList result = null;

				if (fList1 != null)
				{
					result = fList1;
				}
				else if (List1Delegate != null)
				{
					result = List1Delegate.Invoke();
				}

				return result;
			}
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList List2
		{
			get
			{
				IList result = null;

				if (fList2 != null)
				{
					result = fList2;
				}
				else if (List2Delegate != null)
				{
					result = List2Delegate.Invoke();
				}

				return result;
			}
		}

		readonly IList fList1;
		readonly IList fList2;
		readonly GetList List1Delegate;
		readonly GetList List2Delegate;

		#endregion
	}
}
