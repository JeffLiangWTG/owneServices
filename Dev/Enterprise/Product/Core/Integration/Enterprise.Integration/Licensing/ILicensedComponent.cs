using System;

namespace Enterprise.Integration.Licensing
{
	public interface ILicensedComponent
	{
		IDisposable LicensedComponentManager { get; }
	}
}
