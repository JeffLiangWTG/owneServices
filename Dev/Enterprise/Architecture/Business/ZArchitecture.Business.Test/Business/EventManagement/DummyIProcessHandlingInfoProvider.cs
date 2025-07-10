using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	sealed class DummyIProcessHandlingInfoProvider : DummyBusinessObject, IProcessHandlingInfoProvider, IStmALogParent
	{
		public DummyIProcessHandlingInfoProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ProcessHandlingInfo ProcessHandlingInfo
		{
			get { throw new NotImplementedException(); }
		}

		bool IStmALogParent.IsDeleted => this.IsDeleted;

		ZGuid IStmALogParent.LogsParentPK => this.PK;

		string IStmALogParent.LogsParentTableName => this.TableName;

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents => Array.Empty<BusinessObject>();

		bool IStmALogParent.DeferFiringWorkflow => false;

		Logs IStmALogProvider.Logs => new Logs(this);

		BusinessObjectFactory IStmALogProvider.LogsFactory => this.Factory;

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}
	}
}
