using System;
using CargoWise.Bi.Development.Common;
using CargoWise.Bi.Development.SchemaSync;
using CargoWise.Bi.Development.SsasBuilder;
using CargoWise.Data;

namespace CargoWise.Bi.Development.Automation
{
	public class BiManager
	{
		public BiManager()
		{
		}

		public BiManager(Action<string> loggerFunction)
		{
			BiCustomLogger.Initialize(loggerFunction);
		}

		public void SchemaSynchronisation()
		{
			try
			{
				SchemaSynchroniser.LoadBiConfiguration();
				SsasProjectBuilder.BuildSsasProject();
				SchemaSynchroniser.SyncConfiguration(parseSchema: true);
			}
			finally
			{
				Db.ClearServerDetails();
			}
		}

		public void UndoCheckout()
		{
			BiFiles.UndoCheckout();
		}
	}
}
