using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.DbHealth.Shared;
using Enterprise.DbHealth.Shared.Test;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.Check
{
	sealed class DbConsistencyCheckerForTest : DbConsistencyChecker, ICheckerWithRegistryWorkerForTest
	{
		public DbConsistencyCheckerForTest()
			: base(Db.Connection, Db.DatabaseName)
		{
			ShouldExecuteActualCmd = true;
			WarningList = new DbHealthWarningList();
			Logger = new TestServiceLogger();
		}

		public DbConsistencyCheckerForTest(DbConnection primaryConnection, DbConnection workerConnection)
			: base(primaryConnection, workerConnection, Db.DatabaseName)
		{
			ShouldExecuteActualCmd = true;
			WarningList = new DbHealthWarningList();
			Logger = new TestServiceLogger();
		}

		public DbConsistencyCheckerForTest(DbConnection primaryConnection)
			: this(primaryConnection, Db.Connection)
		{
		}

		public string RegistryDb
		{
			get { return (RegistryWorker as DbRegistryWorkerForTest).RegDb; }
			set { (RegistryWorker as DbRegistryWorkerForTest).RegDb = value; }
		}

		public string RegistryTableView
		{
			get { return (RegistryWorker as DbRegistryWorkerForTest).RegTab; }
			set { (RegistryWorker as DbRegistryWorkerForTest).RegTab = value; }
		}

		public int RegistryStep
		{
			get { return (RegistryWorker as DbRegistryWorkerForTest).RegStep; }
			set { (RegistryWorker as DbRegistryWorkerForTest).RegStep = value; }
		}

		public int ForceStepTimeOut { get; set; }

		public DataTable DbccCommandsRunByWorker
		{
			get { return (RegistryWorker as DbRegistryWorkerForTest).StepsTaken; }
		}

		public DataRow[] GetCommandsRunPerDbAndStep(string dbName, StepsOfDbCheck step)
		{
			DataRow[] result = DbccCommandsRunByWorker.Select("db = '" + dbName + "' and step = " + ((int)step).ToString());
			return result;
		}

		#region TABLOCK Option

		public bool UseTablockOption { get; set; }

		protected override bool TryUseTablock(string fullObjectName)
		{
			return (UseTablockOption)
				&& base.TryUseTablock(fullObjectName);
		}

		public bool TryUseTablock_Exposed(string fullObjectName)
		{
			return base.TryUseTablock(fullObjectName);
		}

		#endregion // TABLOCK Option

		protected override void ExecuteCommand(DbCommand cmd)
		{
			if (ForceStepTimeOut > 0 && RegistryWorker.CurrentStep == ForceStepTimeOut)
			{
				throw new CustomTimeoutException(CustomExceptionMessage);
			}
			else if (ShouldExecuteActualCmd)
			{
				base.ExecuteCommand(cmd);
			}

			DataRow row = DbccCommandsRunByWorker.NewRow();
			row[0] = RegistryWorker.CurrentDatabase;
			row[1] = RegistryWorker.CurrentStep;
			row[2] = RegistryWorker.CurrentTableOrView;
			DbccCommandsRunByWorker.Rows.Add(row);
		}

		public bool ShouldExecuteActualCmd { get; set; }

		public override IDbRegistryWorker GetNewWorker()
		{
			return new DbRegistryWorkerForTest();
		}

		public bool IsStuckExceptionThrown { get; set; }

		protected override void HandleRegistryWorkerIsStuck()
		{
			IsStuckExceptionThrown = true;
		}

		protected override IEnumerable<string> GetDatabasesToCheck(DbConnection connection)
		{
			return DatabasesToCheckOverride ?? base.GetDatabasesToCheck(connection);
		}

		public string[] DatabasesToCheckOverride { get; set; }

		#region ICheckerWithRegistryWorkerForTest Members

		public IDbRegistryWorker RegistryWorker_Exposed
		{
			get { return RegistryWorker; }
		}

		#endregion
	}
}
