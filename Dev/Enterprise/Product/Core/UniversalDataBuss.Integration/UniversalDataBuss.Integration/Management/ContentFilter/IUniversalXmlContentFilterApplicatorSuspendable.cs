using System;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalXmlContentFilterApplicatorSuspendable
	{
		bool IsUniversalXmlContentFilterApplicatorSuspended { get; }
		IDisposable SuspendUniversalXmlContentFilterApplicator();
	}
}
