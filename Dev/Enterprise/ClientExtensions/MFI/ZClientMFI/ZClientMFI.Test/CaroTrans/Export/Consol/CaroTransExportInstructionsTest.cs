using System.IO;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.MFI.CaroTrans.Export.Testing
{
	public class CaroTransExportInstructionsTest : TestCase
	{
		public void TestSpecifiedFilePathWithExtension()
		{
			var instruction = new CaroTransExportInstructions()
			{ FileExtension = FileExtensionType.ClientSpecific, BasePath = Env.TempPath, SpecifiedFilename = "ABC_FileName.xyz" };
			AssertEquals("SpecifiedFilePathWithExtension ", Path.Combine(Env.TempPath, "ABC_FileName.xyz"), instruction.SpecifiedFilePathWithExtension);
		}
	}
}
