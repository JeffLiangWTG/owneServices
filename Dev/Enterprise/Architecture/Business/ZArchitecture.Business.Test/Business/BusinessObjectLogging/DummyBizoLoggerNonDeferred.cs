using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyBizoLoggerNonDeferred : DummyBizoLoggerBase
	{
		public DummyBizoLoggerNonDeferred(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override bool RunAfterOnSavingForAllBizos { get; }
	}
}
