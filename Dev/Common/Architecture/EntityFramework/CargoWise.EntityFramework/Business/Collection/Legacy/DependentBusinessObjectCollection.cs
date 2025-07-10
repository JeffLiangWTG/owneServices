using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Business;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public abstract class DependentBusinessObjectCollection<T, MasterT> : BusinessObjectCollection<T>, IDependentBusinessObjectCollection, IBusinessObjectFilterFactory
		where T : BusinessObject
		where MasterT : BusinessObject, ILinkable
	{
		protected DependentBusinessObjectCollection(MasterT master)
			: this(master, master.Factory, false)
		{
		}

		protected DependentBusinessObjectCollection(MasterT master, BusinessObjectFactory factory)
			: this(master, factory, false)
		{
		}

		protected DependentBusinessObjectCollection(MasterT master, BusinessObjectFactory factory, bool allowMasterFactoryToBeDifferent)
			: base(factory)
		{
#if DEBUG
			if (!allowMasterFactoryToBeDifferent && (object)master != null && master.Factory != factory)
			{
				ErrorReporter.ReportOnce(
					"WrongFactoryInDependentCollection",
					"You passed a master with a different factory to Master.Factory.\n" +
					"If you really must have different factories, use the overload\n" +
					"DependentBusinessObjectCollection(BusinessObject Master, BusinessObjectFactory Factory, bool AllowMasterFactoryToBeDifferent)");
			}
#endif
			Initialise(master);
		}

		protected DependentBusinessObjectCollection(MasterT master, ZQuery additionalFilter)
			: base(master.Factory, additionalFilter)
		{
			Initialise(master);
		}

		protected DependentBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			SetReadOnlyIncludingChildren(true);
		}

		void Initialise(MasterT master)
		{
			if ((object)master != null)
			{
				fMaster = master;
			}
		}

		Type IDependentBusinessObjectCollection.ChildType => typeof(T);

		BusinessObject IDependentBusinessObjectCollection.Master => Master;

		public new T AddNew(Type bizOType)
		{
			return base.AddNew(bizOType);
		}

		public virtual void CloneElementsTo(DependentBusinessObjectCollection<T, MasterT> cloneResult)
		{
			Where(data => !data.IsDeleted).ForEach(data => cloneResult.Add(data.Clone()));
		}

		public IEnumerable<T> Find(Func<T, bool> predicate)
		{
			foreach (T element in this)
			{
				if (predicate(element))
				{
					yield return element;
				}
			}
		}

		protected virtual bool DeferEnablingAndDisablingDataRefresh
		{
			get { return false; }
		}

		protected bool EnsureHasMaster()
		{
			if ((object)fMaster == null)
			{
				ErrorReporter.ReportOnce("NullMaster" + GetType().FullName, GetType().FullName + " - Master was null");
				return false;
			}
			return !fMaster.IsDeleted;
		}

		protected virtual void EnsureNotInAnotherCollectionForSameFK(BusinessObject businessObject)
		{
			foreach (BusinessObjectCollection parentCollection in businessObject.ParentCollections)
			{
				bool throwException = false;
				var dependentBusinessObjectCollection = parentCollection as IDependentBusinessObjectCollection;
				if (dependentBusinessObjectCollection != null && parentCollection != this)
				{
					throwException = Master != dependentBusinessObjectCollection.Master &&
									dependentBusinessObjectCollection.FKSchemaColumnInDependent == FKSchemaColumnInDependent;
				}

				if (throwException)
				{
					throw new CannotAddToCollectionException(string.Format(
						"Cannot add the business object '{0}' to collection '{1}' ({4}) because it belongs to another collection '{2}' ({5}) that uses the FK '{3}' (recommend OtherCollection.Remove(BizObj) first).",
						businessObject.GetType().Name, GetType().FullName, parentCollection.GetType().FullName, FKSchemaColumnInDependent.Name, GetMasterDetail(Master), GetMasterDetail(dependentBusinessObjectCollection?.Master)));
				}
			}
		}

		string GetMasterDetail(BusinessObject master)
		{
			return master == null ? "NULL" : System.FormattableString.Invariant($"Type:'{master.GetType().FullName}', TableName:'{master.TableName}', PK='{master.PK}'");
		}

		internal override bool AddFromDatabaseIfMatchLastLoadedFilter(IEnumerable<ZGuid> pKs)
		{
			bool result = false;

			if (Master != null && !Master.IsDeleted)
			{
				result = base.AddFromDatabaseIfMatchLastLoadedFilter(pKs);
				if (result)
				{
					Master.RaiseUpdatedByDataRefreshIncludingChildren();
				}
			}

			return result;
		}

		#region BusinessObjectCollection Overrides

		internal override BusinessObject CreateNewBusinessObject()
		{
			EnsureHasMaster();
			return base.CreateNewBusinessObject();
		}

		protected override ITypeDeciderContext GetTypeDeciderContextCore() => Master as ITypeDeciderContext;

		protected override ZQuery CreateRelationshipFilter()
		{
			var master = Master;
			ZQuery result = null;
			if (master == null)
			{
				result = ZQuery.NoResultQuery;
			}
			else
			{
				result = new ZQuery(FKSchemaColumnInDependent, linkableMaster.LinkPK);
				if (!linkableMaster.LinkIsInDatabase)
				{
					result.FetchOnlyFromLocalCache = true;
				}
				if (master is IClusterKeyEntity masterClusterKeyEntity && master.IsInDatabase && ClusterKeySchemaColumn is SchemaIntColumn clusterKeySchemaColumn)
				{
					var query = new ZQuery(clusterKeySchemaColumn, masterClusterKeyEntity.ClusterKeyPty.Value);
					query.AddToFilter(result);
					result = query;
				}
			}

			return result;
		}

		protected internal override void SetCollectionRelationships(BusinessObject child)
		{
			var master = Master;

			if ((object)master != null && (master is NonPersistentBusinessObject || !master.IsDataRowDeleted))
			{
				// can be extracted to SetClusterKeyIfApplicable
				if (child is IClusterKeyEntity && master is IClusterKeyEntity masterClusterKeyEntity && ClusterKeySchemaColumn is SchemaIntColumn clusterKeySchemaColumn)
				{
					var clusterKeyValue = masterClusterKeyEntity.ClusterKeyPty.Value;
					if (!child[clusterKeySchemaColumn].Equals(clusterKeyValue))
					{
						child[clusterKeySchemaColumn] = clusterKeyValue;
					}
				}

				var linkPK = linkableMaster.LinkPK;
				if (!child[FKSchemaColumnInDependent.Name].Equals(linkPK))
				{
					child[FKSchemaColumnInDependent.Name] = linkPK;
				}
			}

			if (master != null && FKSchemaColumnInDependentIsParentID)
			{
				using (child.GetValidationSuspender())
				{
					child.ZPropertyInfoHash.GetPropertySafe(ParentTableCodeColumnName)?.SetValueFromString(master.TablePrefix);
					child.ZPropertyInfoHash.GetPropertySafe(ParentTableColumnName)?.SetValueFromString(master.TableName);
				}
			}
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			if ((object)Master != null && !child.IsDeleted)
			{
				using (child.GetValidationSuspender())
				{
					child[FKSchemaColumnInDependent.Name] = ZGuid.Empty;
				}

				if (!InRemoveAndDelete && ClusterKeySchemaColumn is SchemaIntColumn clusterKeySchemaColumn)
				{
					using (child.GetValidationSuspender())
					{
						child[clusterKeySchemaColumn] = ZInt.Zero;
					}
				}

				if (FKSchemaColumnInDependentIsParentID)
				{
					using (child.GetValidationSuspender())
					{
						child.ZPropertyInfoHash.GetPropertySafe(ParentTableCodeColumnName)?.SetValueFromString(ZString.Empty);
						child.ZPropertyInfoHash.GetPropertySafe(ParentTableColumnName)?.SetValueFromString(ZString.Empty);
					}
				}
			}
		}

		protected virtual bool EnableRemovingDependentWithoutDeletingErrorReport { get { return false; } }

		public override void Add(BusinessObject businessObject)
		{
			if (fMaster != null)
			{
				if (!businessObject.IsDeleted && !businessObject[FKSchemaColumnInDependent.Name].Equals(linkableMaster.LinkPK))
				{
					EnsureNotInAnotherCollectionForSameFK(businessObject);
				}
			}
			base.Add(businessObject);
		}

		void IDependentBusinessObjectCollection.Add(BusinessObject businessObject) => Add(businessObject);

		public override void Load()
		{
			if (fMaster != null)
			{
				base.Load();
			}
		}

		protected override bool FetchOnlyFromLocalCache
		{
			get { return (object)fMaster != null && !linkableMaster.LinkIsInDatabase; }
		}

		public override void Load(ZQuery filter)
		{
			if (fMaster != null)
			{
				base.Load(filter);
			}
		}

		protected override void NotifyRegisteredChildEditable()
		{
			if (Master != null && Factory != null && !IsManagedForDataRefreshSet)
			{
				IsManagedForDataRefresh = true;
			}
		}

		#endregion

		#region Masters Are In Database

		bool IBusinessObjectCollectionInternals.MastersAreInDatabase
		{
			get
			{
				return linkableMaster.LinkIsInDatabase;
			}
		}

		#endregion

		#region Masters Are Deleted

		bool IBusinessObjectCollectionInternals.MastersAreDeleted
		{
			get { return fMaster != null && fMaster.IsDeleted; }
		}

		#endregion

		#region Master Object

		MasterT fMaster;

		public MasterT Master
		{
			get
			{
				return fMaster;
			}
		}

		ILinkable linkableMaster
		{
			get { return fMaster; }
		}

		#endregion

		#region Foreign Key in Dependent Object

		protected virtual string FkColumnName
		{
			get
			{
				if (fFkColumnName == null)
				{
					fFkColumnName = ZDataUtils.GetFKNameFromDependentTableAndPKColumnName(Table, Master.PKSchemaColumn.Name);
				}
				return fFkColumnName;
			}
		}
		string fFkColumnName;

		protected internal virtual SchemaGuidColumn FKSchemaColumnInDependent
		{
			get
			{
				if (fFKSchemaColumnInDependent == null)
				{
					fFKSchemaColumnInDependent = (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(FkColumnName, Table.TableName);
				}

				return fFKSchemaColumnInDependent;
			}
		}

		SchemaGuidColumn fFKSchemaColumnInDependent;

		bool FKSchemaColumnInDependentIsParentID => (fkSchemaColumnInDependentIsParentID ??= new CachedValue<bool>(() => FKSchemaColumnInDependent.Name.EndsWith(Schema.Schema.ParentIDColumnSuffix, StringComparison.OrdinalIgnoreCase))).Value;
		CachedValue<bool> fkSchemaColumnInDependentIsParentID;

		string ParentTableCodeColumnName => (parentTableCodeColumnName ??= new CachedValue<string>(() => FKSchemaColumnInDependent.ColumnPrefix + Schema.Schema.ParentTableCodeColumnSuffix)).Value;
		CachedValue<string> parentTableCodeColumnName;

		string ParentTableColumnName => (parentTableColumnName ??= new CachedValue<string>(() => FKSchemaColumnInDependent.ColumnPrefix + Schema.Schema.ParentTableColumnSuffix)).Value;
		CachedValue<string> parentTableColumnName;

		#endregion

		#region IBusinessObjectFilterFactory Members

		class DependentBusinessObjectFilter : BusinessObjectFilter
		{
			readonly bool isLoaded;
			readonly ZQuery matchingFilter;

			public DependentBusinessObjectFilter(bool isLoaded, ZQuery matchingFilter)
			{
				this.isLoaded = isLoaded;
				this.matchingFilter = matchingFilter;
			}

			protected override bool IsMatching(BusinessObject bizObj)
			{
				return isLoaded && bizObj.MatchesFilter(matchingFilter);
			}
		}

		public IBusinessObjectFilter NewBusinessObjectFilter()
		{
			return new DependentBusinessObjectFilter(IsLoaded, MatchingFilter);
		}

		ZQuery MatchingFilter
		{
			get
			{
				var filter = new ZQuery();
				filter.AddToFilter(RelationshipFilter, JoinCondition.And);
				filter.AddToFilter(LastLoadedAdditionalFilter, JoinCondition.And);
				return filter;
			}
		}

		#endregion

		SchemaIntColumn ClusterKeySchemaColumn
		{
			get
			{
				if (fClusterKeySchemaColumn == null && !typeof(T).IsAbstract && Master?.Factory.GetNull<T>() is IClusterKeyEntity clusterKeyEntity)
				{
					fClusterKeySchemaColumn = (SchemaIntColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(clusterKeyEntity.ClusterKeyPty.Name, BusinessObjectFactory.GetTableNameFromType(typeof(T)));
				}
				return fClusterKeySchemaColumn;
			}
		}
		SchemaIntColumn fClusterKeySchemaColumn;

		SchemaGuidColumn IDependentBusinessObjectCollection.FKSchemaColumnInDependent => FKSchemaColumnInDependent;
	}
}
