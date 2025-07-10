using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.FTP.Testing
{
	[TestedType(typeof(FtpRegistry))]
	sealed class FtpRegistryTest : RegistryItemSetTestCase<FtpRegistry>
	{
	}
}
