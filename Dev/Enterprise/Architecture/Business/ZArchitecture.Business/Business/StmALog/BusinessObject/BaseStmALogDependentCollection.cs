using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class BaseStmALogDependentCollection<TStmALog> : DependentBusinessObjectCollection<TStmALog, BusinessObject>, IBusinessObjectCollectionWithMaster
		where TStmALog : BaseStmALog
	{
		public BaseStmALogDependentCollection(IStmALogParent master)
			: this(master, ((BusinessObject)master).Factory)
		{
		}

		public BaseStmALogDependentCollection(IStmALogParent master, BusinessObjectFactory logsFactory)
			: base((BusinessObject)master, logsFactory, true)
		{
			SetReadOnlyIncludingChildren(true);
		}

		public BaseStmALogDependentCollection(IStmALogParent master, ZQuery additionalFilter)
			: base((BusinessObject)master, additionalFilter)
		{
			SetReadOnlyIncludingChildren(true);
		}

		#region Implementation

		#region Add New

		/// <summary>
		/// Adds a new StmALog to the collection. Use Events.* to specify an Event type.
		/// </summary>
		public BaseStmALog AddNew(Event eventType, ZString reference)
		{
			var result = AddNew();
			result.SL_Reference = reference.SubstringSafe(0, StmALogSchema.SL_Reference.MaxLength);
			return result;
		}

		#endregion

		#endregion

		#region Relationships

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			var log = (BaseStmALog)child;
			base.SetCollectionRelationships(child);
			log.Master = (IStmALogParent)Master;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return StmALogSchema.SL_Parent; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var log = (BaseStmALog)child;
			base.SetDefaultsForNewChild(child);
			log.SL_Table = Master.TableName;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.OrderBy = AutoStmALog.Schema.SL_PostedTimeUtc + OrderByClause.Descending;
			return result;
		}

		#endregion

		#region Cancel All

		public void CancelAll()
		{
			foreach (BaseStmALog log in this)
			{
				if (!log.SL_IsCancelled)
				{
					log.Cancel();
				}
			}
		}

		#endregion
	}
}
