
using System.Collections.Generic;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	sealed class TestKnownExceptionStlItem : MockScript
	{
		public TestKnownExceptionStlItem(bool isMandatoryForMilestones = true, StlCollectorType collectorType = StlCollectorType.Dynamic)
		{
			IsMandatoryForMilestones = isMandatoryForMilestones;
			CollectorType = collectorType;
		}

		public override string Code => "~KE";
		public override string Feature => "KnownException";
		public override StlDataGrain StlGrain => StlDataGrain.Transactional;
		public override string ScriptText => "";
		public override int TimeoutSecs => 30;

		public override IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange)
		{
			yield break;
		}

		public override IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange)
		{
			var sqlTimeoutException = SqlExceptionBuilder.CreateSqlException(596, "Cannot continue the execution because the session is in the kill state.");
			throw sqlTimeoutException;
		}
	}
}

