using System;
using System.Collections.Generic;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	abstract class UsageScript : BaseStlScript
	{
		public override StlCollectorType CollectorType => StlCollectorType.Custom;
		public override string Name => GetType().Name;
		protected override string MinVersion => string.Empty;
		protected override string MaxVersion => string.Empty;
		protected override DateTime StartDateUtc => DateTime.MinValue;

		readonly IStlTransactionFactory transactionFactory = new UsageTransactionFactory();

		public override IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange) => ScriptRunner.Run(this, dateTimeRange, transactionFactory);
	}
}
