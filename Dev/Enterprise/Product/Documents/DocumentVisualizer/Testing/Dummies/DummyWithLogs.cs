using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyWithLogs : DummyBaseBusinessObject, IStmALogParent
	{
		public DummyWithLogs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Logs Logs
		{
			get { return logs ?? (logs = new Logs(this)); }
		}

		Logs logs;

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return PK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return TableName; }
		}

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get { return Factory; }
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		public BusinessObject[] BusinessObjectsWithRelatedEvents
		{
			get { return System.Array.Empty<BusinessObject>(); }
		}
	}
}