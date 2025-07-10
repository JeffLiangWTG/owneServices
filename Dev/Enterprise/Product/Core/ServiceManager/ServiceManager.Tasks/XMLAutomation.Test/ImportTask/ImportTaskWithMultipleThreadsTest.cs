using System;
using System.IO;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	sealed class ImportTaskWithMultipleThreadsTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestWithMultipleInstances()
		{
			SystemDataRegistry.Instance.LocalCartageDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.TempPath);

			BusinessObjectFactory factory = new BusinessObjectFactory();

			//GlbCompany[] newCompanies = new GlbCompany[NumOfFiles];

			string[] tempTaskDirectories = new string[NumOfFiles];
			string[] filePaths = new string[NumOfFiles];

			try
			{
				for (int i = 0; i < NumOfFiles; ++i)
				{
					//newCompanies[i] = Factory.New<GlbCompany>();
					//newCompanies[i].GC_Code = i.ToString();
					//newCompanies[i].GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
					//GlbBranch newBranch = newCompanies[i].Branches.AddNew();
					//newBranch.GB_Code = i.ToString();
					//newBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompanies[i].GC_RN_NKCountryCode)).RL_Code;

					//string tempTaskDirectory = Temp.GetNewTempSubdirectory();

					//SystemDataRegistry.Instance.LocalCartageDataImportDirectory.SetValue(newCompanies[i].PK.ToGuid(), Guid.Empty, Guid.Empty, tempTaskDirectory);

					filePaths[i] = CreateTestFile(i);
				}

				factory.Save();

				Thread[] threads = new Thread[NumOfThreads];
				var exceptions = new Exception[NumOfThreads];

				var numberOfProcessedFiles = new CounterObject();

				for (int i = 0; i < NumOfThreads; i++)
				{
					var index = i;
					exceptions[index] = null;
					threads[index] = new Thread(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							ThreadProcess(ref exceptions[index], numberOfProcessedFiles);
						}
					});
				}

				for (int i = 0; i < NumOfThreads; i++)
				{
					threads[i].Start();
				}

				for (int i = 0; i < NumOfThreads; i++)
				{
					threads[i].Join();
				}

				for (int i = 0; i < NumOfThreads; i++)
				{
					Assert(exceptions[i] == null ? "" : exceptions[i].ToString() + "Error in thread " + i, exceptions[i] == null);
				}

				AssertEquals("Files are processed and deleted", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				for (int i = 0; i < NumOfFiles; ++i)
				{
					Assert(string.Format(Culture.Invariant, "File {0} should have been deleted", i), !File.Exists(filePaths[i]));
				}
				AssertEquals(NumOfFiles, numberOfProcessedFiles.counter);
			}
			finally
			{
				/*for (int i = 0; i < NumOfFiles; ++i)
				{
					Directory.Delete(tempTaskDirectories[i]);
				}*/
			}
		}

		internal class CounterObject
		{
			internal int counter;
		}

		void ThreadProcess(ref Exception exception, CounterObject numberOfProcessedFiles)
		{
			try
			{
				ImportTaskWithCallback task = new ImportTaskWithCallback(SystemDataRegistry.Instance.LocalCartageDataImportDirectory, fileInfo =>
				{
					Interlocked.Increment(ref numberOfProcessedFiles.counter);
				});
				task.Run();
			}
			catch (Exception ex)
			{
				exception = ex;
			}
		}

		string CreateTestFile(int i)
		{
			var path = FileName(i);
			using (StreamWriter writer = new StreamWriter(path))
			{
				writer.Write("Sample text " + i);
			}
			return path;
		}

		const int NumOfThreads = 20;
		const int NumOfFiles = 500;

		protected override void TearDown()
		{
			base.TearDown();
			for (int i = 0; i < NumOfFiles; ++i)
			{
				DeleteIfExists(FileName(i));
			}
		}

		string FileName(int i)
		{
			return Path.Combine(Env.TempPath, string.Format(Culture.Invariant, "TempFile {0}.xml", i));
		}
	}
}
