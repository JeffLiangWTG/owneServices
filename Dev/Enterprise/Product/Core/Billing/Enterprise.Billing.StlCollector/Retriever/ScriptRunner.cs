using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public static class ScriptRunner
	{
		public static IEnumerable<IStlTransaction> Run(IStlScript script, IDateTimeRange dateTimeRange, IStlTransactionFactory transactionFactory)
		{
			var bizoCollection = new DynamicBusinessObjectCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			bizoCollection.Load(script.ScriptText, script.GetInputParameters(dateTimeRange).ToArray(), script.TimeoutSecs);
			return bizoCollection.ToArray().Select(b => ((IBusinessObjectInternals)b).Row).Select(r => transactionFactory.CreateTransaction(script, r)).Where(r => r != null);
		}
	}
}
