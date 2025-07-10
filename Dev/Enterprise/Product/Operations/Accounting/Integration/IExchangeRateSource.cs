using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Integration
{
	public interface IExchangeRateSource : IExchangeRateSourceBase
	{
		ControllerID SourceController { get; }
	}
}
