using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public class AccGLHeaderRangeFilter : ModuleCodeFilter
	{
		#region Construction

		protected AccGLHeaderRangeFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public AccGLHeaderRangeFilter(ZString description, SchemaStringColumn account1FilterColumn, BusinessObjectCollection account1List, SchemaStringColumn account2FilterColumn, BusinessObjectCollection account2List)
			: base(description, account1FilterColumn, account1List, account2FilterColumn, account2List)
		{
			EnsureListsAreGLHeaderCollections(account1List, account2List);
		}

		public AccGLHeaderRangeFilter(ZString description, GetCodeQuery queryDelegate, BusinessObjectCollection account1List, BusinessObjectCollection account2List)
			: base(description, queryDelegate, account1List, account2List)
		{
			EnsureListsAreGLHeaderCollections(account1List, account2List);
		}

		void EnsureListsAreGLHeaderCollections(BusinessObjectCollection account1List, BusinessObjectCollection account2List)
		{
			if (!typeof(AccGLHeaderCollection).IsAssignableFrom(account1List.GetType()))
			{
				throw new ArgumentException(GetType().Name + ".List1 is not a AccGLHeaderCollection.");
			}

			if (!typeof(AccGLHeaderCollection).IsAssignableFrom(account2List.GetType()))
			{
				throw new ArgumentException(GetType().Name + ".List2 is not a AccGLHeaderCollection.");
			}
		}

		#endregion

		#region GetNewCommonModuleFilter

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new AccGLHeaderRangeFilter(category, parentCollection);
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.NumbersAndReferences; }
		}

		#endregion

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			ZQuery result = new ZQuery();

			if (!Property1.IsEmpty)
			{
				result.AddToFilter(FilterColumn1, SQLComparisonOperator.GreaterThanOrEqualTo, Property1);
			}

			if (!Property2.IsEmpty)
			{
				result.AddToFilter(FilterColumn2, SQLComparisonOperator.LessThanOrEqualTo, Property2);
			}

			return result;
		}

		[List("List1")]
		public override ZString Property1
		{
			get
			{
				return base.Property1;
			}
			set
			{
				base.Property1 = value;
			}
		}

		[List("List2")]
		public override ZString Property2
		{
			get
			{
				return base.Property2;
			}
			set
			{
				base.Property2 = value;
			}
		}
	}
}
