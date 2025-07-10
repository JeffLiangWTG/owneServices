using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.SystemToSystemTrust;
using Enterprise.Integration.Licensing;

namespace Enterprise.Registry.Business;

public class SystemToSystemTrustInfoService : ISystemToSystemTrustInfoService
{
	public string GetEnterpriseCode()
	{
		return ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
	}

	public void ResetSystemToSystemTrustInfo()
	{
		var trustInfo = new SystemToSystemTrustInfo();
		SaveSystemToSystemTrustInfo(trustInfo);
	}

	public ISystemToSystemTrustInfo RetrieveSystemToSystemTrustInfoNoCache()
	{
		SystemDataRegistry.Instance.SystemToSystemCertificate?.Inner.ClearCache();
		return RetrieveSystemToSystemTrustInfo();
	}

	public Task<ITokenConfigInfo> RetrieveTokenConfigAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ITokenConfigInfo tokenConfigInfo = RetrieveSystemToSystemTrustInfo();
		return Task.FromResult(tokenConfigInfo);
	}

	public void SaveSystemToSystemTrustInfo(ISystemToSystemTrustInfo systemToSystemTrustInfo)
	{
		SystemDataRegistry.Instance.SystemToSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemToSystemTrustInfo);
	}

	static ISystemToSystemTrustInfo RetrieveSystemToSystemTrustInfo()
	{
		return SystemDataRegistry.Instance.SystemToSystemCertificate?.Value ?? new SystemToSystemTrustInfo();
	}
}
