using System;
using CargoWise.Application;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// A relationship for a collection that has a dependent master.
	/// </summary>
	public class DependentRelationship : CollectionRelationship
	{
		public DependentRelationship(BusinessObject master, Type elementType)
			: this(master, elementType, null)
		{
		}

		public DependentRelationship(BusinessObject master, Type elementType, ZQuery filter)
			: this(master, elementType, filter, null)
		{
		}

		public DependentRelationship(BusinessObject master, Type elementType, ZQuery filter, SchemaGuidColumn fkColumn)
			: base(elementType, filter)
		{
			this.master = master;
			this.fkSchemaColumnInDependent = fkColumn;
			AddEventHandlerToParentClusterKeyChange();
		}

		void AddEventHandlerToParentClusterKeyChange()
		{
			if (Master is IClusterKeyEntity masterClusterKeyEntity
				&& ClusterKeySchemaColumnInDependent is SchemaIntColumn clusterKeySchemaColumn)
			{
				masterClusterKeyEntity.ClusterKeyPty.ValueChanged += new EventHandler((sender, e) =>
				{
					OnRelationshipFilterChanged(EventArgs.Empty);
				});
			}
		}

		public SchemaGuidColumn FKSchemaColumnInDependent
		{
			get
			{
				if (fkSchemaColumnInDependent == null)
				{
					string fkColumnName = GetFKNameFromDependentTableAndPKColumnName(DependentTableName, Master.PKSchemaColumn.Name);
					fkSchemaColumnInDependent = (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(fkColumnName, DependentTableName);
				}

				return fkSchemaColumnInDependent;
			}
		}
		SchemaGuidColumn fkSchemaColumnInDependent;

		SchemaIntColumn ClusterKeySchemaColumnInDependent
		{
			get
			{
				if (fClusterKeySchemaColumnInDependent == null && !ElementType.IsAbstract && Master?.Factory.GetNull(ElementType) is IClusterKeyEntity clusterKeyEntity)
				{
					fClusterKeySchemaColumnInDependent = (SchemaIntColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(clusterKeyEntity.ClusterKeyPty.Name, BusinessObjectFactory.GetTableNameFromType(ElementType));
				}
				return fClusterKeySchemaColumnInDependent;
			}
		}
		SchemaIntColumn fClusterKeySchemaColumnInDependent;

		bool FKSchemaColumnInDependentIsParentID => (fkSchemaColumnInDependentIsParentID ??= new CachedValue<bool>(() => FKSchemaColumnInDependent.Name.EndsWith(Schema.Schema.ParentIDColumnSuffix, StringComparison.OrdinalIgnoreCase))).Value;
		CachedValue<bool> fkSchemaColumnInDependentIsParentID;

		string ParentTableCodeColumnName => (parentTableCodeColumnName ??= new CachedValue<string>(() => FKSchemaColumnInDependent.ColumnPrefix + Schema.Schema.ParentTableCodeColumnSuffix)).Value;
		CachedValue<string> parentTableCodeColumnName;

		string ParentTableColumnName => (parentTableColumnName ??= new CachedValue<string>(() => FKSchemaColumnInDependent.ColumnPrefix + Schema.Schema.ParentTableColumnSuffix)).Value;
		CachedValue<string> parentTableColumnName;

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			var otherRelationship = obj as DependentRelationship;

			return otherRelationship != null
				&& base.Equals(otherRelationship)
				&& Master == otherRelationship.Master
				&& ElementType == otherRelationship.ElementType
				&& IsAdditionalContextEqual(otherRelationship);
		}

		protected virtual bool IsAdditionalContextEqual(DependentRelationship other) => true;

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ Master.PK.GetHashCode();
		}

		#endregion

		#region Overrides

		protected override ZQuery RelationshipFilterCore
		{
			get
			{
				var result = new ZQuery();
				if (Master.IsInDatabase
					&& Master is IClusterKeyEntity master
					&& ClusterKeySchemaColumnInDependent is SchemaIntColumn clusterKeySchemaColumn)
				{
					result.AddToFilter(clusterKeySchemaColumn, master.ClusterKeyPty.Value);
				}

				result.AddToFilter(FKSchemaColumnInDependent, Master.PK);
				return result;
			}
		}

		protected override bool SupportsAddToRelationshipCore()
		{
			return true;
		}

		protected override void AddToRelationship(BusinessObject businessObject)
		{
			var master = Master;
			if (master != null)
			{
				businessObject[FKSchemaColumnInDependent] = master.PK;
				if (
					businessObject is IClusterKeyEntity
					&& !master.IsDataRowDeleted
					&& master is IClusterKeyEntity masterClusterKeyEntity
					&& ClusterKeySchemaColumnInDependent is SchemaIntColumn clusterKeySchemaColumn)
				{
					var clusterKeyValue = masterClusterKeyEntity.ClusterKeyPty.Value;
					if (!businessObject[clusterKeySchemaColumn].Equals(clusterKeyValue))
					{
						businessObject[clusterKeySchemaColumn] = clusterKeyValue;
					}
				}
				if (FKSchemaColumnInDependentIsParentID)
				{
					using (businessObject.GetValidationSuspender())
					{
						businessObject.ZPropertyInfoHash.GetPropertySafe(ParentTableCodeColumnName)?.SetValueFromString(master.TablePrefix);
						businessObject.ZPropertyInfoHash.GetPropertySafe(ParentTableColumnName)?.SetValueFromString(master.TableName);
					}
				}
			}
		}

		protected override void RemoveFromRelationship(BusinessObject businessObject)
		{
			businessObject[FKSchemaColumnInDependent] = ZGuid.Empty;
			if ((object)Master != null)
			{
				if (!(businessObject is IClusterKeyMasterEntity)
					&& ClusterKeySchemaColumnInDependent is SchemaIntColumn clusterKeySchemaColumn)
				{
					using (businessObject.GetValidationSuspender())
					{
						businessObject[clusterKeySchemaColumn] = ZInt.Zero;
					}
				}
				if (FKSchemaColumnInDependentIsParentID)
				{
					using (businessObject.GetValidationSuspender())
					{
						businessObject.ZPropertyInfoHash.GetPropertySafe(ParentTableCodeColumnName)?.SetValueFromString(ZString.Empty);
						businessObject.ZPropertyInfoHash.GetPropertySafe(ParentTableColumnName)?.SetValueFromString(ZString.Empty);
					}
				}
			}
		}

		public override BusinessObject Master
		{
			get { return master; }
		}
		readonly BusinessObject master;

		#endregion

		#region Implementation

		string DependentTableName
		{
			get { return BusinessObjectFactory.GetTableNameFromType(ElementType); }
		}

		static string GetFKNameFromDependentTableAndPKColumnName(string dependentTableName, string pkColumnName)
		{
			string dependentTablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(dependentTableName); // e.g.: "PL" from "PL_PK", "ZTL" from "ZTL_PK"
			string parentTablePrefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(pkColumnName);

			return dependentTablePrefix + "_" + parentTablePrefix;
		}

		#endregion
	}
}
