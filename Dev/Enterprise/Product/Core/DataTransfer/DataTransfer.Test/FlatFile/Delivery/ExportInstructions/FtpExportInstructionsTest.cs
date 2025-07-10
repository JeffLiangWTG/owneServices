using System.Net;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	[TestedType(typeof(FtpExportInstructions))]
	sealed class FtpExportInstructionsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProxy()
		{
			FtpExportInstructions instructions = new FtpExportInstructions();
			AssertNull("Proxy is null by default", instructions.Proxy);

			WebProxy proxy = new WebProxy();

			instructions.Proxy = proxy;
			AssertEquals("Proxy set", proxy, instructions.Proxy);
		}

		public void TestUriString()
		{
			FtpExportInstructions instructions = new FtpExportInstructions();
			AssertEquals("UriString", @"ftp://", instructions.UriString);

			instructions.ServerAddress = "/www.ServerAddress.com/";
			AssertEquals("UriString", @"ftp://www.ServerAddress.com", instructions.UriString);

			instructions.DestinationPath = @"\/tmpdir\nextdir/whatdir";
			AssertEquals("UriString", @"ftp://www.ServerAddress.com/tmpdir/nextdir/whatdir", instructions.UriString);

			instructions.PortNumber = 323;
			AssertEquals("UriString", @"ftp://www.ServerAddress.com:323/tmpdir/nextdir/whatdir", instructions.UriString);
		}
	}
}
