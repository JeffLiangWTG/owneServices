using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Enterprise.RemotePrinting.Types;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class JobStatusFileWriterTest : TestCase
	{
		public void TestOldFormatJobStatusData()
		{
			var contents = new List<JobDetails>
			{
				new (Guid.NewGuid(), string.Empty, ProcessedStatus.Failed)
			};

			JobStatusFileWriter.Append(contents);

			var status = JobStatusFileWriter.ReadAll().ToList();

			AssertEquals(1, status.Count);
			AssertNullOrEmpty(status[0].FailureReason);
		}

		public void TestFailureReasonHasVerticalSeparatorAndOtherSpecialChars()
		{
			var contents = new List<JobDetails>
			{
				new (Guid.NewGuid(), "Failure 1 line1 ~!@#$%^&*()_+|/<>.,?[]{{}};'\r\nFailure 1 line2", ProcessedStatus.Failed),
				new (Guid.NewGuid(), "Normal Reason", ProcessedStatus.Failed)
			};

			JobStatusFileWriter.Append(contents);

			var status = JobStatusFileWriter.ReadAll().ToList();

			AssertEquals(2, status.Count);
			AssertEquals("Failure 1 line1 ~!@#$%^&*()_+|/<>.,?[]{{}};'  Failure 1 line2", status[0].FailureReason);
			AssertEquals("Normal Reason", status[1].FailureReason);
		}

		public void TestFailureReasonHasMultipleLines()
		{
			var contents = new List<JobDetails>
			{
				new (Guid.NewGuid(), "Failure 1 line1\r\nFailure 1 line2", ProcessedStatus.Failed),
				new (Guid.NewGuid(), "Failure 2 line1\r\nFailure 2 line2", ProcessedStatus.Failed)
			};

			JobStatusFileWriter.Append(contents);

			var status = JobStatusFileWriter.ReadAll().ToList();
			AssertEquals(2, status.Count);
			AssertEquals("Failure 1 line1  Failure 1 line2", status[0].FailureReason);
			AssertEquals("Failure 2 line1  Failure 2 line2", status[1].FailureReason);
		}

		public void TestJobStatusFileWriter()
		{
			var contents = new List<JobDetails>
			{
				new (Guid.NewGuid(), "Failure 1", ProcessedStatus.Failed),
				new (Guid.NewGuid(), "Success", ProcessedStatus.Processed),
				new (Guid.NewGuid(), "Failure 2", ProcessedStatus.Failed),
				new (Guid.NewGuid(), "Success", ProcessedStatus.Processed),
				new (Guid.NewGuid(), "Failure 3", ProcessedStatus.Failed),
				new (Guid.NewGuid(), "Failure 4", ProcessedStatus.Failed),
				new (Guid.NewGuid(), "Success", ProcessedStatus.Processed),
				new (Guid.NewGuid(), "Success", ProcessedStatus.Processed),
				new (Guid.NewGuid(), "Success", ProcessedStatus.Processed)
			};

			JobStatusFileWriter.Append(contents);

			var status = JobStatusFileWriter.ReadAll();
			AssertEquals(9, status.Count());

			Assert(File.Exists(JobStatusFileWriter.FilePath));
			JobStatusFileWriter.Delete();
			Assert(!File.Exists(JobStatusFileWriter.FilePath));
		}

		public void TestAppendLogs()
		{
			Assert(!File.Exists(JobStatusFileWriter.FilePath));

			var contents = new List<JobDetails>
			{
				new (Guid.NewGuid(), string.Empty, ProcessedStatus.Failed)
			};

			var logs = new List<string>();

			var directoryPath = Path.GetDirectoryName(JobStatusFileWriter.FilePath);
			// Cannot delete directory for testing because test may not have rights to delete directory
			var expectCreateDirectoryLog = directoryPath != null && !Directory.Exists(directoryPath);

			JobStatusFileWriter.Append(contents, log => logs.Add(log));

			if (expectCreateDirectoryLog)
			{
				AssertEquals(4, logs.Count);
				AssertEquals("1 processed jobs statuses to be saved.", logs[0]);
				AssertEquals("Creating directory for client temporary local data storage: " + directoryPath, logs[1]);
				AssertEquals("Saving processed jobs statuses to local file.", logs[2]);
				AssertEquals("Processed job statuses successfully saved to local file.", logs[3]);
			}
			else
			{
				AssertEquals(3, logs.Count);
				AssertEquals("1 processed jobs statuses to be saved.", logs[0]);
				AssertEquals("Saving processed jobs statuses to local file.", logs[1]);
				AssertEquals("Processed job statuses successfully saved to local file.", logs[2]);
			}

			logs.Clear();
			JobStatusFileWriter.Append(contents, log => logs.Add(log));

			AssertEquals(3, logs.Count);
			AssertEquals("Saving processed jobs statuses to local file.", logs[1]);

			logs.Clear();
			JobStatusFileWriter.ReadAll(logs.Add);

			AssertEquals(1, logs.Count);
			AssertEquals("Reading 2 processed jobs statuses from local file.", logs[0]);
		}

		public void TestReadAllLogs()
		{
			var contents = new List<JobDetails>
			{
				new (Guid.NewGuid(), "Success", ProcessedStatus.Processed),
				new (Guid.NewGuid(), "Failure", ProcessedStatus.Failed)
			};

			JobStatusFileWriter.Append(contents);

			var logs = new List<string>();

			_ = JobStatusFileWriter.ReadAll(log => logs.Add(log)).ToList();

			AssertEquals(1, logs.Count);
			AssertEquals("Reading 2 processed jobs statuses from local file.", logs[0]);
		}

		public void TestReadAllWithInvalidDataLogs()
		{
			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();
			var guid3 = Guid.NewGuid();
			var guid4 = Guid.NewGuid();
			var guid5 = Guid.NewGuid();

			var contents = new List<JobDetails>
			{
				new (guid1, "Success", ProcessedStatus.Processed),
				new (guid2, "Failure", ProcessedStatus.Failed)
			};

			JobStatusFileWriter.Append(contents);

			File.AppendAllLines(JobStatusFileWriter.FilePath, new []
			{
				"",
				guid3.ToString(),
				guid4 + "|" + ProcessedStatus.Processed,
				"nonsense1",
				guid5 + "|nonsense2",
				"nonsense3|" + ProcessedStatus.Processed,
				"nonsense4|nonsense5",
			} );

			var logs = new List<string>();

			var status = JobStatusFileWriter.ReadAll(log => logs.Add(log)).ToList();

			AssertEquals("Should return objects only for non-empty strings", 8, status.Count);

			AssertEquals(guid1, status[0].PK);
			AssertEquals(guid2, status[1].PK);
			AssertEquals(guid3, status[2].PK);
			AssertEquals(guid4, status[3].PK);
			AssertEquals(Guid.Empty, status[4].PK);
			AssertEquals(guid5, status[5].PK);
			AssertEquals(Guid.Empty, status[6].PK);
			AssertEquals(Guid.Empty, status[7].PK);

			AssertEquals(8, logs.Count);

			AssertEquals("Reading 9 processed jobs statuses from local file.", logs[0]);

			AssertEquals("Invalid data stored in processed jobs status file: " + guid3, logs[1]);

			AssertEquals("Invalid data stored in processed jobs status file: nonsense1", logs[2]);
			AssertEquals("Invalid guid value in processed jobs status file: nonsense1", logs[3]);

			AssertEquals("Invalid status value in processed jobs status file: " + guid5 + "|nonsense2", logs[4]);

			AssertEquals("Invalid guid value in processed jobs status file: nonsense3|" + ProcessedStatus.Processed, logs[5]);

			AssertEquals("Invalid guid value in processed jobs status file: nonsense4|nonsense5", logs[6]);
			AssertEquals("Invalid status value in processed jobs status file: nonsense4|nonsense5", logs[7]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			JobStatusFileWriter.Delete();
		}

		protected override void TearDown()
		{
			JobStatusFileWriter.Delete();
			base.TearDown();
		}
	}
}
