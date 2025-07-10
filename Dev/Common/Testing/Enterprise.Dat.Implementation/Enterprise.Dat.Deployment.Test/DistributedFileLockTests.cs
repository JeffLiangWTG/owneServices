using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	sealed class DistributedFileLockTests : TestCase
	{
		public void TestNullOrEmptyPathIsNotAllowed()
		{
			_ = AssertExceptionThrown<ArgumentNullException>(() => _ = new DistributedFileLock(null));
			_ = AssertExceptionThrown<ArgumentException>(() => _ = new DistributedFileLock(string.Empty));
		}

		public void TestConstructorShouldCheckIfPathIsValid()
		{
			_ = AssertExceptionThrown<ArgumentException>(() => _ = new DistributedFileLock(@"\\A\C:\Random\Path"));
			_ = AssertExceptionThrown<ArgumentException>(() => _ = new DistributedFileLock(@"C:\Random\Path\To\File:"));
			_ = AssertExceptionThrown<ArgumentException>(() => _ = new DistributedFileLock(@"C:\Random\Path|To\File:"));
			AssertNoExceptionThrown(() => _ = new DistributedFileLock(@"C:\Users\Mesut.Soylu\AppData\Local\WiseTechGlobal"));
			AssertNoExceptionThrown(() => _ = new DistributedFileLock(@"C:/Random/Path/To\File"));
			AssertNoExceptionThrown(() => _ = new DistributedFileLock(@"\\SERVER\C$\Random\Path\To\File"));
		}

		public void TestAcquireLockIsSuccessful()
		{
			var fileToLock = GetSampleFilePath();
			using (var fileLock = new DistributedFileLock(fileToLock))
			{
				AssertEquals("Lock acquisition should succeed.", expected: true, fileLock.Acquire());
			}
		}

		public void TestDisposeShouldReleaseLock()
		{
			var fileToLock = GetSampleFilePath();
			using (var fileLock = new DistributedFileLock(fileToLock))
			{
				AssertEquals("Lock acquisition should succeed.", expected: true, fileLock.Acquire());
			}

			using (var newFileLock = new DistributedFileLock(fileToLock))
			{
				AssertEquals("Dispose should release lock", expected: false, newFileLock.Locked);
			}
		}

		public void TestAcquireLockIsFailed()
		{
			var fileToLock = GetSampleFilePath();
			using (var lockHolder = new DistributedFileLock(fileToLock))
			{
				var initialLockStatus = lockHolder.Acquire();

				using (var fileLock = new DistributedFileLock(fileToLock))
				{
					AssertEquals("Lock acquisition should succeed.", expected: true, initialLockStatus);
					AssertEquals("Lock acquisition should fail.", expected: false, fileLock.Acquire());
				}
			}
		}

		public void TestAcquireLockWithTimeoutShouldWait()
		{
			var fileToLock = GetSampleFilePath();
			var timeout = TimeSpan.FromSeconds(2);

			using (var lockHolder = new DistributedFileLock(fileToLock))
			{
				var initialLockStatus = lockHolder.Acquire();

				using (var fileLock = new DistributedFileLock(fileToLock))
				{
					var startTime = DateTime.Now;
					var lockResult = fileLock.Acquire(timeout);
					var elapsedTime = DateTime.Now - startTime;

					AssertEquals("Lock acquisition should succeed.", expected: true, initialLockStatus);
					AssertEquals("Lock acquisition should fail with timeout.", expected: false, lockResult);
					AssertEquals("Timeout should be respected.", expected: true, elapsedTime.TotalMilliseconds >= timeout.TotalMilliseconds);
				}
			}
		}

		public void TestAcquireLockWithTimeoutShouldReturnTrueIfLockAcquired()
		{
			var fileToLock = GetSampleFilePath();
			var timeout = TimeSpan.FromSeconds(2);

			var lockHolder = new DistributedFileLock(fileToLock);
			var initialLockStatus = lockHolder.Acquire();

			var l1 = new Task(() =>
			{
				Thread.Sleep(TimeSpan.FromSeconds(1));
				_ = lockHolder.Release();
			});

			var fileLock = new DistributedFileLock(fileToLock);
			var startTime = DateTime.Now;

			l1.Start();

			var lockResult = fileLock.Acquire(timeout);
			var elapsedTime = DateTime.Now - startTime;

			AssertEquals("Lock acquisition should succeed.", expected: true, initialLockStatus);
			AssertEquals("Lock acquisition should succeed with timeout.", expected: true, lockResult);
			AssertEquals("Timeout should be respected.", expected: true, elapsedTime.TotalSeconds < timeout.TotalSeconds);

			l1.Wait();
			lockHolder.Dispose();
		}

		public void TestReleaseLock()
		{
			var fileToLock = GetSampleFilePath();
			using (var fileLock = new DistributedFileLock(fileToLock))
			{
				var isAcquired = fileLock.Acquire();
				var isReleased = fileLock.Release();
				var lockStatus = fileLock.Locked;

				using (var newFileLock = new DistributedFileLock(fileToLock))
				{
					var newLockIsAcquired = newFileLock.Acquire();

					CombineAssertions("Lock should be released and acquisition should succeed.",
						() =>
						{
							AssertEquals(expected: true, isAcquired);
							AssertEquals(expected: true, isReleased);
							AssertEquals(expected: false, lockStatus);
							AssertEquals(expected: true, newLockIsAcquired);
						});
				}
			}
		}

		public void TestLockedInfoShouldReflectActualLockStatus()
		{
			var fileToLock = GetSampleFilePath();
			var lockFilePath = $"{fileToLock}.lock";

			using (var fileLock = new DistributedFileLock(fileToLock))
			{
				var initialLockStatus = fileLock.Locked;

				File.Create(lockFilePath).Close();

				var lockStatusAfterExtLock = fileLock.Locked;
				var releaseStatusWithExtLock = fileLock.Release();

				File.Delete(lockFilePath);

				var lockStatusAfterExtLockRemoved = fileLock.Locked;
				var releaseStatusAfterExtLockRemoved = fileLock.Release();

				CombineAssertions("Lock should reflect the actual lock status.",
					() =>
					{
						AssertEquals(expected: false, initialLockStatus);
						AssertEquals(expected: true, lockStatusAfterExtLock);
						AssertEquals(expected: false, releaseStatusWithExtLock);
						AssertEquals(expected: false, lockStatusAfterExtLockRemoved);
						AssertEquals(expected: false, releaseStatusAfterExtLockRemoved);
					});
			}
		}

		public void TestShouldAllowOnlyOneLockAcquisition()
		{
			var fileToLock = GetSampleFilePath();

			using (var fileLock = new DistributedFileLock(fileToLock))
			{
				var firstAcquireStatus = fileLock.Acquire();
				var secondAcquireStatus = fileLock.Acquire();

				CombineAssertions("Lock should if reacquired without releasing.",
					() =>
					{
						AssertEquals(expected: true, firstAcquireStatus);
						AssertEquals(expected: false, secondAcquireStatus);
					});
			}
		}

		public void TestConcurrentLocalAccessToLock()
		{
			var fileToLock = GetSampleFilePath();
			const int threadCount = 50;
			var lockObject = new DistributedFileLock(fileToLock);
			var tasks = new List<Task<bool>>();

			for (var i = 0; i < threadCount; i++)
			{
				tasks.Add(Task.Run(() => lockObject.Acquire(TimeSpan.FromSeconds(3))));
			}

			Task.WaitAll(tasks.ToArray());

			var successfulAcquisitions = tasks.Count(task => task.Result);
			AssertEquals("Only one thread should acquire the lock.", 1, successfulAcquisitions);
		}

		public void TestConcurrentDistributedAccessToLock()
		{
			var fileToLock = GetSampleFilePath();
			const int threadCount = 50;
			var tasks = new List<Task<bool>>();

			for (var i = 0; i < threadCount; i++)
			{
				var lockObject = new DistributedFileLock(fileToLock);

				tasks.Add(Task.Run(() => lockObject.Acquire(TimeSpan.FromSeconds(3))));
			}

			Task.WaitAll(tasks.ToArray());

			var successfulAcquisitions = tasks.Count(task => task.Result);
			AssertEquals("Only one thread should acquire the lock.", 1, successfulAcquisitions);
		}

		public void TestAcquireShouldIgnoreStaleLockFile()
		{
			var fileToLock = GetSampleFilePath();
			var lockFilePath = $"{fileToLock}.lock";

			using (var fileLock = new DistributedFileLock(fileToLock))
			{
				var firstAcquireStatus = fileLock.Acquire();
				File.SetCreationTimeUtc(lockFilePath, DateTime.UtcNow.AddDays(-5));

				using (var newFileLock = new DistributedFileLock(fileToLock))
				{
					var newAcquireStatus = newFileLock.Acquire();

					CombineAssertions("Lock should be released and acquisition should succeed.",
						() =>
						{
							AssertEquals(expected: true, firstAcquireStatus);
							AssertEquals(expected: true, newAcquireStatus);
						});
				}
			}
		}

		public void TestIsStaleShouldReturnTrueWhenFileNotFoundToHandleRaceCondition()
		{
			var fileToLock = GetSampleFilePath();
			var lockFilePath = $"{fileToLock}.lock";

			using (var fileLock = new DistributedFileLock(fileToLock))
			{
				File.Delete(lockFilePath);

				Assert(fileLock.IsStale);
			}
		}

		public void TestIfDirectoryNotExistAcquireShouldFailWithoutWait()
		{
			var fileToLock = @"\\Some\None\Existing\Path";

			using (var fileLock = new DistributedFileLock(fileToLock))
			{
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				var acquireResult = fileLock.Acquire(TimeSpan.FromSeconds(5));
				stopwatch.Stop();

				Assert("Acquire should fail", !acquireResult);
				Assert("Acquire should fail early without wait", stopwatch.Elapsed < TimeSpan.FromSeconds(2));
			}
		}

		TempDirectory CommonTempDirectory => commonTempDirectoryLazy ?? (commonTempDirectoryLazy = new TempDirectory());
		TempDirectory commonTempDirectoryLazy;

		string GetSampleFilePath()
		{
			var filename = Guid.NewGuid().ToString("N").Substring(0, 8) + ".tmp";
			return Path.Combine(CommonTempDirectory, filename);
		}

		protected override void TearDown()
		{
			commonTempDirectoryLazy?.Dispose();
			commonTempDirectoryLazy = null;
			base.TearDown();
		}
	}
}
