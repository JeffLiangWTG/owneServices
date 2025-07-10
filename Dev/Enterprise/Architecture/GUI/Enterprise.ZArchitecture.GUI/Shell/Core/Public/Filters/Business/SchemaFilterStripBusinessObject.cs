using System.Collections;
using System.Collections.Generic;
#if NETFRAMEWORK
using System.Linq;
#endif
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class SchemaFilterStripBusinessObject : FilterStripBusinessObject
	{
		public SchemaFilterStripBusinessObject(ITableSchema tableSchema)
		{
			this.tableSchema = tableSchema;
			((IFilterStripBusinessObjectInternals)this).LayoutContext = tableSchema.TableName;
		}

		public SchemaFilterStripBusinessObject()
		{
		}

		readonly ITableSchema tableSchema;

		[BusinessObjectTestExclude]
		public Dictionary<string, IList> ColumnNamesToInclude { get; set; }

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			if (tableSchema != null)
			{
				FillModuleFilters(filters);
			}

			return filters;
		}

		public new ModuleFilterCollection GetModuleFilters() => GetModuleFiltersCore();

		void FillModuleFilters(ModuleFilterCollection filters)
		{
			var flagNames = new List<string>();
			var flagColumns = new List<SchemaBoolColumn>();

			foreach (var column in tableSchema.All)
			{
				if (ColumnNamesToInclude == null || ColumnNamesToInclude.Count == 0 || ColumnNamesToInclude.Keys.Contains(column.Name))
				{
					var headerText = column.Name;
					var caption = DataBoundResourceStrings.GetDataForProperty(null, column.Name);
					if (caption != null)
					{
						headerText = !string.IsNullOrEmpty(caption.Caption) ? caption.Caption : caption.FullDescription;
					}

					if (column is SchemaBoolColumn)
					{
						flagNames.Add(headerText);
						flagColumns.Add((SchemaBoolColumn)column);
					}
					else if (column is SchemaDateTimeColumn)
					{
						filters.AddDateFilter(column.Name, (SchemaDateTimeColumn)column).MultilingualDescription = (NoResString)headerText;
					}
					else if (column is SchemaNumericColumn)
					{
						filters.AddNumberRangeFilter(column.Name, (SchemaNumericColumn)column).MultilingualDescription = (NoResString)headerText;
					}
					else if (column is SchemaStringColumn)
					{
						var textFilterAdded = false;
						if (ColumnNamesToInclude != null && ColumnNamesToInclude.Keys.Contains(column.Name))
						{
							var associatedList = ColumnNamesToInclude[column.Name];
							if (associatedList != null)
							{
								AddTextFilterWithAssociatedList(filters, column, associatedList, headerText);
								textFilterAdded = true;
							}
						}

						if (!textFilterAdded)
						{
							filters.AddTextFilter(column.Name, (SchemaStringColumn)column).MultilingualDescription = (NoResString)headerText;
						}
					}
				}
			}

			if (flagNames.Count > 0)
			{
				filters.AddFlagsFilterWithFixedDescription(flagNames, flagColumns);
			}
		}

		protected internal virtual void AddTextFilterWithAssociatedList(ModuleFilterCollection filters, SchemaColumn column, IList associatedList, string headerText)
		{
			filters.AddTextFilter(column.Name, (SchemaStringColumn)column, associatedList).MultilingualDescription = (NoResString)headerText;
		}
	}
}
