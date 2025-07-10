using System.IO;
using CargoWise.IO;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class FileTypeCheckerTest : TestCase
	{
		public void TestGetFileType()
		{
			//dll and exe aren't quick to distinguish, but we don't want randos to upload dlls either so it works out in practice
			//NOTE: Can't just grab an exe/dll from Bin because BaseSourcePath is banned ( https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/1494/Refactoring-BaseSourcePath )
			//and can't embed one because it's banned to add new exe/dlls to CW1 repo without a good enough reason (and this isn't good enough)
			//so we'll just make our own from bytes ( http://www.phreedom.org/research/tinype/ )
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			AssertEquals("exe", FileTypeChecker.GetFileType(new MemoryStream(new byte[] { 0x4D, 0x5A, 0x00, 0x00, 0x50, 0x45, 0x00, 0x00, 0x4C, 0x01, 0x01, 0x00, 0x6A, 0x2A, 0x58,
					0xC3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x03, 0x01, 0x0B, 0x01, 0x08, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00,
					0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04,
					0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x68, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 })));
			AssertEquals("zip", FileTypeChecker.GetFileType(new MemoryStream(resourceRetriever.GetBytes("CargoWise.EntityFramework.test.zip"))));
			AssertEquals("ico", FileTypeChecker.GetFileType(new MemoryStream(resourceRetriever.GetBytes("CargoWise.EntityFramework.Fax.ico"))));
			AssertEquals(null, FileTypeChecker.GetFileType(new MemoryStream(resourceRetriever.GetBytes("CargoWise.EntityFramework.test.txt"))));
			AssertEquals(null, FileTypeChecker.GetFileType(new MemoryStream(resourceRetriever.GetBytes("CargoWise.EntityFramework.test empty.txt"))));
		}
	}
}
