using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Billing.StlCollector.Retriever.Testing.Scripts;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	sealed class ScriptFactoryForTest : IScriptFactory
	{
		static IEnumerable<IStlItem> GetDefaultScripts(bool includingSnapshots = true)
		{
			yield return new TestTransactionalStlItem();
			yield return new TestMonthlyAllowHistoricalStlItem();
			yield return new TestMonthlyCurrentStlItem();
			yield return new TestDailyStlItem();
			if (includingSnapshots)
			{
				yield return new TestSnapshotTimeBased();
				yield return new TestSnapshotLiveConfiguration();
			}
		}

		public ScriptFactoryForTest(bool includingSnapshots = true)
			: this(GetDefaultScripts(includingSnapshots).ToArray())
		{
		}

		public ScriptFactoryForTest(params IStlItem[] stlScripts)
		{
			this.stlScripts = stlScripts.Select(s => new ScriptWrapperWithCollectionEvent(s)).ToArray();
			this.stlScripts.ForEach(s => s.CollectionOccurredEvent += OnCollectionOccurred);
		}

		void OnCollectionOccurred(IStlItem script)
		{
			CollectionOccurredEvent?.Invoke(script);
		}

		readonly IEnumerable<ScriptWrapperWithCollectionEvent> stlScripts;

		public IEnumerable<IStlItem> CreateScripts(BusinessObjectFactory factory) => stlScripts;

		public event CollectionOccurredDelegate CollectionOccurredEvent;
	}
}
