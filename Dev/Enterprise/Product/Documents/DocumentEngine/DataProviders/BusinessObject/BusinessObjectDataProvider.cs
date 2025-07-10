using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.DataProviders
{
	class BusinessObjectDataProvider : IDataProvider
	{
		public BusinessObjectDataProvider()
		{
		}

		public BusinessObjectDataProvider(DataProviderList topLevelDataSource, VisualiserDataSet overridingDataSource)
			: this(Lazy.Create(() => topLevelDataSource, true), Lazy.Create(() => overridingDataSource, true))
		{
		}

		public BusinessObjectDataProvider(Lazy<DataProviderList> topLevelDataSource, Lazy<VisualiserDataSet> overridingDataSource)
		{
			this.topLevelDataSources = topLevelDataSource;
			this.overridingDataSource = overridingDataSource;
		}

		readonly Lazy<DataProviderList> topLevelDataSources;
		readonly Lazy<VisualiserDataSet> overridingDataSource;
		VisualiserDataSet OverridingDataSource => overridingDataSource?.Value;
		internal DataProviderList TopLevelDataSources => topLevelDataSources?.Value;

		public IDataRowSource GetDataRowSource(string tableIdentifier, bool isForDataSection, int maximumNumberOfRows)
		{
			IDataRowSource result = null;
			if (DoesColumnExist(tableIdentifier))
			{
				object fieldValue = GetColumnValue(null, 0, tableIdentifier);
				if (fieldValue == null)
				{
					result = new EmptyDataSource();
				}
				else
				{
					Type fieldType = fieldValue.GetType();
					if (typeof(IBusinessObjectCollection).IsAssignableFrom(fieldType))
					{
						string visualiserDataSetTableName = VisualiserDataSet.GetTableName(tableIdentifier);
						if (OverridingDataSource != null && OverridingDataSource.Tables.Contains(visualiserDataSetTableName))
						{
							result = new VisualiserDataSource(OverridingDataSource, visualiserDataSetTableName);
						}
						else
						{
							result = new BusinessObjectDataSource(tableIdentifier, (IBusinessObjectCollection)fieldValue);
						}
					}
					else if (fieldType == typeof(ZString[]))
					{
						if (OverridingDataSource != null && OverridingDataSource.Tables.Contains(VisualiserDataSet.GetTableName(tableIdentifier)))
						{
							DataTable table = OverridingDataSource.Tables[VisualiserDataSet.GetTableName(tableIdentifier)];
							if (table.Columns.Count == 0)
							{
								FieldNotFoundException.ReportFieldNotFound(tableIdentifier, TopLevelDataSources);
							}

							List<ZString> fieldValueFromRows = new List<ZString>();
							for (int row = 0; row < table.Rows.Count; row++)
							{
								fieldValueFromRows.Add(new ZString(table.Rows[row][0]));
							}
							result = new ZStringArrayDataSource(tableIdentifier, fieldValueFromRows.ToArray());
						}
						else
						{
							result = new ZStringArrayDataSource(tableIdentifier, (ZString[])fieldValue);
						}
					}
				}
			}
			else if (tableIdentifier.Equals(OneRowDataSource.TableIdentifier, StringComparison.OrdinalIgnoreCase))
			{
				result = new OneRowDataSource();
			}
			if (result == null)
			{
				FieldNotFoundException.ReportFieldNotFound(tableIdentifier, TopLevelDataSources);
			}
			return result;
		}

		public IDataRowSource GetDataRowSource(string tableIdentifier, bool isForDataSection)
		{
			return GetDataRowSource(tableIdentifier, isForDataSection, -1);
		}

		public IDataRowSource GetDataRowSource(string tableIdentifier)
		{
			return GetDataRowSource(tableIdentifier, false);
		}

		public bool DoesColumnExist(string columnName)
		{
			return FindColumn(columnName) != null;
		}

		public MethodInfoChainLink[] FindColumn(string columnName)
		{
			var (methodInfoChain, dataSource, _, _, _) = GetMethodInfoChainAndDataSource(columnName);

			if (dataSource is CompositeDataSourceFieldValueProvider composite)
			{
				return composite.GetNestedMethodChain();
			}

			return methodInfoChain;
		}

		(MethodInfoChainLink[] methodInfoChain, object dataSource, bool hasDataSourcePrefix, ZString dataSourcePrefixError, ZString fieldNameWithoutPrefix) GetMethodInfoChainAndDataSource(string columnName)
		{
			MethodInfoChainLink[] methodInfoChain = null;

			var hasDataSourcePrefix = MacroDataSource.HasDataSourcePrefix(columnName, out _, out var fieldNameWithoutPrefix);
			var dataSourcePrefixError = ZString.Empty;
			object specificDataSource = null;
			var providers = TopLevelDataSources.AllDataProviders;
			if (hasDataSourcePrefix)
			{
				var objects = providers.Select(x =>
				{
					var bo = BODocDataProvider.GetObject(x);
					return new { BO = bo, Type = bo.GetType() };
				}).ToArray();
				dataSourcePrefixError = MacroDataSource.GetDataSourceError(columnName, objects.Select(x => x.Type).ToArray(), out Type type, out fieldNameWithoutPrefix);
				if (dataSourcePrefixError.IsEmpty)
				{
					specificDataSource = objects.FirstOrDefault(x => x.Type == type)?.BO;
					methodInfoChain = new BusinessObjectReflector().GetMethodInfoChain(type, specificDataSource, fieldNameWithoutPrefix);
				}
			}
			else
			{
				var fieldValueSources = GetFieldValueSources(providers, columnName);

				if (fieldValueSources.Count > 0)
				{
					return (
						new[] { new MethodInfoChainLink(CompositeDataSourceFieldValueProvider.GetFieldValueMethod) },
						new CompositeDataSourceFieldValueProvider(fieldNameWithoutPrefix, fieldValueSources, GetFieldValueFromMethodInfoChain),
						hasDataSourcePrefix,
						dataSourcePrefixError,
						fieldNameWithoutPrefix);
				}
			}

			return (methodInfoChain, specificDataSource, hasDataSourcePrefix, dataSourcePrefixError, fieldNameWithoutPrefix);
		}

		List<(MethodInfoChainLink[] methodInfoChain, object dataSource)> GetFieldValueSources(IBODocDataProvider[] providers, string columnName)
		{
			var fieldValueSources = new List<(MethodInfoChainLink[] methodInfoChain, object dataSource)>();

			Exception firstExeptionThrown = null;

			foreach (var dataProvider in providers)
			{
				try
				{
					var dataSource = BODocDataProvider.GetObject(dataProvider);
					var currentMethodInfoChain = new BusinessObjectReflector().GetMethodInfoChain(dataSource.GetType(), dataSource, columnName);
					if (currentMethodInfoChain != null)
					{
						fieldValueSources.Add((currentMethodInfoChain, dataSource));
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (firstExeptionThrown == null)
					{
						firstExeptionThrown = ex;
					}
				}
			}

			if (!providers.IsNullOrEmpty() && firstExeptionThrown != null && fieldValueSources.IsNullOrEmpty())
			{
				throw firstExeptionThrown;
			}

			return fieldValueSources;
		}

		public object GetColumnValue(IDataRowSource bodySectionDataSource, int currentRowIndex, string fullFieldIdentifier, bool onlyForCurrentSource = false, Area area = null)
		{
			if (OverridingDataSource != null)
			{
				if (GetFieldValueFromOverridingDataSet(bodySectionDataSource as VisualiserDataSource, currentRowIndex, fullFieldIdentifier, out var result))
				{
					return result;
				}
			}

			var currentFieldIdentifier = fullFieldIdentifier;

			object currentDataSource;

			if (bodySectionDataSource != null)
			{
				if (bodySectionDataSource.RowCount > currentRowIndex && bodySectionDataSource is ZStringArrayDataSource zStringArrayDataSource)
				{
					return zStringArrayDataSource.Collection[currentRowIndex];
				}

				if (bodySectionDataSource is BusinessObjectDataSource bodySectionBoDataSource)
				{
					var sectionBodyArea = area as SectionBodyArea;
					var isFullyQualifiedMacro = currentFieldIdentifier.StartsWith(bodySectionBoDataSource.Name + ".", StringComparison.OrdinalIgnoreCase) && !currentFieldIdentifier.Contains("(") && sectionBodyArea != null;
					currentFieldIdentifier = bodySectionBoDataSource.RemoveCollectionNameFromGroupByColumnName(currentFieldIdentifier);
					if (currentFieldIdentifier != fullFieldIdentifier)
					{
						IList<BusinessObject> rowsAllowedInCurrentContext = bodySectionBoDataSource.GroupedOrFilteredCollection;
						if (rowsAllowedInCurrentContext.Count == 0 || rowsAllowedInCurrentContext.Count <= currentRowIndex)
						{
							return null;
						}
						if (currentRowIndex == -1)
						{
							if (sectionBodyArea.ParentReport.Renderer.IsProcessingAutoShapes && sectionBodyArea.DataRowSource.RowCount > 0)
							{
								throw new BoxOutOfSectionBodyException(fullFieldIdentifier);
							}

							sectionBodyArea?.ReportRowIndexIsMinusOneOrUnmatchedWithStartEndRow(fullFieldIdentifier);
						}
						currentDataSource = rowsAllowedInCurrentContext[currentRowIndex];
						if (currentDataSource == null)
						{
							return null;
						}

						var methodInfoChain = new BusinessObjectReflector().GetMethodInfoChain(currentDataSource.GetType(), currentDataSource, currentFieldIdentifier);
						if (methodInfoChain != null)
						{
							var result = GetFieldValueFromMethodInfoChainInternalWithExceptionHandling(currentFieldIdentifier, currentDataSource, methodInfoChain);
							if (onlyForCurrentSource && result == null)
							{
								return string.Empty;
							}
							return result;
						}

						if (isFullyQualifiedMacro && ShouldReportFieldNotFoundErrorsForFullyQualifiedMacros(fullFieldIdentifier))
						{
							sectionBodyArea.ParentReport.FieldNotFoundErrorsForFullyQualifiedMacros
								.GetOrAdd(bodySectionBoDataSource.Name, () => new HashSet<string>())
								.Add(new FieldNotFoundException(fullFieldIdentifier, currentDataSource).Message);
						}
					}
				}
			}

			if (onlyForCurrentSource)
			{
				return null;
			}

			var (newMethodInfoChain, specificDataSource, hasDataSourcePrefix, dataSourcePrefixError, fieldName) = GetMethodInfoChainAndDataSource(currentFieldIdentifier);

			if (newMethodInfoChain != null)
			{
				return GetFieldValueFromMethodInfoChainInternalWithExceptionHandling(fieldName, specificDataSource, newMethodInfoChain);
			}

			if (bodySectionDataSource != null)
			{
				return GetColumnValue(null, 0, fullFieldIdentifier);
			}

			if (!dataSourcePrefixError.IsEmpty)
			{
				FieldNotFoundException.ReportDataSourcePrefixError(dataSourcePrefixError);
			}
			else
			{
				if (hasDataSourcePrefix && specificDataSource != null)
				{
					FieldNotFoundException.ReportFieldNotFound(fieldName, specificDataSource);
				}
				else
				{
					FieldNotFoundException.ReportFieldNotFound(fullFieldIdentifier, TopLevelDataSources);
				}
			}
			return "";

			object GetFieldValueFromMethodInfoChainInternalWithExceptionHandling(string fieldIdentifier, object topLevelObject, MethodInfoChainLink[] methodInfoChain)
			{
				try
				{
					return GetFieldValueFromMethodInfoChain(fieldIdentifier, topLevelObject, methodInfoChain);
				}
				catch (IndexOutOfRangeException e)
				{
					if (area != null)
					{
						area.ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("97A95087-A397-4D69-9907-DC28600BA291", "Error getting value for field '{0}': {1}", fieldIdentifier.TrimStart('.'), e.Message), ReportProcessingErrorSeverity.Error, e));
						return string.Empty;
					}

					throw;
				}
			}
		}

		bool ShouldReportFieldNotFoundErrorsForFullyQualifiedMacros(string fullFieldIdentifier)
		{
			foreach (var dataProvider in TopLevelDataSources.AllDataProviders)
			{
				var dataSource = BODocDataProvider.GetObject(dataProvider);
				var methodInfoChain = new BusinessObjectReflector().GetMethodInfoChain(dataSource.GetType(), dataSource, fullFieldIdentifier);
				if (methodInfoChain != null)
				{
					return false;
				}
			}

			return true;
		}

		bool GetFieldValueFromOverridingDataSet(VisualiserDataSource bodyDataSource, int bodyRowIndex, string fieldIdentifier, out object result)
		{
			string fieldIdentifierWithoutDots = fieldIdentifier.Replace(".", "");
			if (OverridingDataSource.MainTable.Columns.Contains(fieldIdentifierWithoutDots))
			{
				result = OverridingDataSource.MainRow[fieldIdentifierWithoutDots];
				return true;
			}

			if (bodyDataSource != null)
			{
				string columnName = VisualiserDataSet.GetColumnName(fieldIdentifier, VisualiserDataSet.GetCollectionName(bodyDataSource.TableName));
				if (bodyRowIndex < bodyDataSource.RowCount && bodyDataSource.ColumnExists(columnName))
				{
					DataRow bodyDataRow = bodyDataSource.RowByIndex(bodyRowIndex);
					result = bodyDataRow[columnName];
					if (result != null && result.GetType() == typeof(bool))
					{
						result = new ZBool(result);
					}

					return true;
				}
			}

			result = null;
			return false;
		}

		internal object GetFieldValueFromMethodInfoChain(object topLevelObject, MethodInfoChainLink[] methodInfoChain)
		{
			return topLevelObject != null && methodInfoChain != null ?
				GetFieldValueFromMethodInfoChain(string.Empty, topLevelObject, methodInfoChain) : null;
		}

		object GetFieldValueFromMethodInfoChain(string fieldIdentifier, object topLevelObject, MethodInfoChainLink[] methodInfoChain)
		{
			object result = topLevelObject;
			foreach (MethodInfoChainLink methodInfoLink in methodInfoChain)
			{
				if (result == null || result is BusinessObject businessObject && businessObject.IsNull)
				{
					return null;
				}

				if (methodInfoLink.ShouldBeIgnoredWhenEvaluating)
				{
					return null;
				}

#if DEBUG
				ShowErrorIfPropertyIsObsolete(fieldIdentifier, methodInfoLink.MethodInfo);
#endif

				result = BODocDataProvider.GetObject(methodInfoLink.ReflectOutObject(result, TopLevelDataSources));
			}
			return result;
		}

#if DEBUG
		static void ShowErrorIfPropertyIsObsolete(string columnName, MethodInfo methodInfo)
		{
			object[] customAttributes = methodInfo.GetCustomAttributes(false);
			foreach (object attrib in customAttributes)
			{
				DocumentEngineObsoleteField obsoleteAttrib = attrib as DocumentEngineObsoleteField;
				if (obsoleteAttrib != null)
				{
					Globals.Message.ShowError("Column " + columnName + " is obsolete, " + obsoleteAttrib.MessageToShowToDeveloper, "Error");
				}
			}
		}
#endif
	}
}
