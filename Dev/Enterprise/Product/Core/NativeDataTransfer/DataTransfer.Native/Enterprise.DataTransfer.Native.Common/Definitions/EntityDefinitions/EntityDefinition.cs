using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Enterprise.DataTransfer.Native.Common.Definitions.Associations;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Keys;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.DataTransfer.Native.Utils.Models;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions
{
	public class EntityDefinition : IEntityDefinition
	{
		internal EntityDefinition(EntityDefinition entityInfo)
			: this(entityName:			entityInfo.EntityName,
				tableName:				entityInfo.TableName,
				suffix:					entityInfo.Suffix,
				optionalEntityCondition: entityInfo.OptionalEntityCondition,
				behaviour:				entityInfo.Behaviour,
				dateRangeStartField:	entityInfo.DateRangeStartField,
				dateRangeEndField:		entityInfo.DateRangeEndField,
				isExternal:				entityInfo.IsExternal,
				isShowAll:				entityInfo.isShowAll,
				requiresAdditionOfActionEqualsMerge: entityInfo.RequiresAdditionOfActionEqualsMerge,
				isUpdateOrInsert:				entityInfo.IsUpdateOrInsert,
				includeParentTableCode:			entityInfo.IncludeParentTableCode,
				hasCustomColumns:				entityInfo.HasCustomColumns,
				entitySetDefinition:			entityInfo.EntitySetDefinition,
				excludedProperties:				entityInfo.excludedProperties,
				excludedFromImportProperties:	entityInfo.excludedFromImportProperties,
				excludedFromExportProperties:	entityInfo.excludedFromExportProperties,
				includedProperties:				entityInfo.includedProperties,
				excludeFromStripProperties:		entityInfo.excludeFromStripProperties,
				crlfStrippedProperties:			entityInfo.crlfStrippedProperties,
				uniqueCriteriaExclusions:		entityInfo.uniqueCriteriaExclusions)
		{
		}

		internal EntityDefinition(string entityName, string tableName, string suffix,
			string behaviour,
			string dateRangeStartField,
			string dateRangeEndField,
			string optionalEntityCondition,
			bool hasCustomColumns,
			bool isUpdateOrInsert,
			bool isExternal,
			bool isShowAll,
			bool includeParentTableCode,
			bool requiresAdditionOfActionEqualsMerge,
			IEnumerable<string> excludedProperties,
			IEnumerable<string> excludedFromImportProperties,
			IEnumerable<string> excludedFromExportProperties,
			IEnumerable<string> includedProperties,
			IEnumerable<string> crlfStrippedProperties,
			IEnumerable<string> excludeFromStripProperties,
			IEnumerable<string> uniqueCriteriaExclusions,
			EntitySetDefinition entitySetDefinition
			) : this(entityName, tableName, suffix)
		{
			this.OptionalEntityCondition = optionalEntityCondition;
			this.Behaviour = behaviour;
			this.DateRangeStartField = dateRangeStartField;
			this.DateRangeEndField = dateRangeEndField;
			this.HasCustomColumns = hasCustomColumns;
			this.IsUpdateOrInsert = isUpdateOrInsert;
			this.IsExternal = isExternal;
			this.isShowAll = isShowAll;
			this.IncludeParentTableCode = includeParentTableCode;
			this.RequiresAdditionOfActionEqualsMerge = requiresAdditionOfActionEqualsMerge;
			this.excludedProperties = ImmutableHashSet.Create(excludedProperties.ToArray());
			this.excludedFromImportProperties = ImmutableHashSet.Create(excludedFromImportProperties.ToArray());
			this.excludedFromExportProperties = ImmutableHashSet.Create(excludedFromExportProperties.ToArray());
			this.includedProperties = ImmutableHashSet.Create(includedProperties.ToArray());
			this.crlfStrippedProperties = ImmutableHashSet.Create(crlfStrippedProperties.ToArray());
			this.excludeFromStripProperties = ImmutableHashSet.Create(excludeFromStripProperties.ToArray());
			this.uniqueCriteriaExclusions = ImmutableHashSet.Create(uniqueCriteriaExclusions.ToArray());
			this.EntitySetDefinition = entitySetDefinition;
		}

		internal EntityDefinition(string entityName, string tableName, string suffix, EntitySetDefinition entitySetDefinition, bool isExternal)
			: this(entityName, tableName, suffix)
		{
			this.EntitySetDefinition = entitySetDefinition;
			this.IsExternal = isExternal;

			this.excludedProperties = ImmutableHashSet.Create(Array.Empty<string>());
			this.excludedFromImportProperties = ImmutableHashSet.Create(Array.Empty<string>());
			this.excludedFromExportProperties = ImmutableHashSet.Create(Array.Empty<string>());
			this.includedProperties = ImmutableHashSet.Create(Array.Empty<string>());
			this.crlfStrippedProperties = ImmutableHashSet.Create(Array.Empty<string>());
			this.excludeFromStripProperties = ImmutableHashSet.Create(Array.Empty<string>());
			this.uniqueCriteriaExclusions = ImmutableHashSet.Create(Array.Empty<string>());
		}

		EntityDefinition(string entityName, string tableName, string suffix)
		{
			this.TableName = tableName;
			this.table = new Lazy<Table>(() => Table.Get(TableName), LazyThreadSafetyMode.PublicationOnly);
			this.Suffix = suffix;
			this.EntityName = GetEntityNameFallingBackToTableNamePlusSuffixIfEmpty(entityName);
		}

		readonly ImmutableHashSet<string> excludedProperties;
		readonly ImmutableHashSet<string> excludedFromImportProperties;
		readonly ImmutableHashSet<string> excludedFromExportProperties;
		readonly ImmutableHashSet<string> includedProperties;
		readonly ImmutableHashSet<string> crlfStrippedProperties;
		readonly ImmutableHashSet<string> excludeFromStripProperties;
		readonly ImmutableHashSet<string> uniqueCriteriaExclusions;

		readonly bool isShowAll;
		public EntitySetDefinition EntitySetDefinition { get; }
		public string EntityName { get; }
		public string TableName { get; }
		public string Suffix { get; }
		public string OptionalEntityCondition { get; }
		public string Behaviour { get; }
		public string DateRangeStartField { get; }
		public string DateRangeEndField { get; }
		public bool HasCustomColumns { get; }
		public bool IsUpdateOrInsert { get; }
		public bool IsExternal { get; }
		public bool IncludeParentTableCode { get; }
		public bool RequiresAdditionOfActionEqualsMerge { get; }

		string GetEntityNameFallingBackToTableNamePlusSuffixIfEmpty(string entityName)
		{
			var result = entityName;
			if (result.IsEmpty())
			{
				if (!TableName.IsEmpty())
				{
					result = TableName;

					if (!Suffix.IsEmpty())
					{
						result += "_" + Suffix;
					}
				}
				else
				{
					result = string.Empty;
				}
			}
			return result;
		}

		public string TablePrefix { get { return Table.Prefix; } }

		public Table Table => table.Value;
		readonly Lazy<Table> table;

		internal bool PropertyIsExcluded(string propertyName)
		{
			return excludedProperties.Contains(propertyName);
		}

		internal bool PropertyIsExcludedFromExport(string propertyName)
		{
			return excludedFromExportProperties.Contains(propertyName);
		}

		internal bool PropertyIsExcludedFromImport(string propertyName)
		{
			return excludedFromImportProperties.Contains(propertyName);
		}

		internal bool PropertyIsIncluded(string propertyName)
		{
			return includedProperties.Contains(propertyName);
		}

		public bool UniqueCriteriaExclusionsContains(string propertyName)
		{
			return uniqueCriteriaExclusions.Contains(propertyName);
		}

		string _fullName;

		public string FullName
		{
			get
			{
				if (_fullName == null)
				{
					_fullName = GetFullName();
				}
				return _fullName;

				string GetFullName()
				{
					if (IsExternal)
					{
						var child = Children.FirstOrDefault();
						if (child != null)
						{
							return child.FullName + "." + EntityName;
						}
						return EntityName;
					}

					if (Parent != null)
					{
						return Parent.FullName + "." + EntityName;
					}
					if (MainAssociation != null)
					{
						var mate = MainAssociation.From;
						return mate.FullName + "." + EntityName;
					}
					return EntityName;
				}
			}
		}

		#region Relationship

		public IEntityDefinition Self
		{
			get { return this; }
		}

		public IEntityDefinition Parent
		{
			get;
			internal set;
		}

		public IEnumerable<IEntityDefinition> Parents
		{
			get { return AssociationCollection.ParentAssociations.Select(a => a.To).OfType<IEntityDefinition>(); }
		}

		public IEnumerable<IEntityDefinition> Children
		{
			get { return AssociationCollection.ChildAssociations.Select(a => a.From).OfType<IEntityDefinition>(); }
		}

		Guid IGraphNode.ID
		{
			get { return iGraphNodeID; }
		}
		readonly Guid iGraphNodeID = Guid.NewGuid();

		#region IGraphNode Members(Deprecated Methods)

		/// <summary>
		/// Deprecated Methods!! Should not be used.
		/// Handle Variance for Generic Type
		/// </summary>

		IGraphNode IGraphNode.Self
		{
			get { return this.Self; }
		}

		IGraphNode IGraphNode.Parent
		{
			get { return this.Parent; }
		}

		IEnumerable<IGraphNode> IGraphNode.Parents
		{
			get { return this.Parents.OfType<IGraphNode>(); }
		}

		IEnumerable<IGraphNode> IGraphNode.Children
		{
			get { return this.Children.OfType<IGraphNode>(); }
		}

		#endregion

		#endregion

		#region Properties

		#region Property Builder

		IEnumerable<IPropertyDef> BuildPropertyDefinitions()
		{
			var result = Table.Columns.Select(c => new PropertyDefinition(c, !excludedFromExportProperties.Contains(c.Name), crlfStrippedProperties.Contains(c.Name), excludeFromStripProperties.Contains(c.Name)));

			result = RemoveExcludedColumns(result);

			result = RemoveParentKey(result);

			if (!IncludeParentTableCode)
			{
				result = RemoveTableCode(result);
			}

			result = RemoveTableName(result);

			result = RemoveForeignKey(result);

			return result.OfType<IPropertyDef>();
		}

		IEnumerable<PropertyDefinition> RemoveExcludedColumns(IEnumerable<PropertyDefinition> columns)
		{
			var localExcludedProperties = excludedProperties;
			var globalExcludedProperties = GlobalDefinition.Instance.ExcludedProperties;
			return columns.Where(column => !localExcludedProperties.Contains(column.ColumnDef.Name) && !globalExcludedProperties.Contains(column.PropertyName));
		}

		IEnumerable<PropertyDefinition> RemoveParentKey(IEnumerable<PropertyDefinition> columns)
		{
			var parentKeyNames = new Dictionary<string, object>(AssociationCollection.ParentAssociations.Count);
			foreach (var association in AssociationCollection.ParentAssociations)
			{
				foreach (var foreignKey in association.ForeignKeys)
				{
					parentKeyNames[foreignKey.Name] = null; // No Junction Table
				}
			}
			return columns.Where(column => !parentKeyNames.ContainsKey(column.ColumnDef.Name));
		}

		IEnumerable<PropertyDefinition> RemoveTableCode(IEnumerable<PropertyDefinition> columns)
		{
			return columns.Where(column => column.ColumnDef.Type != ColumnType.TableCode);
		}

		IEnumerable<PropertyDefinition> RemoveTableName(IEnumerable<PropertyDefinition> columns)
		{
			return columns.Where(column => column.ColumnDef.Type != ColumnType.TableName);
		}

		IEnumerable<PropertyDefinition> RemoveForeignKey(IEnumerable<PropertyDefinition> result)
		{
			return result.Where(c => c.ColumnDef.Type != ColumnType.ForeignKey && c.ColumnDef.Type != ColumnType.NaturalKey && c.ColumnDef.Type != ColumnType.PolymorphicKey);
		}

		#endregion

		public PropertyDefinitionCollection PropertyDefinitions
		{
			get
			{
				if (propertyDefinitions == null)
				{
					var properties = BuildPropertyDefinitions();
					if (IsExternal && !isShowAll)
					{
						properties = CandidateKeyProperties.Where(property => property.ColumnDef.Type != ColumnType.ForeignKey).Union(new[] { Id }).Union(properties.Where(p => PropertyIsIncluded(p.ColumnDef.HumanName)));
					}
					propertyDefinitions = new PropertyDefinitionCollection(EntityName, properties.ToArray());
				}
				return propertyDefinitions;
			}
		}
		PropertyDefinitionCollection propertyDefinitions;

		#endregion

		public IPropertyDef Id
		{
			get { return new PropertyDefinition(Table.Columns.PrimaryKey); }
		}

		public IEnumerable<IPropertyDef> CandidateKeyProperties
		{
			get
			{
				return Table.CandidateKeyConstraints
					.SelectMany(x => x.Columns)
					.Select(c => new PropertyDefinition(c))
					.Distinct(new PropertyDefinitionComparer())
					.ToImmutableList();
			}
		}

		#region Association

		public IEntityDefinition MainAssociationMate
		{
			get
			{
				if (MainAssociation == null)
				{
					return null;
				}

				return MainAssociation.Mate(this);
			}
		}

		public AssociationDefinition MainAssociation
		{
			get { return AssociationCollection.MainAssociation; }
		}

		public AssociationCollection AssociationCollection
		{
			get { return associationCollection; }
		}
		readonly AssociationCollection associationCollection = new AssociationCollection();

		public bool IsFromNodeFor(AssociationDefinition association)
		{
			return (EntityDefinition)association.From == this;
		}

		public bool IsToNodeFor(AssociationDefinition association)
		{
			return (EntityDefinition)association.To == this;
		}

		#endregion

		#region Equality & Formatting

		public override string ToString()
		{
			return string.Format("EntityDefinition[{0}]", FullName);
		}

		public bool Equals(EntityDefinition other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(other.FullName, FullName);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != typeof(EntityDefinition))
			{
				return false;
			}

			return Equals((EntityDefinition)obj);
		}

		public override int GetHashCode()
		{
			return (EntityName != null ? EntityName.GetHashCode() : 0);
		}

		public static bool operator ==(EntityDefinition left, EntityDefinition right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(EntityDefinition left, EntityDefinition right)
		{
			return !Equals(left, right);
		}

		class PropertyDefinitionComparer : IEqualityComparer<IPropertyDef>
		{
			public bool Equals(IPropertyDef x, IPropertyDef y)
			{
				return x.PropertyName == y.PropertyName && x.IsForExport == y.IsForExport;
			}

			public int GetHashCode(IPropertyDef obj)
			{
				return obj.PropertyName != null ? obj.PropertyName.GetHashCode() : 0;
			}
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			throw new NotImplementedException();
		}

		#endregion

		internal bool IsAlreadyAssociatedWith(ForeignKey foreignKey)
		{
			return AssociationCollection.Any(association => association.ForeignKeys.Any(x => x.Equals(foreignKey)));
		}
	}
}
