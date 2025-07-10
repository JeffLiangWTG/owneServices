using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public class GridFilterStripBusinessObject : FilterStripBusinessObject, IFilterStripBusinessObjectInternals
	{
		public GridFilterStripBusinessObject(ZGrid grid)
			: base(grid?.NameForDebugging)
		{
			this.grid = grid;
		}

		public GridFilterStripBusinessObject()
		{
		}

		readonly ZGrid grid;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			if (grid != null)
			{
				var current = grid.ListManager != null ? grid.ListManager.GetCurrent() as BusinessObject : null;
				var undoCurrent = false;
				if (current == null && grid.List is IBusinessObjectCollection)
				{
					var elementType = BusinessObjectCollection.GetElementTypeFromCollectionType(grid.List.GetType());
					if (typeof(BusinessObject).IsAssignableFrom(elementType) && !typeof(NonPersistentBusinessObject).IsAssignableFrom(elementType))
					{
						CreateNewCurrent(elementType, ref current);
						undoCurrent = true;
					}
				}

				try
				{
					FillModuleFilters(filters, current);
				}
				finally
				{
					if (undoCurrent && current != null)
					{
						current.Delete();
					}
				}
			}

			return filters;
		}

		protected virtual void CreateNewCurrent(Type elementType, ref BusinessObject current)
		{
			try
			{
				current = Factory.New(elementType);
			}
			catch (NoConcreteTypeException)
			{
				if (grid.List is IHaveAbstractElementType)
				{
					current = ((IBusinessObjectCollection)grid.List).Factory.CreateNewFactory().New(((IHaveAbstractElementType)grid.List).NonAbstractTypeOfElements);
				}
				else
				{
					throw;
				}
			}
		}

		void FillModuleFilters(ModuleFilterCollection filters, BusinessObject current)
		{
			var flagNames = new List<string>();
			var flagColumns = new List<SchemaBoolColumn>();
			var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();

			var mainTableName = current?.TableName;
			var elementType = grid.List is IBusinessObjectCollection ? BusinessObjectCollection.GetElementTypeFromCollectionType(grid.List.GetType()) : null;

			foreach (ZGridColumnInfo columnInfo in grid.ColumnStyles)
			{
				if (columnInfo.IsSensitiveValue)
				{
					continue;
				}

				if (filters[columnInfo.ColumnName] == null)
				{
					SchemaColumn columnSchema = null;

					var keyCalculator = new ResourceStringKeyCalculator(grid, columnInfo.ColumnName);
					if (!string.IsNullOrEmpty(keyCalculator.FinalPropertyTableName))
					{
						columnSchema = schemaResolver.GetSchemaColumnSafe(keyCalculator.PropertyName, keyCalculator.FinalPropertyTableName);
					}

					if (columnSchema == null && columnInfo.ColumnName.IndexOfAny(new[] { '.', '+' }) <= 0)
					{
						Type propertyType = null;

						if (current != null)
						{
							var propertyInfo = current.ZPropertyInfoHash.GetPropertySafe(columnInfo.ColumnName);
							if (propertyInfo != null)
							{
								propertyType = propertyInfo.PropertyType;
							}
						}
						if (propertyType == null && elementType != null)
						{
							var property = elementType.GetProperties().FirstOrDefault(p => p.Name == columnInfo.ColumnName);
							if (property != null && typeof(IZType).IsAssignableFrom(property.PropertyType))
							{
								propertyType = property.PropertyType;
							}
						}

						if (propertyType != null)
						{
							columnSchema = GetDummySchemaColumnForProperty(columnInfo, propertyType);
						}
					}

					if (columnSchema != null && (columnSchema.TableName == mainTableName || string.IsNullOrEmpty(columnSchema.TableName) || string.IsNullOrEmpty(mainTableName)))
					{
						var moduleFilter = GetModuleFilter(current, columnInfo, columnSchema, flagNames, flagColumns);
						if (moduleFilter != null)
						{
							filters.AddFilter(moduleFilter);
						}
					}
				}
			}

			if (flagNames.Count > 0)
			{
				filters.AddFlagsFilterWithFixedDescription(flagNames, flagColumns);
			}
		}

		ModuleFilter GetModuleFilter(BusinessObject current, ZGridColumnInfo columnInfo, SchemaColumn columnSchema, ICollection<string> flagNames, ICollection<SchemaBoolColumn> flagColumns)
		{
			var columnStyle =
				(
					from column in grid.Columns
					where column.ColumnStyle.MappingName == columnInfo.ColumnName
					select (ZGridColumnStyle)column.ColumnStyle
				).FirstOrDefault();

			if (grid.List.Count == 0 && current is StmNote currentNote && currentNote.Master == null)
			{
					currentNote.Master = grid.DataSource as IStmNoteParent;
			}

			var headerText = GetHeaderText(columnInfo, columnStyle, new ResourceStringKeyCalculator(grid, columnInfo.ColumnName));

			if (string.IsNullOrWhiteSpace(headerText))
			{
				return null;
			}

			return GetModuleFilterCore(current, columnInfo, columnStyle, columnSchema, flagNames, flagColumns, headerText);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		protected virtual ModuleFilter GetModuleFilterCore(BusinessObject current, ZGridColumnInfo columnInfo, ZGridColumnStyle columnStyle, SchemaColumn columnSchema, ICollection<string> flagNames, ICollection<SchemaBoolColumn> flagColumns, string headerText)
		{
			if (columnInfo is ICustomModuleFilterProvider moduleFilterProvider)
			{
				return moduleFilterProvider.GetModuleFilter(columnSchema, headerText);
			}

			if (columnInfo is ZCodeFindBoxColumnStyleInfo codeFindBoxColumnInfo)
			{
				var collection = GetCollectionForColumnInfo(codeFindBoxColumnInfo, columnStyle as ZCodeFindBoxColumnStyle, current);
				if (collection != null)
				{
					var moduleID = GetModuleIdentifier(codeFindBoxColumnInfo, collection);
					if (columnSchema is SchemaGuidColumn)
					{
						return new ModuleGuidFilter(columnInfo.ColumnName, moduleID, (SchemaGuidColumn)columnSchema, collection) { MultilingualDescription = (NoResString)headerText };
					}

					if (columnSchema is SchemaStringColumn)
					{
						return new ModuleNkFilter(columnInfo.ColumnName, (SchemaStringColumn)columnSchema, moduleID, collection) { MultilingualDescription = (NoResString)headerText };
					}
				}
			}

			if (columnInfo is ZDropEditColumnStyleInfo dropEditColumnStyleInfo)
			{
				var list = GetCollectionForColumnInfo(dropEditColumnStyleInfo, columnStyle, current);
				if (list != null && columnSchema is SchemaStringColumn)
				{
					return new ModuleTextFilter(columnInfo.ColumnName, (SchemaStringColumn)columnSchema, list) { MultilingualDescription = (NoResString)headerText };
				}
			}

			if (columnSchema is SchemaDateTimeColumn dateTimeColumn && !(columnInfo is ZTimeEditExColumnStyleInfo))
			{
				return new ModuleDateFilter(columnInfo.ColumnName, dateTimeColumn) { MultilingualDescription = (NoResString)headerText };
			}

			if (columnSchema is SchemaDateTimeOffsetColumn dateTimeOffsetColumn && !(columnInfo is ZTimeEditExColumnStyleInfo))
			{
				return new ModuleDateTimeOffsetFilter(columnInfo.ColumnName, dateTimeOffsetColumn) { MultilingualDescription = (NoResString)headerText };
			}

			if (columnSchema is SchemaNumericColumn numericColumn)
			{
				return new ModuleNumberRangeFilter(columnInfo.ColumnName, numericColumn) { MultilingualDescription = (NoResString)headerText };
			}

			if (columnSchema is SchemaBoolColumn boolColumn)
			{
				flagNames.Add(headerText);
				flagColumns.Add(boolColumn);
				return null;
			}

			if (columnSchema is SchemaStringColumn stringColumn)
			{
				return new ModuleTextFilter(columnInfo.ColumnName, stringColumn) { MultilingualDescription = (NoResString)headerText };
			}

			return null;
		}

		static string GetHeaderText(ZGridColumnInfo columnInfo, ZGridColumnStyle columnStyle, ResourceStringKeyCalculator keyCalculator)
		{
			string headerText = null;

			if (columnInfo.CaptionResourceString != null && !columnInfo.CaptionResourceString.IsEmpty())
			{
				headerText = columnInfo.CaptionResourceString.Caption;
				if (string.IsNullOrEmpty(headerText))
				{
					headerText = columnInfo.CaptionResourceString.FullDescription;
				}
			}
			else
			{
				var data = keyCalculator.DataString;
				if (data != null && !data.IsEmpty())
				{
					headerText = data.Caption;
					if (string.IsNullOrEmpty(headerText))
					{
						headerText = data.FullDescription;
					}
				}
				if (string.IsNullOrEmpty(headerText) && columnStyle != null)
				{
					headerText = columnStyle.HeaderText;
				}
				if (string.IsNullOrEmpty(headerText))
				{
					headerText = columnInfo.ColumnName;
				}
			}

			return headerText;
		}

		SchemaColumn GetDummySchemaColumnForProperty(ZGridColumnInfo columnInfo, Type propertyType)
		{
			if (propertyType == typeof(ZDecimal))
			{
				return new SchemaDecimalColumn(CargoWise.Schema.Schema.GenericTableSchema, columnInfo.ColumnName, 0, SqlDbType.Money, (decimal)0, false, 19, 4);
			}
			if (propertyType == typeof(ZInt) || propertyType == typeof(ZShort) || propertyType == typeof(ZByte))
			{
				return new SchemaIntColumn(CargoWise.Schema.Schema.GenericTableSchema, columnInfo.ColumnName, 0, 0, false);
			}
			if (propertyType == typeof(ZBool))
			{
				return new SchemaBoolColumn(CargoWise.Schema.Schema.GenericTableSchema, columnInfo.ColumnName, 0, false, false, false);
			}
			if (propertyType == typeof(ZGuid))
			{
				return new SchemaGuidColumn(CargoWise.Schema.Schema.GenericTableSchema, columnInfo.ColumnName, 0, Guid.Empty, false);
			}
			if (propertyType == typeof(ZDateTime) || propertyType == typeof(ZDate))
			{
				return new SchemaDateTimeColumn(CargoWise.Schema.Schema.GenericTableSchema, columnInfo.ColumnName, 0, SqlDbType.DateTime, ZDateTime.Now, false);
			}
			if (propertyType == typeof(ZDateTimeOffset))
			{
				return new SchemaDateTimeOffsetColumn(CargoWise.Schema.Schema.GenericTableSchema, columnInfo.ColumnName, 0, SqlDbType.DateTimeOffset, ZDateTimeOffset.Now, false, 4);
			}

			return new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, columnInfo.ColumnName, 0, SqlDbType.NVarChar, "", false, 0);
		}

		static IBusinessObjectCollection GetCollectionForColumnInfo(ZBaseFindBoxColumnStyleInfo columnInfo, ZBaseFindBoxColumnStyle columnStyle, object current)
		{
			return
				columnStyle != null && columnStyle.FindBox != null && columnStyle.FindBox.ListProvider != null && columnStyle.FindBox.ListProvider.List != null
					? columnStyle.FindBox.ListProvider.List
					: GetLastObjectFromPath(current, columnInfo.BindToList, columnStyle) as IBusinessObjectCollection;
		}

		static IList GetCollectionForColumnInfo(ZDropEditColumnStyleInfo columnInfo, ZGridColumnStyle columnStyle, object current)
		{
			return GetLastObjectFromPath(current, columnInfo.BindToList, columnStyle) as IList;
		}

		static object GetLastObjectFromPath(object root, string bindToList, ZGridColumnStyle columnStyle)
		{
			object currentObject = null;

			var path = bindToList;
			if (string.IsNullOrEmpty(path) && columnStyle != null)
			{
				path = columnStyle.PropertyDescriptor != null ? MetadataAccessor.GetListMember("", columnStyle.PropertyDescriptor, columnStyle.MappingName) : null;
			}

			if (!string.IsNullOrEmpty(path))
			{
				var properties = path.Split('.', '+');
				currentObject = root;
				for (var i = 0; i < properties.Length && currentObject != null; i++)
				{
					var info = GetPropertyFromType(currentObject.GetType(), properties[i]);
					currentObject = info?.GetValue(currentObject, BindingFlags.GetProperty, null, null, CultureInfo.CurrentCulture);
				}
			}

			return currentObject;
		}

		static PropertyInfo GetPropertyFromType(Type type, string propertyName)
		{
			PropertyInfo info = null;

			while (info == null && type != null)
			{
				info = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				type = type.BaseType;
			}

			return info;
		}

		static ModuleIdentifier GetModuleIdentifier(IZColumnStyleInfoWithModuleID columnInfo, object collection)
		{
			var moduleID = columnInfo.ModuleID;
			if (moduleID == ModuleIDs.NotAssigned)
			{
				var attributes = (ModuleIDAttribute[])collection.GetType().GetCustomAttributes(typeof(ModuleIDAttribute), true);
				if (attributes.Length > 0)
				{
					moduleID = attributes[0].ModuleIdentifier;
				}
			}
			return moduleID;
		}

		ZString IFilterStripBusinessObjectInternals.LayoutContext
		{
			get
			{
				if (layoutContext.IsEmpty)
				{
					layoutContext = grid.LayoutKey + "|" + grid.DataMember + "|" + (grid.DataSource != null ? grid.DataSource.GetType().Name : "");
				}
				return layoutContext;
			}
			set { layoutContext = value; }
		}
		ZString layoutContext;
	}
}
