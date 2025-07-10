using System.Collections;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.UniversalCopy.GUI
{
	public class UCSchemaFilterStripBusinessObject : SchemaFilterStripBusinessObject
	{
		public UCSchemaFilterStripBusinessObject(ITableSchema tableSchema)
			: base(tableSchema)
		{
		}

		public UCSchemaFilterStripBusinessObject()
		{
		}

		protected internal override void AddTextFilterWithAssociatedList(ModuleFilterCollection filters, SchemaColumn column, IList associatedList, string headerText)
		{
			var moduleTextFilter = new UCModuleTextFilter(column.Name, (SchemaStringColumn)column, associatedList);
			moduleTextFilter.MultilingualDescription = (NoResString)headerText;
			filters.AddFilter(moduleTextFilter);
		}
	}
}
