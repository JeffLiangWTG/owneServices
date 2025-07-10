using System.IO;
using CargoWise.BuildTools;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Setup.Testing
{
	public class BuildBatFileTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShouldEnableFIPSForCandle()
		{
			var batFile = File.ReadAllLines(BuildBatFilePath);
			var candleCommand = batFile[0];

			AssertEquals("The Federal Information Processing Standard (FIPS) appears to be enabled on the machine. Must use FIPS-compliant security algorithms to generate IDs by passing the -fips command-line.", true, candleCommand.Contains("-fips"));
		}

		string BuildBatFilePath => Path.Combine(BuildConstants.LocalEnterprisePath, BuildBatFile);

		const string BuildBatFile = @"Enterprise\Product\Documents\RemotePrinting\Client\Setup\Build.bat";
	}
}
