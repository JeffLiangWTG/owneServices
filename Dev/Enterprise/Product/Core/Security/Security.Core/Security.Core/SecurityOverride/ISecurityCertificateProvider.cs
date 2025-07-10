namespace Enterprise.Security
{
	public interface ISecurityCertificateProvider
	{
		SecurityCertificate this[SecurityCheckpoint checkpoint] { get; }
	}
}
