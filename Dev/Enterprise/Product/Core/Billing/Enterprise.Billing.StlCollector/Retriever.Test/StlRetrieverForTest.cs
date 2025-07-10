using System;
using System.Linq;
using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	class StlRetrieverForTest : StlRetriever
	{
		public StlRetrieverForTest(IBillingDataCollectorFactory dataCollectorFactory)
			: base(new LoggerForTest(), dataCollectorFactory, new ScriptLoader(new ScriptFactoryForTest()), new CollectionTimeProvider())
		{
		}

		public StlRetrieverForTest(bool includingSnapshots = true)
			: this(new ScriptLoader(new ScriptFactoryForTest(includingSnapshots)))
		{
		}

		public StlRetrieverForTest(IScriptLoader scriptLoader)
			: this(new LoggerForTest(), new BillingDataCollectorForTestFactory(), scriptLoader)
		{
		}

		public StlRetrieverForTest(ILogger serviceLogger, IBillingDataCollectorFactory dataCollectorFactory, IScriptLoader scriptLoader)
			: base(serviceLogger, dataCollectorFactory, scriptLoader, new CollectionTimeProvider())
		{
		}

		public string[] LogEntries
		{
			get { return Logger.LogEntries.ToArray(); }
		}
		LoggerForTest Logger { get { return serviceLogger as LoggerForTest; } }

		public void ClearLogAndTransactions(DbConnection connection)
		{
			Logger.ClearLog();
			connection.ExecuteNonQuery("TRUNCATE TABLE StmUsageData");
		}

		public DateTime LoadLastEndDateTimeExclusive_Exposed()
		{
			return LoadLastEndDateTimeExclusive();
		}
	}
}
