using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class DummyWithLogs : DummyBaseBusinessObject, IStmALogParent
	{
		public DummyWithLogs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public event EventHandler<EventArgs> ProcessingLogs;

		public Logs Logs => logs ?? (logs = new Logs(this));
		Logs logs;

		ZGuid IStmALogParent.LogsParentPK => PK;

		string IStmALogParent.LogsParentTableName => TableName;

		BusinessObjectFactory IStmALogProvider.LogsFactory => Factory;

		void IStmALogParent.ProcessLog(IStmALog log)
		{
			ProcessingLogs?.Invoke(this, EventArgs.Empty);
		}

		bool IStmALogParent.DeferFiringWorkflow => false;

		public BusinessObject[] BusinessObjectsWithRelatedEvents => Array.Empty<BusinessObject>();
	}
}
