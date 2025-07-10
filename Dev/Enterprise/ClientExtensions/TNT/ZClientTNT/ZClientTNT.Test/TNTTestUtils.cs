using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Client.TNT.Testing
{
	public class TNTTestUtils : TestCaseWithFactory, IDisposable
	{
		public const string ResourcePrefix = "Enterprise.Client.TNT.Testing.Testing.";
		public string CopyResourceToFile(string fileName, string resourcePrefix = ResourcePrefix)
		{
			var resourceName = resourcePrefix + fileName;
			var destinationPath = Path.Combine(TempDirectory, fileName);

			using (FileStream destination = File.Create(destinationPath))
			{
				try
				{
					GetType().Assembly.GetManifestResourceStream(resourceName).CopyTo(destination);
				}
				catch (NullReferenceException)
				{
				}
			}
			return destinationPath;
		}

		public string[] GetFileLines(string testFileName)
		{
			string fileData = new EmbeddedResourceRetriever().GetString(ResourcePrefix + testFileName);
			return fileData.Split('\r', '\n');
		}

		#region TempDirectory
		public string TempDirectory
		{
			get
			{
				if (fTempDirectory == null)
				{
					fTempDirectory = Path.Combine(Env.TempPath, "TNTTestUtils_" + ZDateTime.Now.ToString("yyyyMMdd"));
					DirectoryInfo directory = new DirectoryInfo(fTempDirectory);
					if (!directory.Exists)
					{
						directory.Create();
					}
				}

				return fTempDirectory;
			}
		}

		protected string fTempDirectory;
		#endregion
		#region File Properties
		#region File Names
		public static string SampleX2Name
		{
			get
			{
				return "SYD.X2.20040817.180100.ok";
			}
		}

		public static string Exit1TestFile1
		{
			get
			{
				return "SYD.X1.20040916.115806.ok";
			}
		}

		public static string Exit1TestFile2
		{
			get
			{
				return "SYD.X1.20040916.141317.ok";
			}
		}

		public static string Exit2TestFile1
		{
			get
			{
				return "SYD.X2.20040916.103614.ok";
			}
		}

		public static string Exit2TestFile2
		{
			get
			{
				return "SYD.X2.20040916.141716.ok";
			}
		}

		public static string Exit2CustomerProblemFile1
		{
			get
			{
				return "BNE.X2.20040916.172804.ok";
			}
		}

		public static string Exit2CustomerFileEntryTypeProblem
		{
			get
			{
				return "SYD.X2.20060119.101057.ok";
			}
		}

		public static string X2TestImportTwice
		{
			get
			{
				return "SYD.X2.20040922.102022.ok";
			}
		}

		public static string X2TestImportTwice2
		{
			get
			{
				return "SYD.X2.20040922.114029.ok";
			}
		}

		public static string X2TestImportTwice3
		{
			get
			{
				return "SYD.X2.20040924.143609.ok";
			}
		}

		public static string TinyFileName
		{
			get
			{
				return "SYD.X1.00000000.000000.ok";
			}
		}

		#endregion
		#region File Full Paths
		#region Public Static
		public string TinyFileFullName
		{
			get
			{
				return CopyResourceToFile(TinyFileName);
			}
		}

		public string InvalidFormatFileFullName
		{
			get
			{
				return CopyResourceToFile("SYD.X1.Invalid..Format.ok");
			}
		}

		public string InvalidFileNameFullName
		{
			get
			{
				return CopyResourceToFile("Invalid.File.Name");
			}
		}

		public string X2TestAttachDetach
		{
			get
			{
				return CopyResourceToFile("SYD.X2.20041102.140848.ok");
			}
		}

		public string IQDownFile
		{
			get
			{
				return CopyResourceToFile("AKL.IQDOWNE.20050716.002629.ok");
			}
		}

		public string TwoMawbsWithSameMasterbillFromOneFile
		{
			get
			{
				return CopyResourceToFile("AKL.X2.20060713.150553.OK");
			}
		}

		#endregion
		protected string SampleX2FullName
		{
			get
			{
				return CopyResourceToFile(SampleX2Name);
			}
		}

		protected string Exit1TestFile1FullName
		{
			get
			{
				return CopyResourceToFile(Exit1TestFile1);
			}
		}

		protected string Exit1TestFile2FullName
		{
			get
			{
				return CopyResourceToFile(Exit1TestFile2);
			}
		}

		protected string Exit2TestFile1FullName
		{
			get
			{
				return CopyResourceToFile(Exit2TestFile1);
			}
		}

		protected string Exit2TestFile2FullName
		{
			get
			{
				return CopyResourceToFile(Exit2TestFile2);
			}
		}

		protected string Exit2CustomerProblemFile1FullName
		{
			get
			{
				return CopyResourceToFile(Exit2CustomerProblemFile1);
			}
		}

		protected string X2TestImportTwiceFullName
		{
			get
			{
				return CopyResourceToFile(X2TestImportTwice);
			}
		}

		protected string X2TestImportTwice2FullName
		{
			get
			{
				return CopyResourceToFile(X2TestImportTwice2);
			}
		}

		protected string X2TestImportTwice3FullName
		{
			get
			{
				return CopyResourceToFile(X2TestImportTwice3);
			}
		}

		#endregion
		#endregion
		#region IDisposable Members
		public void Dispose()
		{
			DeleteTempDirectoryFiles();
		}

		public void DeleteTempDirectoryFiles()
		{
			if (Directory.Exists(TempDirectory))
			{
				CargoWise.IO.TempDirectory.DeleteDirectory(TempDirectory);
			}

			fTempDirectory = null;
		}
		#endregion
	}
}
