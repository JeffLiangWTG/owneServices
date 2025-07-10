using CargoWise.EntityFramework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	sealed class BillingDataCollectorWithUserAttendedLoggerForTest : BillingDataCollector
	{
		public BillingDataCollectorWithUserAttendedLoggerForTest(IUserAttendedStlRetrieverLogger logger)
			: this(logger, new BusinessObjectFactory())
		{
		}

		public BillingDataCollectorWithUserAttendedLoggerForTest(IUserAttendedStlRetrieverLogger logger, BusinessObjectFactory factory)
			: base(logger, factory, new ScriptLoader(new ScriptFactoryForTest()).Load(factory))
		{
		}
		public override bool SkipValidation { get; set; } = true;
	}
}
