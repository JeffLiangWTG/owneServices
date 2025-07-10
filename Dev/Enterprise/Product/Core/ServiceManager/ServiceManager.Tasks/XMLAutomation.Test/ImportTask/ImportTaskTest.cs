using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	internal abstract class ImportTaskTest : TestCaseWithFactory
	{
		public virtual void TestProcessFile()
		{
			string testDirectory = Env.TempPath;
			string fileName = GetFileName();
			try
			{
				CreateTestFile(testDirectory, fileName);
				SetRegistryItem(testDirectory);

				Task.Run();
				AssertEquals("Should contain 'Importing'", true, Notify.AsString.IndexOf("Importing " + fileName + "...") > -1);
				AssertEquals("Should contain 'Finished importing'", true, Notify.AsString.IndexOf("Finished importing " + fileName + ".") > -1);
				AssertEquals(true, !Notify.HasErrors);
			}
			finally
			{
				DeleteIfExists(Path.Combine(testDirectory, fileName));
			}
		}

		public void TestProcessLockedFile()
		{
			string testDirectory = Env.TempPath;
			string fileName = GetFileName();
			try
			{
				using (FileStream file = new FileStream(Path.Combine(testDirectory, fileName), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
				{
					using (StreamWriter writer = new StreamWriter(file))
					{
						writer.Write("Sample text");
						SetRegistryItem(testDirectory);

						Task.Run();
					}
				}

				AssertEquals("Should contain 'Importing'", true, Notify.AsString.IndexOf("Importing " + fileName + "...") > -1);
				AssertEquals(true, !Notify.HasErrors);
			}
			finally
			{
				DeleteIfExists(Path.Combine(testDirectory, fileName));
			}
		}

		#region Task

		protected ImportTask Task
		{
			get { return task ?? (task = GetImportTask()); }
		}
		ImportTask task;

		#endregion

		#region Notify

		protected NotificationBuffer Notify
		{
			get { return notify ?? (notify = new NotificationBuffer()); }
		}
		NotificationBuffer notify;

		#endregion

		protected virtual void CreateTestFile(string testDirectory, string fileName)
		{
			using (StreamWriter writer = new StreamWriter(Path.Combine(testDirectory, fileName)))
			{
				writer.Write("Sample text");
			}
		}

		protected abstract ImportTask GetImportTask();
		protected abstract ZString GetFileName();
		protected abstract void SetRegistryItem(ZString directoryName);
	}
}
