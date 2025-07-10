using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common.Definitions.Associations;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Exceptions;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Keys;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using Microsoft.SqlServer.Types;
using static System.FormattableString;

namespace Enterprise.DataTransfer.Native.Common.Converters
{
	/// <summary>
	/// Entity To DataRow Converter
	/// Helper class provide helper method to convert IEntity to DataRow
	/// </summary>
	public class ERConverter : IEntityConverter<DataRow>
	{
		public ERConverter(DbConnection connection, AncillaryImportServices sessionServices)
		{
			rowRepository = new RowRepository(connection);
			this.sessionServices = sessionServices ?? throw new ArgumentNullException(nameof(sessionServices));
		}
		readonly RowRepository rowRepository;
		readonly AncillaryImportServices sessionServices;

		// Need Refactoring
		public DataRow Convert(IEntity entity)
		{
			return Convert(entity, null);
		}

		public DataRow Convert(IEntity entity, IEntityContext context)
		{
			var definition = entity.Definition;
			var row = rowRepository.Create(definition.Table);
			UpdateRowFromEntity(entity, context, row);
			SetAssociationRows(row, entity, table => null);
			return row;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message strings")]
		public void UpdateRowFromEntity(IEntity entity, IEntityContext context, DataRow row, bool isForDelete = false)
		{
			var propertiesForImport = entity.Properties.Where(o => !((EntityDefinition)entity.Definition).PropertyIsExcludedFromImport(o.Definition.ColumnDef.Name));
			foreach (var property in propertiesForImport)
			{
				var propertyDef = property.Definition;
				var column = propertyDef.ColumnDef;
				var value = property.Value;

				if (column is PrimaryKey && (context == null || !context.AlwaysUseInternalPK))
				{
					continue;
				}

				if (column.DataType == DbDataType.VarBinary && value is string valueAsString && !value.IsEmpty())
				{
					try
					{
						value = System.Convert.FromBase64String(valueAsString);
					}
					catch (FormatException ex)
					{
						throw new NativeXMLUserVisibleException(ex.Message, ex);
					}
				}

				var currentValue = row[column.Name];
				var proposedValue = value.IsEmpty() && column.Nullable ? DBNull.Value : value;
				proposedValue = ConvertAmbiguousTypes(row, column, proposedValue);

				if (!CompareDBValues(currentValue, proposedValue))
				{
					try
					{
						row[column.Name] = proposedValue;
						LogPrecisionErrors(isForDelete, property, entity, column, proposedValue);
						LogMoneyErrors(isForDelete, property, entity, column, proposedValue);
					}
					catch (ArgumentException ex)
					{
						var errMsg = string.Empty;
						if (ex.Message.Contains("Cannot set column")
							&& ex.Message.Contains("The value violates the MaxLength limit of this column."))
						{
							if (proposedValue is string columnValue && columnValue.Length > column.Length)
							{
								errMsg = string.Format(CultureInfo.InvariantCulture, @"ERROR: {0}.{1} - Maximum length allowed in this column is {2} characters, {3} were provided - [{4}].", entity.Definition.FullName, property.Name, column.Length, columnValue.Length, columnValue);
								throw new NativeXMLUserVisibleException(errMsg);
							}
						}
						if (ex.Message.Contains("Expected type is"))
						{
							errMsg = string.Format(CultureInfo.InvariantCulture, @"[{0}.{1}] : {2}", entity.EntityName, column.HumanName, ex.Message.Replace(column.Name, column.HumanName));
							if (ex.Message.Contains("Expected type is Boolean"))
							{
								errMsg += " Data type expected in this column is either 'true' or 'false'.";
							}
							throw new NativeXMLUserVisibleException(errMsg);
						}
						throw;
					}
				}
			}
		}

		void LogPrecisionErrors(bool isForDelete, Property property, IEntity entity, IColumnDef column, object proposedValue)
		{
			if (!isForDelete && (column.DataType.Equals(DbDataType.Decimal, StringComparison.OrdinalIgnoreCase) || column.DataType.Equals(DbDataType.Money, StringComparison.OrdinalIgnoreCase)) && decimal.TryParse(proposedValue as string, out var result))
			{
				var splits = result.ToString(CultureInfo.InvariantCulture).Split('.');
				var maxDecimalSize = property.Definition.ColumnDef.Precision - property.Definition.ColumnDef.Scale;
				if (splits[0].Length > maxDecimalSize)
				{
					var errMsg = Invariant($"[{entity.Definition.FullName}.{property.Name}] : Value was too large for a {column.DataType} type. Couldn't store <{proposedValue}> in {column.DataType} column. Limit is {maxDecimalSize} digits before the decimal point but {splits[0].Length} were provided.");
					throw new NativeXMLUserVisibleException(errMsg);
				}

				if (splits.Length == 2 && splits[1].Length > property.Definition.ColumnDef.Scale)
				{
					var warning = Invariant($"<{entity.Definition.FullName}>.<{property.Name}> - Too many decimal places provided, truncated from {splits[1].Length} to {column.Scale} places.");
					if (!(sessionServices.Logger is MemoryLogger memoryLogger) || !memoryLogger.Buffer.Logs().Any(log => log.Message.Equals(warning, StringComparison.Ordinal)))
					{
						sessionServices.Logger.Log(LogType.Warning, warning);
					}
				}
			}
		}

		void LogMoneyErrors(bool isForDelete, Property property, IEntity entity, IColumnDef column, object proposedValue)
		{
			if (!isForDelete && column.DataType.Equals(DbDataType.Money, StringComparison.OrdinalIgnoreCase) && decimal.TryParse(proposedValue as string, out var result))
			{
				if (!TypeValidation.IsValidMoney(result))
				{
					var errMsg = Invariant($"[{entity.Definition.FullName}.{property.Name}] : Value was either too large or too small for Money. Couldn't store <{result}> in Money Column.");
					throw new NativeXMLUserVisibleException(errMsg);
				}
			}
		}

		static object ConvertAmbiguousTypes(DataRow row, IColumnDef column, object value)
		{
			if (value is string strValue)
			{
				if (row.Table.Columns[column.Name].DataType == typeof(DateTimeOffset))
				{
					return DateTimeOffset.Parse(strValue, CultureInfo.InvariantCulture);
				}
				else if (row.Table.Columns[column.Name].DataType == typeof(SqlGeography))
				{
					return ZGeography.TryParse(strValue, out ZGeography geoVal) ? (SqlGeography)geoVal : value;
				}
				else if (row.Table.Columns[column.Name].DataType == typeof(bool) && (strValue == "0" || strValue == "1"))
				{
					return (strValue == "1").ToString().ToLower();
				}
			}
			return value;
		}

		static bool CompareDBValues(object value1, object value2)
		{
			var converter = TypeDescriptor.GetConverter(value1.GetType());
			if (converter != null && converter.CanConvertFrom(value2.GetType()))
			{
				try
				{
					return value1.Equals(converter.ConvertFrom(value2));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return false;
				}
			}
			//varbinary datatype
			else if (value1 is byte[] b1 && value2 is byte[] b2)
			{
				return b1.SequenceEqual(b2);
			}
			return value1.Equals(value2);
		}

		public static IEntity Revert(DataRow row, IEntity entity)
		{
			var definition = entity.Definition;
			entity.InternalPK = (Guid)row[definition.Id.ColumnDef.Name];
			foreach (var propertyDef in definition.PropertyDefinitions)
			{
				var columnDef = propertyDef.ColumnDef;
				entity[propertyDef.PropertyName] = row[columnDef.Name];
			}
			return entity;
		}

		public object GetValue(IEntity child, string columnName)
		{
			var tempRow = Convert(child);
			try
			{
				return tempRow[columnName];
			}
			finally
			{
				tempRow.Delete();
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		internal void SetAssociationRows(DataRow row, IEntity entity, Func<ManyToManyAssociation, DataRow> getJunctionRow)
		{
			var definition = entity.Definition;
			var associations = definition.AssociationCollection.ParentAssociations;
			var parentAssociationsQuery = from p in entity.Parents
										  join a in associations on (EntityDefinition)p.Definition equals a.To
										  select new { Parent = p, Association = a };
			var parentAssociations = parentAssociationsQuery.ToList();

			// Adds support for inferring grandparents without needing to have them defined explicitly.  e.g. OrgSupplierPart --> CusClassPartPivot --> (Component)CusClassPartPivot
			if (associations.Count > parentAssociations.Count)
			{
				var missingAssociations = (from ass1 in associations where !(parentAssociations.Any(ass2 => ass2.Association == ass1)) select ass1);
				foreach (var missingAssociation in missingAssociations)
				{
					if (entity.Parent != null && entity.Parent.Parent != null && missingAssociation.To.EntityName == entity.Parent.Parent.EntityName)
					{
						SetMissingAssociations(row, entity, missingAssociation);
					}
				}
			}

			foreach (var parentAssociation in parentAssociations)
			{
				var association = parentAssociation.Association;
				var parent = parentAssociation.Parent;

				if (association is ManyToManyAssociation manyToManyAssociation)
				{
					SetAssociationForManyToManyAssociation(entity, getJunctionRow, association, parent, manyToManyAssociation);
				}
				else
				{
					var relation = association.GetRelation(parent.Definition);
					foreach (var keyRelation in relation.Keys)
					{
						SetRelationFromAssociation(row, entity, association, parent, keyRelation);
					}
				}
			}

			if (row != null)
			{
				SetAssociationRowsFromKeyValuePairs(row, entity);
			}
		}

		void SetAssociationRowsFromKeyValuePairs(DataRow row, IEntity entity)
		{
			var keyValueList = from child in entity.Children
							   join association in entity.Definition.AssociationCollection.ChildAssociations on (EntityDefinition)child.Definition equals association.From
							   where !(association is ManyToManyAssociation) && !association.AdditionalKey.IsEmpty() && !association.AdditionalKeyRef.IsEmpty()
							   select new { ParentKey = association.AdditionalKeyRef, ParentValue = GetValue(child, association.AdditionalKey) };
			foreach (var keyValue in keyValueList)
			{
				row[keyValue.ParentKey] = keyValue.ParentValue;
			}
		}

		void SetAssociationForManyToManyAssociation(IEntity entity, Func<ManyToManyAssociation, DataRow> getJunctionRow, AssociationDefinition association, IEntity parent, ManyToManyAssociation manyToManyAssociation)
		{
			var junctionRow = getJunctionRow(manyToManyAssociation);
			var parentRelation = association.GetRelation(parent.Definition);
			var parentKey = parentRelation.Keys[0].FromKey;
			var childKey = parentRelation.Keys[0].ToKey;
			if (parentKey.ReferenceColumnDef is PrimaryKey)
			{
				SetForeignKey(junctionRow, parentKey, parent.InternalPK);
				parent.InternalPKChanged += delegate
				{ SetForeignKey(junctionRow, parentKey, parent.InternalPK); };
			}
			else
			{
				SetForeignKey(junctionRow, parentKey, parent[childKey.HumanName]);
			}

			var childRelation = association.GetRelation(entity.Definition);
			SetForeignKey(junctionRow, childRelation.Keys[0].FromKey, entity.InternalPK);
			entity.InternalPKChanged += delegate
			{ SetForeignKey(junctionRow, childRelation.Keys[0].FromKey, entity.InternalPK); };
		}

		void SetMissingAssociations(DataRow row, IEntity entity, AssociationDefinition missingAssociation)
		{
			var grandParent = entity.Parent.Parent;
			foreach (var foreignKeyToGrandParent in missingAssociation.ForeignKeys)
			{
				if (foreignKeyToGrandParent != null && foreignKeyToGrandParent.ReferenceColumnDef is PrimaryKey)
				{
					if (!grandParent.InternalPK.IsEmpty())
					{
						SetForeignKey(row, foreignKeyToGrandParent, grandParent.InternalPK);
					}
					grandParent.InternalPKChanged += delegate
					{
						if (!grandParent.InternalPK.IsEmpty())
						{
							SetForeignKey(row, foreignKeyToGrandParent, grandParent.InternalPK);
						}
					};
				}
			}
		}

		void SetRelationFromAssociation(DataRow row, IEntity entity, AssociationDefinition association, IEntity parent, DB.Sql.KeyRelation keyRelation)
		{
			var parentKey = keyRelation.FromKey;
			object parentValue;
			if (parentKey.ReferenceColumnDef is PrimaryKey)
			{
				parentValue = parent.InternalPK.IsEmpty() && parentKey.Nullable ? DBNull.Value : parent.InternalPK;
				CheckParentReferenceMismatch(entity, parent, parentValue, row, parentKey, association);
				SetForeignKey(row, parentKey, parentValue);
				parent.InternalPKChanged += delegate
				{
					var newValue = parent.InternalPK.IsEmpty() && parentKey.Nullable ? DBNull.Value : (object)parent.InternalPK;
					CheckParentReferenceMismatch(entity, parent, newValue, row, parentKey, association);
					SetForeignKey(row, parentKey, newValue);
				};
			}
			else
			{
				var key = keyRelation.ToKey.HumanName;
				parentValue = parent.HasProperty(key) ? parent[key] : string.Empty;
				SetForeignKey(row, parentKey, parentValue);
			}
		}

		#region CheckParentReferenceMismatch

		static void CheckParentReferenceMismatch(IEntity entity, IEntity parent, object parentValue, DataRow row, IColumnDef parentKey, AssociationDefinition association)
		{
			object dbParentKey;

			if (parentValue is Guid &&
				row != null &&
				row.RowState != DataRowState.Deleted &&
				row.RowState != DataRowState.Detached &&
				(dbParentKey = row[parentKey.Name]) != null &&
				!dbParentKey.Equals(DBNull.Value) &&
				!dbParentKey.Equals(Guid.Empty) &&
				!dbParentKey.Equals(parentValue) &&
				!association.IsExternal)
			{
				var root = entity;
				while (root.Parent != null && !ReferenceEquals(root.Parent, root))
				{
					root = root.Parent;
				}

				CheckForDuplicatePKinEntitySet(root, entity, GetEntityPk(entity), parent, dbParentKey);
				CheckForDuplicatePKinDb(entity, parent, dbParentKey);

				throw new ParentReferenceMismatchException(entity, parent, dbParentKey);
			}
		}

		static void CheckForDuplicatePKinEntitySet(IEntity currentEntity, IEntity entity, object entityPk, IEntity parent, object dbParentKey)
		{
			if (currentEntity == null || entityPk == null ||
				entityPk is string entityPkAsString && string.IsNullOrWhiteSpace(entityPkAsString) ||
				entityPk is Guid entityPkAsGuid && entityPkAsGuid.IsEmpty())
			{
				return;
			}

			if (!ReferenceEquals(entity, currentEntity) && entityPk.Equals(GetEntityPk(currentEntity)))
			{
				throw new ParentReferenceMismatchException(entity, parent, dbParentKey, FormattableString.Invariant($"Duplicate PK '{entityPk}' found on multiple entities in the supplied XML."));
			}

			foreach (var childEntity in currentEntity.Children)
			{
				CheckForDuplicatePKinEntitySet(childEntity, entity, entityPk, parent, dbParentKey);
			}
		}

		static void CheckForDuplicatePKinDb(IEntity entity, IEntity parent, object dbParentKey)
		{
			if (entity == null)
			{
				return;
			}

			if (entity.Action == EntityAction.INSERT || entity.Action == EntityAction.UPDATE || entity.Action == EntityAction.MERGE)
			{
				var entityPkAsGuid = GetEntityPkAsGuid(entity);
				if (!entityPkAsGuid.IsEmpty() && !string.IsNullOrEmpty(entity.TableName))
				{
					var tableSchema = EnterpriseSchema.GetTableSchema(entity.TableName);
					if (tableSchema != null)
					{
						var factory = new BusinessObjectFactory { RefreshEnabled = false };
						var query = new ZQuery(tableSchema.PK, entityPkAsGuid);
						if (factory.ExistsInDatabase(entity.TableName, query))
						{
							// It is not trivial to check if it is same record or different,
							// but as there is a ParentReferenceMismatchException for an entity with this PK,
							// assume it is possible that this PK in the DB is from different record.
							throw new ParentReferenceMismatchException(entity, parent, dbParentKey, FormattableString.Invariant($"PK '{entityPkAsGuid}' already exists on a row in the database, and is linked to a different parent row."));
						}
					}
				}
			}
		}

		static object GetEntityPk(IEntity entity)
		{
			return entity.Properties.FirstOrDefault(p => p.Name == "PK")?.Value;
		}

		static Guid GetEntityPkAsGuid(IEntity entity)
		{
			var entityPkValue = GetEntityPk(entity);
			if (entityPkValue is Guid entityPk)
			{
				return entityPk;
			}
			if (entityPkValue is string entityPkAsString && Guid.TryParse(entityPkAsString, out entityPk))
			{
				return entityPk;
			}
			return Guid.Empty;
		}

		#endregion

		static void SetForeignKey(DataRow row, Key foreignKey, object foreignKeyId)
		{
			if (row == null ||
				row.RowState == DataRowState.Deleted ||
				row.RowState == DataRowState.Detached ||
				Equals(row[foreignKey.Name], foreignKeyId))
			{
				return;
			}

			if (foreignKeyId is string foreignKeyNk)
			{
				var column = row.Table.Columns[foreignKey.Name];
				if (column != null && column.MaxLength > 0 && foreignKeyNk.Length > column.MaxLength)
				{
					throw new NativeXMLUserVisibleException(FormattableString.Invariant(
						$"Could not set <{row.Table.TableName}>.<{foreignKey.Name}> to [{foreignKeyNk}] as the value exceeds the maximum length of {column.MaxLength} characters. Verify that you did not use a full name if code was expected."));
				}
			}

			row[foreignKey.Name] = foreignKeyId;
			if (foreignKey.Discriminator != null)
			{
				if (foreignKey.Discriminator.Type == ColumnType.TableCode)
				{
					row[foreignKey.Discriminator.Name] = foreignKey.ReferenceTable.Prefix;
				}
				else if (foreignKey.Discriminator.Type == ColumnType.TableName)
				{
					row[foreignKey.Discriminator.Name] = foreignKey.ReferenceTable.Name;
				}
				else
				{
					throw new NativeXMLUserVisibleException("foreignKey.Discriminator should only be set with a Type of TableCode or TableName.");
				}
			}
		}
	}
}
