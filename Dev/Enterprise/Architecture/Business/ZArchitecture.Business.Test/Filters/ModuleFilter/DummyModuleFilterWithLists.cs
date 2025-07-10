using System.Collections;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyModuleFilterWithLists : ModuleFilterWithLists
	{
		protected DummyModuleFilterWithLists(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public DummyModuleFilterWithLists(ZString description, SchemaColumn filterColumn1, IList list1, SchemaColumn filterColumn2, IList list2)
			: base(description, filterColumn1, list1, filterColumn2, list2)
		{
		}

		#region GetNewCommonModuleFilter, CopyPropertiesToFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new DummyModuleFilterWithLists(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
		}

		#endregion

		#region DefaultCategory

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		#endregion

		public override bool IsExpensiveQuery => false;

		protected override bool IsEmptyCore => true;

		protected override void ClearCore()
		{
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new DummyModuleFilterValidation(this);
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			return new ZQuery();
		}

		protected override object[] QueryDelegateParameters
		{
			get { return System.Array.Empty<object>(); }
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
	}
}
