using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.NumberFountain.Internal;
using NUnit.Framework;

namespace Enterprise.NumberFountain.Testing
{
	[UseSnapshotProtection]
	public class LossyNumberFountainTest : TestCase
	{
		const string FountainName = "LossyNumberFountainTest";

		public void TestEqualityMembers()
		{
			var owner1A = Guid.Parse("00000000-0000-0000-0000-000000000001");
			var owner1B = Guid.Parse("00000000-0000-0000-0000-000000000001");
			var owner2 = Guid.Parse("00000000-0000-0000-0000-000000000002");

			var fountain1A = new LossyNumberFountain("A", owner1A);
			var fountain1B = new LossyNumberFountain("a", owner1B);
			var fountain2 = new LossyNumberFountain("A", owner2);
			var fountain3 = new LossyNumberFountain("B", owner1A);
			var fountain4 = new NonFormattedNumberFountain("A", owner1A, false, 1, 1000);

			AssertEquals(false, fountain1A.Equals(null));
			AssertEquals(true, fountain1A.Equals(fountain1A));
			AssertEquals(true, fountain1A.Equals(fountain1B));
			AssertEquals(false, fountain1A.Equals(fountain2));
			AssertEquals(false, fountain1A.Equals(fountain3));
			AssertEquals(false, fountain1A.Equals(fountain4));

			AssertEquals(false, ((IEquatable<INumberFountain>)fountain1A).Equals(null));
			AssertEquals(true, ((IEquatable<INumberFountain>)fountain1A).Equals(fountain1A));
			AssertEquals(true, ((IEquatable<INumberFountain>)fountain1A).Equals(fountain1B));
			AssertEquals(false, ((IEquatable<INumberFountain>)fountain1A).Equals(fountain2));
			AssertEquals(false, ((IEquatable<INumberFountain>)fountain1A).Equals(fountain3));
			AssertEquals(false, ((IEquatable<INumberFountain>)fountain1A).Equals(fountain4));

			AssertEquals(false, object.Equals(null, fountain1A));
			AssertEquals(false, object.Equals(fountain1A, null));
			AssertEquals(true, object.Equals(fountain1A, fountain1A));
			AssertEquals(true, object.Equals(fountain1A, fountain1B));
			AssertEquals(false, object.Equals(fountain1A, fountain2));
			AssertEquals(false, object.Equals(fountain1A, fountain3));
			AssertEquals(false, object.Equals(fountain1A, fountain4));

			AssertEquals(fountain1A.GetHashCode(), fountain1A.GetHashCode());
			AssertEquals(fountain1A.GetHashCode(), fountain1B.GetHashCode());
			AssertNotEquals("(not a strict requirement)", fountain1A.GetHashCode(), fountain2.GetHashCode());
			AssertNotEquals("(not a strict requirement)", fountain1A.GetHashCode(), fountain3.GetHashCode());
		}

		public void TestGetNext()
		{
			var fountain = new LossyNumberFountain(FountainName, Guid.Empty);
			var allValues = new List<long>();

			const int numberCount = 131;

			using (Db.Connection.BeginTransactionWithManager())
			{
				var ctx = Db.Connection.GetFountainContext();
				for (int i = 0; i < numberCount; i++)
				{
					allValues.Add(fountain.GetNext(ctx, false));
				}
			}

			allValues.AssertStrictlyOrdered(numberCount);

			// Since we control sequence creation in this test, we can predict resulting values and check that there are no unnecessary gaps.
			AssertEquals("first value", 1, allValues.First());
			AssertEquals("last value", numberCount, allValues.Last());
		}

		public void TestGetNextWithoutTransaction()
		{
			var fountain = new LossyNumberFountain(FountainName + Guid.NewGuid().ToString(), Guid.Empty, string.Empty, string.Empty, 8, 1, 100);
			Db.Connection.EnsureIsOpen();
			AssertNoExceptionThrown(() => fountain.GetNext(Db.Connection.GetFountainContext(), false));
		}

		public void TestGetNextFormatted()
		{
			var fountain = new LossyNumberFountain(FountainName, Guid.Empty, "XXXXXX", "@@@", 12);
			var allValues = new List<string>();

			const int numberCount = 131;

			using (Db.Connection.BeginTransactionWithManager())
			{
				var ctx = Db.Connection.GetFountainContext();
				for (int i = 0; i < numberCount; i++)
				{
					allValues.Add(fountain.GetNextFormatted(ctx, false));
				}
			}

			AssertEquals(allValues.Count, numberCount);

			// Since we control sequence creation in this test, we can predict resulting values and check that there are no unnecessary gaps.
			AssertEquals("first value", "XXXXXX000000000001@@@", allValues.First());
			AssertEquals("last value", "XXXXXX000000000131@@@", allValues.Last());
		}

		public void TestGetNexts()
		{
			var fountain = new LossyNumberFountain(FountainName, Guid.Empty);
			var allValues = new List<long>();

			using (Db.Connection.BeginTransactionWithManager())
			{
				var ctx = Db.Connection.GetFountainContext();
				allValues.AddRange(fountain.GetNexts(ctx, 1, false).AssertStrictlyOrdered(1));
				allValues.AddRange(fountain.GetNexts(ctx, 10, false).AssertStrictlyOrdered(10));
				allValues.AddRange(fountain.GetNexts(ctx, 100, false).AssertStrictlyOrdered(100));
				allValues.AddRange(fountain.GetNexts(ctx, 1000, false).AssertStrictlyOrdered(1000));
				allValues.AddRange(fountain.GetNexts(ctx, 10000, false).AssertStrictlyOrdered(10000));
				allValues.AddRange(fountain.GetNexts(ctx, 1000, false).AssertStrictlyOrdered(1000));
				allValues.AddRange(fountain.GetNexts(ctx, 100, false).AssertStrictlyOrdered(100));
				allValues.AddRange(fountain.GetNexts(ctx, 10, false).AssertStrictlyOrdered(10));
				allValues.AddRange(fountain.GetNexts(ctx, 1, false).AssertStrictlyOrdered(1));
			}

			allValues.AssertStrictlyOrdered(12222);

			// Since we control sequence creation in this test, we can predict resulting values and check that there are no unnecessary gaps.
			AssertEquals("first value", 1, allValues.First());
			AssertEquals("last value", 12222, allValues.Last());
		}

		public void TestGetNextsFormatted()
		{
			var fountain = new LossyNumberFountain(FountainName, Guid.Empty, "XXXXXX", "@@@", 12);
			var allValues = new List<string>();

			using (Db.Connection.BeginTransactionWithManager())
			{
				var ctx = Db.Connection.GetFountainContext();
				allValues.AddRange(fountain.GetNextsFormatted(ctx, 1, false));
				allValues.AddRange(fountain.GetNextsFormatted(ctx, 10, false));
				allValues.AddRange(fountain.GetNextsFormatted(ctx, 100, false));
				allValues.AddRange(fountain.GetNextsFormatted(ctx, 1000, false));
				allValues.AddRange(fountain.GetNextsFormatted(ctx, 10000, false));
				allValues.AddRange(fountain.GetNextsFormatted(ctx, 1000, false));
				allValues.AddRange(fountain.GetNextsFormatted(ctx, 100, false));
				allValues.AddRange(fountain.GetNextsFormatted(ctx, 10, false));
				allValues.AddRange(fountain.GetNextsFormatted(ctx, 1, false));
			}

			// Since we control sequence creation in this test, we can predict resulting values and check that there are no unnecessary gaps.
			AssertEquals("first value", "XXXXXX000000000001@@@", allValues.First());
			AssertEquals("last value", "XXXXXX000000012222@@@", allValues.Last());
		}

		public void TestParameterValidation()
		{
			AssertExceptionThrown<ArgumentException>(() => new LossyNumberFountain(null, Guid.Empty));

			var fountain = new LossyNumberFountain("LossyNumberFountainTest", Guid.Empty);
			AssertEquals(FountainUtils.MinNumber, fountain.MinValue);
			AssertEquals(FountainUtils.MaxNumber, fountain.MaxValue);

			using (Db.Connection.BeginTransactionWithManager())
			{
				AssertExceptionThrown<ArgumentOutOfRangeException>(() => fountain.GetNexts(Db.Connection.GetFountainContext(), -1, false));
				AssertExceptionThrown<ArgumentOutOfRangeException>(() => fountain.GetNexts(Db.Connection.GetFountainContext(), 0, false));
			}

			_ = new LossyNumberFountain(new string('A', 91), Guid.Empty);
			AssertExceptionThrown<ArgumentException>(() => new LossyNumberFountain(new string('A', 92), Guid.Empty));
			AssertExceptionThrown<ArgumentException>(() => new LossyNumberFountain("LossyNumberFountainTestMaxBeforeMin", Guid.Empty, "", "", FountainUtils.DefaultFormatDigits, 2_000_000, 1_999_999));
			AssertExceptionThrown<ArgumentException>(() => new LossyNumberFountain("[A", Guid.Empty));
			AssertExceptionThrown<ArgumentException>(() => new LossyNumberFountain("A]", Guid.Empty));
			AssertExceptionThrown<ArgumentException>(() => new LossyNumberFountain("[A]", Guid.Empty));

			AssertContains("Sequence name", AssertExceptionThrown<System.Data.Common.DbException>(() => Db.Connection.ExecuteNonQuery($"EXEC FountainLossyCreateSequenceAtLoopback '[TestSpecialCharacters' , 1, 1000")).Message);
			AssertContains("Sequence name", AssertExceptionThrown<System.Data.Common.DbException>(() => Db.Connection.ExecuteNonQuery($"EXEC FountainLossyCreateSequenceAtLoopback 'TestSpecialCharacters]', 1, 1000")).Message);
			AssertContains("Sequence name", AssertExceptionThrown<System.Data.Common.DbException>(() => Db.Connection.ExecuteNonQuery($"EXEC FountainLossyCreateSequenceAtLoopback '[TestSpecialCharacters]', 1, 1000")).Message);
			Db.Connection.ExecuteNonQuery($"EXEC FountainLossyCreateSequenceAtLoopback 'TestSpecialCharacters', 1, 1000");
		}

		public void TestSequenceName()
		{
			var owner1 = Guid.Parse("5AF12F1C-8C2B-44EA-B26A-BF1D30FA5741");
			var owner2 = Guid.Parse("47F53FC9-DE2E-4400-9CF4-86CA7B9D3607");

			var fountain = new LossyNumberFountain("LossyNumberFountainTest1", owner1);
			var fountainLowerCase = new LossyNumberFountain("lossynumberfountaintest1", owner1);
			var fountainUpperCase = new LossyNumberFountain("LOSSYNUMBERFOUNTAINTEST1", owner1);

			var fountainWithDifferentName = new LossyNumberFountain("LossyNumberFountainTestDifferentName", owner1);
			var fountainWithDifferentOwner = new LossyNumberFountain("LossyNumberFountainTest", owner2);

			using (var tx = Db.Connection.BeginTransactionWithManager())
			{
				var ctx = Db.Connection.GetFountainContext();

				AssertEquals(1, fountain.GetNext(ctx, false));
				AssertEquals(2, fountainLowerCase.GetNext(ctx, false));
				AssertEquals(3, fountainUpperCase.GetNext(ctx, false));

				AssertEquals(1, fountainWithDifferentName.GetNext(ctx, false));
				AssertEquals(1, fountainWithDifferentOwner.GetNext(ctx, false));

				tx.CommitTransaction();
			}
		}

		public void TestNumberFountainMaximumValueReachedException()
		{
			var fountain = new LossyNumberFountain(FountainName, Guid.Empty, "", "", FountainUtils.DefaultFormatDigits, 1, 100);

			using (var tx = Db.Connection.BeginTransactionWithManager())
			{
				var ctx = Db.Connection.GetFountainContext();
				AssertEquals(80, fountain.GetNexts(ctx, 80, false).Last());

				AssertEquals("(pre-condition)", true, Db.Connection.IsInTransaction);
				AssertContains(FountainName, AssertExceptionThrown<NumberFountainMaximumValueReachedException>(() => fountain.GetNexts(ctx, 21, false)).Message);

				// while undesirable, it is the behaviour of sp_sequence_get_range
				AssertEquals("reaching maximum rolls back transaction", false, Db.Connection.IsInTransaction);
				AssertExceptionThrown<Exception>(() => fountain.GetNext(ctx, false));
				AssertContains("SqlTransaction has completed", AssertExceptionThrown<InvalidOperationException>(() => tx.CommitTransaction()).Message);
			}

			using (var tx = Db.Connection.BeginTransactionWithManager())
			{
				var ctx = Db.Connection.GetFountainContext();
				AssertEquals("numbers are still available", 81, fountain.GetNext(ctx, false));
				AssertEquals("numbers are still available", 100, fountain.GetNexts(ctx, 19, false).Last());
				tx.CommitTransaction();
			}
		}

		public void TestConcurrentGetNexts()
		{
			const int threadCount = 32;
			const int iterationCount = 100;
			int[] batchSizes = new int[] { 1, 2, 7, 11, 19, 197, 1103 };

			var results = FountainTestUtils.TestConcurrency(threadCount, () => new GetNextsTestTask(iterationCount, batchSizes));
			foreach (var result in results)
			{
				result.AssertStrictlyOrdered(result.Count);
			}

			int expectedNumberCount = threadCount * iterationCount * batchSizes.Sum();
			var allNumbers = results.SelectMany(r => r).ToList();
			var uniqueNumbers = allNumbers.Distinct().ToList();

			AssertEquals("expected number count", expectedNumberCount, allNumbers.Count);
			AssertEquals("numbers must be unique", allNumbers.Count, uniqueNumbers.Count);
		}

		public void TestConcurrentCreate()
		{
			const int threadCount = 500;
			const int fountainCount = 100;

			AssertEquals("(pre-condition) fountains should not exist yet", 0, Db.Connection.ExecuteScalar($"select count(*) from sys.sequences where name like '{CreateSequencesTestTask.FountainNamePrefix}%'"));

			var results = FountainTestUtils.TestConcurrency(threadCount, () => new CreateSequencesTestTask(fountainCount));

			var allNumbers = results.SelectMany(r => r).ToList();
			int expectedNumberCount = threadCount * fountainCount;
			AssertEquals("expected number count", expectedNumberCount, allNumbers.Count);

			var groupedNumbers = allNumbers.GroupBy(n => n).ToList();
			// each fountain produces numbers 1..threadCount
			AssertEquals("different numbers", threadCount, groupedNumbers.Count);
			foreach (var group in groupedNumbers)
			{
				AssertEquals("numbers of same kind", fountainCount, group.Count());
			}
		}

		public void TestNoDeadlock()
		{
			long readyThreads = 0;
			object monitor = new object();
			List<Thread> threads = new List<Thread>();
			List<long> values = new List<long>();
			ConcurrentBag<Exception> errors = new ConcurrentBag<Exception>();

			using (ManualResetEvent continueSignal = new ManualResetEvent(false))
			{
				for (int threadNum = 0; threadNum < 2; threadNum++)
				{
					string firstFountainName = FountainName + "#" + threadNum;
					string secondFountainName = FountainName + "#" + (threadNum + 1) % 2;

					Thread thread = new Thread(() =>
					{
						try
						{
							using (var conn = Db.NewExtraConnectionToMainDb())
							{
								using (var tx = conn.BeginTransactionWithManager())
								{
									var fountain1 = new LossyNumberFountain(firstFountainName, Guid.Empty);
									long value1 = fountain1.GetNext(conn.GetFountainContext(), false);

									lock (monitor)
									{
										values.Add(value1);
									}

									Interlocked.Increment(ref readyThreads);
									continueSignal.WaitOne();

									var fountain2 = new LossyNumberFountain(secondFountainName, Guid.Empty);
									long value2 = fountain2.GetNext(conn.GetFountainContext(), false);

									lock (monitor)
									{
										values.Add(value2);
									}

									tx.CommitTransaction();
								}
							}
						}
						catch (Exception ex)
						{
							Interlocked.Increment(ref readyThreads);
							errors.Add(ex);
						}
					});
					threads.Add(thread);
					thread.IsBackground = true;
					thread.Start();
				}

				while (Interlocked.Read(ref readyThreads) < 2)
				{
					Thread.Sleep(50);
				}

				continueSignal.Set();

				foreach (var thread in threads)
				{
					thread.Join();
				}
			}

			if (errors.Any())
			{
				throw new AggregateException("Error in one of the threads.", errors);
			}

			AssertEquals("1, 1, 2, 2", string.Join(", ", values));
		}

		class GetNextsTestTask : IConcurrencyTestTask<IReadOnlyList<long>>
		{
			readonly int iterationCount;
			readonly int[] batchSizes;

			DbConnection conn;
			LossyNumberFountain fountain;

			public GetNextsTestTask(int iterationCount, int[] batchSizes)
			{
				this.iterationCount = iterationCount;
				this.batchSizes = batchSizes;
			}

			public void Init()
			{
				conn = Db.NewExtraConnectionToMainDb();
				fountain = new LossyNumberFountain(FountainName, Guid.Empty);
			}

			public IReadOnlyList<long> Run(int threadNum)
			{
				List<long> result = new List<long>();
				for (int i = 0; i < iterationCount; i++)
				{
					using (var tx = conn.BeginTransactionWithManager())
					{
						var ctx = conn.GetFountainContext();
						foreach (int batchSize in batchSizes)
						{
							result.AddRange(fountain.GetNexts(ctx, batchSize, false).AssertStrictlyOrdered(batchSize));
						}

						// Sequences should not be affected by transactions. For test we will commit transaction in half of iterations, and rollback in the other half.
						if (i % 2 == 0)
						{
							tx.CommitTransaction();
						}
						else
						{
							tx.RollbackTransaction();
						}
					}
				}

				return result;
			}

			public void Cleanup()
			{
				conn?.Dispose();
			}
		}

		class CreateSequencesTestTask : IConcurrencyTestTask<IReadOnlyList<long>>
		{
			public const string FountainNamePrefix = FountainName + "#";

			readonly int fountainCount;

			DbConnection conn;

			public CreateSequencesTestTask(int fountainCount)
			{
				this.fountainCount = fountainCount;
			}

			public void Init()
			{
				conn = Db.NewExtraConnectionToMainDb();
			}

			public IReadOnlyList<long> Run(int threadNum)
			{
				List<long> result = new List<long>();
				using (var tx = conn.BeginTransactionWithManager())
				{
					var ctx = conn.GetFountainContext();
					for (int i = 0; i < fountainCount; i++)
					{
						var fountain = new LossyNumberFountain(FountainNamePrefix + i, Guid.Empty);
						result.Add(fountain.GetNext(ctx, false));
					}

					tx.CommitTransaction();
				}

				return result;
			}

			public void Cleanup()
			{
				conn?.Dispose();
			}
		}
	}
}
