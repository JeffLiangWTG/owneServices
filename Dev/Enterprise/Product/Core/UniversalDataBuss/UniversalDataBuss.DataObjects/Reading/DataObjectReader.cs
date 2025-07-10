using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public abstract class DataObjectReader<TDataObject, TBusinessObject> : DataObjectWithWorkflowCustomFieldsReader<TDataObject>
		where TDataObject : IDataObject
		where TBusinessObject : BusinessObject
	{
		protected DataObjectReader(TDataObject dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		public TBusinessObject ReadIntoBusinessObject()
		{
			TBusinessObject targetBO = null;
			ReadIntoBusinessObjectCore(ref targetBO);
			return targetBO;
		}

		protected virtual string GetBusinessObjectHumanReadableName(TBusinessObject businessObject) => businessObject == null ? typeof(TBusinessObject).Name : businessObject.GetType().Name;

		internal void ReadIntoBusinessObjectCore(ref TBusinessObject targetBO)
		{
			if (targetBO == null)
			{
				targetBO = GetExistingBusinessObject();
				if (targetBO is ICancellable cancellable && cancellable.IsCancelled && ShouldIgnoreInactiveTargetForMatching)
				{
					targetBO = null;
				}
			}

			var typeName = GetBusinessObjectHumanReadableName(null);
			IsNewBO = false;
			if (targetBO == null)
			{
				IsNewBO = true;
			}
			else
			{
				typeName = GetBusinessObjectHumanReadableName(targetBO);
				LogSuccessfullyLoadedMessage(typeName);
			}

			var reason = GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
			if (reason.IsEmpty)
			{
				if (IsNewBO || ShouldUpdateBO)
				{
					if (IsNewBO)
					{
						targetBO = GetNewBusinessObject();
						typeName = GetBusinessObjectHumanReadableName(targetBO);
						logger.Log(LogType.Information, Res.GetString("c1f945a7-1d18-4cf2-bf33-91af09b377dg", "No matching {0} found, creating new {0}.", typeName));
					}
					PopulateBusinessObjectAndFinaliseImport(targetBO, typeName);
				}
				else
				{
					logger.LogBoth(LogType.Information, Res.GetString("94405C23-E068-4E5A-8FC1-33F3766A8DD9", "{0} wasn't updated because of registry settings '{1}'.", targetBO.HumanReadableName, eAdaptorRegistry.Categories.eServices_UniversalXML_AutomaticUpdateonImport.Replace("/", "->")));
				}
			}
			else
			{
				logger.Log(LogTypeForReasonNotAbleToUpdate, Res.GetString("9F58838D-F0A9-4685-BA1A-680C608F1BEA", "Cannot populate {0} because:{1}{2}", typeName, System.Environment.NewLine, reason));
			}
		}

		internal void ReadDirectlyIntoBusinessObjectOverrideAllChecks(TBusinessObject targetBizo)
		{
			var typeName = GetBusinessObjectHumanReadableName(targetBizo);
			PopulateBusinessObjectAndFinaliseImport(targetBizo, typeName);
		}

		protected virtual LogType LogTypeForReasonNotAbleToUpdate
		{
			get { return LogType.Error; }
		}

		protected bool IsNewBO { get; private set; }

		/// <summary>
		/// Allows a Reader to stop the creation or updating of an entity based on information in the incoming data or the target BO. (where one already exists)
		/// </summary>
		/// <param name="targetBO">The Target BO where an update is being performed. Will be null where a row is being added.</param>
		/// <returns></returns>
		protected virtual ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(TBusinessObject targetBO)
		{
			return ZString.Empty;
		}

		protected virtual bool ShouldUpdateBO
		{
			get { return true; }
		}

		protected virtual TBusinessObject GetNewBusinessObject()
		{
			return factory.New<TBusinessObject>();
		}

		void PopulateBusinessObjectAndFinaliseImport(TBusinessObject targetBO, string typeName)
		{
			LogPopulatingMessage(typeName);

			var uxmlDataImporting = SupportUXMLDataImportingHelper.UXMLDataImporting(targetBO);
			factory.CleanupAfterSaving += (a, b) =>
			{
				uxmlDataImporting?.Dispose();
				uxmlDataImporting = null;
			};

			PopulateBusinessObject(targetBO);
			PopulateAttachedDocuments(targetBO);
			PopulateFromTopLevelObject(targetBO);

			EvaluateValidationRules(targetBO);
			FinaliseImport(targetBO);
		}

		internal virtual void FinaliseImport(TBusinessObject targetBO)
		{
			factory.RecordEndOfEveryRead(targetBO);
		}

		protected abstract TBusinessObject GetExistingBusinessObject();

		protected abstract void PopulateBusinessObject(TBusinessObject targetBO);

		protected internal virtual void PopulateFromTopLevelObject(TBusinessObject targetBO)
		{
		}

		protected virtual void LogSuccessfullyLoadedMessage(string typeName)
		{
			logger.Log(LogType.Information, Res.GetString("948449ee-97ea-4933-b3c8-472db8001eb6", "Successfully loaded matching {0}.", typeName));
		}

		protected virtual void LogPopulatingMessage(string typeName)
		{
			logger.Log(LogType.Information, Res.GetString("7260bf87-fedc-45e3-96a8-23336aaff7cd", "Populating {0}...", typeName));
		}

		void PopulateAttachedDocuments(TBusinessObject businessObject)
		{
			if (dataObject is ITopLevelDataObject topLevelDataObject)
			{
				ObjectFactory.Get<IUniversalXmlContentFilterApplicator>().ImportAttachedDocuments(businessObject, topLevelDataObject, logger);
			}
		}

		void EvaluateValidationRules(TBusinessObject targetBO)
		{
			ObjectFactory.Get<IUniversalValidationRulesEvaluator>().EvaluateRules(dataObject, targetBO, logger);
		}
	}

	public abstract class DataObjectWithWorkflowCustomFieldsReader<T> : DataObjectReader<T>
		where T : IDataObject
	{
		protected DataObjectWithWorkflowCustomFieldsReader(T dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		protected void PopulateWorkflowCustomFields(IWorkflowProviderCore workflowProvider, ICustomizedFieldContainer customizedFieldsContainer)
		{
			PopulateWorkflowCustomFields(workflowProvider, customizedFieldsContainer, null);
		}

		protected void PopulateWorkflowCustomFields(IWorkflowProviderCore workflowProvider, ICustomizedFieldContainer customizedFieldsContainer, (string, DataType?)[] usedCustomFields)
		{
			var usedCustomFieldsCollection = usedCustomFields?.ToList() ?? new List<(string, DataType?)>();

			var customizedFields = customizedFieldsContainer.CustomizedFieldCollection;
			if (customizedFields != null)
			{
				foreach (var customFieldDataObject in customizedFields)
				{
					if (customFieldDataObject.Key.HasValue && !customFieldDataObject.Key.Value.IsDefault && customFieldDataObject.DataType.HasValue)
					{
						var value = GetValue(customFieldDataObject);
						if (value != null)
						{
							string name = customFieldDataObject.Key.Value.ToString();

							if (!usedCustomFieldsCollection.Any(f => f.Item1.Equals(name, StringComparison.OrdinalIgnoreCase)
								&& f.Item2 == customFieldDataObject.DataType))
							{
								if (value is ZString)
								{
									value = ((ZString)value).GetValidMaxLengthValue(GenCustomAddOnValueSchema.XV_Data, logger, columnNameOverride: name);
								}
								name = ((ZString)name).GetValidMaxLengthValue(GenCustomAddOnValueSchema.XV_Name, logger, columnNameOverride: name);

								var customFieldSet = TrySetCustomField((BusinessObject)workflowProvider, name, value);
								if (!customFieldSet)
								{
									SetCustomValue((BusinessObject)workflowProvider, name, value);
								}
							}
						}
					}
				}
			}
		}

		protected virtual void SetCustomValue(BusinessObject targetBO, ZString name, IZType value)
		{
			ObjectFactory.Get<ICustomValuesHelper>().SetUserDefinedValue(targetBO, name, value);
		}

		bool TrySetCustomField(BusinessObject targetBO, ZString name, IZType value)
		{
			bool setCustomFieldSucceeded = false;
			var customBusinessObject = (targetBO as ICustomFieldProvider)?.GetCustomBusinessObject();

			if (customBusinessObject != null)
			{
				var identifier = CustomPropertyHelper.GeneratePropertyIdentifier(name, value.GetType());
				var customProperty = ((ICustomPropertyContainer)customBusinessObject).CustomProperties.FirstOrDefault(x => x.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase));

				if (customProperty != null)
				{
					setCustomFieldSucceeded = customProperty.TrySetValue(customBusinessObject, value);
				}
			}

			return setCustomFieldSucceeded;
		}
	}

	public abstract class DataObjectReader<T> : DataObjectReader
		where T : IDataObject
	{
		protected DataObjectReader(T dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(logger)
		{
			this.dataObject = Argument.NotNull(dataObject, "T dataObject");
			this.factory = Argument.NotNull(factory, "BusinessObjectFactory factory");

			if (typeof(ITopLevelDataObject).IsAssignableFrom(typeof(T)) && !typeof(ITopLevelDataObjectReader).IsAssignableFrom(GetType()))
			{
				throw new InvalidOperationException("You must use a TopLevelDataObjectReader to read in TopLevelDataObjects.");
			}
		}

		protected readonly T dataObject;
		protected readonly UniversalObjectFactory factory;

		protected IColumnIndexer CreateNewColumnIndexer(SchemaGuidColumn columnPK, Type type)
		{
			var bizObj = CreateNewBusinessObject(type);
			IColumnIndexer columnIndexer;

			if (type == null || bizObj == null)
			{
				var row = factory.RowFactory.New(columnPK.TableName);
				columnIndexer = (IColumnIndexer)row;
				columnIndexer.SetValue(columnPK, Guid.NewGuid(), logger);
				row.Table.Rows.Add(row);
			}
			else if (IsDefaultingEnabled)
			{
				columnIndexer = bizObj;
			}
			else
			{
				columnIndexer = (IColumnIndexer)((INeedRow)bizObj).Row;
			}

			return columnIndexer;
		}

		protected BusinessObject CreateNewBusinessObject(Type type)
		{
			BusinessObject result = null;
			if (type != null)
			{
				if (type.IsAbstract)
				{
					logger.Log(LogType.Error, Res.GetString("92049D50-2D9E-43E4-AD0A-BF144A56F4C3", "Cannot create an abstract type '{0}'.", type.FullName));
				}
				else
				{
					result = factory.New(type);
				}
			}
			return result;
		}

		protected void SetValue(IColumnIndexer row, SchemaStringColumn column, UNLOCO property, Dictionary<string, ValueSetter> delaySetters = null)
		{
			SetValueWithDelay(factory.BOFactory, row, column, property, delaySetters);
		}
	}

	public abstract class DataObjectReader
	{
		protected DataObjectReader(IXmlImportLogger logger)
		{
			this.logger = Argument.NotNull(logger, "IXmlImportLogger logger");
		}

		protected readonly IXmlImportLogger logger;
		protected IColumnIndexer GetColumnIndexer(BusinessObject bizObj)
		{
			IColumnIndexer result = null;
			if (bizObj != null)
			{
				if (IsDefaultingEnabled)
				{
					result = bizObj;
				}
				else
				{
					bizObj.HasChanges = true;
					result = GetColumnIndexerFromRow(((IBusinessObjectInternals)bizObj).Row);
				}
			}
			return result;
		}

		public static IColumnIndexer GetColumnIndexerFromRow(BusinessObject bizObj)
		{
			IColumnIndexer result = null;

			if (bizObj != null)
			{
				result = GetColumnIndexerFromRow(((IBusinessObjectInternals)bizObj).Row);
			}

			return result;
		}

		protected IZType GetValue(CustomizedField customFieldDataObject)
		{
			return GetValue(customFieldDataObject, new DataTypeConverter().FromEnumValue(customFieldDataObject.DataType));
		}

		protected IZType GetValue(CustomizedField customFieldDataObject, Type customFieldType)
		{
			IZType result = null;
			if (!customFieldDataObject.Key.GetValueOrDefault().IsEmpty && customFieldDataObject.DataType.HasValue)
			{
				result = new ZTypeParser().TryParseAndValidate(customFieldDataObject.Value, customFieldType, Res.GetString("56783756-0274-4C76-BF65-A589845F8478", "Custom Fields"), logger);
			}
			return result;
		}

		protected bool IsDefaultingEnabled
		{
			get
			{
				if (!isDefaultingEnabled.HasValue)
				{
					isDefaultingEnabled = eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.Value;
				}
				return isDefaultingEnabled.Value;
			}
		}
		bool? isDefaultingEnabled;

		public static IColumnIndexer GetColumnIndexerFromRow(DataRow row)
		{
			return row as IColumnIndexer;
		}

		protected virtual CharacterCase StringValueCharacterCase => CharacterCase.Normal;

		protected virtual CharacterCase CodeValueCharacterCase => CharacterCase.Upper;

		protected void FillDates(IColumnIndexer row, List<Date> dateCollection, ZBool useEstimatedFirst, params DateTypeSchemaColumnMap[] dateFields)
		{
			FillDates(row, dateCollection, useEstimatedFirst, null, dateFields);
		}

		protected void FillDates(IColumnIndexer row, List<Date> dateCollection, ZBool useEstimatedFirst, Dictionary<string, ValueSetter> delaySetters, params DateTypeSchemaColumnMap[] dateFields)
		{
			if (dateFields != null)
			{
				foreach (var dateField in dateFields)
				{
					foreach (var dateType in dateField.DateTypes)
					{
						var date = dateCollection.FirstOrDefault(dateType, useEstimatedFirst) ?? dateCollection.FirstOrDefault(dateType, !useEstimatedFirst);
						if (date != null)
						{
							SetValue(row, dateField.SchemaColumn, date.Value, delaySetters);
							break;
						}
					}
				}
			}
		}

		public static IDisposable IgnoreInactiveTargetForMatching()
		{
			shouldIgnoreInactiveTargetForMatching = true;

			return new DisposableAction(() =>
			{
				shouldIgnoreInactiveTargetForMatching = false;
			});
		}

		protected static bool ShouldIgnoreInactiveTargetForMatching => shouldIgnoreInactiveTargetForMatching;

		[ThreadStatic]
		static bool shouldIgnoreInactiveTargetForMatching;

		#region Setting Values

		protected void SetValue(BusinessObject bo, ZString propertyName, object valueSource, Dictionary<string, ValueSetter> delaySetters = null)
		{
			SetValueWithDelay(bo, propertyName, () => valueSource, delaySetters);
		}

		protected void SetValueWithDelay(BusinessObject bo, ZString propertyName, Func<object> getValueSource, Dictionary<string, ValueSetter> delaySetters)
		{
			AddOrExecuteSetter(delaySetters, new PropertyValueSetter(bo, propertyName, getValueSource, logger));
		}

		protected void SetValue(IColumnIndexer row, SchemaColumn column, IZType valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			switch (column)
			{
				case SchemaStringColumn s:
					SetValue(row, s, (ZString)valueSource);
					break;

				case SchemaDecimalColumn s:
					SetValue(row, s, (ZDecimal)valueSource);
					break;

				case SchemaDateTimeColumn s:
					SetValue(row, s, (ZDateTime)valueSource);
					break;

				case SchemaDateTimeOffsetColumn s:
					SetValue(row, s, (ZDateTimeOffset)valueSource);
					break;

				case SchemaTimeColumn s:
					SetValue(row, s, (ZTime)valueSource);
					break;

				case SchemaGeographyColumn s:
					SetValue(row, s, (ZGeography)valueSource);
					break;

				case SchemaBoolColumn s:
					SetValue(row, s, (ZBool)valueSource);
					break;

				case SchemaIntColumn s:
					SetValue(row, s, (ZInt)valueSource);
					break;

				case SchemaShortColumn s:
					SetValue(row, s, (ZShort)valueSource);
					break;

				case SchemaByteColumn s:
					SetValue(row, s, (ZByte)valueSource);
					break;
			}
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaStringColumn column, Func<ZString?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new StringColumnValueSetter(row, column, getValueSource, logger, StringValueCharacterCase, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaStringColumn column, ZString? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void SetValueWithDelay(IColumnIndexer row, SchemaGuidColumn column, Func<ZGuid?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null, Func<SchemaColumn, string> getColumnName = null)
		{
			AddOrExecuteSetter(delaySetters, new GuidColumnValueSetter(row, column, getValueSource, logger, rowPKSchema, getColumnName));
		}

		protected void SetValue(IColumnIndexer row, SchemaGuidColumn column, ZGuid? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaBoolColumn column, Func<ZBool?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new BooleanColumnValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaBoolColumn column, ZBool? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaBinaryColumn column, Func<ZBlob?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new BinaryColumnValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaBinaryColumn column, ZBlob? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaByteColumn column, Func<ZByte?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new ByteColumnValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaByteColumn column, ZByte? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaDateColumn column, Func<ZDateTime?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => (ZDate?)getValueSource(), delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaDateColumn column, Func<ZDate?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new DateColumnValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaDateColumn column, ZDateTime? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValue(row, column, (ZDate?)valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValue(IColumnIndexer row, SchemaDateColumn column, ZDate? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaDateTimeColumn column, Func<ZDateTime?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new DateTimeColumnValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaDateTimeColumn column, ZDateTime? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void SetValueWithDelay(IColumnIndexer row, SchemaDateTimeOffsetColumn column, Func<ZDateTimeOffset?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new DateTimeOffsetColumnValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaDateTimeOffsetColumn column, ZDateTimeOffset? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}
		protected void SetValueWithDelay(IColumnIndexer row, SchemaTimeColumn column, Func<ZTime?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new TimeColumnValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaTimeColumn column, ZTime? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void SetValueWithDelay(IColumnIndexer row, SchemaGeographyColumn column, Func<ZGeography?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new GeographyColumnValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaGeographyColumn column, ZGeography? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaIntColumn column, Func<ZInt?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new IntColumnValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaIntColumn column, ZInt? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaIntColumn column, Func<ZLong?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new IntColumnWithLongValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaIntColumn column, ZLong? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaLongColumn column, Func<ZLong?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new LongColumnValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaLongColumn column, ZLong? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaLongColumn column, Func<ZInt?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new LongColumnWithIntValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaLongColumn column, ZInt? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaShortColumn column, Func<ZLong?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new ShortColumnWithLongValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaShortColumn column, ZLong? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaShortColumn column, Func<ZInt?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new ShortColumnWithIntValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaShortColumn column, ZInt? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaShortColumn column, Func<ZShort?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new ShortColumnValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaShortColumn column, ZShort? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaDecimalColumn column, Func<ZDecimal?> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new DecimalColumnValueSetter(row, column, getValueSource, logger, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaDecimalColumn column, ZDecimal? valueSource, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => valueSource, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(IColumnIndexer row, SchemaStringColumn column, Func<ICodeDataObject> getValueSource, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			AddOrExecuteSetter(delaySetters, new CodeDataObjectColumnValueSetter(row, column, getValueSource, logger, CodeValueCharacterCase, rowPKSchema));
		}

		protected void SetValue(IColumnIndexer row, SchemaStringColumn column, ICodeDataObject property, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(row, column, () => property, delaySetters, rowPKSchema);
		}

		protected void SetValueWithDelay(BusinessObjectFactory factory, IColumnIndexer row, SchemaStringColumn column, UNLOCO property, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn rowPKSchema = null)
		{
			ZString code;
			if (property.TryGetUNLOCOAsUpperCase(factory, out code))
			{
				SetValueWithDelay(row, column, () => code, delaySetters, rowPKSchema);
			}
		}

		protected void SetValue(BusinessObjectFactory factory, IColumnIndexer row, SchemaStringColumn column, UNLOCO property, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValueWithDelay(factory, row, column, property, delaySetters, rowPKSchema);
		}

		protected void SetValue(BusinessObject targetBO, SchemaStringColumn column, UNLOCO property, Dictionary<string, ValueSetter> delaySetters = null, SchemaPKColumn rowPKSchema = null)
		{
			SetValue(targetBO.Factory, GetColumnIndexer(targetBO), column, property, delaySetters, rowPKSchema);
		}

		protected virtual void AddOrExecuteSetter(Dictionary<string, ValueSetter> delaySetters, ValueSetter setter)
		{
			if (delaySetters == null)
			{
				setter.SetValue();
			}
			else
			{
				delaySetters.Add(setter);
			}
		}

		#region SetValueIfNotReadOnly

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaStringColumn column, ICodeDescriptionPairList list, ICodeDataObject property)
		{
			ZString code;
			if (property.TryGetCodeAsUpperCase(out code))
			{
				SetValueIfNotReadOnly(targetBO, column, list, code);
			}
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaGuidColumn column, Type bizOType, ZGuid value)
		{
			if (!value.IsEmpty)
			{
				if (!IsReadOnly(targetBO, column))
				{
					SetValue(targetBO, column, value);
				}
				else if (!value.Equals(targetBO[column]))
				{
					var currentBizO = targetBO.Factory.Load(bizOType, (ZGuid)targetBO[column]);
					var updateToBizO = targetBO.Factory.Load(bizOType, value);
					var currentValue = GetCodeWithDescription(currentBizO);
					var updateToValue = GetCodeWithDescription(updateToBizO);

					LogReadOnlyMessage(column, currentValue, updateToValue);
				}
			}
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaBinaryColumn column, ZBlob? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaBoolColumn column, ZBool? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaByteColumn column, ZByte? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaDateColumn column, ZDateTime? value)
		{
			SetValueIfNotReadOnly(targetBO, column, (ZDate?)value);
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaDateColumn column, ZDate? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaDateTimeColumn column, ZDateTime? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaDateTimeOffsetColumn column, ZDateTimeOffset? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaDecimalColumn column, ZDecimal? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaGuidColumn column, ZGuid? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaIntColumn column, ZInt? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaShortColumn column, ZInt? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaLongColumn column, ZLong? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaLongColumn column, ZInt? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaShortColumn column, ZShort? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		protected void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaStringColumn column, ZString? value)
		{
			SetValueIfNotReadOnly(targetBO, column, value, () => SetValue(targetBO, column, value));
		}

		void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaColumn column, IZType value, Action setValue)
		{
			if (value != null)
			{
				if (!IsReadOnly(targetBO, column))
				{
					setValue();
				}
				else if (!value.Equals(targetBO[column]))
				{
					LogReadOnlyMessage(column, string.Format("'{0}'", targetBO[column]), string.Format("'{0}'", value));
				}
			}
		}

		static string GetCodeWithDescription(ICodeDescription bizO)
		{
			var result = bizO.Code;
			if (!string.IsNullOrEmpty(result))
			{
				if (!string.IsNullOrEmpty(bizO.Description))
				{
					return string.Format("'{0}' ({1})", result, bizO.Description);
				}
			}
			else
			{
				result = bizO.Description;
			}

			if (string.IsNullOrEmpty(result))
			{
				return ((BusinessObject)bizO).HumanReadableName;
			}

			return string.Format("'{0}'", result);
		}

		void SetValueIfNotReadOnly(BusinessObject targetBO, SchemaStringColumn column, ICodeDescriptionPairList list, ZString value)
		{
			if (!IsReadOnly(targetBO, column))
			{
				SetValue(targetBO, column, value);
			}
			else if (!value.Equals(targetBO[column]))
			{
				var currentValue = GetCodeWithDescription((ZString)targetBO[column], list);
				var updateToValue = GetCodeWithDescription(value, list);

				LogReadOnlyMessage(column, currentValue, updateToValue);
			}
		}

		static ZString GetCodeWithDescription(ZString code, ICodeDescriptionPairList list)
		{
			var description = ListHelper.GetDescription(code, list);
			if (!string.IsNullOrEmpty(description))
			{
				return ZString.Format("'{0}' ({1})", code, description);
			}

			return ZString.Format("'{0}'", code);
		}

		static bool IsReadOnly(BusinessObject targetBO, SchemaColumn column)
		{
			return targetBO.ZPropertyInfoHash[column.Name].ReadOnly;
		}

		void LogReadOnlyMessage(SchemaColumn column, object currentValue, object updateToValue)
		{
			var humanReadableName = DataBoundResourceStrings.GetColumnDescriptiveName(column.TableName, column.Name);
			logger.Log(LogType.Warning, Res.GetString("79c6fae0-bba1-4583-b887-96e0e1e9910c", "Cannot update read-only Field '{0}' [{1}]. Cannot change {2} to {3}.",
				humanReadableName, column.Name, currentValue, updateToValue));
		}

		#endregion

		#endregion
	}
}
