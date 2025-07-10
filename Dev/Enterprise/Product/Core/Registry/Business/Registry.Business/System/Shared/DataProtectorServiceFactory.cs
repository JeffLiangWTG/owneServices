using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.SystemToSystemTrust;

namespace Enterprise.Registry.Business;

public sealed class DataProtectorServiceFactory : IDataProtectorServiceFactory, ITokenSigningStatusChecker
{
	public IDataProtectorService Create()
	{
		var systemDataRegistry = SystemDataRegistry.Instance;
		var mechanism = systemDataRegistry.SystemToSystemTrustDataProtectionMechanism.Value;
		return mechanism switch
		{
			DataProtectionMechanisms.Codes.None => new NoProtectionDataProtectorService(),
			DataProtectionMechanisms.Codes.ActiveDirectory => new ActiveDirectoryDataProtectorService(systemDataRegistry.SystemToSystemTrustDataProtectionGroup.SecurityIdentifier),
			_ => throw new NotSupportedException($"Data protection mechanism '{mechanism}' is not supported.")
		};
	}

	public Task<bool> IsTokenSigningServiceActiveAsync(CancellationToken cancellationToken)
	{
		var mechanism = SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism.Value;
		var tokenSigningActive = mechanism switch
		{
			DataProtectionMechanisms.Codes.None => false,
			DataProtectionMechanisms.Codes.ActiveDirectory => true,
			_ => throw new NotSupportedException($"Data protection mechanism '{mechanism}' is not supported.")
		};
		return Task.FromResult(tokenSigningActive);
	}
}
