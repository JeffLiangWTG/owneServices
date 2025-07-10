using System;
using System.IO;
using System.Threading;
using CargoWise.IO;
using NUnit.Framework;

namespace CargoWise.Common
{
	public class BackgroundDataSaverTest : TestCase
	{
		public void TestDisposeDeletesBackupFile()
		{
			using (var saver = new BackgroundDataSaver(saveFilePath, TimeSpan.FromSeconds(1), (fileStream) =>
			{
			}, (e) =>
			{
			}))
			{
				for (int i = 0; !File.Exists(saveFilePath) && i < 50; i++)
				{
					Thread.Sleep(100);
				}

				Assert(File.Exists(saveFilePath));
				saver.DeleteFileOnDispose = true;
			}

			Assert(!File.Exists(saveFilePath));
		}

		public void TestDisposeWaitsForPendingSave()
		{
			using (var beginSaveEvent = new AutoResetEvent(false))
			{
				var saveActionComplete = false;
				var saveAction = new BackgroundDataFileAction((fileStream) =>
				{
					if (!saveActionComplete)
					{
						beginSaveEvent.Set();
						Thread.Sleep(1000);
						saveActionComplete = true;
					}
				});
				using (var saver = new BackgroundDataSaver(saveFilePath, TimeSpan.FromMilliseconds(100), saveAction, (e) =>
				{
				}))
				{
					Assert(beginSaveEvent.WaitOne(TimeSpan.FromSeconds(10)));
				}

				Assert(saveActionComplete);
				Assert(File.Exists(saveFilePath));
			}
		}

		public void TestBackupFileIsSavedAtFrequency()
		{
			using (var saver = new BackgroundDataSaver(saveFilePath, TimeSpan.FromSeconds(2), (fileStream) =>
			{
			}, (e) =>
			{
			}))
			{
				int saveCount = 0;
				for (int i = 0; i < 70; i++)
				{
					if (File.Exists(saveFilePath))
					{
						File.Delete(saveFilePath);
						saveCount++;
					}

					Thread.Sleep(100);
				}

				AssertEquals(3, saveCount);
			}

			Assert(File.Exists(saveFilePath));
		}

		public void TestExceptionsInSaveAreHandled()
		{
			var throwOnSave = true;
			var exceptionToThrow = new InvalidOperationException();
			void saveAction(Stream fs)
			{
				if (throwOnSave)
				{
					throw exceptionToThrow;
				}
			}

			using (var exceptionHandledEvent = new AutoResetEvent(false))
			{
				Exception exceptionHandled = null;
				void exceptionHandler(Exception e)
				{
					exceptionHandled = e;
					exceptionHandledEvent.Set();
				}

				using (var saver = new BackgroundDataSaver(saveFilePath, TimeSpan.FromMilliseconds(100), saveAction, exceptionHandler))
				{
					try
					{
						Assert(exceptionHandledEvent.WaitOne(TimeSpan.FromSeconds(5)));
						AssertEquals(exceptionToThrow, exceptionHandled);
					}
					finally
					{
						throwOnSave = false;
					}
				}
			}
		}

		public void TestOneBackupAtATime()
		{
			int sleepTime = 101;
			void saveAction(Stream fs)
			{
				--sleepTime;
				if (sleepTime < 0)
				{
					sleepTime = 100;
				}

				Thread.Sleep(TimeSpan.FromMilliseconds(sleepTime));
				for (int i = 0; i < 100; i++)
				{
					fs.Write(new byte[100], 0, 100);
				}
			}

			Exception exceptionHandled = null;
			using (var saver = new BackgroundDataSaver(saveFilePath, TimeSpan.FromMilliseconds(1), saveAction, e => exceptionHandled = e))
			{
				Thread.Sleep(TimeSpan.FromSeconds(1));
			}

			AssertNull(exceptionHandled);
		}

		public void TestLoad()
		{
			using (var saver = new BackgroundDataSaver(saveFilePath, TimeSpan.FromSeconds(2), (fileStream) =>
			{
			}, (e) =>
			{
			}))
			{
				bool loadActionCalled = false;
				var loadAction = new BackgroundDataFileAction((fileStream) =>
				{
					loadActionCalled = true;
				});
				//Load when file does not exist, shouldn't call action
				saver.Load(loadAction);
				Assert(!loadActionCalled);
				//Load when file does exist, should call action
				using (File.Create(saveFilePath))
				{
				}

				Assert(File.Exists(saveFilePath));
				saver.Load(loadAction);
				Assert(loadActionCalled);
			}
		}

		protected override void SetUp()
		{
			temp = new TempDirectory();
			saveFilePath = Path.Combine(temp.DirectoryName, "BackgroundDataSaverTest.txt");
			base.SetUp();
		}

		protected override void TearDown()
		{
			temp.Dispose();
			base.TearDown();
		}

		TempDirectory temp;
		string saveFilePath;
	}
}
