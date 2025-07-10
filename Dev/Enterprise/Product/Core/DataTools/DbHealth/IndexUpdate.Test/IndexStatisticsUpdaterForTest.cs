using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbHealth.Shared;
using Enterprise.DbHealth.Shared.Test;
using Enterprise.Integration;

namespace Enterprise.DbHealth.IndexUpdate.Testing
{
	sealed class IndexStatisticsUpdaterForTest : IndexStatisticsUpdater, ICheckerWithRegistryWorkerForTest
	{
		readonly Dictionary<string, string> objectStatisticsUpdateCommands = new Dictionary<string, string>();

		public IndexStatisticsUpdaterForTest(ILogger logger)
			: base(logger)
		{
			objectStatisticsUpdateCommands = new Dictionary<string, string>();
			UserActionStatisticsUpdate_ForTest = (statsName, cmd) => objectStatisticsUpdateCommands[statsName] = cmd.CommandText;
		}
		internal override int EmptyStatisticsRowsThreshold => 1000;

		public override IDbRegistryWorker GetNewWorker()
		{
			return new IndexStatisticsUpdateRegistryWorkerForTest();
		}

		public bool TryGetUpdateStatisticsCommandForObject(string objectName, out string commandText)
		{
			return objectStatisticsUpdateCommands.TryGetValue(objectName, out commandText);
		}

		#region ICheckerWithRegistryWorkerForTest Members

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
			get { return 0; }
			set { }
		}

		public int ForceStepTimeOut { get; set; }

		#endregion

		public bool SpoilExecuteCommand { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		protected override int CheckAndReturnRemainingTimeoutInSeconds()
		{
			if (SpoilExecuteCommand)
			{
				Db.Connection.ExecuteNonQuery("muhaha");
			}

			if (ForceStepTimeOut > 0)
			{
				throw new CustomTimeoutException(CustomExceptionMessage);
			}

			return base.CheckAndReturnRemainingTimeoutInSeconds();
		}

		public bool IsStuckExceptionThrown { get; set; }

		protected override void HandleRegistryWorkerIsStuck()
		{
			IsStuckExceptionThrown = true;
		}

		#region ICheckerWithRegistryWorkerForTest Members

		public IDbRegistryWorker RegistryWorker_Exposed
		{
			get
			{
				return RegistryWorker;
			}
		}

		#endregion
	}
}
