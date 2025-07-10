using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Integration
{
	public interface ICertificateManager
	{
		bool IsAboutToExpire(byte[] certificate);
		bool IsExpired(byte[] certificate);
		string RequestCertificate(string csr);
		string RegisterCertificate(string csr, string module, string applicationDescription, string caRoot);
		string RolloverCertificate(string clientId, string csr, string caRoot = null);
		(string TenantId, string ClientId, byte[] Certificate, string StatusCode) DownloadCertificate(string operationId);
		IEnumerable<byte[]> DownloadCertificates(string clientId);
		Task<string> LoadDatabaseNumberByClientIdAsync(CancellationToken cancellationToken);
	}
}
