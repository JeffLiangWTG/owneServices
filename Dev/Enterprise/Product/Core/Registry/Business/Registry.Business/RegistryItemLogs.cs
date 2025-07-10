using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	public class RegistryItemLogs : NonPersistentBusinessObject, IStmALogParent, ILinkable, IObsoleteValidation
	{
		public RegistryItemLogs(ZGuid registryItemPK, BusinessObjectFactory factory)
			: base(factory)
		{
			this.registryItemPK = registryItemPK;
		}

		readonly ZGuid registryItemPK;

		#region Implementation of IStmALogParent

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return registryItemPK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return StmDataSchema.Constants.TableName; }
		}

		Logs IStmALogProvider.Logs
		{
			get { return logs ?? (logs = new Logs(this)); }
		}
		Logs logs;

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get { return Factory; }
		}

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
		{
			get { return System.Array.Empty<BusinessObject>(); }
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion

		#region Implementation of ILinkable

		// This is needed to force DependentBusinessObjectCollection to load logs from database
		bool ILinkable.LinkIsInDatabase
		{
			get { return true; }
		}

		ZGuid ILinkable.LinkPK
		{
			get { return registryItemPK; }
		}

		string ILinkable.LinkTableName
		{
			get { return StmDataSchema.Constants.TableName; }
		}

		string ILinkable.LinkTablePrefix
		{
			get { return StmDataSchema.Constants.Prefix; }
		}

		#endregion
	}
}
