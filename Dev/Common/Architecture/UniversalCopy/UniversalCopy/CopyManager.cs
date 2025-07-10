using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Integration.UniversalCopy;

namespace CargoWise.UniversalCopy
{
	/// <summary>
	/// Universal copy manager
	/// </summary>
	/// 
	public abstract class CopyManager
	{
		#region Copy

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is only error details that will be useful for error reporting.")]
		public CopyResult Copy(object source, CopyTemplateTree copyTemplate)
		{
			Argument.NotNull(copyTemplate, nameof(copyTemplate)); // Suggested By ReviewBot 
			object @object = null;
			string errorMessage = null;

			PrepareForCopy(source, copyTemplate);
			try
			{
				copiedEntities = new Dictionary<object, object>();
				keyMappings = new Dictionary<object, object>();
				relatedEntitiesToReplace = new Dictionary<object, List<KeyValuePair<object, RelatedEntityCopyTemplateNode>>>();
				var currentPath = new List<string>();
				currentRoots = new Stack<object>();

				@object = CopyEntity(null, source, copyTemplate.InnerNode, ref currentPath);

				if (@object == null)
				{
					string serializedCopyTemplate;
					using (var serializedCopyTemplateStream = new MemoryStream())
					{
						copyTemplate.Serialize(serializedCopyTemplateStream);
						serializedCopyTemplate = Encoding.Unicode.GetString(serializedCopyTemplateStream.ToArray());
					}
					errorMessage = string.Format(CultureInfo.InvariantCulture, "Template: {0}\r\nPath: {1}", serializedCopyTemplate, currentPath.Aggregate(string.Empty, (c, p) => (!string.IsNullOrEmpty(c) ? "," : string.Empty) + p));
				}

				if (@object is IUniversalCopiedIdentifier clonedIdentifier)
				{
					clonedIdentifier.MarkAsCopiedByUniversalCopy();
				}
			}
			catch (UniversalCopyAbortException ex)
			{
				errorMessage = ex.Message;
			}
			finally
			{
				FinishCopy(source, @object, copyTemplate);

				copiedEntities = null;
				keyMappings = null;
				relatedEntitiesToReplace = null;
				currentRoots = null;
			}

			return new CopyResult(@object, errorMessage);
		}

		protected virtual void PrepareForCopy(object source, CopyTemplateTree copyTemplate) { }
		protected virtual void FinishCopy(object source, object copy, CopyTemplateTree copyTemplate) { }

		protected Dictionary<object, object> copiedEntities;
		Dictionary<object, object> keyMappings;
		Dictionary<object, List<KeyValuePair<object, RelatedEntityCopyTemplateNode>>> relatedEntitiesToReplace;
		Stack<object> currentRoots;

		object CopyEntity(object parentEntity, object sourceEntity, CopyTemplateNode copyTemplateNode, ref List<string> currentPath, string skipParentProperty = null, Action<object> targetInitialization = null, string propertyName = null)
		{
			Argument.NotNull(this.copiedEntities, nameof(this.copiedEntities));
			Argument.NotNull(currentPath, nameof(currentPath));

			if (sourceEntity == null || copyTemplateNode == null)
			{
				return null;
			}

			object targetEntity;
			if (copiedEntities.TryGetValue(sourceEntity, out targetEntity))
			{
				return targetEntity;
			}

			if (copyTemplateNode is TemplateCopyTemplateNode templateConfiguration)
			{
				return CopyEntity(parentEntity, sourceEntity, templateConfiguration.InnerNode, ref currentPath, skipParentProperty, targetInitialization, propertyName);
			}

			if (!(copyTemplateNode is EntityCopyTemplateNode entityConfiguration))
			{
				throw new ArgumentException("CopyEntity may accept only EntityCopyTemplateNode and TemplateCopyTemplateNode, was: " + copyTemplateNode.GetType().Name, nameof(copyTemplateNode));
			}
			currentRoots.Push(sourceEntity);
			targetEntity = CreateNewEntityFrom(parentEntity, sourceEntity, copyTemplateNode, propertyName);
			if (targetEntity != null && !CopyTemplateTree.IsOnBusinessObjectIgnoreList(targetEntity.GetType()))
			{
				PreCopyEntity(sourceEntity, targetEntity, copyTemplateNode);

				targetInitialization?.Invoke(targetEntity);

				entityConfiguration.Nodes.Sort(new TemplateNodeCopyOrderComparer());
				EnsureElementsOrder(sourceEntity, copyTemplateNode);

				foreach (var node in entityConfiguration.Nodes)
				{
					var configurationNode = node;
					var relatedEntityCopyTemplateNode = configurationNode as RelatedEntityCopyTemplateNode;
					if (relatedEntityCopyTemplateNode != null && relatedEntityCopyTemplateNode.IsMandatory && relatedEntityCopyTemplateNode.CopyMethod == RelatedEntityCopyMethod.None)
					{
						throw new UniversalCopyAbortException(Res.GetString("EDAD7A73-6913-4BB8-97EF-F8C18E91C56B", "Related entity {0} is mandatory to be copied. Please modify the template you are using.", configurationNode.Name));
					}

					var propertyCopyTemplateNode = configurationNode as PropertyCopyTemplateNode;
					CollectionCopyTemplateNode collectionCopyTemplateNode;
					var customCopyTemplateNode = propertyCopyTemplateNode?.CustomCopyTemplateNode ?? string.Empty;
					if (!string.IsNullOrEmpty(customCopyTemplateNode))
					{
						configurationNode = GetRelatedCopyTemplateNode(sourceEntity, customCopyTemplateNode, configurationNode);
						propertyCopyTemplateNode = configurationNode as PropertyCopyTemplateNode;
					}

					if (propertyCopyTemplateNode != null)
					{
						if (propertyCopyTemplateNode.Name != skipParentProperty)
						{
							CopyProperty(targetEntity, sourceEntity, propertyCopyTemplateNode);
						}
					}
					else if (relatedEntityCopyTemplateNode != null)
					{
						currentPath.Add(relatedEntityCopyTemplateNode.Name);
						CopyRelatedEntity(targetEntity, sourceEntity, relatedEntityCopyTemplateNode, ref currentPath);
						if (currentPath.Count > 0)
						{
							currentPath.RemoveAt(currentPath.Count - 1);
						}
					}
					else if ((collectionCopyTemplateNode = configurationNode as CollectionCopyTemplateNode) != null)
					{
						currentPath.Add(collectionCopyTemplateNode.Name);
						CopyCollection(targetEntity, sourceEntity, collectionCopyTemplateNode, ref currentPath);
						if (currentPath.Count > 0)
						{
							currentPath.RemoveAt(currentPath.Count - 1);
						}
					}
				}

				PostCopyEntity(sourceEntity, targetEntity, copyTemplateNode);
			}
			if (currentRoots.Count > 0)
			{
				currentRoots.Pop();
			}

			return targetEntity;
		}

		protected virtual CopyTemplateNode GetRelatedCopyTemplateNode(object sourceEntity, string customCopyTemplateNode, CopyTemplateNode copyTemplateNode) => copyTemplateNode;

		protected virtual void PreCopyEntity(object sourceEntity, object targetEntity, CopyTemplateNode copyTemplateNode) { }

		protected virtual void PostCopyEntity(object sourceEntity, object targetEntity, CopyTemplateNode copyTemplateNode) { }

		protected virtual void EnsureElementsOrder(object sourceEntity, CopyTemplateNode copyTemplateNode) { }

		protected virtual void EnsureUniversalCopyMappingKeys(object sourceEntity, object newEntity, Dictionary<object, object> mappingKeys) { }

		#endregion

		#region Copy Property

		protected virtual void CopyProperty(object target, object source, PropertyCopyTemplateNode propertyCopyTemplateNode)
		{
			Argument.NotNull(propertyCopyTemplateNode, nameof(propertyCopyTemplateNode)); // Suggested By ReviewBot 

			if (propertyCopyTemplateNode.CopyMethod != CopyMethod.None)
			{
				object value = null;
				switch (propertyCopyTemplateNode.CopyMethod)
				{
					case CopyMethod.Default:
						value = GetDefaultValue(target, propertyCopyTemplateNode);
						break;
					case CopyMethod.Value:
						value = propertyCopyTemplateNode.Value ?? GetEmptyValue(target, propertyCopyTemplateNode);
						break;
					case CopyMethod.Copy:
						value = GetPropertyValue(source, propertyCopyTemplateNode.Name);
						break;
					case CopyMethod.Property:
						value = GetPropertyValue(source, propertyCopyTemplateNode.Value.ToString());
						break;
					case CopyMethod.Empty:
						value = GetEmptyValue(target, propertyCopyTemplateNode);
						break;
					case CopyMethod.Macro:
						var translatedValue = ProcessMacros(propertyCopyTemplateNode.Value.ToString());
						value = string.IsNullOrEmpty(translatedValue) ? GetEmptyValue(target, propertyCopyTemplateNode) : translatedValue;
						break;
					default:
						return;
				}

				SetPropertyValue(target, propertyCopyTemplateNode.Name, value);
			}
		}

		object GetDefaultValue(object target, PropertyCopyTemplateNode propertyCopyTemplateNode)
		{
			Argument.NotNull(propertyCopyTemplateNode, nameof(propertyCopyTemplateNode)); // Suggested By ReviewBot 
			return propertyCopyTemplateNode.DefaultValue ?? GetEmptyValue(target, propertyCopyTemplateNode);
		}

		object GetEmptyValue(object target, PropertyCopyTemplateNode propertyCopyTemplateNode)
		{
			Argument.NotNull(propertyCopyTemplateNode, nameof(propertyCopyTemplateNode)); // Suggested By ReviewBot 

			var targetRow = target as DataRow;
			var targetColumn = targetRow != null && targetRow.Table.Columns.Contains(propertyCopyTemplateNode.Name) ? targetRow.Table.Columns[propertyCopyTemplateNode.Name] : null;
			if (targetColumn != null && targetColumn.AllowDBNull)
			{
				return DBNull.Value;
			}

			Type propertyType = GetPropertyType(target, propertyCopyTemplateNode.Name);
			if (propertyType == typeof(string))
			{
				return string.Empty;
			}
			if (propertyType != null && propertyType.IsValueType)
			{
				return Activator.CreateInstance(propertyType);
			}
			return null;
		}

		protected object GetPropertyValue(object source, string propertyName)
		{
			DataRow sourceRow = source as DataRow;
			if (sourceRow != null)
			{
				if (sourceRow.Table.Columns.Contains(propertyName))
				{
					EnsureBlobField(sourceRow, propertyName);
					return sourceRow[propertyName];
				}
				else
				{
					return null;
				}
			}

			return GetPropertyValueCore(source, propertyName);
		}

		protected virtual void EnsureBlobField(DataRow row, string columnName) { }

		protected abstract object GetPropertyValueCore(object source, string propertyName);

		protected virtual void SetDataPropertyValue(object target, string propertyName, object value)
		{
			SetPropertyValue(target, propertyName, value);
		}

		protected void SetPropertyValue(object target, string propertyName, object value)
		{
			Type propertyType = GetPropertyType(target, propertyName);
			if (value != null && propertyType != null && propertyType != value.GetType())
			{
				try
				{
					var converter = TypeDescriptor.GetConverter(propertyType);
					if (converter.CanConvertFrom(value.GetType()))
					{
						value = converter.ConvertFrom(value);
					}
					else
					{
						converter = TypeDescriptor.GetConverter(value.GetType());
						if (converter.CanConvertTo(propertyType))
						{
							value = converter.ConvertTo(value, propertyType);
						}
					}
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					var formatException = exception as FormatException ?? exception.InnerException as FormatException;
					if (formatException != null)
					{
						if (!HandleFormatException(target, propertyName, propertyType, ref value, formatException))
						{
							return;
						}
					}
					else
					{
						throw;
					}
				}
			}

			var targetRow = target as DataRow;
			if (targetRow != null && targetRow.Table.Columns.Contains(propertyName))
			{
				var dataValue = value;

				var column = targetRow.Table.Columns[propertyName];
				if (column != null)
				{
					var stringValue = dataValue as string;
					if (stringValue != null)
					{
						if (string.IsNullOrEmpty(stringValue) && column.AllowDBNull)
						{
							dataValue = DBNull.Value;
						}
						else if (column.MaxLength > 0 && column.MaxLength < int.MaxValue && stringValue.Length > column.MaxLength)
						{
							dataValue = stringValue.Substring(0, column.MaxLength);
						}
					}
				}

				if ((dataValue != null && dataValue != DBNull.Value) || column == null || column.AllowDBNull)
				{
					targetRow[propertyName] = dataValue;
				}
			}

			SetPropertyValueCore(target, propertyName, value);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String used as a single char flag")]
		bool HandleFormatException(object target, string propertyName, Type propertyType, ref object value, FormatException formatException)
		{
			Argument.NotNull(propertyType, nameof(propertyType));
			Argument.NotNull(target, nameof(target));

			var validTarget = target as DataRow;
			var stringValue = value as string;

			if (stringValue != null)
			{
				if (propertyType == typeof(bool))
				{
					if (stringValue == "n" || stringValue == "N" || string.IsNullOrWhiteSpace(stringValue))
					{
						value = false;
						return true;
					}
					if (stringValue == "y" || stringValue == "Y")
					{
						value = true;
						return true;
					}
				}
				else if (propertyType == typeof(Guid) && string.IsNullOrWhiteSpace(stringValue))
				{
					value = Guid.Empty;
					return true;
				}
			}

			var targetRow = validTarget;
			ReportError(
				string.Format(SetPropertyWrongValueErrorMessage,
					targetRow != null ? targetRow.Table.TableName : target.GetType().Name,
					propertyName,
					propertyType.Name,
					value
				),
				formatException);

			return false;
		}

		protected abstract void SetPropertyValueCore(object target, string propertyName, object value);

		[SuppressMessage("Microsoft.Contracts", "Nonnull-69-0")] // sourceRow.Table.Columns[propertyName] is not null as determined by the previous check. Static CC go Figure
		[SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant")]
		Type GetPropertyType(object source, string propertyName)
		{
			DataRow sourceRow = source as DataRow;
			if (sourceRow != null)
			{
				return sourceRow.Table != null && sourceRow.Table.Columns != null && sourceRow.Table.Columns.Contains(propertyName) ? sourceRow.Table.Columns[propertyName].DataType : null;
			}

			return GetPropertyTypeCore(source, propertyName);
		}

		protected abstract Type GetPropertyTypeCore(object source, string propertyName);

		string ProcessMacros(string value)
		{
			return ProcessMacrosCore(value, currentRoots);
		}

		protected virtual string ProcessMacrosCore(string value, IEnumerable<object> rootEntities)
		{
			return value;
		}

		#endregion

		#region Copy objects

		void CopyRelatedEntity(object targetEntity, object sourceEntity, RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode, ref List<string> currentPath)
		{
			Argument.NotNull(relatedEntityCopyTemplateNode, nameof(relatedEntityCopyTemplateNode)); // Suggested By ReviewBot 
			Argument.NotNull(currentPath, nameof(currentPath));

			if (relatedEntityCopyTemplateNode.CopyMethod != RelatedEntityCopyMethod.None)
			{
				var relatedEntity = GetPropertyValue(sourceEntity, relatedEntityCopyTemplateNode.Name);

				if (relatedEntity == null && GetPropertyType(sourceEntity, relatedEntityCopyTemplateNode.Name) == null)
				{
					relatedEntity = GetRelatedEntityFromDb(sourceEntity, relatedEntityCopyTemplateNode);
				}

				if (relatedEntity != null)
				{
					if (relatedEntityCopyTemplateNode.CopyMethod == RelatedEntityCopyMethod.Copy)
					{
						relatedEntity = CopyEntity(targetEntity, relatedEntity, relatedEntityCopyTemplateNode.InnerNode, ref currentPath, propertyName: relatedEntityCopyTemplateNode.Name);
					}
					else if (relatedEntityCopyTemplateNode.CopyMethod == RelatedEntityCopyMethod.LinkCopied)
					{
						object copiedEntity;
						if (copiedEntities.TryGetValue(relatedEntity, out copiedEntity))
						{
							relatedEntity = copiedEntity;
						}
						else
						{
							List<KeyValuePair<object, RelatedEntityCopyTemplateNode>> replaceList;
							if (!relatedEntitiesToReplace.TryGetValue(relatedEntity, out replaceList))
							{
								replaceList = new List<KeyValuePair<object, RelatedEntityCopyTemplateNode>>();
								relatedEntitiesToReplace.Add(relatedEntity, replaceList);
							}
							replaceList.Add(new KeyValuePair<object, RelatedEntityCopyTemplateNode>(targetEntity, relatedEntityCopyTemplateNode));
						}
					}
				}
				else if (relatedEntityCopyTemplateNode.CopyMethod == RelatedEntityCopyMethod.Link && !string.IsNullOrEmpty(relatedEntityCopyTemplateNode.RelatedPropertyName))
				{
					CopyProperty(targetEntity, sourceEntity, new PropertyCopyTemplateNode { CopyMethod = CopyMethod.Copy, Name = relatedEntityCopyTemplateNode.RelatedPropertyName });
				}

				if (relatedEntity != null)
				{
					SetEntityRelationship(targetEntity, relatedEntity, relatedEntityCopyTemplateNode);
				}
			}
		}

		void CopyCollection(object targetEntity, object sourceEntity, CollectionCopyTemplateNode collectionCopyTemplateNode, ref List<string> currentPath)
		{
			Argument.NotNull(collectionCopyTemplateNode, nameof(collectionCopyTemplateNode)); // Suggested By ReviewBot \
			Argument.NotNull(currentPath, nameof(currentPath));
			if (collectionCopyTemplateNode.CopyMethod != CollectionCopyMethod.None)
			{
				PrepareTargetCollection(targetEntity, collectionCopyTemplateNode);

				var collection = GetCollection(sourceEntity, collectionCopyTemplateNode);
				if (collection != null)
				{
					if (collectionCopyTemplateNode.CopyMethod == CollectionCopyMethod.Filter)
					{
						collection = FilterCollection(collection, collectionCopyTemplateNode, currentPath);
					}

					if (collection != null)
					{
						foreach (var item in collection.Cast<object>().Where(item => item != null).ToArray())
						{
							CopyEntity(targetEntity, item, collectionCopyTemplateNode.InnerNode, ref currentPath, collectionCopyTemplateNode.ItemPropertyName,
								newItem => SetCollectionRelationship(targetEntity, newItem, collectionCopyTemplateNode));
						}
					}

					FinishTargetCollection(targetEntity, collectionCopyTemplateNode);
				}
			}
			else if (collectionCopyTemplateNode.Filter != null && !string.IsNullOrEmpty(collectionCopyTemplateNode.Filter.FilterData))
			{
				var collection = GetCollection(sourceEntity, collectionCopyTemplateNode);
				if (collection != null)
				{
					collection = FilterCollection(collection, collectionCopyTemplateNode, currentPath);

					if (collection != null)
					{
						foreach (var item in collection.Cast<object>().Where(item => item != null).ToArray())
						{
							if (!copiedEntities.ContainsKey(item))
							{
								copiedEntities.Add(item, null);
							}
						}
					}
				}
			}
		}

		protected abstract object GetRelatedEntityFromDb(object sourceEntity, RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode);

		protected virtual void PrepareTargetCollection(object targetEntity, CollectionCopyTemplateNode collectionCopyTemplateNode) { }
		protected virtual void FinishTargetCollection(object targetEntity, CollectionCopyTemplateNode collectionCopyTemplateNode) { }

		object CreateNewEntityFrom(object parentEntity, object sourceEntity, CopyTemplateNode copyTemplateNode, string propertyName)
		{
			Argument.NotNull(sourceEntity, nameof(sourceEntity));

			object newEntity;

			DataRow sourceRow = sourceEntity as DataRow;
			if (sourceRow != null)
			{
				DataRow newRow = sourceRow.Table.NewRow();
				DataColumn pkColumn = GetRowKeyColumn(newRow, "PK");
				if (pkColumn != null)
				{
					newRow[pkColumn] = Guid.NewGuid();
				}

				sourceRow.Table.Rows.Add(newRow);
				newEntity = newRow;
			}
			else
			{
				newEntity = CreateNewEntityFromCore(parentEntity, sourceEntity, copyTemplateNode, propertyName);
			}

			if (newEntity != null)
			{
				copiedEntities.Add(sourceEntity, newEntity);
				EnsureUniversalCopyMappingKeys(sourceEntity, newEntity, keyMappings);

				List<KeyValuePair<object, RelatedEntityCopyTemplateNode>> replaceList;
				if (relatedEntitiesToReplace != null && relatedEntitiesToReplace.TryGetValue(sourceEntity, out replaceList))
				{
					if (replaceList != null)
					{
						foreach (var replaceItem in replaceList)
						{
							SetEntityRelationship(replaceItem.Key, newEntity, replaceItem.Value);
						}
					}
					relatedEntitiesToReplace.Remove(sourceEntity);
				}
			}

			return newEntity;
		}

		protected abstract object CreateNewEntityFromCore(object parentEntity, object sourceEntity, CopyTemplateNode copyTemplateNode, string propertyName);

		protected virtual bool WasCreatedByMe(object entity)
		{
			return copiedEntities != null && copiedEntities.ContainsValue(entity);
		}

		protected object GetTargetEntity(object entity)
		{
			return copiedEntities != null && entity != null ? copiedEntities.TryGetValue(entity, out var result) ? result : null : null;
		}

		protected object GetTargetEntityPK(object entityPK)
		{
			return keyMappings != null && entityPK != null ? keyMappings.TryGetValue(entityPK, out var result) ? result : null : null;
		}

		public object GetEntityPK(object entity)
		{
			DataRow sourceRow = entity as DataRow;
			if (sourceRow != null)
			{
				object pk = GetRowKeyValue(sourceRow, "PK");
				if (pk != null)
				{
					return pk;
				}
			}

			return GetEntityPKCore(entity);
		}

		protected abstract object GetEntityPKCore(object entity);

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "The string is used as a key")]
		string GetEntityCode(object entity)
		{
			DataRow sourceRow = entity as DataRow;
			if (sourceRow != null)
			{
				object code = GetRowKeyValue(sourceRow, "Code");
				if (code != null)
				{
					return code.ToString();
				}
			}

			return GetEntityCodeCore(entity);
		}

		protected abstract string GetEntityCodeCore(object entity);

		object GetRowKeyValue(DataRow sourceRow, string keyColumnName)
		{
			Argument.NotNull(sourceRow, nameof(sourceRow));
			DataColumn keyColumn = GetRowKeyColumn(sourceRow, keyColumnName);
			return keyColumn != null ? GetPropertyValue(sourceRow, keyColumn.ColumnName) : null;
		}

		static DataColumn GetRowKeyColumn(DataRow sourceRow, string keyColumn)
		{
			Argument.NotNull(sourceRow, nameof(sourceRow)); // Suggested By ReviewBot 
			return sourceRow.Table.Columns.Cast<DataColumn>().FirstOrDefault(
				column => column.ColumnName.Length == (keyColumn.Length + 3) && column.ColumnName.EndsWith("_" + keyColumn, StringComparison.Ordinal));
		}

		protected virtual void SetEntityRelationship(object targetEntity, object relatedEntity, RelatedEntityCopyTemplateNode relatedEntityCopyTemplateNode)
		{
			Argument.NotNull(relatedEntityCopyTemplateNode, nameof(relatedEntityCopyTemplateNode)); // Suggested By ReviewBot 
			SetPropertyValue(targetEntity, relatedEntityCopyTemplateNode.Name, relatedEntity);

			if (!string.IsNullOrEmpty(relatedEntityCopyTemplateNode.RelatedPropertyName))
			{
				Type propertyType = GetPropertyType(targetEntity, relatedEntityCopyTemplateNode.RelatedPropertyName);
				if (propertyType != null)
				{
					if (propertyType == typeof(Guid) || propertyType.Name == "ZGuid")
					{
						SetDataPropertyValue(targetEntity, relatedEntityCopyTemplateNode.RelatedPropertyName, GetEntityPK(relatedEntity));
					}
					else if (propertyType == typeof(string) || propertyType.Name == "ZString")
					{
						SetDataPropertyValue(targetEntity, relatedEntityCopyTemplateNode.RelatedPropertyName, GetEntityCode(relatedEntity));
					}
				}
			}
		}

		IEnumerable GetCollection(object sourceEntity, CollectionCopyTemplateNode collectionCopyTemplateNode)
		{
			Argument.NotNull(collectionCopyTemplateNode, nameof(collectionCopyTemplateNode));
			var collection = GetCollectionCore(sourceEntity, collectionCopyTemplateNode) ?? GetCollectionFromDb(sourceEntity, collectionCopyTemplateNode);
			return SortCollection(collectionCopyTemplateNode, collection);
		}

		IEnumerable SortCollection(CollectionCopyTemplateNode collectionCopyTemplateNode, IEnumerable collection)
		{
			foreach (var universalCopyTypeCollectionSorter in
				ObjectFactory.Get<IEnumerable>("UniversalCopyTypeCollectionSortersList"))
			{
				if (((IUniversalCopyTypeCollectionSorter)universalCopyTypeCollectionSorter).ItemsTableName == collectionCopyTemplateNode.ItemsTableName)
				{
					return ((IUniversalCopyTypeCollectionSorter)universalCopyTypeCollectionSorter).GetSortedCollection(collection);
				}
			}

			return collection;
		}

		protected virtual IEnumerable GetCollectionCore(object sourceEntity, CollectionCopyTemplateNode collectionCopyTemplateNode)
		{
			Argument.NotNull(collectionCopyTemplateNode, nameof(collectionCopyTemplateNode)); // Suggested By ReviewBot 
			return GetPropertyValue(sourceEntity, collectionCopyTemplateNode.Name) as IEnumerable;
		}

		protected abstract IEnumerable GetCollectionFromDb(object sourceEntity, CollectionCopyTemplateNode collectionCopyTemplateNode);

		IEnumerable FilterCollection(IEnumerable collection, CollectionCopyTemplateNode collectionCopyTemplateNode, IEnumerable<string> currentPath)
		{
			Argument.NotNull(collectionCopyTemplateNode, nameof(collectionCopyTemplateNode)); // Suggested By ReviewBot 
			Argument.NotNull(currentPath, nameof(currentPath));
			return collectionCopyTemplateNode.Filter == null ? collection : FilterCollectionCore(collection, collectionCopyTemplateNode, currentPath);
		}

		protected abstract IEnumerable FilterCollectionCore(IEnumerable collection, CollectionCopyTemplateNode collectionCopyTemplateNode, IEnumerable<string> path);

		protected virtual void SetCollectionRelationship(object targetEntity, object collectionItem, CollectionCopyTemplateNode collectionCopyTemplateNode)
		{
			Argument.NotNull(collectionCopyTemplateNode, nameof(collectionCopyTemplateNode)); // Suggested By ReviewBot 
			SetDataPropertyValue(collectionItem, collectionCopyTemplateNode.ItemPropertyName, GetEntityPK(targetEntity));

			if (!string.IsNullOrEmpty(collectionCopyTemplateNode.ItemParentTablePropertyName))
			{
				SetCollectionItemParentTableProperty(targetEntity, collectionItem, collectionCopyTemplateNode);
			}
		}

		protected abstract void SetCollectionItemParentTableProperty(object targetEntity, object collectionItem, CollectionCopyTemplateNode collectionCopyTemplateNode);

		#endregion

		#region Error Handling

		protected virtual void ReportError(string message, Exception exception = null)
		{
			throw new UniversalCopyAbortException(message, exception);
		}

		/// <summary>
		/// Has 4 string parameters:
		///   0 - component type
		///   1 - property name
		///   2 - property type
		///   3 - value
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is not a literal string")]
		protected virtual string SetPropertyWrongValueErrorMessage
		{
			get
			{
				return "Cannot set property {0}.{1} of type {2} with value '{3}'.";
			}
		}

		#endregion

		#region TemplateNodeCopyOrderComparer

		public class TemplateNodeCopyOrderComparer : IComparer<CopyTemplateNode>
		{
			public int Compare(CopyTemplateNode x, CopyTemplateNode y)
			{
				if (x == null && y == null)
				{
					return 0;
				}

				var priorityCompare = (x?.Priority ?? 0).CompareTo(y?.Priority ?? 0);
				if (priorityCompare != 0)
				{
					return priorityCompare;
				}

				var xIsProperty = x is PropertyCopyTemplateNode;
				var yIsProperty = y is PropertyCopyTemplateNode;
				if (xIsProperty ^ yIsProperty)
				{
					return Comparer<bool>.Default.Compare(yIsProperty, xIsProperty);
				}

				if (x != null && y != null)
				{
					int nameCompare = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
					if (nameCompare != 0)
					{
						return nameCompare;
					}
				}

				if (x is CollectionCopyTemplateNode collectionNodeX && y is CollectionCopyTemplateNode collectionNodeY)
				{
					var xHasFilter = collectionNodeX.Filter != null;
					var yHasFilter = collectionNodeY.Filter != null;
					if (xHasFilter ^ yHasFilter)
					{
						return Comparer<bool>.Default.Compare(yHasFilter, xHasFilter);
					}
					else if (collectionNodeX.IsSplitCollection ^ collectionNodeY.IsSplitCollection)
					{
						//run split collection sections before the parent
						return Comparer<bool>.Default.Compare(collectionNodeY.IsSplitCollection, collectionNodeX.IsSplitCollection);
					}
					else
					{
						return Comparer<int>.Default.Compare(collectionNodeX.Order, collectionNodeY.Order);
					}
				}

				return 0;
			}
		}

		#endregion
	}
}
