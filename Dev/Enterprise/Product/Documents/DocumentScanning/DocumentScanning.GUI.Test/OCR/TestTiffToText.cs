using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;

namespace Enterprise.DocumentScanning.OCR.Testing
{
	public class TestTiffToText : TestCaseWithFactory
	{
		readonly string WorkingPath = Path.Combine(Temp.TempPath, "OCR Test Directory");
		TiffToText tiffToText;

		public TestTiffToText()
			: base()
		{
		}

		protected override void SetUp()
		{
			base.SetUp();
			CleanUpWorkingDirectory();
			Directory.CreateDirectory(WorkingPath);
			tiffToText = new TiffToText();
		}

		// Intermittently fails when running on Auto Tester. Don't know why.
		protected void CleanUpWorkingDirectory()
		{
			int tries = 0;

			try
			{
				// Will fail if the code under test has left files open,
				// which is what we want because the production code should not leave any files open!
				DirectoryInfo working = new DirectoryInfo(WorkingPath);
				if (working.Exists)
				{
					foreach (FileSystemInfo f in working.GetFileSystemInfos())
					{
						f.Attributes &= ~(FileAttributes.ReadOnly);
						f.Delete();
					}
					working.Attributes &= ~(FileAttributes.ReadOnly);
					working.Delete();
				}
			}
			catch
			{
				// Attempt to recover from race conditions caused by delays in deleting the file.
				if (tries++ > 5)
				{
					throw;
				}
				else
				{
					System.Threading.Thread.Sleep(500);
				}
			}
		}

		protected override void TearDown()
		{
			if (tiffToText != null)
			{
				tiffToText = null;
			}

			CleanUpWorkingDirectory();
			base.TearDown();
		}

		public void TestIsOCRAvailable()
		{
			if (tiffToText.IsMODIXPAvailable())
			{
				AssertEquals(true, tiffToText.IsOCRAvailable());
			}
			else
			{
				AssertEquals(false, tiffToText.IsOCRAvailable());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		public void TestIsMODI2003Available()
		{
			Type mODIAvailable = Type.GetTypeFromProgID("MSPaper.Document");

			if (mODIAvailable != null)
			{
				if (mODIAvailable.GUID == new Guid("E1D05D9E-AEB3-49A4-A95E-1663676A507E"))
				{
					AssertEquals(true, tiffToText.IsMODI2003Available());
				}
				else
				{
					AssertEquals(false, tiffToText.IsMODI2003Available());
				}
			}
			else
			{
				Assert("MODI not installed on this computer. Can't run this test", true);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		public void TestIsMODIXPAvailable()
		{
			Type mODIAvailable = Type.GetTypeFromProgID("MSPaper.Document");

			if (mODIAvailable != null)
			{
				if (mODIAvailable.GUID == new Guid("F086132E-222E-410A-BED7-343FF4D963A7"))
				{
					AssertEquals(true, tiffToText.IsMODIXPAvailable());
				}
				else
				{
					AssertEquals(false, tiffToText.IsMODIXPAvailable());
				}
			}
			else
			{
				Assert("MODI not installed on this computer. Can't run this test", true);
			}
		}
	}
}
