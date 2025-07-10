using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyBizoLoggerDeferred : DummyBizoLoggerBase
	{
		public DummyBizoLoggerDeferred(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override BusinessObject CreateSaveLog(BusinessObject loggingBizo)
		{
			base.CreateSaveLog(loggingBizo);
			return ReturnNullOnCreateSaveLog ? null : factory.New<DummyBizoForLoggerTests>();
		}

		public override bool RunAfterOnSavingForAllBizos { get; } = true;

		public bool ReturnNullOnCreateSaveLog { get; set; }
	}
}
