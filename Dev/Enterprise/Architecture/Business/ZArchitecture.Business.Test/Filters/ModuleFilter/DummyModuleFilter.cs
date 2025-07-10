using System;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyModuleFilter : ModuleFilter
	{
		protected DummyModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public DummyModuleFilter(ZString description, SchemaColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public DummyModuleFilter(ZString description, Delegate queryDelegate)
			: base(description, queryDelegate)
		{
		}

		#region GetNewCommonModuleFilter, CopyPropertiesToFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new DummyModuleFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var dummyFilterToCopyFrom = (DummyModuleFilter)filterToCopyFrom;
			DummyProperty = dummyFilterToCopyFrom.DummyProperty;
		}

		#endregion

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		protected override void ClearCore()
		{
		}

		protected override bool IsEmptyCore => DummyProperty.IsEmpty;

		public ZString DummyProperty
		{
			get { return fDummyProperty; }
			set
			{
				if (DummyProperty != value)
				{
					InvalidateCachedQuery();
				}

				fDummyProperty = value;
			}
		}

		ZString fDummyProperty;

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new DummyModuleFilterValidation(this);
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { DummyProperty }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			return new ZQuery(FilterColumn, DummyProperty);
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
		}

		protected override void FillWithValidTestFilterValueCore()
		{
		}

		#region Overrides of ModuleFilter

		protected override ZQuery GetQuery()
		{
			GetQueryExecutionCount++;

			return base.GetQuery();
		}

		public int GetQueryExecutionCount { get; set; }

		#endregion
	}
}
