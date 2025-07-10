using System.ComponentModel;
using System.IO;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class DirectTempFileDeleteTest : TestCase
	{
		public void TestDeletingInUseFile()
		{
			string fileName;
			using (TempFile tempFile = TempFile.New())
			{
				fileName = tempFile.Filename;
				using (FileStream s = File.Create(tempFile.Filename))
				{
					s.Close();
				}
				JamFileOpenForALittleWhileOnAnotherThread(tempFile.Filename);
				tempFile.Dispose();
			}
			Assert(!File.Exists(fileName));
		}

		void JamFileOpenForALittleWhileOnAnotherThread(string fileName)
		{
			worker = new BackgroundWorker();
			worker.DoWork += new DoWorkEventHandler(worker_DoWork);
			worker.RunWorkerAsync(fileName);
		}

		void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			using (FileStream s = File.Open(e.Argument.ToString(), FileMode.Open))
			{
				System.Threading.Thread.Sleep(1500);
			}
		}
		BackgroundWorker worker;

		protected override void TearDown()
		{
			if (worker != null)
			{
				worker.Dispose();
			}
			base.TearDown();
		}
	}
}
